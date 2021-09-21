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

/* TODO: Make the tool dynamic by adding the ability of adding slides to the tool from
 * the window where the slider is used, and then make the slider access those slides.
 */
namespace iShield
{
    /// <summary>
    /// Interaction logic for ContentSlider.xaml
    /// </summary>
    public partial class ContentSlider : UserControl
    {
        public event EventHandler FinishedSliding;

        List<StackPanel> slides;
        int currentSlide = -1;

        string alternativeText = "Start";

        public ContentSlider()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSlides();
            if (slides.Count > 0)
            {
                slides[0].Visibility = Visibility.Visible;
                currentSlide = 0;
            }
        }

        private void LoadSlides()
        {
            slides = new List<StackPanel>()
            {
                Slide1, Slide2, Slide3, Slide4, Slide5, Slide6, Slide7, Slide8, Slide9, Slide10, Slide11, Slide12, Slide13, Slide14
            };
        }

        private void NextSlide(object sender, RoutedEventArgs e)
        {
            if (currentSlide == -1) return;

            if (currentSlide < slides.Count - 1)
            {
                slides[currentSlide].Visibility = Visibility.Hidden;
                ++currentSlide;
                slides[currentSlide].Visibility = Visibility.Visible;

                if (currentSlide == slides.Count - 1)
                {
                    var temp = NextButtonText.Text;
                    NextButtonText.Text = alternativeText;
                    alternativeText = temp;
                }
            } else {
                if (FinishedSliding != null)
                    FinishedSliding(this, EventArgs.Empty);
            }
        }

        public void Reset()
        {
            if (currentSlide == -1 || currentSlide == 0) return;

            if (currentSlide == slides.Count - 1)
            {
                var temp = NextButtonText.Text;
                NextButtonText.Text = alternativeText;
                alternativeText = temp;
            }

            slides[currentSlide].Visibility = Visibility.Hidden;
            currentSlide = 0;
            slides[0].Visibility = Visibility.Visible;
        }
    }
}
