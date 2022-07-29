
using sample2.User_Controls;
using sample2.viewModel;
using System.Collections.Generic;

using System.Windows.Controls;
using System.Windows.Input;


namespace sample2
{
    /// <summary>
    /// Interaction logic for _18_Vending_Screen.xaml
    /// </summary>
    public partial class _18_Vending_Screen_copy : Page
    {

        VendScreenViewModel viewModel;

        public _18_Vending_Screen_copy(Page previous_page, List<cart_item> cart_items, string order_amount,
            string order_quantity, string received_amount, string class_name, string bill_number)
        {
            InitializeComponent();
            viewModel = new VendScreenViewModel(previous_page, cart_items, order_amount,
            order_quantity, received_amount, class_name, bill_number, this);
            this.DataContext = viewModel;
        }

        private void btn_exit_Click(object sender, TouchEventArgs e)
        {

        }

        private void btn_category_back_Click(object sender, TouchEventArgs e)
        {

        }
    }
}
