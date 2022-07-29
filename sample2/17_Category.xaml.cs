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
using Microsoft.Win32;
using sample2.remote;
using sample2.models;
using ToastNotifications;
using ToastNotifications.Position;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;

namespace sample2
{
    /// <summary>
    /// Interaction logic for _17_Category.xaml
    /// </summary>
    public partial class _17_Category : Page
    {
        private _4_Product_Entry _4_Product_Entry;
        string fn = "";
        CategoryModel category = new CategoryModel();
        Notifier noti = new Notifier(cfg =>
        {
            cfg.PositionProvider = new WindowPositionProvider(
                parentWindow: Application.Current.MainWindow,
                corner: Corner.BottomCenter,
                offsetX: 10,
                offsetY: 10);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(2),
                maximumNotificationCount: MaximumNotificationCount.FromCount(5));

            cfg.Dispatcher = Application.Current.Dispatcher;
        });



        public _17_Category(_4_Product_Entry _4_Product_Entry)
        {
            InitializeComponent();
            this._4_Product_Entry = _4_Product_Entry;
            refresh_categories();
        }

        public void refresh_categories()
        {
            this.CategoryNametxt.Items.Clear();
           
            List<string> CtNames = SqliteChange.getCategories();
            foreach (string CtName in CtNames)
                this.CategoryNametxt.Items.Add(CtName);
        }

        private void Uploadimage_Click(object sender, RoutedEventArgs e)
        {
            {
                OpenFileDialog open = new OpenFileDialog();
                open.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
                
                if (open.ShowDialog() == true)
                {
                    fn = open.FileName;
                    imagePhoto.Source = new BitmapImage(new Uri(open.FileName));
                }


            }
        } 

        private void Back(object sender, RoutedEventArgs e)
        {
            _4_Product_Entry.refresh_categories();
            this.NavigationService.Navigate(_4_Product_Entry);
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            int success = 0;
            if (this.CategoryNametxt.Text != "" || this.imagePhoto.Source != null)
                success = SqliteChange.insertCategory(this.CategoryNametxt.Text, fn, category.CT_Image);
              

            if (success == 1)
            {
                this.imagePhoto.Source = null;
                this.CategoryNametxt.Text = "";
                refresh_categories();
            }

        }

        private void CategoryName_TextChanged(object sender, RoutedEventArgs e)
        {
            
            if (this.CategoryNametxt.Text != "")
            {
                int CategoryNameAvailability = 0;
                string CategoryName = this.CategoryNametxt.Text;
                this.CategoryNametxt.IsDropDownOpen = true;
                CategoryNameAvailability = SqliteChange.CategoryNamecombo(CategoryName);
                
                if (CategoryNameAvailability != 0)
                {
                    category = SqliteChange.getCategoryDetails(CategoryName);
                    this.CategoryNametxt.Text = "" + category.CT_Name;
                    imagePhoto.Source = SqliteChange.byteArrayToImage(category.CT_Image);
                    this.btn_Delete.Visibility = Visibility.Visible;
                }
                else
                {
                    this.btn_Delete.Visibility = Visibility.Hidden;
                    imagePhoto.Source = null;
                    fn = "";
                }
               
            }
           else
            {
                this.CategoryNametxt.Text = "";
                imagePhoto.Source = null;
                fn = "";
                this.btn_Delete.Visibility = Visibility.Hidden;
            }

            

        }

      

        private void Delete(object sender, RoutedEventArgs e)
        {
            int success = SqliteChange.deleteEntryInCategoryTable(this.CategoryNametxt.Text);
            if (success > 0)
            {
                noti.ShowSuccess("Successfully Deleted!");
                refresh_categories();
            }
            else
            {
                noti.ShowError("Something went wrong! Please contact Admin!");
            }
        }
    }
}
