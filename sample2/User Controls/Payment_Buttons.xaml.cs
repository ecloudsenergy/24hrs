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
using System.Windows.Threading;

namespace sample2.User_Controls
{
    /// <summary>
    /// Interaction logic for Payment_Buttons.xaml
    /// </summary>
    public partial class Payment_Buttons : UserControl
    {
        public delegate void Next_Page();
        public Next_Page gotoPage;
        public Payment_Buttons()
        {
            InitializeComponent();
            dt.Interval = new TimeSpan(0, 0, 0, 0, 200);
        }

        DispatcherTimer dt = new DispatcherTimer();

        private void Border_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            if ((back_color.Background as SolidColorBrush).Color == Colors.LightGray)
            {

                back_color.Background = new SolidColorBrush(Colors.OliveDrab);
                text_button.FontSize = 30;
                text_button.Foreground = new SolidColorBrush(Colors.White);
                ShadowEffect.Opacity = 0;
                timerStarted();
                gotoPage?.Invoke();
            }
        }

        private void timerStarted()
        {
            dt.Tick += delayCounter;
            dt.Start();
        }

        private void delayCounter(object sender, EventArgs e)
        {
            back_color.Background = new SolidColorBrush(Colors.LightGray);
            text_button.FontSize = 26;
            text_button.Foreground = new SolidColorBrush(Colors.OliveDrab);
            ShadowEffect.Opacity = 0.5;
            dt.Stop();
        }

        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((back_color.Background as SolidColorBrush).Color == Colors.LightGray)
            {

                back_color.Background = new SolidColorBrush(Colors.OliveDrab);
                text_button.FontSize = 30;
                text_button.Foreground = new SolidColorBrush(Colors.White);
                ShadowEffect.Opacity = 0;
                timerStarted();
                gotoPage?.Invoke();
            }
        }
    }
}
