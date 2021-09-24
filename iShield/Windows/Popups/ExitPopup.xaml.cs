using iShield.Utilities;
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
using System.Windows.Shapes;

namespace iShield.Windows.Popups
{
    /// <summary>
    /// Interaction logic for ExitPopup.xaml
    /// </summary>
    public partial class ExitPopup : Window
    {
        public static bool m_exit = false;

        public ExitPopup()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DwmDropShadow.DropShadowToWindow(this);
        }

        private void CloseDialog(object sender, RoutedEventArgs e)
        {
            m_exit = (((Button)sender) == btnYes);
            this.Close();
        }
    }
}
