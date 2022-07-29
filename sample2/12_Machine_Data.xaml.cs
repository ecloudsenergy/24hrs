using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;
using sample2.remote;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using sample2.models;

namespace sample2
{
    /// <summary>
    /// Interaction logic for _12_Machine_Data.xaml
    /// </summary>
    public partial class _12_Machine_Data : Page
    {
        private _6_Menu _6_Menu;
        string fn = "";
        MachineModel machineDetails;


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

        public _12_Machine_Data(_6_Menu _6_Menu)
        {
            InitializeComponent();
            this._6_Menu = _6_Menu;
            machineDetails = SqliteDataAccess.getMachineMasterDetails();
            if (machineDetails.MM_Machine_No != "" || machineDetails.MM_Machine_No != null)
            {
                Machine_Year.Text = machineDetails.MM_Machine_No.Substring(1, 2);
                Machine_Year_hint.Text = "";
                Machine_month.Text = machineDetails.MM_Machine_No.Substring(3, 2); 
                Machine_month_hint.Text = "";
                
                MachineNumber.Text = machineDetails.MM_Machine_No.Substring(5, 2);
            }
            videoFile.Text = (machineDetails.MM_Open_file != null || machineDetails.MM_Open_file != "") ? machineDetails.MM_Open_file : "";
            Location_of_Machine.Text = (machineDetails.MM_Location != null || machineDetails.MM_Location != "") ? machineDetails.MM_Location : "";
            Help_Line_Number.Text = (machineDetails.MM_HelpNo != null || machineDetails.MM_HelpNo != "") ? machineDetails.MM_HelpNo : "";
            Location_of_Desc.Text = (machineDetails.MM_Loc_description != null || machineDetails.MM_Loc_description != "") ? machineDetails.MM_Loc_description : "";
            if(machineDetails.MM_Location_Logo.Length > 0)
            {
                img.Source = SqliteChange.byteArrayToImage(machineDetails.MM_Location_Logo);
            }

            if (machineDetails.MM_Mode == "LIVE" || machineDetails.MM_Mode == "" || machineDetails.MM_Mode == null)
                Live.IsChecked = true;
            else Test.IsChecked = true;

        }
        private void Uploadimage_Click(object sender, RoutedEventArgs e)
        {
            
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Image Files(*.jpg;*.png;)|*.jpg;,*.png";

            if (open.ShowDialog() == true)
            {
                fn = open.FileName;
                img.Source = new BitmapImage(new Uri(fn));
            }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            int sucess = 0;
            
            
            if (this.machine_serial.Text != "" || this.Machine_Year.Text.Length == 2 || this.Machine_month.Text.Length == 2 || 
                this.MachineNumber.Text.Length == 2 || !videoFile.Text.Contains("\\"))
            {
                string machine_no = this.machine_serial.Text + this.Machine_Year.Text + this.Machine_month.Text + this.MachineNumber.Text;
                string col_names = "MM_Machine_No"; string content = "'" + machine_no + "'";

                if (videoFile.Text.Length > 0)
                {
                    col_names += ", MM_Open_file"; content += ", '" + videoFile.Text + "'";
                }
                if (Location_of_Machine.Text.Length > 0)
                {
                    col_names += ", MM_Location"; content += ", '" + Location_of_Machine.Text + "'";
                }
                if (Location_of_Desc.Text.Length > 0)
                {
                    col_names += ", MM_Loc_description"; content += ", '" + Location_of_Desc.Text + "'";
                }
                if (fn.Length > 0 || img.Source != null)
                {
                    col_names += ", MM_Location_Logo"; content += ", @img";
                }
                if (Help_Line_Number.Text.Length > 0)
                {
                    col_names += ", MM_HelpNo"; content += ", '" + Help_Line_Number.Text + "'";
                }
                if (Test.IsChecked != true)
                {
                    col_names += ", MM_Mode"; content += ", 'LIVE'";
                }
                else
                {
                    col_names += ", MM_Mode"; content += ", 'TEST'";
                }

                if (fn.Length == 0 && machineDetails.MM_Location_Logo != null)
                    sucess = Sqlitedatavr.insertIntoMachineMaster(col_names, content, "", machineDetails.MM_Location_Logo);
                else if (machineDetails.MM_Location_Logo == null && fn.Length > 0)
                    sucess = Sqlitedatavr.insertIntoMachineMaster(col_names, content, fn);
                else if (machineDetails.MM_Location_Logo != null && fn.Length > 0)
                    sucess = Sqlitedatavr.insertIntoMachineMaster(col_names, content, fn);
                else
                    sucess = Sqlitedatavr.insertIntoMachineMaster(col_names, content);



                this.NavigationService.Navigate(new _12_Machine_Data(_6_Menu));
            }
            else
            {
                noti.ShowError("Please enter the Machine number and Video file path as shown in the hint/instruction.");
            }
           
        }

        private void BackBtn(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(_6_Menu);
        }

        private void MachineNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
        }

        private void MachineMonthHint_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
            if((sender as TextBox).Text != "" )
            this.Machine_month_hint.Text = "";
            else this.Machine_month_hint.Text = "MM";
        }

        private void MachineYearHint_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
            if ((sender as TextBox).Text != "")
                this.Machine_Year_hint.Text = "";
            else this.Machine_Year_hint.Text = "YY";
        }

        private void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }
    }
}
