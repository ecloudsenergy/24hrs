using sample2.models;
using sample2.remote;
using sample2.User_Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace sample2.reports
{
    /// <summary>
    /// Interaction logic for Daily_Report.xaml
    /// </summary>
    public partial class Daily_Report : Window
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
       
        DateTime selectedDate = new DateTime();

        public Daily_Report(DateTime selectedDate)
        {
            InitializeComponent();
            this.selectedDate = selectedDate;
            DateTime fromTime = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, 0, 0, 0);
            DateTime toTime = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, 23, 59, 59);
            string fromTimeString = fromTime.ToString("yyyy-MM-dd HH:mm:ss");
            string toTimeString = toTime.ToString("yyyy-MM-dd HH:mm:ss");
            date.Text = selectedDate.ToString("dd-MM-yyyy");
            List<ProductTransactionModel> transDetails = Sqlitedatavr.getTransactionDetails(fromTimeString, toTimeString);
           
            if (transDetails.Count > 0)
            {
                addProduct(getDistinctProducts(transDetails));
            }
            else
            {
                TextBlock no_records = new TextBlock();
                no_records.FontSize = 16;
                no_records.Text = "No Products Sold.";
                no_records.Foreground = new SolidColorBrush(Colors.Black);
                product_list.Children.Add(no_records);
            }
            List<int> Denominations = new List<int> { 10, 20, 50, 100, 200 };
            foreach (var denom in Denominations)
            {
                List<CurrencyTransactionModel> BillAcceptorTransactionDetails = Sqlitedatavr.getCurrencyDetails(fromTimeString, toTimeString, denom, "BA");
                BACurrencyOnBillModel currencyDenom = new BACurrencyOnBillModel();
                currencyDenom.Cr_Denomination = denom;
                currencyDenom.Cr_Transaction_Qty = 0;

                foreach (var currencyDetail in BillAcceptorTransactionDetails)
                {
                    currencyDenom.Cr_Transaction_Qty += currencyDetail.Cr_Transaction_Qty;
                }
                TextBlock QtyTextblock = (TextBlock)this.FindName("in_qty_" + denom);
                QtyTextblock.Text = "" + currencyDenom.Cr_Transaction_Qty;
            }
            total_cash.Text = "" + Sqlitedatavr.getCollectedAmount(fromTimeString, toTimeString, "Cash");
            total_UPI.Text = "" + Sqlitedatavr.getCollectedAmount(fromTimeString, toTimeString, "UPI");
            //TODO: add coin and cash dispensing quantity.
            print("Daily Report - " + selectedDate.ToString("dd-MM-yyyy"));
        }

        public List<ProductTransactionModel> getDistinctProducts(List<ProductTransactionModel> transactionDetails)
        {
            List<ProductTransactionModel> distinctArrangement = new List<ProductTransactionModel>();
            transactionDetails.OrderBy(x => x.CTT_Product_Name);
            foreach (var transaction in transactionDetails)
            {
               if(distinctArrangement.Exists(x => x.CTT_Product_Name == transaction.CTT_Product_Name 
               && x.CTT_Transaction_Price == transaction.CTT_Transaction_Price))
                {
                    ProductTransactionModel productDetail = distinctArrangement.Find(x => x.CTT_Product_Name == transaction.CTT_Product_Name
                    && x.CTT_Transaction_Price == transaction.CTT_Transaction_Price);
                    int index = distinctArrangement.IndexOf(productDetail);
                    productDetail.CTT_Transaction_Qty = productDetail.CTT_Transaction_Qty + transaction.CTT_Transaction_Qty;
                    productDetail.CTT_Transaction_Total_Amount = productDetail.CTT_Transaction_Total_Amount 
                        + transaction.CTT_Transaction_Total_Amount;
                    distinctArrangement[index] = productDetail;
                }
               else
                {
                    distinctArrangement.Add(transaction);
                }
            }
            return distinctArrangement;
        }

        public void addProduct(List<ProductTransactionModel> distinctProductDetails)
        {
            daily_report_item header = new daily_report_item();
            header.line_visibility.Visibility = Visibility.Visible;
            header.line_visibility_top.Visibility = Visibility.Visible;
            product_list.Children.Add(header);
            foreach (var productDetail in distinctProductDetails)
            {
                daily_report_item productDetails = new daily_report_item();
                productDetails.productName.Text = productDetail.CTT_Product_Name;
                productDetails.soldQty.Text = productDetail.CTT_Transaction_Qty.ToString();
                productDetails.Rate.Text = productDetail.CTT_Transaction_Price.ToString();
                productDetails.Total.Text = productDetail.CTT_Transaction_Total_Amount.ToString();
                product_list.Children.Add(productDetails);
            }
            
        }

        public void print(string reportName)
        {
            try
            {
                PrintServer ps = new PrintServer();
                PrintDialog pr = new PrintDialog();
                pr.PrintQueue = ps.GetPrintQueue(@"" + SqliteDataAccess.getPort("printer"));
                pr.PrintVisual(daily_report, reportName);
                // if (ps.BeepEnabled)
                //TODO: send alert and log details into errors table
            }
            catch (Exception m)
            {
                MessageBox.Show(m.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



    }

    public class BACurrencyOnBillModel
    {
        public int Cr_Denomination { get; set; }
        public int Cr_Transaction_Qty { get; set; }
    }

}
