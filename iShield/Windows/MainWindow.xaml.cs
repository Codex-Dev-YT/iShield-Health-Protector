using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using iShield.Classes;
using iShield.Utilities;

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

        DispatcherTimer colorFilterTimer = new DispatcherTimer();
        DispatcherTimer blinkTimer = new DispatcherTimer();
        bool isBlinking = false;

        public MainWindow()
        {
            InitializeComponent();
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
            bool firstRun = true;
            if (firstRun) btnPresentation.IsChecked = true;
            else btnSettings.IsChecked = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterPages();
            finishedInitialization = true;
            LoadAppropriateStartPage();
            DwmDropShadow.DropShadowToWindow(this);
            
            ScreenManager.Initialize();

            ScreenManager.AddGamaRampProfile("Main", (int)sldBrightness.Value, (int)sldTemperature.Value, false);
            ScreenManager.AddGamaRampProfile("Blink", (int)sldBrightness_blink.Value, (int)sldTemperature_blink.Value, false);

            colorFilterTimer.Tick += colorFilterTimer_Tick;
            colorFilterTimer.Interval = TimeSpan.FromMilliseconds(50);
            colorFilterTimer.Start();

            blinkTimer.Tick += blinkTimer_Tick;
            blinkTimer.Interval = TimeSpan.FromMilliseconds(2000);
            blinkTimer.Start();

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
            this.Close();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e) 
            => this.WindowState = WindowState.Minimized;

        private void ColorFilterSliders_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!finishedInitialization) return;
            
            ScreenManager.AddGamaRampProfile("Main", (int)(sldBrightness.Value), (int)(sldTemperature.Value), false);
            ScreenManager.SelectGamaRampProfile("Main");
            //ScreenManager.ApplyGamaRamp();
        }
         
        private void BlinkSliders_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!finishedInitialization) return;

            ScreenManager.AddGamaRampProfile("Blink", (int)(sldBrightness_blink.Value), (int)(sldTemperature_blink.Value), false);
        }

        private void imgIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isEnabled)
            {
                colorFilterTimer.Stop();
                blinkTimer.Stop();
                ScreenManager.RestoreDefaultGamaRampProfile();
                ScreenManager.ApplyGamaRamp();
            } else {
                colorFilterTimer.Start();
                blinkTimer.Start();
                ScreenManager.SelectGamaRampProfile("Main");
                ScreenManager.ApplyGamaRamp();
            }

            isEnabled = !isEnabled;
        }

        private void colorFilterTimer_Tick(object sender, EventArgs e)
        {
            if (!isEnabled) return;

            if (isBlinking)
            {
                ScreenManager.SelectGamaRampProfile("Main");
                isBlinking = false;
            }

            ScreenManager.ApplyGamaRamp();
        }
        private void blinkTimer_Tick(object sender, EventArgs e)
        {
            if (!isEnabled) return;
            
            colorFilterTimer.Stop();

            ScreenManager.SelectGamaRampProfile("Blink");
            ScreenManager.ApplyGamaRamp();
            isBlinking = true;

            colorFilterTimer.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ScreenManager.RestoreDefaultGamaRampProfile();
            ScreenManager.ApplyGamaRamp();
            ScreenManager.Finalize();
        }
    }
}
