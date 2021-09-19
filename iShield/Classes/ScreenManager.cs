using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace iShield.Classes
{
    public static class ScreenManager
    {
		private static List<IntPtr> monitors = new List<IntPtr>();
        private static Dictionary<string, GammaRampProfileSettings> gammaRampProfiles 
            = new Dictionary<string, GammaRampProfileSettings>();

        private static GammaRampProfile [] ramps;
        private static string currentProfile = "";
        private static uint index = 0;
        private static bool initialized = false;

        private const int DEFAULT_BRIGHTNESS = 128;
        private const int DEFAULT_TEMPERATURE = 6500;
        private const bool DEFAULT_INVERTION = false;

        public unsafe static void Initialize()
		{
			LoadDevices();
            gammaRampProfiles.Add("Default", new GammaRampProfileSettings()
            {
                brightness = DEFAULT_BRIGHTNESS,
                temperature = DEFAULT_TEMPERATURE,
                invertion = DEFAULT_INVERTION
            });

            currentProfile = "Default";
            initialized = true;
            GenerateRamps();
        }

        /// <summary>
        /// Registers a new GammaRampProfileSettings in the gammaRampProfiles list if the id does not exist.
        /// Otherwise, it updates the current one with that id.
        /// </summary>
        public static void AddGamaRampProfile(string id, int brightness, int temperature, bool invertion = false)
        {
            if (!initialized) return;

            // Cannot update the default profile
            if (id == "Default") return;

            GammaRampProfileSettings settings = new GammaRampProfileSettings()
            {
                brightness = brightness,
                temperature = temperature,
                invertion = invertion
            };

            if (gammaRampProfiles.ContainsKey(id))
                gammaRampProfiles[id] = settings;
            else
                gammaRampProfiles.Add(id, settings);

            // If the current active profile was updated, then generate new ramps based on the new settings:
            if (id == currentProfile)
            {
                GenerateRamps();
                // No need to apply the new ramp to the screen since the app does that using a timer.
            }
        }

        public static void DeleteGamaRampProfile(string id)
        {
            if (!initialized) return;

            // Cannot delete the default profile
            if (id == "Default") return;

            gammaRampProfiles.Remove(id);

            // If the current active profile was deleted, then change back to the default profile:
            if (id == currentProfile)
            {
                currentProfile = "Default";
                GenerateRamps();
                // No need to apply the new ramp to the screen since the app does that using a timer.
            }
        }

        public static void SelectGamaRampProfile(string id)
        {
            if (!initialized) return;

            if (id == currentProfile || !gammaRampProfiles.ContainsKey(id)) return;
            
            currentProfile = id;
            GenerateRamps();
            // No need to apply the new ramp to the screen since the app does that using a timer.
        }

        public static void RestoreDefaultGamaRampProfile()
        {
            if (!initialized) return;

            currentProfile = "Default";
            GenerateRamps();
            // No need to apply the new ramp to the screen since the app does that using a timer.
        }

        private static void GenerateRamps()
        {
            if (!initialized) return;

            if (!gammaRampProfiles.ContainsKey(currentProfile)) return;

            GammaRampProfileSettings settings = gammaRampProfiles[currentProfile];

            // Some drivers don't apply the gamma ramp sent to them when it's identical to the 
            // one currently applied. This causes the screen filter to flicker on and off. so 
            // to solve this, I create 2 filters instead of 1 and alternate between them, one 
            // of them is exactly as specified from the settings (brightness, temperature, 
            // invertion), the other is slightly different. that difference is not noticeable 
            // by naked eyes but it is distinguishable by the drivers, thus, they will be forced 
            // to apply the filter.
            GammaRampProfile ramp1 = new GammaRampProfile(settings.brightness, settings.temperature, settings.invertion, false);
            GammaRampProfile ramp2 = new GammaRampProfile(settings.brightness, settings.temperature, settings.invertion, true);

            ramps = new GammaRampProfile[2] {
                ramp1, ramp2
            };

            index = 0;
        }

        public static void ApplyGamaRamp()
        {
            if (!initialized) return;

            foreach (var monitor in monitors)
                Win32Methods.SetDeviceGammaRamp(monitor, ref ramps[index % 2]);

            ++index;
        }

        public static void Finalize()
        {
            if (!initialized) return;
            RestoreDefaultGamaRampProfile();
            ApplyGamaRamp();
			UnloadDevices();
		}

        // <summary>
        /// A gama ramp profile is just a higher-level representation of the byte array 
        /// required by the native methods of windows in which the ramp's data is stored.
        /// This type is set up so that its internal (unmanaged) representation is 
        /// equivalent to the one required by the system.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct GammaRampProfile
        {
            [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public ushort[] Red { get; set; }

            [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public ushort[] Green { get; set; }

            [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public ushort[] Blue { get; set; }

            public GammaRampProfile(int brightness, int colorTemperature, bool invertion = false, bool applyVariation = false)
            {
                this.Red = new ushort[256];
                this.Green = new ushort[256];
                this.Blue = new ushort[256];

                int[] array = temperature2RGB(colorTemperature);

                double red = (double)array[0] / 255.0;
                double green = (double)array[1] / 255.0;
                double blue = (double)array[2] / 255.0;

                for (int i = 0; i < 256; i++)
                {
                    double value = (double)(i * (brightness + 128));
                    if (invertion)
                    {
                        value = 65535.0 - (double)(i * (brightness + 128));

                        if (value < 0.0) value = 0.0;
                    }

                    if (value > 65535.0) value = 65535.0;

                    this.Red[i] = (ushort)(value * red);
                    this.Green[i] = (ushort)(value * green);
                    this.Blue[i] = (ushort)(value * blue);
                }

                if (applyVariation)
                {
                    this.Red[255] += (ushort)(this.Red[255] > 0 ? -1 : 1);
                    this.Green[255] += (ushort)(this.Green[255] > 0 ? -1 : 1);
                    this.Blue[255] += (ushort)(this.Blue[255] > 0 ? -1 : 1);
                }
            }

            // Converts a temperature value (in Kelvin) into an RGB format.
            // Works for temperatures from 1000K up to 40000K
            // Credit: https://tannerhelland.com/2012/09/18/convert-temperature-rgb-algorithm-code.html
            private static int[] temperature2RGB(int temperature)
            {
                temperature = temperature / 100;
                int red, green, blue;

                // Calculate Red:

                if (temperature <= 66)
                    red = 255;
                else
                {
                    red = temperature - 60;
                    red = (int)(329.698727446 * Math.Pow((double)red, -0.1332047592));
                    if (red < 0) red = 0;
                    if (red > 255) red = 255;
                }

                // Calculate green:

                if (temperature <= 66)
                {
                    green = temperature;
                    green = (int)(99.4708025861 * Math.Log((double)green) - 161.1195681661);
                    if (green < 0) green = 0;
                    if (green > 255) green = 255;
                }
                else
                {
                    green = temperature - 60;
                    green = (int)(288.1221695283 * Math.Pow((double)green, -0.0755148492));
                    if (green < 0) green = 0;
                    if (green > 255) green = 255;
                }

                // Calculate blue:

                if (temperature >= 66)
                    blue = 255;
                else
                {
                    if (temperature <= 19)
                    {
                        blue = 0;
                    }
                    else
                    {
                        blue = temperature - 10;
                        blue = (int)(138.5177312231 * Math.Log((double)blue) - 305.0447927307);
                        if (blue < 0) blue = 0;
                        if (blue > 255) blue = 255;
                    }
                }

                // Fixing some edge cases:
                
                if (green >= 254) green = 255;
                if (blue >= 250) blue = 255;

                int[] rgb = new int[] { red, green, blue };
                return rgb;
            }

        }

        private struct GammaRampProfileSettings
        {
            public int brightness, temperature;
            public bool invertion;
        };

        private static unsafe void LoadDevices()
        {
            foreach (var screen in Screen.AllScreens)
            {
                IntPtr context = Win32Methods.CreateDC(screen.DeviceName, null, null, IntPtr.Zero);
                monitors.Add(context);
            }
        }

        private static unsafe void UnloadDevices()
        {
            foreach (var monitor in monitors)
            {
                Win32Methods.DeleteDC(monitor);
            }
        }

	}
}
