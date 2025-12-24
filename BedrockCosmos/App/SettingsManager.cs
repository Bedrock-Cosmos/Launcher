using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BedrockCosmos.App
{
    public class SettingsManager
    {
        public int devMenuClicks = 0;
        public bool devMenuEnabled = false;

        public bool DevMenuCheck()
        {
            if (devMenuClicks < 7)
            { 
                devMenuClicks++;
                return false;
            }
            else
            {
                devMenuEnabled = true;
                return true;
            }
        }
    }
}
