using System;
using System.Threading;
using System.Windows.Forms;
using BedrockCosmos.App;
using BedrockCosmos.Proxy;
using System.IO;

// =============================================================================
// Bedrock Cosmos - Copyright (c) 2026
//
// This file is part of Bedrock Cosmos, licensed under the MIT License.
// You must read and agree to the terms of the MIT License before using,
// copying, modifying, or distributing this code.
//
// MIT License - Full terms: https://opensource.org/licenses/MIT
// =============================================================================

namespace BedrockCosmos
{
    internal static class Program
    {
        private const string AppName = "BedrockCosmos_139af231-3f91-4712-87a0-86f7d0fcfef5";
        private static Mutex mutex = null;

        [STAThread]
        static void Main(string[] args)
        {
            BootstrapApplicationState();

            bool createdNew;
            mutex = new Mutex(true, AppName, out createdNew);

            if (!createdNew) // App is already running
            {
                if (args.Length > 0)
                    SingleInstanceHelper.SendArgsToRunningInstance(args);
                else
                    MessageBox.Show(
                        LanguageHandler.Get("Program.InstanceAlreadyOpen.Message"),
                        LanguageHandler.Get("Program.InstanceAlreadyOpen.Title"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ProxyStartupRecoveryResult startupRecoveryResult;
            using (var proxyLifecycleManager = ProxyLifecycleFactory.CreateDefault())
            {
                startupRecoveryResult = proxyLifecycleManager
                    .RecoverStaleProxySettingsAsync()
                    .GetAwaiter()
                    .GetResult();
            }

            var mainForm = new MainForm(startupRecoveryResult);

            // Listens for URI/file from future instances
            SingleInstanceHelper.StartListening(mainForm);

            // Handles URI/file passed on this launch
            if (args.Length > 0)
                mainForm.Load += (s, e) => mainForm.HandleIncomingArgs(args);

            Application.Run(mainForm);

            mutex.ReleaseMutex();
        }

        private static void BootstrapApplicationState()
        {
            if (!Directory.Exists(PathDefinitions.CosmosAppData))
                Directory.CreateDirectory(PathDefinitions.CosmosAppData);

            LanguageHandler.Load("en_US");
            SettingsManager.LoadSettings();
            LanguageHandler.Load(SettingsManager.Language);
        }
    }
}
