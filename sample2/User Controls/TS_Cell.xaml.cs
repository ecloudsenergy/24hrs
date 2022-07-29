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
    /// Interaction logic for TS_Cell.xaml
    /// </summary>
    public partial class TS_Cell : UserControl
    {
        public TS_Cell()
        {
            InitializeComponent();
            Grid grid_box = this.TTtextbox;
        }
    }
}
