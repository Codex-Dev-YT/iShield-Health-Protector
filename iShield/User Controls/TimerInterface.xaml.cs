using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace iShield
{
    public partial class TimerInterface : UserControl
    {
        // Events ==============================================
        public event EventHandler ActivationChanged;
        public event EventHandler TimeChanged;

        // Constants ===========================================
        const int MAX_SECONDS = 59;
        const int MAX_MINUTES = 99;

        // Private Members =====================================
        private int m_seconds = 5;
        private int m_minutes = 0;

        private string Seconds
        {
            get { 
                return ((m_seconds < 10 ? "0" : "") + m_seconds.ToString()); 
            }
        }

        private string Minutes
        {
            get {
                return ((m_minutes < 10 ? "0" : "") + m_minutes.ToString());
            }
        }

        // Constructor =========================================

        public TimerInterface()
        {
            InitializeComponent();

            sldTime.Minimum = 1;
            sldTime.Maximum = MAX_MINUTES * 60 + MAX_SECONDS;
            sldTime.Value = m_seconds = 5;

            txtSeconds.Text = Seconds;
            txtMinutes.Text = Minutes;
        }

        // Public Methods ======================================

        public int GetSeconds() => (int)sldTime.Value;

        public void SetTime(int seconds)
        {
            if (seconds < 1) seconds = 1;

            if (seconds > (MAX_MINUTES * 60 + MAX_SECONDS))
                seconds = MAX_MINUTES * 60 + MAX_SECONDS;

            sldTime.Value = seconds;
        }

        // Properties ==========================================

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(TimerInterface), new PropertyMetadata("Reminder Name"));


        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(TimerInterface), new PropertyMetadata(false));


        // Other Methods =======================================

        private void ReminderActivationChanged(object sender, RoutedEventArgs e)
        {
            // Convert bool? to bool:
            bool check = ActivationToggle.IsChecked ?? false;
            pnlControls.IsEnabled = check;

            // Fire "ActivationChanged" event:
            if (ActivationChanged != null)
                ActivationChanged(this, EventArgs.Empty);
        }

        private void sldTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsInitialized) return;

            m_minutes = (int)(sldTime.Value / 60);
            m_seconds = (int)(sldTime.Value % 60);

            txtSeconds.Text = Seconds;
            txtMinutes.Text = Minutes;

            // No need to fire the event if the control was deactivated
            if (!IsActive) return;

            // Fire the "TimeChanged" event:
            if (TimeChanged != null)
                TimeChanged(this, EventArgs.Empty);
        }
    }
}
