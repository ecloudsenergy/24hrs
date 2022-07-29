using System;
using System.Windows;
using System.Windows.Controls;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using sample2.reports;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace sample2
{
    /// <summary>
    /// Interaction logic for _11_Reports.xaml
    /// </summary>
    public partial class _11_Reports : Page
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        private static DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
       
        Page _6_Menu;

        public Notifier noti = new Notifier(cfg =>
        {
            cfg.PositionProvider = new WindowPositionProvider(
                parentWindow: Application.Current.MainWindow,
                corner: Corner.BottomCenter,
                offsetX: 10,
                offsetY: 10);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(2),
                maximumNotificationCount: MaximumNotificationCount.FromCount(5));

            cfg.Dispatcher = Application.Current.Dispatcher;
        });

        public _11_Reports()
        {
            InitializeComponent();
        }

        public _11_Reports(Page _6_Menu)
        {
            InitializeComponent();
            this._6_Menu = _6_Menu;
            for (int i = 0; i < 24; i++)
            {
                string text_form = i.ToString();
                if (text_form.Length < 2)
                    text_form = "0" + text_form;
                start_timepicker_hrs.Items.Add(text_form);
                end_timepicker_hrs.Items.Add(text_form);
            }
            
            for (int i = 0; i < 60; i++)
            {
                string text_form = i.ToString();
                if (text_form.Length < 2)
                    text_form = "0" + text_form;

                start_timepicker_mins.Items.Add(text_form);
                end_timepicker_mins.Items.Add(text_form);
            }

            start_timepicker_hrs.Text = "00";
            start_timepicker_mins.Text = "00";
            end_timepicker_hrs.Text = "23";
            end_timepicker_mins.Text = "59";

            if (indianTime.Month > 1 && indianTime.Month < 3)
            {
                DateTime last_year = new DateTime(indianTime.Year - 1, indianTime.Month, indianTime.Day);
                Start_Date.Text = last_year.ToString("01-04-yyyy");
            }
            else
                Start_Date.Text = indianTime.ToString("01-04-yyyy");
            End_Date.Text = indianTime.ToString("dd-MM-yyyy");
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(_6_Menu);
        }

        private void Get_Stock_Report(object sender, RoutedEventArgs e)
        {
            DateTime startdate = (DateTime)Start_Date.SelectedDate;
            DateTime enddate = (DateTime)End_Date.SelectedDate;
            if (startdate <= enddate)
            {
                string fromDateString = startdate.ToString("yyyy-MM-dd") + " " + start_timepicker_hrs.Text + ":" + start_timepicker_mins.Text + ":00";
                string toDateString = enddate.ToString("yyyy-MM-dd") + " " + end_timepicker_hrs.Text + ":" + end_timepicker_mins.Text + ":59";
                StockReport stockReport = new StockReport();
                stockReport.reportContent(fromDateString, toDateString);
            }
            else
            {
                MessageBox.Show("End Date is greater than Start Date. Please change the End Date.");
            }

        }
        private void Get_Stock_Sales_Report(object sender, RoutedEventArgs e)
        {
            // StockSalesReport stockSalesReport = new StockSalesReport();
            //stockSalesReport.reportContent();
        }

        private void Get_New_Arrangement_Report(object sender, RoutedEventArgs e)
        {
            DateTime startdate = (DateTime)Start_Date.SelectedDate;
            DateTime enddate = (DateTime)End_Date.SelectedDate;

            if (startdate <= enddate)
            {
                string fromDateString = startdate.ToString("yyyy-MM-dd") + " " + start_timepicker_hrs.Text + ":" + start_timepicker_mins.Text + ":00";
                string toDateString = enddate.ToString("yyyy-MM-dd") + " " + end_timepicker_hrs.Text + ":" + end_timepicker_mins.Text + ":59";
                NewArrangementReport newarrangementReport = new NewArrangementReport();
                newarrangementReport.reportContent(fromDateString, toDateString);
            }
            else
            {
                MessageBox.Show("End Date is greater than Start Date. Please change the End Date.");
            }

            
        }

      

        private void Hours_Verification(object sender, TextCompositionEventArgs e)
        {
            ComboBox hr_TBox = (ComboBox)sender;
            Regex reg = new Regex("^0-9$");
            bool verify = reg.IsMatch(hr_TBox.Text);
            if (!verify)
            {
                hr_TBox.Text = "";
            }
            else
            {
                int hr_val = int.Parse(hr_TBox.Text);
                if (hr_val > 23) hr_TBox.Text = "23";
            }
        }

        private void Mins_Verification(object sender, TextCompositionEventArgs e)
        {
            ComboBox min_TBox = (ComboBox)sender;
            Regex reg = new Regex("^0-9$");
            bool verify = reg.IsMatch(min_TBox.Text);
            if (!verify)
            {
                min_TBox.Text = "";
            }
            else
            {
                int min_val = int.Parse(min_TBox.Text);
                if (min_val > 59) min_TBox.Text = "59";

                
            }
        }

        private void lost_focus_combobox(object sender, RoutedEventArgs e)
        {
            ComboBox comboBox = (ComboBox) sender;
            if (comboBox.Text.Length < 2)
                comboBox.Text = "0" + comboBox.Text;
        }


    }

    
    
}
