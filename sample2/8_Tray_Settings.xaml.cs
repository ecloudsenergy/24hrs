using sample2.User_Controls;
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
using sample2.remote;
using sample2.models;

namespace sample2
{
   
    /// <summary>
    /// Interaction logic for _8_Tray_Settings.xaml
    /// </summary>
    public partial class _8_Tray_Settings : Page
    {
        private _6_Menu _6_Menu;

        public _8_Tray_Settings(_6_Menu _6_Menu)
        {
            InitializeComponent();
            this._6_Menu = _6_Menu;
            Grid grid_box = this.TTgridbox;



            List<CellModel> tray = SqliteChange.getTrayDetails();
            CellModel cel = new CellModel();

            int m = 1;
            for (int i = 2; i < 18; i = i + 2)
            {
                int n = 1;
                TS_Cell columns = new TS_Cell();

                Grid.SetRow(columns, i);

                for (int j = 1; j < 17; j = j + 2)
                {
                    TS_TextBlock btngrid = new TS_TextBlock(m, n, this);
                    Grid.SetColumn(btngrid, j);

                    if (tray.Exists(t => (t.CT_Col_No == n) && (t.CT_Row_No == m)))
                    {
                        CellModel cell = tray.Single(t => (t.CT_Col_No == n) && (t.CT_Row_No == m));
                        btngrid.g.Visibility = Visibility.Visible;
                        cel = SqliteChange.getPrductofCell(m, n);
                        btngrid.ColNum.Text = n.ToString();
                        btngrid.RowNum.Text = m.ToString();
                        if (cel.CT_Enable_Tag == 1)
                        {
                            
                            btngrid.g.Text = cel.CT_Product_name;
                            
                        }
                        else
                        {
                            btngrid.g.Foreground = Brushes.Red;
                            btngrid.g.Text = "R"+m.ToString()+" "+"C"+  n.ToString() ;
                        }
                    }
                    columns.TTtextbox.Children.Add(btngrid);
                    n++;
                }
                grid_box.Children.Add(columns);
                m++;

            }

            
        }

        public void refresh()
        {
            this.NavigationService.Navigate(new _8_Tray_Settings(_6_Menu));
        }


        private void Cancel_GoBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(_6_Menu);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            refresh();
        }
    }
}
