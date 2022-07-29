using sample2.models;
using sample2.remote;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;

namespace sample2
{
    
    /// <summary>
    /// Interaction logic for _9_Coin_Refill.xaml
    /// </summary>
    public partial class _9_Coin_Refill : Page
    {
        private _6_Menu _6_Menu;
        LogModel user_details;
        Int32 a = 0;
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        static Notifier noti = new Notifier(cfg =>
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

        public _9_Coin_Refill(_6_Menu _6_Menu)
        {
            InitializeComponent();
            this._6_Menu = _6_Menu;
            user_details = SqliteDataAccess.getLastLogEvent();
            this.UserNameTxt.Text = user_details.LT_username;
            StartClock();
            this.BalanceAmountTxt.Text = "" + SqliteChange.getBalance("CD", Sqlitedatavr.getCoinDenomination());
            int coin_deno = 1;
            int convert = coin_deno * Sqlitedatavr.getCoinDenomination();
            DenominationTxt.Text = convert.ToString();
        }


        private void StartClock()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += tickevent;
            timer.Start();
        }

        private void tickevent(object sender, EventArgs e)
        {
            this.Time.Text = DateTime.Now.ToString();
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(_6_Menu);
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            int success = 0;
            String formatdate = indianTime.ToString("yyyy-MM-dd HH:mm:ss"); //imthi 20-01-2021
            if (this.AdditionalCountTxt.Text != "" && Int32.Parse(this.FinalBalanceAmountTxt.Text) >= 0) {
                success = SqliteChange.InsertIntoCurrencyTransactionTable(Sqlitedatavr.getCoinDenomination(), this.Invoice_Req_No.Text,
                    Int32.Parse(this.AdditionalCountTxt.Text), formatdate, this.UserNameTxt.Text,this.TransactionTypeTxt.Text, //imthi 20-01-2021
                    Int32.Parse(this.BalanceAmountTxt.Text), Int32.Parse(this.FinalBalanceAmountTxt.Text), "CD", "Completed");
            }
            else if (this.FinalBalanceAmountTxt.Text.Length > 0 && Int32.Parse(this.FinalBalanceAmountTxt.Text) < 0) noti.ShowError("Cannot save negative value.");
            else
            {
                noti.ShowError("Please Fill the Box Correctly!");
                //MessageBox.Show("Please Fill the Box Correctly!");
            }
            this.NavigationService.Navigate(new _9_Coin_Refill(_6_Menu));
        }

        private void AdditionalCountTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.TransactionTypeTxt.Text == "Credit")
            {
                if (AdditionalCountTxt.Text != "")
                {
                    Int32 a = Convert.ToInt32(AdditionalCountTxt.Text);
                    Int32 b = a * Sqlitedatavr.getCoinDenomination();
                    Int32 c = Convert.ToInt32(BalanceAmountTxt.Text);
                    Int32 d = (c + a);
                    this.AdditionalAmountTxt.Text = b.ToString();
                    this.FinalBalanceAmountTxt.Text = d.ToString();
                }
                else
                {
                    this.AdditionalCountTxt.Text = "";
                    this.AdditionalAmountTxt.Text = "";
                    this.FinalBalanceAmountTxt.Text = "";

                }
            }
            else
            {
                if (AdditionalCountTxt.Text != "")
                {
                    Int32 a = Convert.ToInt32(AdditionalCountTxt.Text);
                    Int32 b = a * Sqlitedatavr.getCoinDenomination();
                    Int32 c = Convert.ToInt32(BalanceAmountTxt.Text);
                    Int32 d = (c - a);
                    this.AdditionalAmountTxt.Text = b.ToString();
                    this.FinalBalanceAmountTxt.Text = d.ToString();
                }
                else
                {
                    this.AdditionalCountTxt.Text = "";
                    this.AdditionalAmountTxt.Text = "";
                    this.FinalBalanceAmountTxt.Text = "";

                }

            }
        }

        private void AdditionalCountTxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
        }

       /* private void DenominationTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            int coin_deno = 1;
            int convert = coin_deno * Sqlitedatavr.getCoinDenomination();
            DenominationTxt.Text = convert.ToString();

        }*/
    }
}
