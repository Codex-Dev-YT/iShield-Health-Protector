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
    /// <summary>
    /// Interaction logic for TimerInterface.xaml
    /// </summary>
    public partial class TimerInterface : UserControl
    {
        // Events ==============================================
        public event EventHandler ActivationChanged;
        public event EventHandler TimeChanged;

        // Constants ===========================================
        const int MIN_SECONDS = 1;
        const int MIN_MINUTES = 0;
        const int MAX_SECONDS = 59;
        const int MAX_MINUTES = 99;

        // Private Members =====================================
        private int m_seconds = 1;
        private int m_minutes = 0;

        private string Seconds
        {
            get { 
                return ((m_seconds < 10 ? "0" : "") + m_seconds.ToString()); 
            }
            set
            {
                int s = 0;
                int.TryParse(value, out s);

                if (s == m_seconds) return;

                if (s < MIN_SECONDS) s = MIN_SECONDS;
                if (s > MAX_SECONDS) s = MAX_SECONDS;

                m_seconds = s;

                // Fire the "TimeChanged" event:
                if (TimeChanged != null)
                    TimeChanged(this, EventArgs.Empty);
            }
        }

        private string Minutes
        {
            get
            {
                return ((m_minutes < 10 ? "0" : "") + m_minutes.ToString());
            }
            set
            {
                int m = 0;
                int.TryParse(value, out m);

                if (m == m_minutes) return;

                if (m < MIN_MINUTES) m = MIN_MINUTES;
                if (m > MAX_MINUTES) m = MAX_MINUTES;

                m_minutes = m;

                // Fire the "TimeChanged" event:
                if (TimeChanged != null)
                    TimeChanged(this, EventArgs.Empty);
            }
        }

        // Constructor =========================================

        public TimerInterface()
        {
            Trace.Assert(MIN_SECONDS < MAX_SECONDS);
            Trace.Assert(MIN_MINUTES < MAX_MINUTES);
            InitializeComponent();

            txtSeconds.Text = Seconds;
            txtMinutes.Text = Minutes;
        }

        // Public Methods ======================================

        // Convert time from minute-second format to number of seconds and return it:
        public int GetSeconds()
        {
            return m_minutes * 60 + m_seconds;
        }

        // Convert number of seconds back to minute-second format, and update the display:
        public void SetTime(int seconds)
        {
            m_minutes = (int)(seconds / 60);
            m_seconds = seconds % 60;
            txt_LostFocus(null, null);
        }

        // Properties ==========================================

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(TimerInterface), new PropertyMetadata("Reminder Name"));


        // Other Methods =======================================

        private void ReminderActivationChanged(object sender, RoutedEventArgs e)
        {
            // Convert bool? to bool:
            bool check = ActivationToggle.IsChecked ?? false;
            ValuesPanel.IsEnabled = check;

            // Fire "ActivationChanged" event:
            if (ActivationChanged != null)
                ActivationChanged(this, EventArgs.Empty);
        }

        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            Seconds = txtSeconds.Text;
            Minutes = txtMinutes.Text;
        }

        private void txt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Allow only numbers and backspace:

            int key = (int)e.Key;

            e.Handled = !(key >= 34 && key <= 43 ||
                          key >= 74 && key <= 83 ||
                          key == 2);
        }

        private void txt_LostFocus(object sender, RoutedEventArgs e)
        {
            txtSeconds.Text = Seconds;
            txtMinutes.Text = Minutes;
        }
    }
}
