using sample2.User_Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using ToastNotifications;
using ToastNotifications.Position;
using ToastNotifications.Lifetime;
using System.Windows;
using System.Windows.Controls;
using sample2.remote;
using System.IO.Ports;
using System.Windows.Threading;
using System.Threading;
using sample2.models;
using sample2.helpers;
using sample2.windows;
using System.Windows.Input;

namespace sample2
{
    /// <summary>
    /// Interaction logic for _16_Cash_Payment.xaml
    /// </summary>
    public partial class _16_Cash_Payment : Page
    {
        _14_Payment previous_page;
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        string recieved_data; List<int> denominations = new List<int>();

        SerialPort cart_port = new SerialPort(SqliteDataAccess.getPort("BA"), 9600);
        List<cart_item> cart_items;
        int timerCount = 0;
        DispatcherTimer dt = new DispatcherTimer();
        bool starting_reset = false, close_port = false;
        string machineMode = Sqlitedatavr.getMachineMode();
        int coin_amt = 0;

        Notifier noti = new Notifier(cfg =>
        {
            cfg.PositionProvider = new WindowPositionProvider(
                parentWindow: Application.Current.MainWindow,
                corner: Corner.BottomCenter,
                offsetX: 10,
                offsetY: 10);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(3),
                maximumNotificationCount: MaximumNotificationCount.FromCount(5));

            cfg.Dispatcher = Application.Current.Dispatcher;
        });

        public _16_Cash_Payment(_14_Payment previous_page, List<cart_item> cart_items, string order_amount, string order_quantity)
        {
            InitializeComponent();
            btn_exit.text_button.Text = "Exit";
            btn_exit.gotoPage = btn_exit_Click;
            btn_category_back.text_button.Text = "Back";
            btn_category_back.gotoPage = btn_category_back_Click;
            btn_reset.text_button.Text = "Reset Device";
            btn_reset.gotoPage = btn_reset_device_Click;
            dt.Interval = new TimeSpan(0, 0, 1);
            this.cart_items = cart_items;
            string MachineMode = Sqlitedatavr.getMachineMode();

            this.btn_category_back.Visibility = Visibility.Visible;
            this.btn_exit.Visibility = Visibility.Visible;

            this.previous_page = previous_page;
            this.order_amount.Text = order_amount;
            this.order_quantity.Text = order_quantity;


            //if (MachineMode == "TEST")
            //{

            //    //this.NavigationService.Navigate(new _18_Vending_Screen(this, cart_items, this.order_amount.Text, this.order_quantity.Text, "Rs. 0", "upi", "test_bill"));
            //}
            //else
            //{
            cart_port.Handshake = Handshake.None;
            cart_port.Parity = Parity.Even;
            cart_port.DataBits = 8;
            cart_port.StopBits = StopBits.One;
            cart_port.ReadTimeout = 200;
            cart_port.WriteTimeout = 50;
            foreach (cart_item item in cart_items)
            {
                item.btn_cart_add.Visibility = Visibility.Hidden;
                item.btn_cart_sub.Visibility = Visibility.Hidden;
                cart_item addItem = new cart_item(item);
                this.cart.Children.Add(addItem);
            }

            cart_port.DataReceived += new SerialDataReceivedEventHandler(Recieve);
            errors.Text = "Please Wait...";
            first_reset();
            gif.Visibility = Visibility.Visible;
            denomination_grid.Visibility = Visibility.Visible;

            string[] split = order_amount.Split(' ');
            int order_amt = int.Parse(split[1]);
            coin_amt = order_amt % 10;



            timerStarted();
            //}
        }


        void make_buttons_invisible()
        {
            this.btn_category_back.Visibility = Visibility.Collapsed;
            this.btn_exit.Visibility = Visibility.Collapsed;
        }

        void first_reset()
        {
            if (starting_reset == false)
            {
                starting_reset = true;
                reset();
            }
        }

        private void btn_reset_device_Click()
        {
            reset();
            Thread.Sleep(2000);
            accept();
            errors.Text = "Please insert cash one by one...";
            

        }

        private void timerStopped()
        {
            dt.Stop();
            timerCount = 0;
            dt = new DispatcherTimer();
            dt.Interval = new TimeSpan(0, 0, 1);
        }


        private void timerStarted()
        {
            dt.Tick += delayCounter;
            dt.Start();

        }

        private void delayCounter(object sender, EventArgs e)
        {
            timerCount++;
            if (timerCount == 2)
            {
                accept();
                errors.Text = "Please insert cash one by one...";
                timerStopped();
                
            }
        }

        private void can_take_more_bills()
        {
            dt.Tick += can_take_more_bills_counter;
            dt.Start();

        }

        private void can_take_more_bills_counter(object sender, EventArgs e)
        {
            timerCount++;
            if (timerCount == 1)
            {
                errors.Text = "Please insert cash one by one...";
                timerStopped();
            }
        }

        private delegate void UpdateUiTextDelegate(string text);
        private void Recieve(object sender, SerialDataReceivedEventArgs e)
        {
            
            // Collecting the characters received to our 'buffer' (string).
            byte[] buf = new byte[10];
            cart_port.Read(buf, 0, 10);
            recieved_data = "";
            foreach (byte item in buf)
            {
                if (item != 0)
                    recieved_data += item.ToString("X2") + " ";
            }
            Dispatcher.BeginInvoke(DispatcherPriority.Send, new UpdateUiTextDelegate(WriteData), recieved_data);
        }
        void hold()
        {
            SerialCmdSend("18");
        }
        void Rupee(string _denomination)
        {
            hold();
            int incremented_denomination = 0;
            int denomination = Int32.Parse(_denomination);
            TextBlock deno_qty_textblock = FindName("deno_for_" + _denomination) as TextBlock;
            TextBlock amount_textblock = FindName("Amount_for_" + _denomination) as TextBlock;

            int total_amount = 0;
            if (this.total_amount.Text.Length > 0)
            { total_amount = Int32.Parse(this.total_amount.Text); }
            string[] splited_amount = this.order_amount.Text.Split(' ');

            if (balance_check((total_amount+denomination) - Int32.Parse(splited_amount[1])))
            {
                if (deno_qty_textblock.Text.Length > 0)
                { incremented_denomination = Int32.Parse(deno_qty_textblock.Text); }
                incremented_denomination++;
                deno_qty_textblock.Text = incremented_denomination.ToString();
                amount_textblock.Text = (incremented_denomination * denomination).ToString();
                total_amount += denomination;
                this.total_amount.Text = "" + total_amount;
               if (Int32.Parse(splited_amount[1]) <= total_amount)
                {
                    
                    close_port = true;
                    reset();
                    go_to_vending_screen();
                    timerStopped();
                }
            }
            else
            {
                reject();
                this.errors.Text = "No change. Please reduce amount or use different payment method.";
                can_take_more_bills();
            }
                
            
        }

        bool balance_check(int return_amount)
        {
            bool return_data = false;
            if (return_amount > 0)
            {
                int tens = return_amount / 10;
                int ones = return_amount % 10;
                if (tens <= SqliteChange.getBalance("ND", Sqlitedatavr.getBillDenomination()) && tens <= 6 && ones <= SqliteChange.getBalance("CD", Sqlitedatavr.getCoinDenomination()) && ones <= 9)
                {
                    accept();
                    return_data = true;
                }
               
            }
            else
            {
                accept();
                return_data = true;
            }


            return return_data;
        }

        void go_to_vending_screen()
        {
            int success = 0;
            string bill_number = SqliteDataAccess.getBillNumber();
            String formatdate = indianTime.ToString("yyyy-MM-dd HH:mm:ss"); //imthi 20-01-2021
            for (int i =0; i<cart_items.Count; i++)
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
                    
                    String formatteddate = expiry.ToString("yyyy-MM-dd HH:mm:ss");//imthi 20-01-2021

                    int Add_Quantity = int.Parse(this.cart_items[i].Product_quantity.Text);
                    int transaction_price = int.Parse(productDetails.Pr_Selling_Price);
                    int Total_transaction_price = int.Parse(total_trans_amount[1]);
                    float SGST = float.Parse(productDetails.Pr_SGST);
                    float CGST = float.Parse(productDetails.Pr_CGST);
                    float IGST = float.Parse(productDetails.Pr_IGST);


                    if (quantity_ordered_remaining <= lastTransDetails.CTT_Closing_Stock)
                    {
                        int current_closing_stock = lastTransDetails.CTT_Closing_Stock - quantity_ordered_remaining;
                        success += SqliteChange.InsertIntoTrayTransaction(
                        productCell.CT_Row_No, productCell.CT_Col_No, productDetails.Pr_Name, Add_Quantity, "Selling",
                                transaction_price, SGST, CGST, IGST, formatdate, "Cash", bill_number, lastTransDetails.CTT_Closing_Stock, current_closing_stock,  //imthi 20-01-2021
                       Total_transaction_price, "cash_paid", "Customer", formatteddate, "paid", machineMode);        //imthi 20-01-2021
                        SqliteChange.UpdateCellTable(productCell.CT_Col_No, productCell.CT_Row_No, lastTransDetails.CTT_Closing_Stock - quantity_ordered_remaining,
                                productCell.CT_Balance_Qty);
                        quantity_ordered_remaining = 0;
                    }
                    else
                    {
                        success += SqliteChange.InsertIntoTrayTransaction(
                        productCell.CT_Row_No, productCell.CT_Col_No, productDetails.Pr_Name, Add_Quantity, "Selling",
                                transaction_price, SGST, CGST, IGST,
                        formatdate, "Cash", bill_number, lastTransDetails.CTT_Closing_Stock, 0,  //imthi 20-01-2021
                        Total_transaction_price, "cash_paid", "Customer", formatteddate, "paid", machineMode);        //imthi 20-01-2021
                        SqliteChange.UpdateCellTable(productCell.CT_Col_No, productCell.CT_Row_No, 0, productCell.CT_Balance_Qty);
                        quantity_ordered_remaining = quantity_ordered_remaining - lastTransDetails.CTT_Closing_Stock;
                    }

                }

            }

            currencyInsertedUpdate(bill_number);

            this.NavigationService.Navigate(new _18_Vending_Screen_copy(this, cart_items, this.order_amount.Text, this.order_quantity.Text, "Rs. "+ this.total_amount.Text, "cash", bill_number));
        }

      


        private void WriteData(string text)
        {

            string denomination = "";
            int response_check = 0;
            if (starting_reset)
            {
                if (text.Contains("40")) { denomination = "5"; response_check = 1; }
                if (text.Contains("41")) { denomination = "10"; response_check = 1; }
                if (text.Contains("42")) { denomination = "20"; response_check = 1; }
                if (text.Contains("43")) { denomination = "50"; response_check = 1; }
                if (text.Contains("44")) { denomination = "100"; response_check = 1; }
                if (text.Contains("45")) { denomination = "500"; response_check = 1; }
                if (text.Contains("47")) { reject(); errors.Text = "2000 notes are not accepted!"; response_check = 1; can_take_more_bills(); }
                if (text.Contains("48")) { denomination = "200"; response_check = 1; }
                if (denomination.Length > 0)
                    Rupee(denomination);
                if (text.Contains("10")) { errors.Text = "Successfull"; response_check = 1; can_take_more_bills(); }
                if (text.Contains("11")) { errors.Text = "Not Successfull"; response_check = 1; can_take_more_bills(); }
                if (text.Contains("20") || text.Contains("21") || text.Contains("22") || text.Contains("23")
                    || text.Contains("24") || text.Contains("25") || text.Contains("26") || text.Contains("27")
                    || text.Contains("28"))
                {
                    errors.Text = "Tap on the Reset Device button!! Device Problem! Please Call " + SqliteDataAccess.getHelplineNumber()
                                    + " and Inform!"; reset(); response_check = 1;
                }
                if (text.Contains("29"))
                {
                    if (errors.Text != "500 notes are not accepted!" && errors.Text != "2000 notes are not accepted!"
                         && this.errors.Text != "No change. Please reduce amount or use different payment method.")
                        errors.Text = "Invalid Note! Retry or put valid amount.";
                    response_check = 1;
                    can_take_more_bills();
                }
                if (text.Contains("80") || text.Contains("8F") || text.Contains("2A") || text.Contains("2E") || text.Contains("2F")
                    || text.Contains("3E") || text.Contains("5E")) response_check = 1;
                if (response_check == 0)
                {
                    errors.Text = "Unknown Problem! Please Call " + SqliteDataAccess.getHelplineNumber() + " and Inform!";
                }
            }
        }
      

        public void SerialCmdSend(string data)
        {
            try
                {
                // Send the binary data out the port
                byte[] hexstring = new byte[1];
                hexstring[0] = StringToByteArray(data)[0];
                if(!cart_port.IsOpen)
                cart_port.Open();
                // hexstring[0] = byte.Parse("0x"+data);

                //There is a intermitant problem that I came across
                //If I write more than one byte in succesion without a 
                //delay the PIC i'm communicating with will Crash
                //I expect this id due to PC timing issues ad they are
                //not directley connected to the COM port the solution
                //Is a ver small 1 millisecound delay between chracters
                foreach (byte hexval in hexstring)
                    {
                        byte[] _hexval = new byte[] { hexval }; // need to convert byte to byte[] to write
                        cart_port.Write(_hexval, 0, 1);
                        Thread.Sleep(1);
                    }
                }
                catch (Exception ex)
                {
                    errors.Text = "Something Wrong with Cash Acceptor! Sorry for Inconvenience";
                }
            finally
            {
                if(close_port)
                cart_port.Close();
            }
            
           
        }
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        private void btn_exit_Click()
        {
            timerStopped();
            close_port = true;
            reset();
            currencyInsertedUpdate();
            this.NavigationService.Navigate(new _1_Idle());
        }


     
        private void btn_category_back_Click()
        {
            timerStopped();
            close_port = true;
            reset();
            currencyInsertedUpdate();
            this.NavigationService.Navigate(previous_page);
        }
        void reset()
        {
            SerialCmdSend("30");
        }
        void accept()
        {
            SerialCmdSend("02");
        }


        void goBackPage()
        {
            timerStopped();
            reset();
            cart_port.Close();
            close_port = true;
            this.NavigationService.GoBack();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
           /* if (coin_amt > 0)
            {
                TimeoutError popup = new TimeoutError("OK", "Go Back",
                  "No Coin Change available, if you are expecting coin change, tap on go back and please use UPI.",
                  "Critical Information");
                popup.gotoPage += goBackPage;
                popup.gotoPage += popup.Close;
                popup.ShowDialog();
            }*/
        }

        private void currencyInsertedUpdate(string bill_number = "cancelled")
        {

            String formatdate = indianTime.ToString("yyyy-MM-dd HH:mm:ss"); //imthi 20-01-2021
            routines.DenominationsCA(denominations);
            for (int i = 0; i < denominations.Count; i++)
            {

                TextBlock deno_qty_textblock = FindName("deno_for_" + denominations[i]) as TextBlock;
                if (deno_qty_textblock.Text.Length > 0)
                    if (int.Parse(deno_qty_textblock.Text) > 0)
                    {
                        int saved_successfully = 0;
                        CurrencyTransactionModel lastTransDetails = SqliteChange.getLastCurrencyTransaction(denominations[i], "BA");
                        saved_successfully += SqliteChange.InsertIntoCurrencyTransactionTable(denominations[i], bill_number, Int32.Parse(deno_qty_textblock.Text),
                                           formatdate, "Customer", "Credit", lastTransDetails.Cr_Closing_Balance_Qty,   //imthi 20-01-2021
                                           (lastTransDetails.Cr_Closing_Balance_Qty + Int32.Parse(deno_qty_textblock.Text)), "BA", "Completed", "Went Back");
                    }
            }
        }

        private void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            timerCount = 0;
            e.Handled = true;
        }

        void reject()
        {
            SerialCmdSend("0F");
        }
    }
}
