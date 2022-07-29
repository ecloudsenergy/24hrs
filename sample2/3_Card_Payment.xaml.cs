using Newtonsoft.Json;
using sample2.models;
using sample2.remote;
using sample2.User_Controls;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;


namespace sample2
{
    /// <summary>
    /// Interaction logic for _3_Card_Payment.xaml
    /// </summary>
    public partial class _3_Card_Payment : Page
    {
        _14_Payment previous_page;
        string recieved_data;
        List<cart_item> cart_items;
        SerialPort cart_port;
        DispatcherTimer dt = new DispatcherTimer();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        string machineMode = Sqlitedatavr.getMachineMode();
        public _3_Card_Payment(_14_Payment previous_page, List<cart_item> cart_items, string order_amount, string order_quantity)
        {
            InitializeComponent();
            btn_exit.text_button.Text = "Exit";
            btn_exit.gotoPage = btn_exit_Click;
            btn_category_back.text_button.Text = "Back";
            btn_category_back.gotoPage = btn_category_back_Click;
            dt.Interval = new TimeSpan(0, 0, 120);
            dt.Tick += TimeOut;
            dt.Start();
            

            cart_port = new SerialPort(SqliteDataAccess.getPort("card"), 9600, Parity.None, 8, StopBits.One);
            this.previous_page = previous_page;
            this.order_amount.Text = order_amount;
            this.order_quantity.Text = order_quantity;
            this.cart_items = cart_items;
            foreach (cart_item item in cart_items)
            {
                cart_item addItem = new cart_item(item);
                item.btn_cart_add.Visibility = Visibility.Hidden;
                item.btn_cart_sub.Visibility = Visibility.Hidden;
                this.cart.Children.Add(addItem);
            }
            cart_port.ReadTimeout = 200;
            cart_port.WriteTimeout = 50;
            cart_port.DataReceived += new SerialDataReceivedEventHandler(Recieve);

            displayWaitStatus();
            SendJson(order_amount);
           
        }

        private void TimeOut(object sender, EventArgs e)
        {
            displayErrorStatus();
            dt.Stop();
            displayTheButtons();
        }

        private delegate void UpdateUiTextDelegate(string text);
        private void Recieve(object sender, SerialDataReceivedEventArgs e)
        {
            recieved_data = cart_port.ReadExisting();
            Dispatcher.BeginInvoke(DispatcherPriority.Send, new UpdateUiTextDelegate(WriteData), recieved_data);
        }
        private void WriteData(string text)
        {
            if (text.Length == 0 || text == null)
                displayWaitStatus();
            else if (text.Contains("OK"))
                displayOKStatus();
           if (text.Length > 2 && !text.Contains("OK"))
            {
                int receivedSuccessfully = ReceiveJson(text);
                if (receivedSuccessfully == 1)
                {
                    displayPaymentSuccessStatus();
                    int success = 0;
                    String bill_number = SqliteDataAccess.getBillNumber();
                    for (int i = 0; i < cart_items.Count; i++)
                    {
                        string[] total_trans_amount = this.cart_items[i].Product_price.Text.Split(' ');
                        int quantity_ordered_remaining = int.Parse(this.cart_items[i].Product_quantity.Text);
                        ProductModel productDetails = SqliteDataAccess.getProductDetails(cart_items[i].Product_Name.Text);

                        while (quantity_ordered_remaining != 0)
                        {
                            CellModel productCell = SqliteChange.getCellNumber(productDetails.Pr_Name);
                            ProductTransactionModel lastTransDetails = SqliteChange.getLastProductTransaction(productDetails.Pr_Name, productCell.CT_Row_No, productCell.CT_Col_No);
                            DateTime expiry = DateTime.Parse(productDetails.Pr_Expiry_Date);    //imthi 20-01-2021
                            expiry = new DateTime(expiry.Year, expiry.Month, expiry.Day, 23, 59, 59);   //imthi 20-01-2021
                            String formatdate = indianTime.ToString("yyyy-MM-dd HH:mm:ss"); //imthi 20-01-2021
                            String formatteddate = expiry.ToString("yyyy-MM-dd HH:mm:ss");//imthi 20-01-2021
                            if (quantity_ordered_remaining <= lastTransDetails.CTT_Closing_Stock)
                            {
                                success += SqliteChange.InsertIntoTrayTransaction(
                                productCell.CT_Row_No, productCell.CT_Col_No, productDetails.Pr_Name, int.Parse(this.cart_items[i].Product_quantity.Text), "Selling",
                                int.Parse(productDetails.Pr_Selling_Price), int.Parse(productDetails.Pr_SGST), int.Parse(productDetails.Pr_CGST), int.Parse(productDetails.Pr_IGST),
                                formatdate, "Card", bill_number, lastTransDetails.CTT_Closing_Stock, lastTransDetails.CTT_Closing_Stock - quantity_ordered_remaining,  //imthi 20-01-2021
                                int.Parse(total_trans_amount[1]), "", "Customer", formatteddate, "paid", machineMode, text);        //imthi 20-01-2021
                                SqliteChange.UpdateCellTable(productCell.CT_Col_No, productCell.CT_Row_No, productCell.CT_Balance_Qty, lastTransDetails.CTT_Closing_Stock - quantity_ordered_remaining);
                                quantity_ordered_remaining = 0;
                            }
                            else
                            {
                                success += SqliteChange.InsertIntoTrayTransaction(
                                productCell.CT_Row_No, productCell.CT_Col_No, productDetails.Pr_Name, int.Parse(this.cart_items[i].Product_quantity.Text), "Selling",
                                int.Parse(productDetails.Pr_Selling_Price), int.Parse(productDetails.Pr_SGST), int.Parse(productDetails.Pr_CGST), int.Parse(productDetails.Pr_IGST),
                                formatdate, "Card", bill_number, lastTransDetails.CTT_Closing_Stock, 0,  //imthi 20-01-2021
                                int.Parse(total_trans_amount[1]), "", "Customer", formatteddate, "paid", machineMode, text);        //imthi 20-01-2021
                                SqliteChange.UpdateCellTable(productCell.CT_Col_No, productCell.CT_Row_No, productCell.CT_Balance_Qty, 0);
                                quantity_ordered_remaining = quantity_ordered_remaining - lastTransDetails.CTT_Closing_Stock;
                            }

                        }
                    }
                    this.NavigationService.Navigate(new _18_Vending_Screen(this, cart_items, this.order_amount.Text, this.order_quantity.Text, "Rs. 0", "card", bill_number));
                }
                else
                {
                    displayReceiveErrorStatus();
                    displayTheButtons();
                    cart_port.Close();
                }
            }
        }

        private int ReceiveJson(string text)
        {
            var receivedContent = JsonConvert.DeserializeObject<Card_Response>(text);
            if (receivedContent.CardHolderName.Length > 1)
                return 1;
            else
                return 0;
        }

        private void SendJson(string amount)
        {
            try {
                cart_port.Open();
                string[] separated_amount = amount.Split(' ');
                if (separated_amount[1].IndexOf('.') == -1)
                    separated_amount[1] = separated_amount[1] + ".00";
                DateTime currentTime = DateTime.Now.ToLocalTime();
                string billInvoiceNumber = "110"; //Need to have a Data access for getting the before bill number.
                string transactiontime = currentTime.ToString("yy") + currentTime.Month + currentTime.Date + currentTime.Hour + currentTime.Minute + currentTime.Second;
                string sendJsonString = "{\"BillInvoiceNumber\":\"" + billInvoiceNumber + "\", \"TransType\":\"2\", \"BaseAmt\":\"00\", \"Discount\":\"00\", \"Amount\":\"" + separated_amount[1] + "\",  \"CurrencyCode\":\"INR\",  \"TransTime\":\"" + transactiontime + "\", \"MobNum\":\"\" }";
                Send_Data(sendJsonString);
            }
            catch (Exception ex)
            {
                displayConnectionProblem();
                displayTheButtons();
                cart_port.Close();
            }
        }

        private void displayTheButtons()
        {
            this.btn_exit.Visibility = Visibility.Visible;
            this.btn_category_back.Visibility = Visibility.Visible;
        }

        private void displayErrorStatus()
        {
            this.message_display.Text = "Error/Timeout while sending, please try another payment option.";
        }

        private void displayReceiveErrorStatus()
        {
            this.message_display.Text = "Payment failed, please try another payment option.";
        }

        private void displayConnectionProblem()
        {
            this.message_display.Text = "Connection Problem. Please contact the support: "+ SqliteDataAccess.getHelplineNumber();
        }

        private void displayOKStatus()
        {
            this.message_display.Text = "Please Insert / Swipe your card. Amount to be paid: " + this.order_amount.Text + ". Press \"X\" button in the device to cancel the transaction.";
        }

        private void displayWaitStatus()
        {
            this.message_display.Text = "Please Wait...";
        }

        private void displayPaymentSuccessStatus()
        {
            this.message_display.Text = "Payment Successful. Intiating vending..";
        }

        private void Send_Data(string Text)
        {
            SerialCmdSend(Text);
        }

        public void SerialCmdSend(string data)
        {
            if (cart_port.IsOpen)
            {
                try
                {
                    cart_port.WriteLine(data);
                }
                catch (Exception ex)
                {
                    displayErrorStatus();
                    displayTheButtons();
                }
            }
            else
            {
                displayConnectionProblem();
                displayTheButtons();
            }
        }

        private void btn_exit_Click()
        {
            this.NavigationService.Navigate(new _1_Idle());
            cart_port.Close();
        }

        private void btn_category_back_Click()
        {
            this.NavigationService.Navigate(previous_page);
            cart_port.Close();
        }
    }
}
