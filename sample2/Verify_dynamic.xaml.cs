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

namespace sample2
{
    /// <summary>
    /// Interaction logic for Verify_dynamic.xaml
    /// </summary>
    public partial class Verify_dynamic : UserControl
    {
        public Verify_dynamic()
        {
            InitializeComponent();
        }
        public Verify_dynamic(Verify_dynamic DYNAMIC)
        {
            InitializeComponent();
            this.Deno.Text = DYNAMIC.Deno.Text;
            this.Dbdqty.Text = DYNAMIC.Dbdqty.Text;
            this.Crdqty.Text = DYNAMIC.Crdqty.Text;
        }
    }
}
