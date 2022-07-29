using sample2.User_Controls;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace sample2
{
    /// <summary>
    /// Interaction logic for _14_Payment.xaml
    /// </summary>
    public partial class _14_Payment : Page
    {
        _2_Product_Selection previous_page;
        List<cart_item> cart_items;
        public _14_Payment(_2_Product_Selection page, List<cart_item> cart_items, string order_amount, string order_quantity)
        {
            InitializeComponent();
            btn_exit.text_button.Text = "Exit";
            btn_exit.gotoPage = btn_exit_Click;
            btn_category_back.text_button.Text = "Back";
            btn_category_back.gotoPage = btn_category_back_Click;
            btn_reset.text_button.Text = "Reset";
            btn_reset.gotoPage = btn_reset_Click;

            upi_pay.text_button.Text = "UPI Payment";
            //card_pay.text_button.Text = "Credit / Debit Card Payment";
            cash_pay.text_button.Text = "Cash Payment";

            upi_pay.image.Width = 100;
           // card_pay.image.Width = 100;
            cash_pay.image.Width = 90;

            upi_pay.image.Source = new BitmapImage(new Uri("/images/UPI.png", UriKind.Relative));
           // card_pay.image.Source = new BitmapImage(new Uri("/images/credit.png", UriKind.Relative));
            cash_pay.image.Source = new BitmapImage(new Uri("/images/cash.png", UriKind.Relative));

            upi_pay.gotoPage = btn_upi_payment;
         //   card_pay.gotoPage = btn_card_payment;
            cash_pay.gotoPage = btn_cash_payment;

            previous_page = page;
            this.cart_items = cart_items;
            this.order_amount.Text = order_amount;
            this.order_quantity.Text = order_quantity;
            foreach (cart_item item in cart_items)
            {
                item.btn_cart_add.Visibility = System.Windows.Visibility.Hidden;
                item.btn_cart_sub.Visibility = System.Windows.Visibility.Hidden;
                cart_item addItem = new cart_item(item);
                this.cart.Children.Add(addItem);
            }
        }


        private void btn_category_back_Click()
        {
            foreach (var item in cart_items)
            {
                item.btn_cart_add.Visibility = System.Windows.Visibility.Visible;
                item.btn_cart_sub.Visibility = System.Windows.Visibility.Visible;
            }
            this.NavigationService.Navigate(previous_page);
        }
        private void btn_exit_Click()
        {
            this.NavigationService.Navigate(new _1_Idle());
        }
        private void btn_reset_Click()
        {
            this.NavigationService.Navigate(new _2_Product_Selection());
        }

      /*  private void btn_card_payment()
        {
            this.NavigationService.Navigate(new _3_Card_Payment(this, cart_items, this.order_amount.Text, this.order_quantity.Text));
        } */

        private void btn_cash_payment()
        {
            cash_pay.IsEnabled = false;
            this.NavigationService.Navigate(new _16_Cash_Payment(this, cart_items, this.order_amount.Text, this.order_quantity.Text));
            cash_pay.IsEnabled = true;
        } 
        private void btn_upi_payment()
        {
            upi_pay.IsEnabled = false;
            this.NavigationService.Navigate(new _20_UPI_Payment(this, cart_items, this.order_amount.Text, this.order_quantity.Text));
            upi_pay.IsEnabled = true;
        }

        private void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            
            e.Handled = true;
        }
    }
}
