using BedrockCosmos.App;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

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

            if (!createdNew)
            {
                if (args.Length > 0)
                    SingleInstanceHelper.SendArgsToRunningInstance(args);
                else
                    LocalizedMessageBox.ShowInfo(
                        LanguageHandler.Get("Program.InstanceAlreadyOpen.Message"),
                        "Program.InstanceAlreadyOpen.Title");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var mainForm = new MainForm();

            SingleInstanceHelper.StartListening(mainForm);

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
