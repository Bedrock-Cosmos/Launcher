using Newtonsoft.Json;
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

namespace BedrockCosmos.App
{
    internal static class SettingsManager
    {
        private static bool _proxyStarted = false;
        private static bool _backgroundMode = false;
        private static string _language = "en_US";
        private static int _devMenuClicks = 0;
        private static bool _devMenuEnabled = false;
        private static bool _enableLogging = false;
        private static bool _detailedLogging = false;
        private static bool _launcherUpdatePrompted = false;

        internal static bool ProxyStarted
        {
            get { return _proxyStarted; }
            set { _proxyStarted = value; }
        }

        internal static bool BackgroundMode
        {
            get { return _backgroundMode; }
            set { _backgroundMode = value; SaveSettings(); }
        }

        internal static string Language
        {
            get { return _language; }
            set { _language = value; SaveSettings(); }
        }

        internal static int DevMenuClicks
        {
            get { return _devMenuClicks; }
            set { _devMenuClicks = value; }
        }

        internal static bool DevMenuEnabled
        {
            get { return _devMenuEnabled; }
            set { _devMenuEnabled = value; SaveSettings(); }
        }

        internal static bool EnableLogging
        {
            get { return _enableLogging; }
            set { _enableLogging = value; SaveSettings(); }
        }

        internal static bool DetailedLogging
        {
            get { return _detailedLogging; }
            set { _detailedLogging = value; SaveSettings(); }
        }

        internal static bool LauncherUpdatePrompted
        {
            get { return _launcherUpdatePrompted; }
            set { _launcherUpdatePrompted = value; }
        }

        internal static bool DevMenuCheck()
        {
            if (_devMenuClicks < 7)
            { 
                _devMenuClicks++;
                return false;
            }
            else
            {
                if (!_devMenuEnabled)
                    CosmosConsole.WriteLine(LanguageHandler.Get("Logs.DevModeEnabled"));

                _devMenuEnabled = true;
                SaveSettings();
                return true;
            }
        }

        internal static void DisableDevMenu()
        {
            _devMenuClicks = 0;
            _devMenuEnabled = false;
            SaveSettings();
            CosmosConsole.WriteLine(LanguageHandler.Get("Logs.DevModeDisabled"));
        }

        internal static void SaveSettings()
        {
            string settingsFile = PathDefinitions.CosmosAppData + @"Settings.json";
            var savedSettings = new
            {
                BackgroundMode = _backgroundMode,
                Language = _language,
                DevMenuEnabled = _devMenuEnabled,
                EnableLogging = _enableLogging,
                DetailedLogging = _detailedLogging
            };

            string json = JsonConvert.SerializeObject(savedSettings, Formatting.Indented);
            File.WriteAllText(settingsFile, json);

            //CosmosConsole.WriteLine(consoleSender, "Saved settings to local file.");
        }

        internal static void LoadSettings()
        {
            string settingsFile = PathDefinitions.CosmosAppData + @"Settings.json";

            if (File.Exists(settingsFile))
            {
                string json = File.ReadAllText(settingsFile);
                var settings = JsonConvert.DeserializeObject<dynamic>(json);

                try
                {
                    _backgroundMode = settings.BackgroundMode;
                    _language = settings.Language;
                    _devMenuEnabled = settings.DevMenuEnabled;
                    _enableLogging = settings.EnableLogging;
                    _detailedLogging = settings.DetailedLogging;
                }
                catch
                {

                }

                CosmosConsole.WriteLine(LanguageHandler.Get("Logs.SettingsLoaded"));
            }
        }
    }
}
