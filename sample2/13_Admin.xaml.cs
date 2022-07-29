using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using sample2.remote;
using sample2.models;
using System.Windows.Media;

namespace sample2
{
    /// <summary>
    /// Interaction logic for _13_Admin.xaml
    /// </summary>
    public partial class _13_Admin : Page
    {
        private _6_Menu _6_Menu;
        LogModel user_details;

        public _13_Admin()
        {
            InitializeComponent();
        }

        public _13_Admin(_6_Menu _6_Menu)
        {
            InitializeComponent();
            this._6_Menu = _6_Menu;
            user_details = SqliteDataAccess.getLastLogEvent();
            this.machineNumber.Text = ""+SqliteDataAccess.getMachineNumber();
            List<string> usernames = SqliteDataAccess.getUsernames();
            usernames.Sort();
            foreach (string username in usernames)
                this.username.Items.Add(username);
            if (user_details.LT_usertype == "OP")
            {
                this.username.SelectedItem = user_details.LT_username;
                this.username.IsEnabled = false;
                this.username.Background = new SolidColorBrush(Colors.Silver);
                this.userType.SelectedIndex = 2;
                this.userType.IsEnabled = false;
                this.userType.Background = new SolidColorBrush(Colors.Silver);
                this.DeleteBtn.Visibility = Visibility.Hidden;
            }

        }

        private void Back(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(_6_Menu);
        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            int success = SqliteDataAccess.deleteEntryInAdminTable(this.username.Text, SqliteDataAccess.getMachineNumber());
            if (success > 0)
            {
                MessageBox.Show("Success");
                this.username.Items.Clear();
                List<string> usernames = SqliteDataAccess.getUsernames();
                foreach (string username in usernames)
                    this.username.Items.Add(username);
            }
            else
            {
                MessageBox.Show("Failed");
            }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            string username = this.username.Text;
            username.Trim();
            if (username.Length > 0 && this.userType.Text != "Select The User Type")
            {
                string userType = this.userType.Text;
                bool process = true;
                if (userType == "AD - Admin")
                    userType = "AD";
                else if (userType == "OP - Operator")
                    userType = "OP";
                else
                {
                    process = false;
                    MessageBox.Show("Usertype Problem.");
                }
                if (process)
                {
                    int success = SqliteDataAccess.updateAdminTable(username, this.password.Text, userType, SqliteDataAccess.getMachineNumber());

                    if (success > 0)
                    {
                        MessageBox.Show("Success");
                        this.username.Items.Clear();
                        List<string> usernames = SqliteDataAccess.getUsernames();
                        foreach (string user in usernames)
                            this.username.Items.Add(username);
                    }
                    else
                    {
                        MessageBox.Show("Failed");
                    }
                }
            }
            else {
                MessageBox.Show("Enter atleast Username and User Type.");
            }
        }

        private void username_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.username.SelectedItem as string != "" || this.username.Text != "")
            {
                int usernameAvailability = 0;
                string usernameString = "";
                if (this.username.SelectedItem != null)
                {
                    usernameAvailability = SqliteDataAccess.getUsernameVerification(this.username.SelectedItem.ToString());
                    usernameString = this.username.SelectedItem.ToString();
                }
                else
                {
                    usernameAvailability = SqliteDataAccess.getUsernameVerification(this.username.Text);
                    usernameString = this.username.Text;
                }

                if (usernameAvailability > 0)
                {
                    if (user_details.LT_usertype != "OP")
                        this.DeleteBtn.Visibility = Visibility.Visible;
                    string userType = SqliteDataAccess.getUsertype(usernameString);
                    usertype_selection(userType);
                }
                else
                {
                    this.DeleteBtn.Visibility = Visibility.Hidden;
                }
            }
        }

        private void usertype_selection(string usertype)
        {
            if(usertype == "AD")
            {
                this.userType.SelectedIndex = 1;
            }
            else if(usertype == "OP") { this.userType.SelectedIndex = 2; }
        }

        private void username_TextChanged(object sender, RoutedEventArgs e)
        {
            ComboBox combobox = sender as ComboBox;
            int usernameAvailability = 0;
            string username = this.username.Text;
            if (combobox.Items.Contains(username))
                combobox.IsDropDownOpen = true;
            else combobox.IsDropDownOpen = false;
            usernameAvailability = SqliteDataAccess.getUsernameVerification(this.username.Text);
           
            
            if (usernameAvailability == 0)
            {
                this.DeleteBtn.Visibility = Visibility.Hidden;
                this.userType.SelectedIndex = 0;
                this.password.Text = "";
            }

            else if (usernameAvailability > 0)
            {
                if (user_details.LT_usertype != "OP")
                    this.DeleteBtn.Visibility = Visibility.Visible;
                string userType = SqliteDataAccess.getUsertype(username);
                usertype_selection(userType);
            }
        }
    }
}
