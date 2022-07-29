using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace sample2.User_Controls
{
    /// <summary>
    /// Interaction logic for cart_item.xaml
    /// </summary>
    public partial class cart_item : UserControl
    {
        public cart_item()
        {
            InitializeComponent();
            btn_cart_sub.text_button.Text = "-";
            btn_cart_add.text_button.Text = "+";
        }

        public cart_item(cart_item cartItem)
        {
            InitializeComponent();
            this.Product_Name.Text = cartItem.Product_Name.Text;
            this.Product_price.Text = cartItem.Product_price.Text;
            this.Product_quantity.Text = cartItem.Product_quantity.Text;
            
        }
    }
}
