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
using sample2.remote;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using sample2.models;
using System.IO;
using ToastNotifications;
using ToastNotifications.Position;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using System.Windows.Threading;

namespace sample2
{
    /// <summary>
    /// Interaction logic for _4_Product_Entry.xaml
    /// </summary>
    public partial class _4_Product_Entry : Page
    {
        Notifier noti = new Notifier(cfg =>
        {
            cfg.PositionProvider = new WindowPositionProvider(
                parentWindow: Application.Current.MainWindow,
                corner: Corner.TopRight,
                offsetX: 10,
                offsetY: 10);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(3),
                maximumNotificationCount: MaximumNotificationCount.FromCount(5));

            cfg.Dispatcher = Application.Current.Dispatcher;
        });
        private _6_Menu _6_Menu;
        string fn = "";
        float Buying_Price_Parsed, Selling_Price_Parsed, SGSTRate_Parsed, IGSTRate_Parsed, CGSTRate_Parsed;



        public _4_Product_Entry(_6_Menu _6_Menu)
        {
            InitializeComponent();
            StartClock();

            List<string> ProductNames = SqliteChange.getProductNames();
            ProductNames.Sort();
            foreach (string ProductName in ProductNames)
                this.ProductNametxt.Items.Add(ProductName);
            
            refresh_categories();

            this._6_Menu = _6_Menu;

        }

        public void refresh_categories()
        {
            this.Category.SelectionChanged -= Category_SelectionChanged;
            this.Category.Items.Clear();
            this.Category.Items.Add("Select Category");
            List<string> CtNames = SqliteChange.getCategories();
            foreach (string CtName in CtNames)
                this.Category.Items.Add(CtName);
            this.Category.Items.Add("Add or Edit Category");
            this.Category.SelectedItem = "Select Category";
            this.Category.SelectionChanged += Category_SelectionChanged;
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
                Delete.Visibility = Visibility.Visible;



                if (this.ProductNametxt.SelectedItem != null)
                {
                    ProductNameAvailability = SqliteChange.ProductNamecombo(this.ProductNametxt.SelectedItem.ToString());
                    ProductNameString = this.ProductNametxt.SelectedItem.ToString();
                    this.Delete.Visibility = Visibility.Visible;

                }
                else
                {
                    ProductNameAvailability = SqliteChange.ProductNamecombo(this.ProductNametxt.Text);
                    ProductNameString = this.ProductNametxt.Text;
                    this.ProductDescriptiontxt.Text = "";
                    this.IGSTRatetxt.Text = "";
                    this.SGSTRatetxt.Text = "";
                    this.CGSTRatetxt.Text = "";
                    this.Min_Qty.Text = "";
                    this.Notify_Before.Text = "";
                    this.imagePhoto.Source = null;
                    this.HSNCodetxt.Text = "";
                    this.BuyingPriceTxt.Text = "";
                    this.SellingPriceTxt.Text = "";

                    this.Category.SelectedItem = "Select Category";

                    this.Delete.Visibility = Visibility.Hidden;
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
                this.ProductNametxt.IsDropDownOpen = true;

                ProductNameAvailability = SqliteChange.ProductNamecombo(this.ProductNametxt.Text);
                productdetails = SqliteDataAccess.getProductDetails(ProductName);
                if (ProductNameAvailability != 0)
                {
                    this.ProductDescriptiontxt.Text = "" + productdetails.Pr_Description;
                    this.IGSTRatetxt.Text = "" + productdetails.Pr_IGST;
                    this.SGSTRatetxt.Text = "" + productdetails.Pr_SGST;
                    this.CGSTRatetxt.Text = "" + productdetails.Pr_CGST;
                    this.Min_Qty.Text = "" + productdetails.Pr_Min_Qty;
                    this.Notify_Before.Text = "" + productdetails.Pr_Notify_Before;
                    this.HSNCodetxt.Text = "" + productdetails.Pr_HSN;
                    this.BuyingPriceTxt.Text = "" + productdetails.Pr_Buying_Price;
                    this.SellingPriceTxt.Text = "" + productdetails.Pr_Selling_Price;

                    this.Category.SelectedItem = "" + productdetails.Pr_Category;

                    imagePhoto.Source = SqliteChange.byteArrayToImage(productdetails.Pr_image);

                    this.Delete.Visibility = Visibility.Visible;
                }
            }
            else
            {
                this.ProductDescriptiontxt.Text = "";
                this.IGSTRatetxt.Text = "";
                this.SGSTRatetxt.Text = "";
                this.CGSTRatetxt.Text = "";
                this.Min_Qty.Text = "";
                this.Notify_Before.Text = "";
                this.imagePhoto.Source = null;
                this.HSNCodetxt.Text = "";
                this.BuyingPriceTxt.Text = "";
                this.SellingPriceTxt.Text = "";

                this.Category.SelectedItem = "Select Category";
                this.Delete.Visibility = Visibility.Hidden;
            }

        }



        private void Rate_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9.]+").IsMatch(e.Text);
        }

        private void FinalRate_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9.]+").IsMatch(e.Text);
        }

        private void Save(object sender, RoutedEventArgs e)
        {

            CGSTRate_Parsed = (this.CGSTRatetxt.Text.Length > 0) ? float.Parse(this.CGSTRatetxt.Text) : 0;
            IGSTRate_Parsed = (this.IGSTRatetxt.Text.Length > 0) ? float.Parse(this.IGSTRatetxt.Text) : 0;
            Selling_Price_Parsed = (this.SellingPriceTxt.Text.Length > 0) ? float.Parse(this.SellingPriceTxt.Text) : 0;
            Buying_Price_Parsed = (this.BuyingPriceTxt.Text.Length > 0) ? float.Parse(this.BuyingPriceTxt.Text) : 0;
            SGSTRate_Parsed = (this.SGSTRatetxt.Text.Length > 0) ? float.Parse(this.SGSTRatetxt.Text) : 0;

            if (this.ProductDescriptiontxt.Text == "" ||
            this.IGSTRatetxt.Text == "" ||
            this.SGSTRatetxt.Text == "" ||
            this.CGSTRatetxt.Text == "" ||
            this.Min_Qty.Text == "" ||
            this.Notify_Before.Text == "" ||
            this.imagePhoto.Source == null ||
            this.HSNCodetxt.Text == "" ||
            this.BuyingPriceTxt.Text == "" ||
            this.SellingPriceTxt.Text == "" || this.Category.Text == "Select Category")
            {
                noti.ShowInformation("Enter All Details!");
            } else {

                int success = SqliteChange.updateProductTable(this.ProductNametxt.Text,
                   this.ProductDescriptiontxt.Text, this.Category.Text, this.HSNCodetxt.Text, Selling_Price_Parsed, Buying_Price_Parsed,
                   IGSTRate_Parsed, CGSTRate_Parsed, SGSTRate_Parsed, Int32.Parse(this.Min_Qty.Text), Int32.Parse(this.Notify_Before.Text), 
                   fn, productdetails.Pr_image);

                if (success==1)
                this.NavigationService.Navigate(new _4_Product_Entry(_6_Menu));
            }
        }

        private void Delete_btn(object sender, RoutedEventArgs e)
        {
            int success = SqliteChange.deleteEntryInProductTable(this.ProductNametxt.Text);
            if (success > 0)
            { 
                noti.ShowSuccess("Successfully Deleted!");
                this.ProductNametxt.Items.Clear();
                List<string> ProductNames = SqliteChange.getProductNames();
                foreach (string ProductName in ProductNames)
                    this.ProductNametxt.Items.Add(ProductName);
            }
            else
            {
                noti.ShowError("Something went wrong! Please contact Admin!");
            }
        }

       
        private void Category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if((sender as ComboBox).SelectedItem.ToString() == "Add or Edit Category")
            {
                this.NavigationService.Navigate(new _17_Category(this));
            }
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(_6_Menu);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new _17_Category(this));
        }


        //private void Ratetxt_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    double a = Convert.ToDouble(GSTRatetxt.Text);
        //    double b = Convert.ToDouble(Ratetxt.Text);


        //}

        private void imgUpload(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

            if (open.ShowDialog() == true)
            {
                fn = open.FileName;
                imagePhoto.Source = new BitmapImage(new Uri(fn));
            }

        }

        private void GSTRate_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9.]+").IsMatch(e.Text);
        }
    }
}
