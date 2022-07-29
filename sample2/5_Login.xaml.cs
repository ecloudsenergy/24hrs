using System;
using System.Collections.Generic;
using System.Linq;

using System.Windows;
using System.Windows.Controls;

using System.Windows.Input;

using sample2.remote;
using sample2.models;
using System.Windows.Media;
using sample2.User_Controls;

namespace sample2
{
    /// <summary>
    /// Interaction logic for _5_Login.xaml
    /// </summary>
    public partial class _5_Login : Page
    {
       
        public _5_Login()
        {
            InitializeComponent();
            InitializeButton(btn_cancel, "Cancel", new SolidColorBrush(Colors.Gray), new SolidColorBrush(Colors.Silver),  
                new SolidColorBrush(Colors.White), new SolidColorBrush(Colors.White));
            btn_cancel.gotoPage = Cancel_GoBack;
            InitializeButton(btn_confirm, "Confirm", (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA500")),
                (SolidColorBrush)(new BrushConverter().ConvertFrom("#32CD32")),
                new SolidColorBrush(Colors.White), new SolidColorBrush(Colors.White));
            btn_confirm.gotoPage = Confrim_Credentials;
            InitializeButton(btn_cancel_again, "Cancel", new SolidColorBrush(Colors.Gray), new SolidColorBrush(Colors.Silver),
                new SolidColorBrush(Colors.White), new SolidColorBrush(Colors.White));
            btn_cancel_again.gotoPage = Cancel_GoBack;
            this.Error_text.Text = this.Error_text.Text + " " + SqliteDataAccess.getHelplineNumber();
        }

        void InitializeButton(Navigation_Buttons button, string btn_name, SolidColorBrush existing_backgorund, 
            SolidColorBrush change_backgorund, SolidColorBrush change_foregorund, SolidColorBrush existing_foregorund)
        {
            button.existing_backgorund = existing_backgorund;
            button.change_backgorund = change_backgorund;
            button.change_foregorund = change_foregorund;
            button.existing_foregorund = existing_foregorund;
            button.border.BorderThickness = new Thickness(0);
            button.back_color.Background = button.existing_backgorund;
            button.text_button.Foreground = button.existing_foregorund;
            button.text_button.Text = btn_name;
        }

       
        //private void password_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    PasswordBox textbox = sender as PasswordBox;
        //    VirtualKeyboard keyboardWindow = new VirtualKeyboard(this.password, this.passwordbox, Window.GetWindow(this), true);
        //    windowLocation(keyboardWindow);
        //    if (keyboardWindow.ShowDialog() == true)
        //    {
        //        this.passwordbox.Password = keyboardWindow.Result;
        //        this.password.Text = keyboardWindow.Result;
        //    }
        //}

        //private void windowLocation(VirtualKeyboard keyboardWindow)
        //{
        //    keyboardWindow.WindowStartupLocation = WindowStartupLocation.Manual;
        //    Point relativePoint = this.username.TransformToAncestor(Application.Current.MainWindow)
        //                  .Transform(new Point(0, 0));
        //    keyboardWindow.Top = relativePoint.X - 100;
        //    keyboardWindow.Left = relativePoint.Y;
        //}

        private void Visibility_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image image = sender as Image;
            if (image.Name == "visible_Icon")
            {
                image.Visibility = Visibility.Collapsed;
                this.password.Visibility = Visibility.Collapsed;

                this.invisible_Icon.Visibility = Visibility.Visible;
                this.passwordbox.Visibility = Visibility.Visible;
            }
            if (image.Name == "invisible_Icon")
            {
                image.Visibility = Visibility.Collapsed;
                this.passwordbox.Visibility = Visibility.Collapsed;

                this.visible_Icon.Visibility = Visibility.Visible;
                this.password.Visibility = Visibility.Visible;
            }
        }

        private void Confrim_Credentials()
        {
            if (this.username.Text != "" || this.password.Text != "")
            {
                int confirmation = SqliteDataAccess.getLoginVerification(this.username.Text, this.password.Text);
                if (confirmation == 0)
                {
                    this.credentials.Visibility = Visibility.Collapsed;
                    this.error_message.Visibility = Visibility.Visible;
                }
                else
                {
                    LogModel lastLog = SqliteDataAccess.getLastLogEvent();
                    if (lastLog.LT_Event_code == 1)
                    {
                       SqliteDataAccess.insertLogDetails(lastLog.LT_username, 2, SqliteDataAccess.getMachineNumber(), lastLog.LT_usertype);
                    }
                    string usertype = SqliteDataAccess.getUsertype(this.username.Text);
                    int log = SqliteDataAccess.insertLogDetails(this.username.Text, 1, SqliteDataAccess.getMachineNumber(), usertype);
                    if(log>0)
                        this.NavigationService.Navigate(new _6_Menu());
                }
            }
        }

        private void Cancel_GoBack()
        {
            this.NavigationService.Navigate(new _1_Idle());
        }

        private void passwordbox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            password.Text = passwordbox.Password;
        }

        private void password_TextChanged(object sender, TextChangedEventArgs e)
        {
             passwordbox.Password = password.Text;
        }
    }
}
