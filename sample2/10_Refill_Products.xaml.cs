using sample2.User_Controls;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using sample2.remote;
using sample2.models;

namespace sample2
{
    /// <summary>
    /// Interaction logic for _10_Refill_Products.xaml
    /// </summary>
    public partial class _10_Refill_Products : Page
    {
        private _6_Menu _6_Menu;

        public _10_Refill_Products(_6_Menu _6_Menu)
        {
            InitializeComponent();
            this._6_Menu = _6_Menu;
            Grid grid_box = this.gridbox;
            

        List<CellModel> tray= SqliteChange.getTrayDetails();
            CellModel cel = new CellModel();

            int m = 1;
            for (int i = 2; i < 18; i = i + 2)
            {
                int n = 1;
                Refill_btn columns = new Refill_btn();
                Grid.SetRow(columns, i);              

                for (int j = 1; j < 17; j = j + 2)
                {
                    textBlock btngrid = new textBlock();
                    Grid.SetColumn(btngrid, j);

                    if (tray.Exists(t => (t.CT_Col_No == n) && (t.CT_Row_No == m)))
                    {
                        btngrid.product_name.Visibility = Visibility.Visible;
                        cel = SqliteChange.getPrductofCell(m, n);
                        if (cel.CT_Enable_Tag == 1)
                        {
                            btngrid.ColNum.Text = cel.CT_Col_No.ToString();
                            btngrid.RowNum.Text = cel.CT_Row_No.ToString();
                            btngrid.product_name.Text = cel.CT_Product_name;
                            btngrid.parent.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            btngrid.parent.Visibility = Visibility.Visible;
                        }                        
                    }
                    columns.textbox.Children.Add(btngrid);
                    n++;
                }
                grid_box.Children.Add(columns);
                m++;
               
            }
        }

        private void Cancel_GoBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(_6_Menu);
        }
    }
}
