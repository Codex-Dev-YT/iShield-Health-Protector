using iShield.Classes;
using iShield.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace iShield
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // The first time the application runs, the settings will be null, 
        // so I made sure they are created:
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (Settings.Default.Config == null)
            {
                Settings.Default.Config = new iShieldConfig();
            }
        }

        // Save the settings when the user exits the app:
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Settings.Default.Save();
        }
    }
}
