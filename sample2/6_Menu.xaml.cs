using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Runtime.InteropServices;
using sample2.models;
using sample2.remote;

namespace sample2
{
    /// <summary>
    /// Interaction logic for _6_Menu.xaml
    /// </summary>
    public partial class _6_Menu : Page
    {
        public _6_Menu()
        {
            InitializeComponent();
            Product_Entry.text_button.Text = "Product Entry";
            Cash_Refill.text_button.Text = "Cash Refill";
            Tray_Settings.text_button.Text = "Tray Settings";
            Coin_Refill.text_button.Text = "Coin Refill";
            Refill_Products.text_button.Text = "Refill Products";
            Reports.text_button.Text = "Reports";
            Machine_Data.text_button.Text = "Machine Data";
            Desktop.text_button.Text = "Desktop";
            Admin.text_button.Text = "Admin";
            Restart.text_button.Text = "Restart";
            Back.text_button.Text = "Back";
            Shutdown.text_button.Text = "Shutdown";
            Initialization.text_button.Text = "Initialization";

            Product_Entry.gotoPage = Product_Entry_btn;
            Cash_Refill.gotoPage = Cash_Refill_btn;
            Tray_Settings.gotoPage = Tray_Settings_btn;
            Coin_Refill.gotoPage = Coin_Refill_btn;
            Refill_Products.gotoPage = Refill_Products_btn;
            Reports.gotoPage = Reports_btn;
            Machine_Data.gotoPage = Machine_Data_btn;
            Desktop.gotoPage = Desktop_btn;
            Admin.gotoPage = Admin_btn;
            Restart.gotoPage = Restart_btn;
            Back.gotoPage = Cancel_GoBack;
            Shutdown.gotoPage = Shutdown_btn;
            Initialization.gotoPage = Initialising;

            LogModel user_details = SqliteDataAccess.getLastLogEvent();
            if (user_details.LT_usertype == "OP")
            {
                this.Restart.Visibility = Visibility.Hidden;
                this.Shutdown.Visibility = Visibility.Hidden;
                this.Desktop.Visibility = Visibility.Hidden;
                this.Initialization.Visibility = Visibility.Hidden;
                this.Admin.Visibility = Visibility.Hidden;
            }
        }

        private void Cancel_GoBack()
        {
            exit_log();
            this.NavigationService.Navigate(new _1_Idle());
        }
        private void Product_Entry_btn()
        {
            this.NavigationService.Navigate(new _4_Product_Entry(this));
        }
        private void Cash_Refill_btn()
        {
            this.NavigationService.Navigate(new _7_Cash_Refill(this));
        }
        private void Tray_Settings_btn()
        {
            this.NavigationService.Navigate(new _8_Tray_Settings(this));
        }
        private void Coin_Refill_btn()
        {
            this.NavigationService.Navigate(new _9_Coin_Refill(this));
        }
        private void Refill_Products_btn()
        {
            this.NavigationService.Navigate(new _10_Refill_Products(this));

        }
        private void Reports_btn()
        {
            this.NavigationService.Navigate(new _11_Reports(this));
        }
        private void Machine_Data_btn()
        {
            this.NavigationService.Navigate(new _12_Machine_Data(this));
        }
        private void Desktop_btn()
        {
            exit_log();
            Application.Current.Shutdown();
        }
        private void Admin_btn()
        {
            this.NavigationService.Navigate(new _13_Admin(this));
        }
        private void Restart_btn()
        {
            exit_log();
            Process.Start("shutdown", "/r /t 0");
        }
        private void Shutdown_btn()
        {
            exit_log();
            Process.Start("shutdown", "/s /t 0");
        }
        private void Initialising()
        {
            this.NavigationService.Navigate(new _21_Initialising_process(this));
        }

        private void exit_log()
        {
            LogModel lastLog = SqliteDataAccess.getLastLogEvent();
            if (lastLog.LT_Event_code == 1)
            {
                SqliteDataAccess.insertLogDetails(lastLog.LT_username, 2, SqliteDataAccess.getMachineNumber(), lastLog.LT_usertype);
            }
        }

    }
}
