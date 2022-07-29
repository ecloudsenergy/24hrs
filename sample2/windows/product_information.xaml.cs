using sample2.models;
using sample2.remote;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// <summary>
    /// Interaction logic for product_information.xaml
    /// </summary>
    public partial class product_information : Window
    {
        ProductModel product;
        DispatcherTimer dt = new DispatcherTimer();
        int timerCount = 0;

        public product_information(ProductModel product)
        {
            InitializeComponent();
            close.text_button.Text = "Close";
            close.text_button.FontSize = 16;
            close.gotoPage = close_Click;
            dt.Interval = new TimeSpan(0, 0, 15);
            timerStarted();
            this.product = product;
            this.product_Name.Text = product.Pr_Name;
            MemoryStream ms = new MemoryStream(product.Pr_image);
            System.Drawing.Image image_temp = System.Drawing.Image.FromStream(ms);
            this.product_Image.Source = ToWpfImage(image_temp);
            this.product_Des.Text = product.Pr_Description;
            CellModel productCells = SqliteChange.getCellNumber(product.Pr_Name);
            ProductTransactionModel productLastTransaction = SqliteChange.getLastProductTransaction(product.Pr_Name, productCells.CT_Row_No, productCells.CT_Col_No);
            this.product_Exp_Date.Text = productLastTransaction.CTT_ExpiryDate;
            this.product_Rate.Text = "Rs. " + product.Pr_Selling_Price;
        }

        private void timerStarted()
        {
            dt.Tick += delayCounter;
            dt.Start();
        }

        private void delayCounter(object sender, EventArgs e)
        {
            timerCount++;
            if (timerCount >= 3)
            {
                dt.Stop();
                this.Close();
            }
        }

        private void close_Click()
        {
            dt.Stop();
            this.Close();
            close.text_button.FontSize = 20;
        }

        private BitmapImage ToWpfImage(System.Drawing.Image img)
        {
            MemoryStream ms = new MemoryStream();  // no using here! BitmapImage will dispose the stream after loading
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

            BitmapImage ix = new BitmapImage();
            ix.BeginInit();
            ix.CacheOption = BitmapCacheOption.OnLoad;
            ix.StreamSource = ms;
            ix.EndInit();
            return ix;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            timerCount = 0;
        }

        private void Window_TouchDown(object sender, TouchEventArgs e)
        {
            timerCount = 0;
        }
    }
}
