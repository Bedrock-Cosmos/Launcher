using System;
using System.IO;
using Newtonsoft.Json;

namespace BedrockCosmos.App
{
    internal class SettingsManager
    {
        private static bool _proxyStarted = false;
        private static bool _backgroundMode = false;
        private static int _devMenuClicks = 0;
        private static bool _devMenuEnabled = false;
        private static bool _enableLogging = false;
        private static string consoleSender = "App";

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
                    CosmosConsole.WriteLine(consoleSender, "Developer mode enabled.");

                _devMenuEnabled = true;
                return true;
            }
        }

        internal static void DisableDevMenu()
        {
            _devMenuClicks = 0;
            _devMenuEnabled = false;
            CosmosConsole.WriteLine(consoleSender, "Developer mode disabled.");
        }

        internal static void SaveSettings()
        {
            string settingsFile = AppDomain.CurrentDomain.BaseDirectory + @"settings.json";
            var savedSettings = new
            {
                BackgroundMode = _backgroundMode,
                DevMenuEnabled = _devMenuEnabled,
                EnableLogging = _enableLogging
            };

            string json = JsonConvert.SerializeObject(savedSettings, Formatting.Indented);
            File.WriteAllText(settingsFile, json);

            //CosmosConsole.WriteLine(consoleSender, "Saved settings to local file.");
        }

        // Method to load settings from a JSON file and apply them
        internal static void LoadSettings()
        {
            string settingsFile = AppDomain.CurrentDomain.BaseDirectory + @"settings.json";

            if (File.Exists(settingsFile))
            {
                string json = File.ReadAllText(settingsFile);
                var settings = JsonConvert.DeserializeObject<dynamic>(json);

                _backgroundMode = settings.BackgroundMode;
                _devMenuEnabled = settings.DevMenuEnabled;
                _enableLogging = settings.EnableLogging;

                CosmosConsole.WriteLine(consoleSender, "Settings loaded from local file.");
            }
        }
    }
}
