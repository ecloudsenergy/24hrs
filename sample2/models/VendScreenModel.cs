using sample2.User_Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace sample2.models
{
    public class VendScreenModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private string _VendScreenStatus;
        public string VendScreenStatus
        {
            get { return _VendScreenStatus; }
            set { _VendScreenStatus = value; OnPropertyChanged("VendScreenStatus"); }
        }

        private string _Product_name;
        public string Product_name
        {
            get { return _Product_name; }
            set { _Product_name = value; OnPropertyChanged("Product_name"); } 
        }

        private int _Order_qty;
        public int Order_qty
        {
            get { return _Order_qty; }
            set { _Order_qty = value; OnPropertyChanged("Order_qty"); }
        }

        private int _Order_amt;
        public int Order_amt
        {
            get { return _Order_amt; }
            set { _Order_amt = value; OnPropertyChanged("Order_amt"); }
        }

        private int _Amt_paid;
        public int Amt_paid
        {
            get { return _Amt_paid; }
            set { _Amt_paid = value; OnPropertyChanged("Amt_paid"); }
        }


        private int _Item_Order_qty;
        public int Item_Order_qty
        {
            get { return _Item_Order_qty; }
            set { _Item_Order_qty = value; OnPropertyChanged("Item_Order_qty"); } 
        }

        private int _Dispensed_qty;
        public int Dispensed_qty
        {
            get { return _Dispensed_qty; }
            set { _Dispensed_qty = value; OnPropertyChanged("Dispensed_qty"); }
        }

        private List<cart_item> _Item_Details;
        public List<cart_item> Item_Details
        {
            get { return _Item_Details; }
            set { _Item_Details = value; OnPropertyChanged("Item_Details"); }
        }

       
    }
}
