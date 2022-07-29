using Newtonsoft.Json;
using sample2.models;
using sample2.remote;
using sample2.User_Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace sample2
{
    /// <summary>
    /// Interaction logic for UPI_Payment.xaml
    /// </summary>
    public partial class _20_UPI_Payment : Page
    {
        Page previous_page; List<cart_item> cart_items; bool IsQrGenerated = false, IsTransacting = false;
        UpiQrRequestModel qr_requestmodel = new UpiQrRequestModel();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        DispatcherTimer dt = new DispatcherTimer();
        UpiTransStatusRequestModel qr_TransStatusrequestmodel = new UpiTransStatusRequestModel();
        string machineMode = Sqlitedatavr.getMachineMode();

        public _20_UPI_Payment(Page previous_page, List<cart_item> cart_items, string order_amount, string order_quantity)
        {
            InitializeComponent();
            this.previous_page = previous_page;
            btn_exit.text_button.Text = "Exit";
            btn_exit.gotoPage = btn_exit_Click;
            btn_category_back.text_button.Text = "Back";
            btn_category_back.gotoPage = btn_category_back_Click;

            this.previous_page = previous_page;
            this.order_amount.Text = order_amount;
            this.order_quantity.Text = order_quantity;
            this.cart_items = cart_items;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


            foreach (cart_item item in cart_items)
            {
                cart_item addItem = new cart_item(item);
                item.btn_cart_add.Visibility = Visibility.Hidden;
                item.btn_cart_sub.Visibility = Visibility.Hidden;
                this.cart.Children.Add(addItem);
            }

            message_display.Text = "Please Wait....";
            string[] split = order_amount.Split(' ');
            InitializeQrRequestModel(split[1]);
            Apiresult();
            dt.Interval = new TimeSpan(0, 0, 0, 1);
            dt.Tick += transactionStatusCheck;
            dt.Start();

        }


        private void timerStop()
        {
            dt.Stop();
            dt = new DispatcherTimer();
        }
       

        private string PostQRrequest()
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://www.ezswype.in/EzSwypeApi/upi/dynamicqrcode/version2");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Timeout = 5000;
            try
            {
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = JsonConvert.SerializeObject(qr_requestmodel);

                    streamWriter.Write(json);
                }
                try
                {
                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        return result.ToString();
                    }
                }

                catch (Exception m)
                {
                    return m.Message;
                }
            }            
            catch (Exception m)
            {
                return m.Message;
            }

        }
        private string PostQRTransStatusrequest()
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://www.ezswype.in/EzSwypeApi/upi/txnstatus/version2");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(qr_TransStatusrequestmodel);

                streamWriter.Write(json);
            }
            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    return result.ToString();
                }
            }
            catch (Exception m)
            {
                return m.Message;
            }

        }

        // FOR eClouds Energy LLP
        //void InitializeQrRequestModel(string amnt = "2.0")
        //{
        //    qr_requestmodel.accessToken = "VR2bfFvpscwWXeQr3TtfYg==";
        //    qr_requestmodel.merchantId = "E01020101013449";
        //    qr_requestmodel.terminalId = "E0024729";
        //    qr_requestmodel.amount = amnt;
        //    qr_requestmodel.requestKey = "smzctc18A5NOsEVM1J9OjsrcK49xLMocKy+7ECnOTb8=";
        //    qr_requestmodel.qrRequestType = "DYNAMIC";
        //    qr_requestmodel.vpaId = "EzE0024729@CUB";
        //    qr_requestmodel.uniqueId = "CUBE0024729" + indianTime.Year + indianTime.Month + indianTime.Day + indianTime.Hour + indianTime.Minute + indianTime.Second + "332";
        //    qr_requestmodel.phoneNo = "7868020710";
        //    qr_requestmodel.currency = "INR";
        //}

            //24 HRS
        void InitializeQrRequestModel(string amnt = "2.0")
        {
            qr_requestmodel.accessToken = "5uWwK+yN9T9EFzIIeOeOjw==";
            qr_requestmodel.merchantId = "E01020101023996";
            qr_requestmodel.terminalId = "E0045828";
            qr_requestmodel.amount = amnt;
            qr_requestmodel.requestKey = "bFkFqKFQUlEkl9fUVh6fNmUGnkjb+4B+FBWafEWvMzU=";
            qr_requestmodel.qrRequestType = "DYNAMIC";
            qr_requestmodel.vpaId = "EzE0045828@CUB";
            qr_requestmodel.uniqueId = "CUBE0045828" + indianTime.Year + indianTime.Month + indianTime.Day + indianTime.Hour + indianTime.Minute + indianTime.Second + "332";
            qr_requestmodel.phoneNo = "7868020710";
            qr_requestmodel.currency = "INR";
        }



        void InitializeQrTransStatusRequestModel()
        {
            qr_TransStatusrequestmodel.accessToken = qr_requestmodel.accessToken;
            qr_TransStatusrequestmodel.merchantId = qr_requestmodel.merchantId;
            qr_TransStatusrequestmodel.terminalId = qr_requestmodel.terminalId;
            qr_TransStatusrequestmodel.amount = qr_requestmodel.amount;
            qr_TransStatusrequestmodel.requestKey = qr_requestmodel.requestKey;
            qr_TransStatusrequestmodel.qrRequestType = qr_requestmodel.qrRequestType;
            qr_TransStatusrequestmodel.vpaId = qr_requestmodel.vpaId;
            qr_TransStatusrequestmodel.uniqueId = qr_requestmodel.uniqueId;
        }

        private async void Apiresult()
        {
            dt.Stop();
            Task<string> task = new Task<string>(PostQRrequest);
            task.Start();
            Loading_gif.Visibility = Visibility.Visible;
            QR.Visibility = Visibility.Hidden;
            transaction_message_display.Visibility = Visibility.Hidden;
            string response = await task;
            try
            {
                UpiQrResponseModel responseQR = JsonConvert.DeserializeObject<UpiQrResponseModel>(response);
                QR.Source = new BitmapImage(new Uri(responseQR.result.qrCodeURL, UriKind.Absolute));

                Loading_gif.Visibility = Visibility.Hidden;
                QR.Visibility = Visibility.Visible;
                message_display.Text = "Scan the QR with your Payment App. Please wait after scanning for the process to complete.";
                IsQrGenerated = true;
            }
            catch
            {
                Loading_gif.Visibility = Visibility.Hidden;
                QR.Visibility = Visibility.Hidden;
                message_display.Text = "Connection Problem. Please contact the support: " + SqliteDataAccess.getHelplineNumber();
                transaction_message_display.Visibility = Visibility.Visible;
                transaction_message_display.Text = response;
                transaction_message_display.Foreground = new SolidColorBrush(Colors.Red);
            }
            dt.Start();
        }
        private async void ApiTransStatusresult()
        {
            dt.Stop();
            Task<string> task = new Task<string>(PostQRTransStatusrequest);
            task.Start();
            string response = await task;
            UpiTransStatusResponseModel responseQrTransStatus = new UpiTransStatusResponseModel();
            try
            {
                responseQrTransStatus = JsonConvert.DeserializeObject<UpiTransStatusResponseModel>(response);
                if (response.Contains("Success"))
                {
                    responseQrTransStatus = JsonConvert.DeserializeObject<UpiTransStatusResponseModel>(response);
                    transaction_message_display.Text = "Transaction Success!!";
                    transaction_message_display.Visibility = Visibility.Visible;
                    message_display.Text = "";
                    QR.Visibility = Visibility.Hidden;

                    int success = 0;
                    string bill_number = SqliteDataAccess.getBillNumber();
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

                            int Add_Quantity = int.Parse(this.cart_items[i].Product_quantity.Text);
                            int transaction_price = int.Parse(productDetails.Pr_Selling_Price);

                            int Total_transaction_price = int.Parse(total_trans_amount[1]);
                            float SGST = float.Parse(productDetails.Pr_SGST);
                            float CGST = float.Parse(productDetails.Pr_CGST);
                            float IGST = float.Parse(productDetails.Pr_IGST);


                            if (quantity_ordered_remaining <= lastTransDetails.CTT_Closing_Stock)
                            {
                                success += SqliteChange.InsertIntoTrayTransaction(
                                productCell.CT_Row_No, productCell.CT_Col_No, productDetails.Pr_Name, Add_Quantity, "Selling",
                                transaction_price, SGST, CGST, IGST, formatdate, "UPI", bill_number, lastTransDetails.CTT_Closing_Stock, lastTransDetails.CTT_Closing_Stock - quantity_ordered_remaining,  //imthi 20-01-2021
                                Total_transaction_price, "upi_paid", "Customer", formatteddate, "paid", machineMode, response);        //imthi 20-01-2021
                                SqliteChange.UpdateCellTable(productCell.CT_Col_No, productCell.CT_Row_No, lastTransDetails.CTT_Closing_Stock - quantity_ordered_remaining,
                                productCell.CT_Balance_Qty);
                                quantity_ordered_remaining = 0;
                            }
                            else
                            {
                                success += SqliteChange.InsertIntoTrayTransaction(
                                productCell.CT_Row_No, productCell.CT_Col_No, productDetails.Pr_Name, Add_Quantity, "Selling",
                                transaction_price, SGST, CGST, IGST, formatdate, "UPI", bill_number, lastTransDetails.CTT_Closing_Stock, 0,  //imthi 20-01-2021
                                Total_transaction_price, "upi_paid", "Customer", formatteddate, "paid", machineMode, response);        //imthi 20-01-2021
                                SqliteChange.UpdateCellTable(productCell.CT_Col_No, productCell.CT_Row_No, 0, productCell.CT_Balance_Qty);
                                quantity_ordered_remaining = quantity_ordered_remaining - lastTransDetails.CTT_Closing_Stock;
                            }

                        }
                    }
                    timerStop();
                    this.NavigationService.Navigate(new _18_Vending_Screen_copy(this, cart_items, this.order_amount.Text, this.order_quantity.Text, this.order_amount.Text, "upi", bill_number));
                }
                else dt.Start();
            }
            catch (Exception ex)
            {
                message_display.Text = ex.Message;
                message_display.FontSize = 12;
                message_display.Text += ". Contact Support: " + SqliteDataAccess.getHelplineNumber();
                Exception newException = new Exception("Response: " + response, ex);
                throw newException;
            }
            
            IsTransacting = false;
        }

        private void transactionStatusCheck(object sender, EventArgs e)
        {
            if (IsQrGenerated && !IsTransacting)
            {
                InitializeQrTransStatusRequestModel();
                IsTransacting = true;
                ApiTransStatusresult();
            }
        }

        private void btn_exit_Click()
        {
            timerStop();
            this.NavigationService.Navigate(new _1_Idle());
        }


        private void goto_next_screen_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            
            if (machineMode == "TEST")
            {
                int success = 0;
                string bill_number = SqliteDataAccess.getBillNumber();
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

                        int Add_Quantity = int.Parse(this.cart_items[i].Product_quantity.Text);
                        int transaction_price = int.Parse(productDetails.Pr_Selling_Price);

                        int Total_transaction_price = int.Parse(total_trans_amount[1]);
                        float SGST = float.Parse(productDetails.Pr_SGST);
                        float CGST = float.Parse(productDetails.Pr_CGST);
                        float IGST = float.Parse(productDetails.Pr_IGST);

                        if (quantity_ordered_remaining <= lastTransDetails.CTT_Closing_Stock)
                        {
                            success += SqliteChange.InsertIntoTrayTransaction(
                            productCell.CT_Row_No, productCell.CT_Col_No, productDetails.Pr_Name, Add_Quantity, "Selling",
                            transaction_price, SGST, CGST, IGST, formatdate, "UPI", bill_number, lastTransDetails.CTT_Closing_Stock, lastTransDetails.CTT_Closing_Stock - quantity_ordered_remaining,  //imthi 20-01-2021
                            Total_transaction_price, "upi_paid", "Customer", formatteddate, "paid", machineMode);        //imthi 20-01-2021
                            SqliteChange.UpdateCellTable(productCell.CT_Col_No, productCell.CT_Row_No, lastTransDetails.CTT_Closing_Stock - quantity_ordered_remaining,
                                productCell.CT_Balance_Qty);
                            quantity_ordered_remaining = 0;
                        }
                        else
                        {
                            success += SqliteChange.InsertIntoTrayTransaction(
                            productCell.CT_Row_No, productCell.CT_Col_No, productDetails.Pr_Name, Add_Quantity, "Selling",
                            transaction_price, SGST, CGST, IGST, formatdate, "UPI", bill_number, lastTransDetails.CTT_Closing_Stock, 0,  //imthi 20-01-2021
                            Total_transaction_price, "upi_paid", "Customer", formatteddate, "paid", machineMode);        //imthi 20-01-2021
                            SqliteChange.UpdateCellTable(productCell.CT_Col_No, productCell.CT_Row_No, 0, productCell.CT_Balance_Qty);
                            quantity_ordered_remaining = quantity_ordered_remaining - lastTransDetails.CTT_Closing_Stock;
                        }
                    }
                }

                timerStop();
                this.NavigationService.Navigate(new _18_Vending_Screen_copy(this, cart_items, this.order_amount.Text, this.order_quantity.Text, this.order_amount.Text, "upi", "test"));
            }
        }

        private void btn_category_back_Click()
        {
            timerStop();
            this.NavigationService.Navigate(previous_page);
        }

        ~_20_UPI_Payment()
        {
            dt.Stop();
            dt.IsEnabled = false;
        }


    }
}
