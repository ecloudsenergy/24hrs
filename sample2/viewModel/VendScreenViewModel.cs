using sample2.models;
using sample2.User_Controls;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using sample2.helpers;
using System.ComponentModel;
using System.Windows;

namespace sample2.viewModel
{
    public class VendScreenViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public VendScreenModel model = new VendScreenModel();
        public VendScreenViewModel(Page previous_page, List<cart_item> cart_items, string order_amount,
            string order_quantity, string received_amount, string class_name, string bill_number, Page current_page)
        {
            //Intializing all the devices - PLC, Note Dispenser and Coin Dispenser
          
            
            this.txtOrderAmt = order_amount;
            this.txtOrderQty = order_quantity;
            this.txtAmtPaid = received_amount;
            this.items = cart_items;
            this.className = class_name;
            this.currentPage = current_page;
            this.bill_number = bill_number;
            txtVendStatusFontSize = 24;
            

            Port_BackgroundWorker backgroundWorker = new Port_BackgroundWorker(this);
            
            //Converting the amount paid and bill_amount from text to int
            int amt_paid = Int32.Parse(routines.split_amount(this.txtAmtPaid));
            int bill_amount = Int32.Parse(routines.split_amount(txtOrderAmt));

            //finding total value need to be returned
            int total = amt_paid - bill_amount;
            

        }

        public string txtProductName
        {
            get { return model.Product_name; }
            set { model.Product_name = value; OnPropertyChanged("txtProductName"); }
        }
        public string txtVendStatus
        {
            get { return model.VendScreenStatus; }
            set { model.VendScreenStatus = value; OnPropertyChanged("txtVendStatus"); }
        }
        public string txtOrderAmt
        {
            get { return "Rs. "+ model.Order_amt.ToString(); }
            set
            {
                string[] split = value.Split(' ');
                model.Order_amt = int.Parse(split[1]);
                OnPropertyChanged("txtOrderAmt");
            }
        }
        public string txtAmtPaid
        {
            get { return "Rs. " + model.Amt_paid.ToString(); }
            set
            {
                string[] split = value.Split(' ');
                model.Amt_paid = int.Parse(split[1]);
                OnPropertyChanged("txtAmtPaid");
            }
        }
        public string txtOrderQty
        {
            get { return model.Order_qty.ToString(); }
            set { model.Order_qty = int.Parse(value);
                OnPropertyChanged("txtOrderQty");
            }
        }

        public string className { get; set; }
        public string bill_number { get; set; }
        public Page currentPage { get; set; }

        private BitmapImage _imgProduct;
        public BitmapImage imgProduct
        {
            get { return _imgProduct; }
            set { _imgProduct = value; OnPropertyChanged("imgProduct"); }
        }

        public string txtItemOrderQty
        {
            get { return model.Item_Order_qty.ToString(); }
            set { model.Item_Order_qty = int.Parse(value); OnPropertyChanged("txtItemOrderQty"); }
        }
        public string txtDispensingQty
        {
            get { return model.Dispensed_qty.ToString(); }
            set { model.Dispensed_qty = int.Parse(value); OnPropertyChanged("txtDispensedQty"); }
        }

        private int _txtVendStatusFontSize;

        public int txtVendStatusFontSize
        {
            get { return _txtVendStatusFontSize; }
            set { _txtVendStatusFontSize = value; OnPropertyChanged("txtVendStatusFontSize"); }
        }



        public List<cart_item> items
        {
            get { return model.Item_Details; }
            set
            {
                model.Item_Details = value;
            }
        }
    }
}
