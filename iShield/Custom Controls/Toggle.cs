using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace iShield
{
    /* Based on: https://github.com/TacticDevGit/Control-Template-Toggle-Switch-WPF
     * YT tutorial: https://www.youtube.com/watch?v=fG620XzIYD4
     * The styles related to this control are written in /Themes/Generic.xaml
     */
    public class Toggle : ToggleButton
    {

        static Toggle()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Toggle), new FrameworkPropertyMetadata(typeof(Toggle)));
        }
    }
}
