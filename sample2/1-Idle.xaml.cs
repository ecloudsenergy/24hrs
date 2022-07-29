using sample2.remote;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace sample2
{
    /// <summary>
    /// Interaction logic for _1_Idle.xaml
    /// </summary>
    public partial class _1_Idle : Page
    {
        int timerCount = 0;
        DispatcherTimer dt = new DispatcherTimer();


        public _1_Idle()
        {
            InitializeComponent();
            AdTimeline.Source = new Uri(@"" + SqliteDataAccess.getMachineVideo(), UriKind.RelativeOrAbsolute);
            dt.Interval = new TimeSpan(0, 0, 1);
            txtHelp.Text += SqliteDataAccess.getHelplineNumber();
            //AdTimeline.Source = new Uri(@"C:\Users\Admin\Desktop\24Hrs\24Hrs Presentation\Thulasi Vending Machine", UriKind.RelativeOrAbsolute);
        }


        private void touch_here_touch(object sender, TouchEventArgs e)
        {
            this.NavigationService.Navigate(new _2_Product_Selection());
        }



        private void EnterHoldState(object sender, TouchEventArgs e)
        {
            timerStarted();
        }

        private void EnterHoldState(object sender, RoutedEventArgs e)
        {
            timerStarted();
        }

        private void ExitHoldState(object sender, RoutedEventArgs e)
        {
            timerStopped();
        }

        private void ExitHoldState(object sender, TouchEventArgs e)
        {
            timerStopped();
        }


        private void timerStopped()
        {
            dt.Stop();
            timerCount = 0;
        }


        private void timerStarted()
        {
            dt.Tick += delayCounter;
            dt.Start();

        }

        private void delayCounter(object sender, EventArgs e)
        {
            timerCount++;
            if (timerCount == 2)
            {
                this.NavigationService.Navigate(new _5_Login());
            }
        }

       
        private void btn_touch_here_click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new _2_Product_Selection());
        }

       
    }
}
