using sample2.models;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace sample2
{
    public delegate void updatePage();
    /// <summary>
    /// Interaction logic for TraySetPopUp.xaml
    /// </summary>
    public partial class TraySetPopUp : Window
    {
        LogModel user_details;
        int enable = 0; int MaxQty = 0;
        Int32 ExistingQty = 0; _8_Tray_Settings previous_page;
        public TraySetPopUp(string product,string Row_, string Col_, _8_Tray_Settings previous_page)
        {
            InitializeComponent();
            StartClock();
            this.previous_page = previous_page;
            this.ProductNametxt.Text = product;
            ExistingQty = SqliteChange.getExistingQty(product, int.Parse(Row_), int.Parse(Col_));
            this.ExistingQuantitytxt.Text = ExistingQty.ToString();
            List<string> ProductNames = SqliteChange.getProductNames();
            ProductNames.Sort();
            foreach (string ProductName in ProductNames)
                this.ProductNametxt.Items.Add(ProductName);
            MaxQty = SqliteChange.getMaxQuantity(ProductNametxt.Text, int.Parse(Row_), int.Parse(Col_));
            this.MaxQuantitytxt.Text = MaxQty.ToString();
            this.Row.Text = Row_;
            this.Col.Text = Col_;
            user_details = SqliteDataAccess.getLastLogEvent();
            this.username.Text = user_details.LT_username;
            int r = int.Parse(Row_);
            int c = int.Parse(Col_);
            int e_d = 0;
            e_d = SqliteChange.getEnableStatus(r, c);
            if (e_d == 1)
            {
                EnableCB.IsChecked = true;
            }
            else if(e_d == 0)
            {
                EnableCB.IsChecked = false;
            }

            

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

        private void ProductName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ProductNametxt.SelectedItem as string != "" || this.ProductNametxt.Text != "")
            {
                int ProductNameAvailability = 0;
                string ProductNameString = "";             
                
                if (this.ProductNametxt.SelectedItem != null)
                {
                    ProductNameAvailability = SqliteChange.ProductNamecombo(this.ProductNametxt.SelectedItem.ToString());
                    ProductNameString = this.ProductNametxt.SelectedItem.ToString();
                    

                }
                else
                {
                    ProductNameAvailability = SqliteChange.ProductNamecombo(this.ProductNametxt.Text);
                    ProductNameString = this.ProductNametxt.Text;
                    
                }

            }
        }

        ProductModel productdetails = new ProductModel();

        private void ProductName_TextChanged(object sender, RoutedEventArgs e)
        {
            if (this.ProductNametxt.Text != "")
            {
                int ProductNameAvailability = 0;
                string ProductName = this.ProductNametxt.Text;
                //this.ProductNametxt.IsDropDownOpen = true;

                ProductNameAvailability = SqliteChange.ProductNamecombo(this.ProductNametxt.Text);
               
            }
            else
            {
                MessageBox.Show("Enter the Product Name!");
                
            }

        }

        private void Save(object sender, RoutedEventArgs e)
        {
            int max = Int32.Parse(this.MaxQuantitytxt.Text); //imthi 05/03/2021
            int exq = Int32.Parse(ExistingQty.ToString());      //imthi 05/03/2021

            int checkProduct = SqliteChange.getPrnameVerification(ProductNametxt.Text);
            if (exq == 0 && checkProduct > 0)                                      //imthi 05/03/2021
            {
                //TrayModel cellNo = SqliteChange.getCellNumber();
                int RowNo = Int32.Parse(this.Row.Text);
                int ColNo = Int32.Parse(this.Col.Text);
                //int Qty = Int32.Parse(this.AddQuantitytxt.Text);
                //int FinalQty = Int32.Parse(this.FinalQuantitytxt.Text);
                //imthi 05/03/2021

                int success1 = 0;
                success1 = SqliteChange.Trayinsert(RowNo, ColNo, enable, ExistingQty, max, ProductNametxt.Text);

                this.Close();
                TraySetPopUp rf = new TraySetPopUp(ProductNametxt.Text, RowNo.ToString(), ColNo.ToString(), previous_page);
                rf.Show();
                //if (success1 > 0)
                //{
                //    MessageBox.Show("Successfully updated!");
                //}
                //
            }
            else if (exq != 0)
            {
                MessageBox.Show("Existing Quantity has to be 0 before changing!");  // imthi = 05/03/2021
            }
            else
            {
                MessageBox.Show("This Product Does not Exist! Please update in Product Entry.");
            }
                //tray existing quantity

        }

        private void Back(object sender, RoutedEventArgs e)
        {
            this.Close();
            previous_page.refresh();
        }

        private void EnableCB_Checked(object sender, RoutedEventArgs e)
        {            
            if (EnableCB.IsChecked == true)
            {
                enable = 1;
            }
            else
            {
                enable = 0;
            }            
        }

        private void MaxQuantitytxt_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Text = string.Empty;
            tb.GotFocus -= MaxQuantitytxt_GotFocus;
        }
    }
}
