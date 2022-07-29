using System.Windows.Controls;


namespace sample2.User_Controls
{
    /// <summary>
    /// Interaction logic for product_item.xaml
    /// </summary>
    public partial class product_item : UserControl
    {
        

        public product_item()
        {
            InitializeComponent();
            btn_sub.text_button.Text = "-";
            btn_add.text_button.Text = "+";
        }
          
    }

}
