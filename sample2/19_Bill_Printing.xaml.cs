using sample2.helpers;
using sample2.models;
using sample2.remote;
using sample2.User_Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Threading;

namespace sample2
{
    /// <summary>
    /// Interaction logic for _19_Bill_Printing.xaml
    /// </summary>
    public partial class _19_Bill_Printing : Page
    {
        string bill_number = "";
       
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        string MachineMode = "", error = "";

        int timerCount = 0;
        DispatcherTimer dt = new DispatcherTimer();

        public _19_Bill_Printing(string error = "")
        {
            this.error = error;
            InitializeComponent();
        }

        public void Bill_Printing(List<product_count_ledger> product_ledger, string bill_number)
        {
            dt.Interval = new TimeSpan(0, 0, 1);
            this.bill_number = bill_number;
            List<BillProduct> item_bills = new List<BillProduct>();
            MachineMode = Sqlitedatavr.getMachineMode();
            foreach (product_count_ledger product in product_ledger)
            {
                ProductModel product_details = SqliteDataAccess.getProductDetails(product.Pr_Name);
                BillProduct bill_product = new BillProduct();
                bill_product.Pr_Name = product_details.Pr_Name;
                bill_product.Pr_Qty = product.Pr_Qty;
                bill_product.Pr_Rate = double.Parse(product_details.Pr_Selling_Price);
                bill_product.Pr_HSN = product_details.Pr_HSN;
                bill_product.Pr_SGST = double.Parse(product_details.Pr_SGST);
                bill_product.Pr_CGST = double.Parse(product_details.Pr_CGST);
                bill_product.Pr_IGST = double.Parse(product_details.Pr_IGST);
                item_bills.Add(bill_product);
            }

            BillPrint print_format = new BillPrint();

            print_format.AddColumnText("Date & Time:", 1);
            print_format.AddColumnText("" + indianTime.ToShortDateString() + " " + indianTime.ToShortTimeString(), 2);
            print_format.AddColumnText("Invoice No:", 1);
            print_format.AddColumnText(bill_number, 2); // should check with database
            Bill_NumOfItems item = new Bill_NumOfItems();
            print_format.items.Children.Add(item);

            foreach (BillProduct product in item_bills)
            {
                print_format.AddItem(product.Pr_Name, product.Pr_HSN, product.Pr_Rate, product.Pr_Qty,
                    product.Pr_SGST, product.Pr_CGST, product.Pr_IGST);
            }

            List<TaxModel> SGST_value = new List<TaxModel>(), CGST_value = new List<TaxModel>(), IGST_value = new List<TaxModel>();

            foreach (BillProduct bill_item in item_bills)
            {
                double SGST = bill_item.Pr_SGST;
                double IGST = bill_item.Pr_IGST;
                double CGST = bill_item.Pr_CGST;
                if (SGST != 0) tax_caculation(SGST_value, bill_item.Pr_IGST, bill_item.Pr_SGST, bill_item.Pr_CGST, bill_item.Pr_Rate, bill_item.Pr_Qty, bill_item.Pr_SGST);
                if (IGST != 0) tax_caculation(IGST_value, bill_item.Pr_IGST, bill_item.Pr_SGST, bill_item.Pr_CGST, bill_item.Pr_Rate, bill_item.Pr_Qty, bill_item.Pr_IGST);
                if (CGST != 0) tax_caculation(CGST_value, bill_item.Pr_IGST, bill_item.Pr_SGST, bill_item.Pr_CGST, bill_item.Pr_Rate, bill_item.Pr_Qty, bill_item.Pr_CGST);
            }

            Tax_format(SGST_value, print_format, "SGST");
            Tax_format(CGST_value, print_format, "CGST");
            Tax_format(IGST_value, print_format, "IGST");
            double sub_total = 0, Total_Ex_Tax = 0, Grand_Total = 0;

            foreach (TaxModel tax_mod in SGST_value) Total_Ex_Tax += tax_mod.Total_Value;
            foreach (TaxModel tax_mod in CGST_value) Total_Ex_Tax += tax_mod.Total_Value;
            foreach (TaxModel tax_mod in IGST_value) Total_Ex_Tax += tax_mod.Total_Value;


            foreach (BillProduct bill_item in item_bills)
            {
                sub_total += bill_item.Pr_Rate * bill_item.Pr_Qty;
            }

            sub_total = print_format.roundoff(sub_total);
            Total_Ex_Tax = print_format.roundoff(Total_Ex_Tax);

            Grand_Total = sub_total;
            int temp = (int)Grand_Total;
            Grand_Total = print_format.roundoff(temp);

            print_format.Total_Ex_Tax.Text = print_format.displayRupee(Total_Ex_Tax);
            print_format.Grand_Total.Text = print_format.displayRupee(Grand_Total);

            
                print_format.print(bill_number);
            //Thread.Sleep(2000);
            timerStarted();


        }


        private void timerStopped()
        {
            dt.Stop();
            timerCount = 0;
        }


        private void timerStarted()
        {
            dt.Tick += delayCounter;
            dt.Start();

        }

        private void delayCounter(object sender, EventArgs e)
        {
            timerCount++;
            if (timerCount == 3)
            {
                timerStopped();
                if (error.Length == 0)
                    this.NavigationService.Navigate(new _1_Idle());
                else
                    this.NavigationService.Navigate(new _0_Initialization(error));
            }
        }


        void Tax_format(List<TaxModel> tax_mod, BillPrint print_format, string mod_name)
        {
            if (tax_mod.Count > 0)
            {
                foreach (TaxModel model in tax_mod)
                {
                    print_format.AddColumnText(mod_name + " " +  model.Tax_Rate + "%", 3);
                    print_format.AddColumnText(print_format.displayRupee( print_format.roundoff(model.Total_Value)), 4);
                }
            }

        }

       
        void tax_caculation(List<TaxModel> tax_model, double IGST, double SGST, double CGST, double pr_rate, double pr_Qty, double tax_rate)
        {

            if (tax_model.Exists(t => t.Tax_Rate == tax_rate))
            {
                TaxModel tax_mod = tax_model.Single(t => t.Tax_Rate == tax_rate);
                int index = tax_model.IndexOf(tax_mod);
                double principal_amount = (pr_rate / (100 + IGST + SGST + CGST)) * 100;
                tax_mod.Total_Value += principal_amount * (tax_rate / 100) * pr_Qty;
                tax_model[index] = tax_mod;
            }
            else
            {
                TaxModel tax_mod = new TaxModel();
                tax_mod.Tax_Rate = tax_rate;
                double principal_amount = (pr_rate / (100 +IGST + SGST + CGST)) * 100;
                tax_mod.Total_Value = principal_amount * (tax_rate/100) * pr_Qty;
                tax_model.Add(tax_mod);
            }

        }
    }
 }
