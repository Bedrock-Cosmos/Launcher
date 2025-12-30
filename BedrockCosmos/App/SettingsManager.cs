using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BedrockCosmos.App
{
    internal class SettingsManager
    {
        private static int _devMenuClicks = 0;
        private static bool _devMenuEnabled = false;
        private static bool _enableLogging = false;
        private static string consoleSender = "App";

        internal static int DevMenuClicks
        {
            get { return _devMenuClicks; }
            set { _devMenuClicks = value; }
        }

        internal static bool DevMenuEnabled
        {
            get { return _devMenuEnabled; }
            set { _devMenuEnabled = value; }
        }

        internal static bool EnableLogging
        {
            get { return _enableLogging; }
            set { _enableLogging = value; }
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
    }
}
