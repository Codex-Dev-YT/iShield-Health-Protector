using System;
using System.Collections.Generic;
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
        }

        private void ContentSlider_FinishedSliding(object sender, EventArgs e)
        {
            btnSettings.IsChecked = true;
        }
    }
}
