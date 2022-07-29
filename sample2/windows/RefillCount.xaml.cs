using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using sample2.models;
using sample2.remote;
using System.Windows.Threading;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;

namespace sample2
{
    /// <summary> 
    /// Interaction logic for RefillCount.xaml
    /// </summary>
    public partial class RefillCount : Window
    {
        LogModel user_details;
        int MaxQty = 0;
        ProductModel productDetails = new ProductModel();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        ProductTransactionModel LastTransaction = new ProductTransactionModel();
        DispatcherTimer timer = new DispatcherTimer();
        string machineMode = Sqlitedatavr.getMachineMode();

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


        public RefillCount(string product, string Row, string Col)
        {
            InitializeComponent();
            StartClock();
            ProductNametxt.Text = product;
            LastTransaction = SqliteChange.getLastProductTransaction(product, int.Parse( Row), int.Parse(Col));
            productDetails = SqliteDataAccess.getProductDetails(product);
            MaxQty = SqliteChange.getMaxQuantity(product, int.Parse(Row), int.Parse(Col));
            this.Row.Text = Row.ToString();
            this.Col.Text = Col.ToString();
            user_details = SqliteDataAccess.getLastLogEvent();
            this.username.Text = user_details.LT_username;
            Int32 ExistingQty = SqliteChange.getExistingQty(product, Int32.Parse(Row), Int32.Parse(Col));
            this.Opening_Stock.Text = ExistingQty.ToString();
            this.transaction_price.Text = "" + productDetails.Pr_Buying_Price;
            this.SGST.Text = "" + productDetails.Pr_SGST;
            this.CGST.Text = "" + productDetails.Pr_CGST;
            this.IGST.Text = "" + productDetails.Pr_IGST;
            this.FinalQuantitytxt.Text = "" + 0;
            this.TransactionTypeTxt.SelectionChanged += TransactionTypeTxt_SelectionChanged;
          
            DateTime dt;
            if (DateTime.TryParse((LastTransaction.CTT_ExpiryDate), out dt))
            {
                this.expdt.SelectedDate = dt;
                
            }
            else
            {
                this.expdt.SelectedDate = indianTime.Date;
            }

        }
        private void StartClock()
        {
           
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += tickevent;
            timer.Start();
        }
        private void StopClock()
        {
            timer.Stop();
            timer = new DispatcherTimer();
        }

        private void tickevent(object sender, EventArgs e)
        {
            this.Time.Text = DateTime.Now.ToString();
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            StopClock();
            this.Close();
        }

        private void AddQuantitytxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            int RowNo = Int32.Parse(this.Row.Text);
            int ColNo = Int32.Parse(this.Col.Text);
            //int Qty = Int32.Parse(this.AddQuantitytxt.Text);
            //int FinalQty = Int32.Parse(this.FinalQuantitytxt.Text);
            int success = 0;
            if (this.ProductNametxt.Text != "" && this.AddQuantitytxt.Text != "" && this.TransactionTypeTxt.Text != "" && this.transaction_price.Text != ""
                && this.SGST.Text != "" && this.CGST.Text != "" && this.IGST.Text != "" && this.Req_No.Text != ""
                && this.Opening_Stock.Text != "" && this.FinalQuantitytxt.Text != "" && this.Total_transaction_price.Text != ""
                && this.expdt.Text != "")
            {
                DateTime expiry = DateTime.Parse(this.expdt.Text);
                expiry = new DateTime(expiry.Year, expiry.Month, expiry.Day, 23, 59, 59);
                string formatdate = indianTime.ToString("yyyy-MM-dd HH:mm:ss"); //imthi 20-01-2021
                string formatteddate = expiry.ToString("yyyy-MM-dd HH:mm:ss");//imthi 20-01-2021

                string productName = this.ProductNametxt.Text;
                int Add_Quantity = int.Parse(this.AddQuantitytxt.Text);
                string transactionType = this.TransactionTypeTxt.Text;
                int transaction_price = int.Parse(this.transaction_price.Text);
                int Opening_Stock = int.Parse(this.Opening_Stock.Text);
                int FinalQuantity = int.Parse(this.FinalQuantitytxt.Text);
                int Total_transaction_price = int.Parse(this.Total_transaction_price.Text);
                float SGST = float.Parse(this.SGST.Text);
                float CGST = float.Parse(this.CGST.Text);
                float IGST = float.Parse(this.IGST.Text);
              

                success += SqliteChange.InsertIntoTrayTransaction(
                  RowNo, ColNo, productName, Add_Quantity, transactionType, transaction_price, SGST, CGST, IGST, formatdate, 
                  "Office", this.Req_No.Text, Opening_Stock, FinalQuantity,//imthi 20-01-2021
                 Total_transaction_price, "", username.Text, formatteddate, "Completed", machineMode);//imthi 20-01-2021



                if (this.TransactionTypeTxt.Text == "Buying")
                    success += SqliteChange.UpdateProductTable(this.ProductNametxt.Text, int.Parse(productDetails.Pr_Selling_Price),
                        int.Parse(this.transaction_price.Text), float.Parse(this.SGST.Text), float.Parse(this.CGST.Text), float.Parse(this.IGST.Text), formatteddate);//imthi 20-01-2021
                else
                {


                    success += SqliteChange.UpdateProductTable(this.ProductNametxt.Text, int.Parse(this.transaction_price.Text),
                  int.Parse(productDetails.Pr_Buying_Price), float.Parse(this.SGST.Text), float.Parse(this.CGST.Text), float.Parse(this.IGST.Text), formatteddate);//imthi 20-01-2021
                }


            success += SqliteChange.UpdateCellTable(ColNo, RowNo, int.Parse(this.FinalQuantitytxt.Text), int.Parse(this.FinalQuantitytxt.Text));
            }
            else noti.ShowError("Please enter all details.");

            if (success >= 3)
            {
                StopClock();
                this.Close();
                RefillCount refillCount = new RefillCount(ProductNametxt.Text, RowNo.ToString(), ColNo.ToString());
                refillCount.Show();
            }
            else
            {
                noti.ShowError("Contact the Software Admin!");
                StopClock();
                this.Close();
            }
        }


        

        private void AddQuantitytxt_TextChanged(object sender, TextChangedEventArgs e)
        {
           total_price_calculation(); int total_qty = 0;
            if (this.AddQuantitytxt.Text.Length > 0)
            {
                if (int.Parse(this.AddQuantitytxt.Text) != 0)
                {
                    if (this.TransactionTypeTxt.Text == "Buying")
                    {
                        total_qty = int.Parse(this.AddQuantitytxt.Text) + int.Parse(this.Opening_Stock.Text);
                        if (total_qty > MaxQty)
                        {
                            noti.ShowError("Cannot stock more than the max quantity.");
                            this.AddQuantitytxt.Text = "";
                            StopClock();
                            this.Close();
                        }
                    }
                    else if (int.Parse(this.AddQuantitytxt.Text) <= int.Parse(this.Opening_Stock.Text))
                    { total_qty = int.Parse(this.Opening_Stock.Text) - int.Parse(this.AddQuantitytxt.Text); }
                    else
                    {
                        noti.ShowError("Existing stock quatity exceeded. Reduce the transaction quantity.");
                        this.AddQuantitytxt.Text = "";
                        StopClock();
                        this.Close();
                    }
                    this.FinalQuantitytxt.Text = "" + total_qty;
                }
            }
            else this.FinalQuantitytxt.Text = "";
        }






        //private void AddQuantitytxt_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    TextBox tb = (TextBox)sender;
        //    tb.Text = string.Empty;
        //    tb.GotFocus -= AddQuantitytxt_GotFocus;
        //}

        //private void RemovedQuantitytxt_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    TextBox tb = (TextBox)sender;
        //    tb.Text = string.Empty;
        //    tb.GotFocus -= RemovedQuantitytxt_GotFocus;
        //}

        private void TransactionTypeTxt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedItem.ToString().Contains("Buying"))
            {
                this.transaction_price.Text = productDetails.Pr_Buying_Price;
                this.transaction_price.IsEnabled = true; //imthi 05/03/2021
            }
            else

            {
                this.transaction_price.Text = productDetails.Pr_Selling_Price;
                this.transaction_price.IsEnabled = false; //imthi 05/03/2021
            }
        }

        private void transaction_price_TextChanged(object sender, TextChangedEventArgs e)
        {
            total_price_calculation();
        }

        void total_price_calculation()
        {
            if (this.AddQuantitytxt.Text.Length > 0 && this.transaction_price.Text.Length > 0)
            {
                if (int.Parse(this.AddQuantitytxt.Text) != 0 && int.Parse(this.transaction_price.Text) != 0)
                {
                    int total_price = int.Parse(this.AddQuantitytxt.Text) * int.Parse(this.transaction_price.Text);
                    this.Total_transaction_price.Text = "" + total_price;
                }
            }
            else this.Total_transaction_price.Text = "";
        }
    }
}
       
    

