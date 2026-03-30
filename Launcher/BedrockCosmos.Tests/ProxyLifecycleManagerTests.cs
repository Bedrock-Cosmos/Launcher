using BedrockCosmos.Proxy;
using System;
using System.Collections.Generic;
using Xunit;

namespace BedrockCosmos.Tests
{
    public class ProxyLifecycleManagerTests
    {
        [Fact]
        public async Task ApplyProxyAsync_RefusesToApplyWhenProxyIsNotListening()
        {
            var accessor = new FakeProxySettingsAccessor(CreatePacSettings());
            var store = new FakeProxyStateStore();
            var probe = new FakeProbe(false);
            var manager = CreateManager(accessor, store, probe);

            ProxyApplyResult result = await manager.ApplyProxyAsync("127.0.0.1", 8000);

            Assert.False(result.Applied);
            Assert.Equal(0, accessor.ApplyCallCount);
            Assert.Null(store.State);
            Assert.Equal("pac-script", result.PreviousSettings.Classification);
        }

        [Fact]
        public async Task ApplyProxyAsync_WaitsForProxyToBecomeReadyWithinStartupWindow()
        {
            var accessor = new FakeProxySettingsAccessor(CreatePacSettings());
            var store = new FakeProxyStateStore();
            var probe = new FakeProbe(false, false, true);
            var manager = CreateManager(
                accessor,
                store,
                probe,
                TimeSpan.FromMilliseconds(25),
                TimeSpan.FromMilliseconds(1));

            ProxyApplyResult result = await manager.ApplyProxyAsync("127.0.0.1", 8000);

            Assert.True(result.Applied);
            Assert.Equal(1, accessor.ApplyCallCount);
            Assert.NotNull(store.State);
        }

        [Fact]
        public async Task RestoreIfOwned_RestoresPreviousSettingsOnCleanShutdown()
        {
            var previous = CreatePacSettings();
            var accessor = new FakeProxySettingsAccessor(previous);
            var store = new FakeProxyStateStore();
            var probe = new FakeProbe(true);
            var manager = CreateManager(accessor, store, probe);

            ProxyApplyResult applyResult = await manager.ApplyProxyAsync("127.0.0.1", 8000);
            ProxyRestoreResult restoreResult = manager.RestoreIfOwned("clean_shutdown");

            Assert.True(applyResult.Applied);
            Assert.True(restoreResult.Restored);
            Assert.True(previous.SemanticallyEquals(accessor.CurrentSettings));
            Assert.Null(store.State);
        }

        [Fact]
        public async Task RecoverStaleProxySettingsAsync_RestoresAfterCrash()
        {
            var previous = CreateManualSettings("http=proxy.local:3128", "localhost");
            var applied = ProxySettingsSnapshot.CreateBedrockCosmosProxy("127.0.0.1", 8000, previous.ProxyBypass);
            var accessor = new FakeProxySettingsAccessor(applied.Clone());
            var store = new FakeProxyStateStore
            {
                State = new ProxyOwnershipState
                {
                    ProxyHost = "127.0.0.1",
                    ProxyPort = 8000,
                    PreviousSettings = previous.Clone(),
                    AppliedSettings = applied.Clone()
                }
            };
            var probe = new FakeProbe(false);
            var manager = CreateManager(accessor, store, probe);

            ProxyStartupRecoveryResult recoveryResult = await manager.RecoverStaleProxySettingsAsync();

            Assert.True(recoveryResult.RepairedSettings);
            Assert.True(previous.SemanticallyEquals(accessor.CurrentSettings));
            Assert.Null(store.State);
        }

        [Fact]
        public async Task RestoreIfOwned_DoesNotOverwriteUnrelatedUserSettings()
        {
            var previous = CreatePacSettings();
            var accessor = new FakeProxySettingsAccessor(previous);
            var store = new FakeProxyStateStore();
            var probe = new FakeProbe(true);
            var manager = CreateManager(accessor, store, probe);

            await manager.ApplyProxyAsync("127.0.0.1", 8000);
            accessor.CurrentSettings = CreateManualSettings("http=other-proxy:8080", "intranet.local");

            ProxyRestoreResult restoreResult = manager.RestoreIfOwned("manual_repair");

            Assert.False(restoreResult.Restored);
            Assert.True(restoreResult.SkippedBecauseNotOwned);
            Assert.Equal("http=other-proxy:8080", accessor.CurrentSettings.ProxyServer);
        }

        [Fact]
        public async Task ApplyProxyAsync_AndRestore_PreservesExistingPacAndAutoDetectSettings()
        {
            var previous = new ProxySettingsSnapshot
            {
                Flags = ProxySettingsSnapshot.ProxyTypeDirect | ProxySettingsSnapshot.ProxyTypeAutoProxyUrl | ProxySettingsSnapshot.ProxyTypeAutoDetect,
                AutoConfigUrl = "http://wpad.example/proxy.pac",
                ProxyServer = null,
                ProxyBypass = null
            };
            previous.RefreshClassification();

            var accessor = new FakeProxySettingsAccessor(previous);
            var store = new FakeProxyStateStore();
            var probe = new FakeProbe(true);
            var manager = CreateManager(accessor, store, probe);

            await manager.ApplyProxyAsync("127.0.0.1", 8000);
            manager.RestoreIfOwned("clean_shutdown");

            Assert.True(previous.SemanticallyEquals(accessor.CurrentSettings));
            Assert.Equal("mixed", accessor.CurrentSettings.Classification);
        }

        [Fact]
        public async Task PerformHealthCheckAsync_RestoresSettingsWhenProxyStopsResponding()
        {
            var previous = CreatePacSettings();
            var accessor = new FakeProxySettingsAccessor(previous);
            var store = new FakeProxyStateStore();
            var probe = new FakeProbe(true);
            var manager = CreateManager(accessor, store, probe);
            bool healthFailureRaised = false;
            manager.ProxyHealthCheckFailed += (sender, args) => healthFailureRaised = true;

            await manager.ApplyProxyAsync("127.0.0.1", 8000);
            probe.IsListening = false;

            bool healthy = await manager.PerformHealthCheckAsync();

            Assert.False(healthy);
            Assert.True(healthFailureRaised);
            Assert.True(previous.SemanticallyEquals(accessor.CurrentSettings));
            Assert.Null(store.State);
        }

        private static ProxyLifecycleManager CreateManager(
            FakeProxySettingsAccessor accessor,
            FakeProxyStateStore store,
            FakeProbe probe,
            TimeSpan? startupReadinessTimeout = null,
            TimeSpan? startupReadinessPollInterval = null)
        {
            return new ProxyLifecycleManager(
                accessor,
                store,
                probe,
                new FakeLogger(),
                TimeSpan.FromMinutes(5),
                startupReadinessTimeout ?? TimeSpan.FromMilliseconds(10),
                startupReadinessPollInterval ?? TimeSpan.FromMilliseconds(1));
        }

        private static ProxySettingsSnapshot CreatePacSettings()
        {
            var snapshot = new ProxySettingsSnapshot
            {
                Flags = ProxySettingsSnapshot.ProxyTypeDirect | ProxySettingsSnapshot.ProxyTypeAutoProxyUrl,
                AutoConfigUrl = "http://wpad.example/proxy.pac",
                ProxyServer = null,
                ProxyBypass = null
            };
            snapshot.RefreshClassification();
            return snapshot;
        }

        private static ProxySettingsSnapshot CreateManualSettings(string proxyServer, string proxyBypass)
        {
            var snapshot = new ProxySettingsSnapshot
            {
                Flags = ProxySettingsSnapshot.ProxyTypeDirect | ProxySettingsSnapshot.ProxyTypeProxy,
                ProxyServer = proxyServer,
                ProxyBypass = proxyBypass,
                AutoConfigUrl = null
            };
            snapshot.RefreshClassification();
            return snapshot;
        }

        private sealed class FakeProxySettingsAccessor : IInternetProxySettingsAccessor
        {
            public FakeProxySettingsAccessor(ProxySettingsSnapshot currentSettings)
            {
                CurrentSettings = currentSettings.Clone();
            }

            public ProxySettingsSnapshot CurrentSettings { get; set; }
            public int ApplyCallCount { get; private set; }

            public void ApplySettings(ProxySettingsSnapshot settings)
            {
                ApplyCallCount++;
                CurrentSettings = settings.Clone();
                CurrentSettings.RefreshClassification();
            }

            public ProxySettingsSnapshot ReadCurrentSettings()
            {
                return CurrentSettings.Clone();
            }
        }

        private sealed class FakeProxyStateStore : IProxyStateStore
        {
            public ProxyOwnershipState State { get; set; }

            public void Delete()
            {
                State = null;
            }

            public ProxyOwnershipState Load()
            {
                return State;
            }

            public void Save(ProxyOwnershipState state)
            {
                State = state;
            }
        }

        private sealed class FakeProbe : IProxyAvailabilityProbe
        {
            private readonly Queue<bool> responses;

            public FakeProbe(bool isListening)
            {
                IsListening = isListening;
                responses = new Queue<bool>();
            }

            public FakeProbe(params bool[] responses)
            {
                this.responses = new Queue<bool>(responses ?? Array.Empty<bool>());
                IsListening = this.responses.Count > 0 ? this.responses.Peek() : false;
            }

            public bool IsListening { get; set; }

            public Task<bool> IsListeningAsync(string host, int port)
            {
                if (responses.Count > 0)
                {
                    bool next = responses.Dequeue();
                    IsListening = responses.Count > 0 ? responses.Peek() : next;
                    return Task.FromResult(next);
                }

                return Task.FromResult(IsListening);
            }
        }

        private sealed class FakeLogger : IProxyLifecycleLogger
        {
            public void Log(string eventName, object details)
            {
            }
        }
    }
}
