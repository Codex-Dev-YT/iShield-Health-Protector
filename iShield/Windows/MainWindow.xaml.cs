using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Forms = System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using iShield.Classes;
using iShield.Properties;
using iShield.Utilities;
using iShield.Windows;
using Microsoft.Win32;
using System.IO;
using iShield.Windows.Popups;

namespace iShield
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<RadioButton, StackPanel> contentPages;
        RadioButton currentPage = null;
        bool finishedInitialization = false;

        bool isEnabled = true;
        bool wasFirstRun = false;
        bool pagesLoaded = false;

        DispatcherTimer colorFilterTimer = new DispatcherTimer();
        DispatcherTimer eyeRestTimer = new DispatcherTimer();
        DispatcherTimer breakTimeTimer = new DispatcherTimer();
        DispatcherTimer hydrationTimer = new DispatcherTimer();
        DispatcherTimer blinkTimer = new DispatcherTimer();

        Window eyeRestPopup = new EyeRestPopup();
        Window breakTimePopup = new BreakTimePopup();
        Window hydrationPopup = new HydrationPopup();

        bool isBlinking = false;

        int originalScreenBrightness = 0;

        Forms.NotifyIcon notifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/../Assets/Icons/iShield Icon.ico")).Stream;
            notifyIcon = new Forms.NotifyIcon();
            notifyIcon.Icon = new System.Drawing.Icon(iconStream);
            notifyIcon.Text = "iShield Health Protector";
            notifyIcon.ContextMenuStrip = new Forms.ContextMenuStrip();
            
            notifyIcon.ContextMenuStrip.Items.Add("Hide iShield Health Protector", null, (object sender, EventArgs e) =>
            {
                if (!finishedInitialization) return;

                Forms.ToolStripItem item = (Forms.ToolStripItem)sender;

                if (this.Visibility == Visibility.Visible)
                {
                    this.Hide();
                    item.Text = "Show";
                } else {
                    this.Show();
                    item.Text = "Hide";
                }

                item.Text += " iShield Health Protector";
            });

            notifyIcon.ContextMenuStrip.Items.Add("Disable Protection", null, (object sender, EventArgs e) => {
                if (!finishedInitialization) return;
                imgIcon_MouseLeftButtonDown(null, null);
            });

            notifyIcon.ContextMenuStrip.Items.Add(new Forms.ToolStripSeparator());
            notifyIcon.ContextMenuStrip.Items.Add("Exit", null, (object sender, EventArgs e) =>
            {
                if (!finishedInitialization) return;
                this.Close();
            });

            notifyIcon.Visible = true;
        }

        private void RegisterPages()
        {
            contentPages = new Dictionary<RadioButton, StackPanel>
            {
                {btnSettings,       pnlSettings },
                {btnReminders,      pnlReminders },
                {btnPresentation,   pnlPresentation},
                {btnAbout,          pnlAbout }
            };
        }

        private void Titlebar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void ChangePage(object sender, RoutedEventArgs e)
        {
            if (!finishedInitialization) return;

            if (wasFirstRun && pagesLoaded)
            {
                isEnabled = true;
                wasFirstRun = false;
            }

            if (currentPage != null)
                contentPages[currentPage].Visibility = Visibility.Hidden;

            if (currentPage == btnPresentation)
                Presentation.Reset();

            currentPage = (RadioButton)sender;
            contentPages[currentPage].Visibility = Visibility.Visible;
        }

        // This method shows the presentation page on first run and the settings page otherwise.
        private void LoadAppropriateStartPage()
        {
            if (Settings.Default.Config.firstRun) 
                btnPresentation.IsChecked = true;
            else 
                btnSettings.IsChecked = true;

            Settings.Default.Config.firstRun = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterPages();
            ApplySettings();
            SetupPopups();

            // If this is the first run, then the app must not be enabled until the user 
            // finishes reading the presetation:
            if (Settings.Default.Config.firstRun)
            {
                isEnabled = false;
                wasFirstRun = true;
            }

            txtVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            finishedInitialization = true;
            LoadAppropriateStartPage();
            pagesLoaded = true;
            DwmDropShadow.DropShadowToWindow(this);
            
            ScreenManager.Initialize();

            ScreenManager.AddGamaRampProfile("Main", (int)sldBrightness.Value, (int)sldTemperature.Value, (bool)chkInvertion.IsChecked);
            ScreenManager.AddGamaRampProfile("Blink", (int)sldBrightness_blink.Value, (int)sldTemperature_blink.Value, (bool)chkInvertion.IsChecked);

            colorFilterTimer.Tick += colorFilterTimer_Tick;
            colorFilterTimer.Interval = TimeSpan.FromMilliseconds(30);
            colorFilterTimer.Start();

            eyeRestTimer.Tick += eyeRestTimer_Tick;
            eyeRestTimer.Interval = TimeSpan.FromSeconds(Settings.Default.Config.eye_rest_timer);
            eyeRestTimer.Start();

            breakTimeTimer.Tick += breakTimeTimer_Tick;
            breakTimeTimer.Interval = TimeSpan.FromSeconds(Settings.Default.Config.break_time_timer);
            breakTimeTimer.Start();

            hydrationTimer.Tick += hydrationTimer_Tick;
            hydrationTimer.Interval = TimeSpan.FromSeconds(Settings.Default.Config.hydration_timer);
            hydrationTimer.Start();

            blinkTimer.Tick += blinkTimer_Tick;
            blinkTimer.Interval = TimeSpan.FromSeconds(Settings.Default.Config.blink_timer);
            blinkTimer.Start();
        }

        private void SetupPopups()
        {
            // Instead of actually closing a popup window (this will prevent using them again),
            // I just hide them and enable their corresponding timer:

            eyeRestPopup.Closing += (object sender, System.ComponentModel.CancelEventArgs e) =>
            {
                eyeRestTimer.IsEnabled = true;
                e.Cancel = true;
                eyeRestPopup.Visibility = Visibility.Hidden;
            };

            breakTimePopup.Closing += (object sender, System.ComponentModel.CancelEventArgs e) =>
            {
                breakTimeTimer.IsEnabled = true;
                e.Cancel = true;
                breakTimePopup.Visibility = Visibility.Hidden;
            };

            hydrationPopup.Closing += (object sender, System.ComponentModel.CancelEventArgs e) => 
            {
                hydrationTimer.IsEnabled = true;
                e.Cancel = true;
                hydrationPopup.Visibility = Visibility.Hidden;
            };
        }

        private void ContentSlider_FinishedSliding(object sender, EventArgs e)
        {
            btnSettings.IsChecked = true;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            notifyIcon.ContextMenuStrip.Items[0].Text = "Show iShield Health Protector";
            notifyIcon.ShowBalloonTip(3000, "iShield Health Protector", "iShield Health Protector was minimized to system try.", Forms.ToolTipIcon.Info);
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
            => this.WindowState = WindowState.Minimized;

        private void ColorFilterSliders_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!finishedInitialization) return;

            Settings.Default.Config.brightness = (int)sldBrightness.Value;
            Settings.Default.Config.temperature = (int)sldTemperature.Value;

            ScreenManager.AddGamaRampProfile("Main", (int)(sldBrightness.Value), (int)(sldTemperature.Value), (bool)chkInvertion.IsChecked);
        }
         
        private void BlinkSliders_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!finishedInitialization) return;

            Settings.Default.Config.brightness_blink = (int)sldBrightness_blink.Value;
            Settings.Default.Config.temperature_blink = (int)sldTemperature_blink.Value;

            ScreenManager.AddGamaRampProfile("Blink", (int)(sldBrightness_blink.Value), (int)(sldTemperature_blink.Value), (bool)chkInvertion.IsChecked);
        }

        private void imgIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ResetTimers();

            isEnabled = !isEnabled;

            if (isEnabled)
                ScreenManager.SelectGamaRampProfile("Main");
             else 
                ScreenManager.RestoreDefaultGamaRampProfile();

            ScreenManager.ApplyGamaRamp();

            if (isBlinking) isBlinking = false;

            if (Settings.Default.Config.alwaysKeepMaxBrightness)
                WindowsSettingsBrightnessController.Set(isEnabled ? 100 : originalScreenBrightness);

            this.eyeRestPopup.Close();
            this.breakTimePopup.Close();
            this.hydrationPopup.Close();

            notifyIcon.ContextMenuStrip.Items[1].Text = (isEnabled ? "Disable" : "Enable") + " Protection";
        }

        private void ResetTimers()
        {
            eyeRestTimer.IsEnabled = breakTimeTimer.IsEnabled = hydrationTimer.IsEnabled = blinkTimer.IsEnabled = false;
            eyeRestTimer.IsEnabled = breakTimeTimer.IsEnabled = hydrationTimer.IsEnabled = blinkTimer.IsEnabled = true;
        }

        private void colorFilterTimer_Tick(object sender, EventArgs e)
        {
            if (!finishedInitialization || !isEnabled) return;

            ScreenManager.SelectGamaRampProfile("Main");
            ScreenManager.ApplyGamaRamp();

            if (isBlinking)
                isBlinking = false;
        }
        
        private void blinkTimer_Tick(object sender, EventArgs e)
        {
            if (!finishedInitialization || !isEnabled || !Blink_Timer.IsActive) return;

            // I reset the colorFilterTimer timer here to guarantee that the blink 
            // time is the same every time (which is equal to the colorFilterTimer's
            // Interval)
            colorFilterTimer.IsEnabled = false;

            ScreenManager.SelectGamaRampProfile("Blink");
            ScreenManager.ApplyGamaRamp();
            isBlinking = true;

            colorFilterTimer.IsEnabled = true;
        }

        private void eyeRestTimer_Tick(object sender, EventArgs e)
        {
            if (!finishedInitialization || !isEnabled || !Eye_Rest_Timer.IsActive) return;
            
            eyeRestPopup.Show();
            eyeRestTimer.IsEnabled = false;
        }

        private void breakTimeTimer_Tick(object sender, EventArgs e)
        {
            if (!finishedInitialization || !isEnabled || !Break_Time.IsActive) return;
            
            breakTimePopup.Show();
            breakTimeTimer.IsEnabled = false;
        }

        private void hydrationTimer_Tick(object sender, EventArgs e)
        {
            if (!finishedInitialization || !isEnabled || !Hydration_Timer.IsActive) return;
            
            hydrationPopup.Show();
            hydrationTimer.IsEnabled = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Disable all try icon context menu items so that the user doesn't mess with them while 
            // the exit dialog is shown. Otherwise, some conflicts may occur, resulting in crashes.
            foreach (Forms.ToolStripItem item in notifyIcon.ContextMenuStrip.Items)
                item.Enabled = false;

            new ExitPopup().ShowDialog();

            if (!ExitPopup.m_exit)
            {
                // Set finishedInitialization to false so that the menu items' events don't fire while 
                // re-enabling them:
                finishedInitialization = false;

                // Re-enable the context menu items since the user chose not to exit the program.
                foreach (Forms.ToolStripItem item in notifyIcon.ContextMenuStrip.Items)
                    item.Enabled = true;

                finishedInitialization = true;

                e.Cancel = true;
                return;
            }

            // This will prevent the timers from working at all, this way, they will not 
            // change the gamma ramp of the screen again after reseting it:
            finishedInitialization = false;

            notifyIcon.Dispose();

            ScreenManager.RestoreDefaultGamaRampProfile();
            ScreenManager.ApplyGamaRamp();
            ScreenManager.Finalize();

            if (Settings.Default.Config.alwaysKeepMaxBrightness)
            {
                WindowsSettingsBrightnessController.Set(originalScreenBrightness);
            }

            // This will close all the windows/popups and make sure that the application closes:
            System.Windows.Application.Current.Shutdown();
        }

        private void ApplySettings()
        {
            var settings = Settings.Default.Config;
            
            // Sliders:
            sldBrightness.Value = settings.brightness;
            sldTemperature.Value = settings.temperature;
            sldBrightness_blink.Value = settings.brightness_blink;
            sldTemperature_blink.Value = settings.temperature_blink;
            // Toggles:
            chkInvertion.IsChecked = settings.invertion;
            chkMaxBrightness.IsChecked = settings.alwaysKeepMaxBrightness;
            chkStartup.IsChecked = settings.runOnStartup;
            // Reminers:
            Eye_Rest_Timer.SetTime(settings.eye_rest_timer);
            Eye_Rest_Timer.IsActive = settings.eye_rest_timer_enabled;
            Break_Time.SetTime(settings.break_time_timer);
            Break_Time.IsActive = settings.break_time_timer_enabled;
            Hydration_Timer.SetTime(settings.hydration_timer);
            Hydration_Timer.IsActive = settings.hydration_timer_enabled;
            Blink_Timer.SetTime(settings.blink_timer);
            Blink_Timer.IsActive = settings.blink_timer_enabled;

            if (settings.alwaysKeepMaxBrightness)
            {
                originalScreenBrightness = WindowsSettingsBrightnessController.Get();
                WindowsSettingsBrightnessController.Set(100);
            }

            if (settings.firstRun)
                SetStartup((bool)chkStartup.IsChecked);

        }

        private void InvertionChanged(object sender, RoutedEventArgs e)
        {
            Settings.Default.Config.invertion = (bool)chkInvertion.IsChecked;
            ColorFilterSliders_ValueChanged(null, null);
            BlinkSliders_ValueChanged(null, null);
        }

        private void Blink_Timer_ActivationChanged(object sender, EventArgs e)
        {
            Settings.Default.Config.blink_timer_enabled = Blink_Timer.IsActive;

            // Reset the timer
            blinkTimer.IsEnabled = false;
            blinkTimer.IsEnabled = true;
        }

        private void Blink_Timer_TimeChanged(object sender, EventArgs e)
        {
            var time = Blink_Timer.GetSeconds();
            Settings.Default.Config.blink_timer = time;
            blinkTimer.IsEnabled = false;
            blinkTimer.Interval = TimeSpan.FromSeconds(time);
            blinkTimer.IsEnabled = true;
        }

        private void Hydration_Timer_ActivationChanged(object sender, EventArgs e)
        {
            Settings.Default.Config.hydration_timer_enabled = Hydration_Timer.IsActive;

            // Reset the timer:

            if (!hydrationTimer.IsEnabled) return;

            hydrationTimer.IsEnabled = false;
            hydrationTimer.IsEnabled = true;
        }

        private void Hydration_Timer_TimeChanged(object sender, EventArgs e)
        {
            var time = Hydration_Timer.GetSeconds();
            Settings.Default.Config.hydration_timer = time;
            var temp = hydrationTimer.IsEnabled;
            hydrationTimer.IsEnabled = false;
            hydrationTimer.Interval = TimeSpan.FromSeconds(time);
            hydrationTimer.IsEnabled = temp;
        }

        private void Break_Time_ActivationChanged(object sender, EventArgs e)
        {
            Settings.Default.Config.break_time_timer_enabled = Break_Time.IsActive;

            // Reset the timer:

            if (!breakTimeTimer.IsEnabled) return;

            breakTimeTimer.IsEnabled = false;
            breakTimeTimer.IsEnabled = true;
        }

        private void Break_Time_TimeChanged(object sender, EventArgs e)
        {
            var time = Break_Time.GetSeconds();
            Settings.Default.Config.break_time_timer = time;
            var temp = breakTimeTimer.IsEnabled;
            breakTimeTimer.IsEnabled = false;
            breakTimeTimer.Interval = TimeSpan.FromSeconds(time);
            breakTimeTimer.IsEnabled = temp;
        }

        private void Eye_Rest_Timer_ActivationChanged(object sender, EventArgs e)
        {
            Settings.Default.Config.eye_rest_timer_enabled = Eye_Rest_Timer.IsActive;

            // Reset the timer:

            if (!eyeRestTimer.IsEnabled) return;

            eyeRestTimer.IsEnabled = false;
            eyeRestTimer.IsEnabled = true;
        }

        private void Eye_Rest_Timer_TimeChanged(object sender, EventArgs e)
        {
            var time = Eye_Rest_Timer.GetSeconds();
            Settings.Default.Config.eye_rest_timer = time;
            var temp = eyeRestTimer.IsEnabled;
            eyeRestTimer.IsEnabled = false;
            eyeRestTimer.Interval = TimeSpan.FromSeconds(time);
            eyeRestTimer.IsEnabled = temp;
        }

        private void RestoreDefaultSettings(object sender, RoutedEventArgs e)
        {
            Settings.Default.Config = new iShieldConfig();
            ApplySettings();
        }

        private void chkStartup_CheckChanged(object sender, RoutedEventArgs e)
        {
            bool startup = (bool)chkStartup.IsChecked;
            Settings.Default.Config.runOnStartup = startup;
            SetStartup(startup);
        }

        private void SetStartup(bool startup)
        {
            const string AppName = "iShield";

            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (startup)
                rk.SetValue(AppName, System.Reflection.Assembly.GetExecutingAssembly().Location);
            else
                rk.DeleteValue(AppName, false);

        }

        private void chkMaxBrightness_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (!finishedInitialization) return;

            bool check = (bool)chkMaxBrightness.IsChecked;

            Settings.Default.Config.alwaysKeepMaxBrightness = check;

            if (check) {
                originalScreenBrightness = WindowsSettingsBrightnessController.Get();
                WindowsSettingsBrightnessController.Set(100);
            } else {
                WindowsSettingsBrightnessController.Set(originalScreenBrightness);
            }
        }
    }
}
