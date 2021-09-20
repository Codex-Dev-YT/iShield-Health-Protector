using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iShield.Classes
{
    [Serializable]
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class iShieldConfig
    {
        public int brightness;
        public int temperature;
        public int brightness_blink;
        public int temperature_blink;

        public bool invertion;
        public bool alwaysKeepMaxBrightness;
        public bool runOnStartup;

        public bool eye_rest_timer_enabled;
        public bool break_time_timer_enabled;
        public bool hydration_timer_enabled;
        public bool blink_timer_enabled;
        
        // All in seconds:
        public int eye_rest_timer;
        public int break_time_timer;
        public int hydration_timer;
        public int blink_timer;

        // This indicates wether this is the first time the application runs or not:
        public bool firstRun;

        public iShieldConfig()
        {
            brightness = 100;
            temperature = 6500;
            brightness_blink = 100;
            temperature_blink = 6500;

            invertion = false;
            alwaysKeepMaxBrightness = true;
            runOnStartup = true;

            eye_rest_timer_enabled = true;
            break_time_timer_enabled = true;
            hydration_timer_enabled = false;
            blink_timer_enabled = false;

            eye_rest_timer = 1200;
            break_time_timer = 2400;
            hydration_timer = 3600;
            blink_timer = 3;

            firstRun = true;
        }
    }
}
