using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BedrockCosmos.Proxy
{
    public sealed class ProxyLifecycleManager : IDisposable
    {
        private const string OwnershipMarker = "bedrock-cosmos";

        private readonly IInternetProxySettingsAccessor settingsAccessor;
        private readonly IProxyStateStore stateStore;
        private readonly IProxyAvailabilityProbe readinessProbe;
        private readonly IProxyLifecycleLogger logger;
        private readonly TimeSpan monitoringInterval;
        private readonly TimeSpan startupReadinessTimeout;
        private readonly TimeSpan startupReadinessPollInterval;
        private readonly object syncRoot = new object();

        private ProxyOwnershipState activeState;
        private Timer healthTimer;
        private bool healthFailureHandled;

        public event EventHandler<ProxyHealthFailureEventArgs> ProxyHealthCheckFailed;

        public ProxyLifecycleManager(
            IInternetProxySettingsAccessor settingsAccessor,
            IProxyStateStore stateStore,
            IProxyAvailabilityProbe readinessProbe,
            IProxyLifecycleLogger logger,
            TimeSpan monitoringInterval,
            TimeSpan startupReadinessTimeout,
            TimeSpan startupReadinessPollInterval)
        {
            this.settingsAccessor = settingsAccessor;
            this.stateStore = stateStore;
            this.readinessProbe = readinessProbe;
            this.logger = logger;
            this.monitoringInterval = monitoringInterval;
            this.startupReadinessTimeout = startupReadinessTimeout;
            this.startupReadinessPollInterval = startupReadinessPollInterval;
        }

        public async Task<ProxyStartupRecoveryResult> RecoverStaleProxySettingsAsync()
        {
            ProxyOwnershipState persistedState = stateStore.Load();
            if (persistedState == null)
                return ProxyStartupRecoveryResult.NoRecovery();

            ProxySettingsSnapshot currentSettings = settingsAccessor.ReadCurrentSettings();
            if (!persistedState.AppliedSettings.SemanticallyEquals(currentSettings))
            {
                logger.Log("crash_recovery_skipped_settings_not_owned", new
                {
                    expected = persistedState.AppliedSettings,
                    current = currentSettings
                });

                stateStore.Delete();
                return ProxyStartupRecoveryResult.NoRecovery();
            }

            bool proxyIsListening = await readinessProbe.IsListeningAsync(
                persistedState.ProxyHost,
                persistedState.ProxyPort).ConfigureAwait(false);

            if (proxyIsListening)
            {
                logger.Log("crash_recovery_skipped_proxy_responding", new
                {
                    persistedState.ProxyHost,
                    persistedState.ProxyPort
                });

                return ProxyStartupRecoveryResult.NoRecovery();
            }

            settingsAccessor.ApplySettings(persistedState.PreviousSettings);
            stateStore.Delete();

            logger.Log("crash_recovery_triggered", new
            {
                persistedState.ProxyHost,
                persistedState.ProxyPort,
                previousSettings = persistedState.PreviousSettings
            });

            return new ProxyStartupRecoveryResult(true, persistedState.PreviousSettings);
        }

        public async Task<ProxyApplyResult> ApplyProxyAsync(string host, int port)
        {
            ProxySettingsSnapshot previousSettings = settingsAccessor.ReadCurrentSettings();
            logger.Log("previous_proxy_state_captured", previousSettings);

            bool readinessPassed = await WaitForProxyReadinessAsync(host, port).ConfigureAwait(false);
            logger.Log(readinessPassed ? "proxy_readiness_passed" : "proxy_readiness_failed", new { host, port });

            if (!readinessPassed)
                return ProxyApplyResult.NotReady(previousSettings);

            var appliedSettings = ProxySettingsSnapshot.CreateBedrockCosmosProxy(host, port, previousSettings.ProxyBypass);
            var state = new ProxyOwnershipState
            {
                OwnershipMarker = OwnershipMarker,
                OwnerProcessId = Process.GetCurrentProcess().Id,
                LauncherVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                AppliedAtUtc = DateTime.UtcNow,
                ProxyHost = host,
                ProxyPort = port,
                PreviousSettings = previousSettings.Clone(),
                AppliedSettings = appliedSettings.Clone()
            };

            stateStore.Save(state);
            activeState = state;

            try
            {
                settingsAccessor.ApplySettings(appliedSettings);
                logger.Log("proxy_applied", appliedSettings);
                StartMonitoring();
                return ProxyApplyResult.Success(previousSettings, appliedSettings);
            }
            catch
            {
                stateStore.Delete();
                activeState = null;
                throw;
            }
        }

        public ProxyRestoreResult RestoreIfOwned(string reason)
        {
            StopMonitoring();

            ProxyOwnershipState state = activeState ?? stateStore.Load();
            if (state == null)
                return ProxyRestoreResult.NotOwned();

            ProxySettingsSnapshot currentSettings = settingsAccessor.ReadCurrentSettings();
            if (!state.AppliedSettings.SemanticallyEquals(currentSettings))
            {
                logger.Log("proxy_restore_skipped_not_owned", new
                {
                    reason,
                    expected = state.AppliedSettings,
                    current = currentSettings
                });

                stateStore.Delete();
                activeState = null;
                return ProxyRestoreResult.Skipped();
            }

            settingsAccessor.ApplySettings(state.PreviousSettings);
            stateStore.Delete();
            activeState = null;

            logger.Log("proxy_restored", new
            {
                reason,
                restoredSettings = state.PreviousSettings
            });

            return ProxyRestoreResult.FromRestoredSettings(state.PreviousSettings);
        }

        public async Task<bool> PerformHealthCheckAsync()
        {
            ProxyOwnershipState state = activeState;
            if (state == null)
                return true;

            bool listening = await readinessProbe.IsListeningAsync(state.ProxyHost, state.ProxyPort).ConfigureAwait(false);
            if (listening)
                return true;

            lock (syncRoot)
            {
                if (healthFailureHandled)
                    return false;

                healthFailureHandled = true;
            }

            ProxyRestoreResult restoreResult = RestoreIfOwned("runtime_health_check_failed");
            var handler = ProxyHealthCheckFailed;
            if (handler != null)
                handler(this, new ProxyHealthFailureEventArgs(state.ProxyHost, state.ProxyPort, restoreResult));

            return false;
        }

        private async Task<bool> WaitForProxyReadinessAsync(string host, int port)
        {
            if (startupReadinessTimeout <= TimeSpan.Zero)
                return await readinessProbe.IsListeningAsync(host, port).ConfigureAwait(false);

            DateTime deadlineUtc = DateTime.UtcNow + startupReadinessTimeout;

            while (true)
            {
                bool listening = await readinessProbe.IsListeningAsync(host, port).ConfigureAwait(false);
                if (listening)
                    return true;

                TimeSpan remaining = deadlineUtc - DateTime.UtcNow;
                if (remaining <= TimeSpan.Zero)
                    return false;

                TimeSpan delay = startupReadinessPollInterval <= TimeSpan.Zero
                    ? remaining
                    : (remaining < startupReadinessPollInterval ? remaining : startupReadinessPollInterval);

                await Task.Delay(delay).ConfigureAwait(false);
            }
        }

        private void StartMonitoring()
        {
            lock (syncRoot)
            {
                healthFailureHandled = false;

                if (healthTimer == null)
                {
                    healthTimer = new Timer(
                        state => RunHealthCheckCallback(),
                        null,
                        monitoringInterval,
                        monitoringInterval);
                }
                else
                {
                    healthTimer.Change(monitoringInterval, monitoringInterval);
                }
            }
        }

        private void RunHealthCheckCallback()
        {
            try
            {
                PerformHealthCheckAsync().GetAwaiter().GetResult();
            }
            catch
            {
                // Health checks are best-effort and must not crash the launcher.
            }
        }

        private void StopMonitoring()
        {
            lock (syncRoot)
            {
                if (healthTimer != null)
                    healthTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        public void Dispose()
        {
            lock (syncRoot)
            {
                if (healthTimer != null)
                {
                    healthTimer.Dispose();
                    healthTimer = null;
                }
            }
        }
    }

    public sealed class ProxyStartupRecoveryResult
    {
        public ProxyStartupRecoveryResult(bool repairedSettings, ProxySettingsSnapshot restoredSettings)
        {
            RepairedSettings = repairedSettings;
            RestoredSettings = restoredSettings;
        }

        public bool RepairedSettings { get; private set; }
        public ProxySettingsSnapshot RestoredSettings { get; private set; }

        public static ProxyStartupRecoveryResult NoRecovery()
        {
            return new ProxyStartupRecoveryResult(false, null);
        }
    }

    public sealed class ProxyApplyResult
    {
        private ProxyApplyResult(bool applied, ProxySettingsSnapshot previousSettings, ProxySettingsSnapshot appliedSettings)
        {
            Applied = applied;
            PreviousSettings = previousSettings;
            AppliedSettings = appliedSettings;
        }

        public bool Applied { get; private set; }
        public ProxySettingsSnapshot PreviousSettings { get; private set; }
        public ProxySettingsSnapshot AppliedSettings { get; private set; }

        public static ProxyApplyResult Success(ProxySettingsSnapshot previousSettings, ProxySettingsSnapshot appliedSettings)
        {
            return new ProxyApplyResult(true, previousSettings, appliedSettings);
        }

        public static ProxyApplyResult NotReady(ProxySettingsSnapshot previousSettings)
        {
            return new ProxyApplyResult(false, previousSettings, null);
        }
    }

    public sealed class ProxyRestoreResult
    {
        private ProxyRestoreResult(bool restored, bool skippedBecauseNotOwned, ProxySettingsSnapshot restoredSettings)
        {
            Restored = restored;
            SkippedBecauseNotOwned = skippedBecauseNotOwned;
            RestoredSettings = restoredSettings;
        }

        public bool Restored { get; private set; }
        public bool SkippedBecauseNotOwned { get; private set; }
        public ProxySettingsSnapshot RestoredSettings { get; private set; }

        public static ProxyRestoreResult FromRestoredSettings(ProxySettingsSnapshot restoredSettings)
        {
            return new ProxyRestoreResult(true, false, restoredSettings);
        }

        public static ProxyRestoreResult Skipped()
        {
            return new ProxyRestoreResult(false, true, null);
        }

        public static ProxyRestoreResult NotOwned()
        {
            return new ProxyRestoreResult(false, false, null);
        }
    }

    public sealed class ProxyHealthFailureEventArgs : EventArgs
    {
        public ProxyHealthFailureEventArgs(string host, int port, ProxyRestoreResult restoreResult)
        {
            Host = host;
            Port = port;
            RestoreResult = restoreResult;
        }

        public string Host { get; private set; }
        public int Port { get; private set; }
        public ProxyRestoreResult RestoreResult { get; private set; }
    }
}
