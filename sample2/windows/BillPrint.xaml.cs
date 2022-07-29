using sample2.remote;
using sample2.User_Controls;
using System;
using System.IO;
using System.Printing;

using System.Windows;
using System.Windows.Controls;

using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace sample2
{
    /// <summary>
    /// Interaction logic for BillPrint.xaml
    /// </summary>
    public partial class BillPrint : Window
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private delegate double roundingoff(double price);
        string MachineMode = Sqlitedatavr.getMachineMode();

        public BillPrint()
        {
            InitializeComponent();         
           
        }


        public void AddColumnText(string text_content, int code)
        {
            TextBlock text = new TextBlock();
            text.Text = text_content;
            switch (code)
            {
                case 1:
                    text.TextAlignment = TextAlignment.Left;
                    this.Left_Top_Column.Children.Add(text);
                    break;
                case 2:
                    text.TextAlignment = TextAlignment.Right;
                    this.Right_Top_Column.Children.Add(text);
                    break;
                case 3:
                    text.TextAlignment = TextAlignment.Left;
                    this.Left_Bottom_Column.Children.Add(text);
                    break;
                case 4:
                    text.TextAlignment = TextAlignment.Right;
                    this.Right_Bottom_Column.Children.Add(text);
                    break;
            }

        }

        public void AddItem(string item_name, string item_hsn, double item_amount, int qty, double SGST, double CGST, double IGST = 0, double Vat = 0, double discount = 0)
        {

            Bill_NumOfItems item = new Bill_NumOfItems();

            item.product_name_HSN.Text = item_name + " [" + item_hsn + "]";
            item.product_name_HSN.TextWrapping = TextWrapping.Wrap;
            item.selling_price.Text = displayRupee(item_amount);
            item.Qty.Text = ""+qty;
            

            item.Qty.TextAlignment = TextAlignment.Right;
            item.selling_price.TextAlignment = TextAlignment.Right;
            item.CGST.TextAlignment = TextAlignment.Right;
            item.SGST.TextAlignment = TextAlignment.Right;
           
            item.Net_Amount.TextAlignment = TextAlignment.Right;
           
            double net_amount = item_amount * qty;
            item.Net_Amount.Text = displayRupee(roundoff(net_amount));
            item.CGST.Text = SGST + "%";
            item.SGST.Text = CGST + "%";
            
            item.line_visibility.Visibility = Visibility.Collapsed;
            this.items.Children.Add(item);
        }

        public string displayRupee(double price)
        {
            string text_price = "" + price;

            string[] splited_price = text_price.Split('.');
            string price_text = "₹" + splited_price[0] + ".";
            if (splited_price.Length == 2)
            {
                if (splited_price[1].Length == 1) price_text += splited_price[1] + "0";
                else price_text += splited_price[1];
            }
            else price_text += "00";
            return price_text;
        }
        public double roundoff(double rate)
        {
            rate = rate * 100;
            int temp = (int)rate;
            rate = (double) temp / 100;
            return rate;
        }


        public void print(string bill_number, bool isCancelled = false)
        {
            try
            {
                if (isCancelled)
                {

                }
                PrintServer ps = new PrintServer();
                PrintDialog pr = new PrintDialog();
                pr.PrintQueue = ps.GetPrintQueue(@"" + SqliteDataAccess.getPort("printer"));
                pr.PrintVisual(TextBlock1, bill_number);
                // if (ps.BeepEnabled)
                //TODO: send alert and log details into errors table

                //< get Screenshot of Element >
                TextBlock1.Width = 240;
                TextBlock1.Margin = new Thickness(5);
                this.Show();
                double height = this.ActualHeight * 1.5;
                int width = (int)this.ActualWidth * 2;
                String filename = "D:\\Bills\\" + bill_number + "-" + indianTime.Hour + indianTime.Minute + ".jpg";
                RenderTargetBitmap bmp = new RenderTargetBitmap((int)this.ActualWidth, (int)height, 150, 150, PixelFormats.Pbgra32);

                bmp.Render(this);

                //</ get Screenshot of Element >

                //< create Encoder >

                JpegBitmapEncoder encoder = new JpegBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(bmp));

                //</ create Encoder >
                //< save >

                FileStream fs = new FileStream(filename, FileMode.Create);

                encoder.Save(fs);

                fs.Close();
                this.Close();
                if (MachineMode == "LIVE")
                {
                    int success = SqliteDataAccess.insertBill(bill_number, filename, MachineMode);
                    if (success == 0) { /*TODO: Bug Logging Needed */}
                }
                //</ save >

            }
            catch (Exception m)
            {

                MessageBox.Show(m.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
           


        }
    }
}
