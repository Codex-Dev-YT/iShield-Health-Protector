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

namespace iShield.Windows
{
    /// <summary>
    /// Interaction logic for HydrationPopup.xaml
    /// </summary>
    public partial class HydrationPopup : Window
    {
        public HydrationPopup()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DwmDropShadow.DropShadowToWindow(this);
        }
    }
}
