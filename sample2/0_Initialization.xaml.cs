using sample2.remote;
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

namespace sample2
{
    /// <summary>
    /// Starts up every time when the application is opened new.
    /// </summary>
    public partial class _0_Initialization : Page
    {
        int timerCount = 0;
        DispatcherTimer dt = new DispatcherTimer();

        public _0_Initialization()
        {
            InitializeComponent();
            dt.Interval = new TimeSpan(0, 0, 1);
            support.Text += SqliteDataAccess.getHelplineNumber();
        }

        public _0_Initialization(string Error)
        {
            InitializeComponent();
            dt.Interval = new TimeSpan(0, 0, 1);
            support.Text = "Error: "+Error + support.Text;
            support.Text += SqliteDataAccess.getHelplineNumber();
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
    }
}
