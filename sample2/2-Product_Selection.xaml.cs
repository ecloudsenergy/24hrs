﻿using sample2.models;
using sample2.remote;
using sample2.User_Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ToastNotifications;
using ToastNotifications.Position;
using ToastNotifications.Lifetime;


namespace sample2
{
    /// <summary>
    /// Interaction logic for _2_Product_Selection.xaml
    /// </summary>
    public partial class _2_Product_Selection : Page
    {

        DispatcherTimer dt = new DispatcherTimer();
        int timerCount = 0;

        List<ProductModel> products = SqliteDataAccess.getProducts();
        List<string> categories = new List<string>();
        List<List<ProductModel>> categorywiseProducts = new List<List<ProductModel>>();
        double selectionPanelWidth = 0;
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        string MachineMode = "";
        Notifier noti = new Notifier(cfg =>
        {
            cfg.PositionProvider = new WindowPositionProvider(
                parentWindow: Application.Current.MainWindow,
                corner: Corner.BottomCenter,
                offsetX: 10,
                offsetY: 10);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(3),
                maximumNotificationCount: MaximumNotificationCount.FromCount(5));

            cfg.Dispatcher = Application.Current.Dispatcher;
        });

        public _2_Product_Selection()
        {
            MachineMode = Sqlitedatavr.getMachineMode();
            try
            {
                InitializeComponent();
                dt.Interval = new TimeSpan(0, 1, 0);
                timerStarted();
                btn_exit.text_button.Text = "Exit";
                btn_exit.gotoPage = btn_exit_Click;
                btn_category_back.text_button.Text = "Back";
                btn_category_back.gotoPage = btn_category_back_Click;
                btn_reset.text_button.Text = "Reset";
                btn_reset.gotoPage = btn_reset_Click;
                btn_pay.text_button.Text = "Pay";
                btn_pay.gotoPage = Pay_Page;
            }
            catch (Exception m) { MessageBox.Show(m.Message, "Error", MessageBoxButton.OK); }
        }

        private void timerStarted()
        {
            dt.Tick += delayCounter;
            dt.Start();

        }

        private void timerStopped()
        {
            dt.Stop();
            dt.IsEnabled = false;
            dt.Tick -= delayCounter;
            dt = new DispatcherTimer();

        }


        private void delayCounter(object sender, EventArgs e)
        {
            timerCount++;
            if (timerCount == 3)
            {
                timerStopped();
                this.NavigationService.Navigate(new _1_Idle());

            }
        }

        private void AddProductToCategory(string category_name)
        {
            try
            {


                category_header header = new category_header();
                header.Width = selectionPanelWidth;
                header.category_name.Text = category_name;
                this.selection_panel.Children.Add(header);
                int product_count = 0;
                // CellModel productCells = SqliteChange.getCellNumber(product.Pr_Name);
                while (product_count < categorywiseProducts[categories.IndexOf(category_name)].Count())
                {
                    ProductModel product = categorywiseProducts[categories.IndexOf(category_name)][product_count];
                    product_item product_panel = new product_item();
                    CellModel productCells = SqliteChange.getCellNumber(product.Pr_Name);
                    product_panel.instruction.PreviewTouchDown += ProductDetailsInstruction_PreviewTouchDown;
                    if(MachineMode == "TEST")
                    product_panel.instruction.PreviewMouseDown += ProductDetailsInstruction_PreviewMouseDown;

                    MemoryStream ms = new MemoryStream(product.Pr_image);
                    System.Drawing.Image image_temp = System.Drawing.Image.FromStream(ms);
                    product_panel.Product_image.Source = ToWpfImage(image_temp);
                    product_panel.Product_Name.Text = product.Pr_Name;
                    product_panel.Product_price.Text = "Rs. " + product.Pr_Selling_Price;
                    if (productCells.CT_Balance_Qty <= 0)
                    {
                        product_panel.sold_out_strip.Visibility = Visibility.Visible;
                        product_panel.quantity_strip.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        ProductTransactionModel productLastTransaction = SqliteChange.getLastProductTransaction(product.Pr_Name, productCells.CT_Row_No, productCells.CT_Col_No);
                        if (indianTime < DateTime.Parse(productLastTransaction.CTT_ExpiryDate) && productLastTransaction.CTT_Closing_Stock > 0)
                        {
                            product_panel.sold_out_strip.Visibility = Visibility.Hidden;
                            product_panel.quantity_strip.Visibility = Visibility.Hidden;
                            if (MachineMode == "TEST")
                                product_panel.PreviewMouseDown += Product_panel_PreviewMouseDown;
                            product_panel.PreviewTouchDown += Product_panel_PreviewTouchDown;
                            product_panel.btn_add.PreviewTouchDown += Btn_add_Click;
                            if (MachineMode == "TEST")
                                product_panel.btn_add.PreviewMouseDown += Btn_add_Click;
                            product_panel.btn_sub.PreviewTouchDown += Btn_sub_Click;
                            if (MachineMode == "TEST")
                                product_panel.btn_sub.PreviewMouseDown += Btn_sub_Click;
                        }
                        else
                        {
                            product_panel.sold_out_strip.Visibility = Visibility.Visible;
                            product_panel.quantity_strip.Visibility = Visibility.Hidden;
                        }
                    }
                    CheckForCartItems(product_panel);
                    this.selection_panel.Children.Add(product_panel);
                    product_count++;
                }
                this.btn_category_back.Visibility = Visibility.Visible;
                this.Navigation_Path_arrow.Visibility = Visibility.Visible;
                this.Navigation_Path_category_name.Visibility = Visibility.Visible;
            }
            catch (Exception m) { MessageBox.Show(m.Message, "Error", MessageBoxButton.OK); }
        }

        private void ProductDetailsInstruction_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            timerCount = 0;
            FrameworkElement UIelement = sender as FrameworkElement;
            product_item product_panel = GetParents(UIelement) as product_item;
            moreInformation(product_panel);
        }

        private void AddProductToCart(product_item product_panel)
        {
            if (this.order_quantity.Text != "5")
            {
                product_panel.quantity_strip.Visibility = Visibility.Visible;
                if (MachineMode == "TEST")
                    product_panel.PreviewMouseDown -= Product_panel_PreviewMouseDown;
                product_panel.PreviewTouchDown -= Product_panel_PreviewTouchDown;

                if (this.cart.Children.Count == 0)
                {
                    cart_item Cart_Product = new cart_item();
                    Cart_Product.Product_Name.Text = product_panel.Product_Name.Text;
                    Cart_Product.Product_price.Text = product_panel.Product_price.Text;
                    Cart_Product.btn_cart_add.PreviewTouchDown += Btn_cart_add_Click;
                    if (MachineMode == "TEST")
                        Cart_Product.btn_cart_add.PreviewMouseDown += Btn_cart_add_Click;
                    Cart_Product.btn_cart_sub.PreviewTouchDown += Btn_cart_sub_Click;
                    if (MachineMode == "TEST")
                        Cart_Product.btn_cart_sub.PreviewMouseDown += Btn_cart_sub_Click;
                    AddQuantityAndOrderAmount(Cart_Product);
                    Cart_Product.btn_cart_add.Visibility = Visibility.Visible;
                    Cart_Product.btn_cart_sub.Visibility = Visibility.Visible;
                    this.cart.Children.Add(Cart_Product);
                    this.btn_reset.Visibility = Visibility.Visible;
                }
                else
                {
                    int child_count = 0;
                    bool present = false;
                    cart_item cart_item_exist = new cart_item();
                    while (child_count < this.cart.Children.Count)
                    {
                        cart_item_exist = this.cart.Children[child_count] as cart_item;
                        if (product_panel.Product_Name.Text == cart_item_exist.Product_Name.Text)
                        {
                            present = true;
                            break;
                        }
                        child_count++;
                    }

                    if (present)
                    {
                        int product_quantity = Int32.Parse(cart_item_exist.Product_quantity.Text);
                        string price_text = product_panel.Product_price.Text;
                        string[] separated = price_text.Split(' ');
                        double price = Double.Parse(separated[1]);
                        product_quantity++;
                        cart_item_exist.Product_quantity.Text = "" + product_quantity;
                        product_panel.Product_quantity.Text = "" + product_quantity;
                        price = product_quantity * price;
                        cart_item_exist.Product_price.Text = "Rs. " + price;
                        AddQuantityAndOrderAmount(cart_item_exist);
                    }
                    else if (!present && child_count == this.cart.Children.Count)
                    {
                        cart_item Cart_Product = new cart_item();
                        Cart_Product.Product_Name.Text = product_panel.Product_Name.Text;
                        Cart_Product.Product_price.Text = product_panel.Product_price.Text;
                        Cart_Product.btn_cart_add.PreviewTouchDown += Btn_cart_add_Click;
                        if (MachineMode == "TEST")
                            Cart_Product.btn_cart_add.PreviewMouseDown += Btn_cart_add_Click;
                        Cart_Product.btn_cart_sub.PreviewTouchDown += Btn_cart_sub_Click;
                        if (MachineMode == "TEST")
                            Cart_Product.btn_cart_sub.PreviewMouseDown += Btn_cart_sub_Click;
                        AddQuantityAndOrderAmount(Cart_Product);
                        Cart_Product.btn_cart_add.Visibility = Visibility.Visible;
                        Cart_Product.btn_cart_sub.Visibility = Visibility.Visible;
                        this.cart.Children.Add(Cart_Product);
                    }
                }
            }
            else
            {
                MessageBox.Show("Only 5 products can be dispensed at a time", "Capacity Reached!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AddQuantityAndOrderAmount(cart_item Cart_Product)
        {
            int quantity = Int32.Parse(Cart_Product.Product_quantity.Text);
            int product_quantity = Int32.Parse(this.order_quantity.Text);
            product_quantity = 1 + product_quantity;
            this.order_quantity.Text = "" + product_quantity;

            string price_text = Cart_Product.Product_price.Text;
            string[] separated = price_text.Split(' ');
            double price = Double.Parse(separated[1]) / quantity;

            string order_amount = this.order_amount.Text;
            string[] separated_1 = order_amount.Split(' ');
            double order_amount_price = Double.Parse(separated_1[1]);
            order_amount_price = order_amount_price + price;
            this.order_amount.Text = "Rs. " + order_amount_price;
        }

        /*----------------------Add Button Imthi------------------------------------------ */
        private void AddBtnOfTheProduct(product_item product_add) //find non existing product error
        {

            int bal_qty = SqliteChange.getbalanceqty(product_add.Product_Name.Text);
            int quantity = Int32.Parse(product_add.Product_quantity.Text);
            
            if ((quantity+1) <= bal_qty)   //check the stock
            {
                if (this.order_quantity.Text != "5")
                {
                    cart_item cart_product = new cart_item();    //1
                    int child_count = 0;
                    bool present = false;
                    quantity++;
                    product_add.Product_quantity.Text = "" + quantity;
                    while (child_count < this.cart.Children.Count)
                    {
                        cart_product = this.cart.Children[child_count] as cart_item;
                        if (product_add.Product_Name.Text == cart_product.Product_Name.Text) { present = true; break; }

                        child_count++;
                    }
                    if (present)
                    {
                        string price_text = product_add.Product_price.Text;
                        string[] separated = price_text.Split(' ');
                        double price = Double.Parse(separated[1]);
                        cart_product.Product_quantity.Text = "" + quantity;
                        price = quantity * price;

                        cart_product.Product_price.Text = "Rs. " + price;
                        AddQuantityAndOrderAmount(cart_product);
                    }
                }
                else
                {
                    MessageBox.Show("Only 5 products can be dispensed at a time", "Capacity Reached!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Out of stock", "Stock Unavailable!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /*------------------------------------------------------------------------------------------------------------------------------------------------------ */
        private void AddBtnOfTheProduct(cart_item cart_add)
        {
            int bal_qty = SqliteChange.getbalanceqty(cart_add.Product_Name.Text);
            //convert int into string
            int quantity = Int32.Parse(cart_add.Product_quantity.Text);
            if ((quantity+1) <= bal_qty)   //check the stock
            {
                if (this.order_quantity.Text != "5")
                {
                    product_item product_panel = new product_item();
                    
                    int child_count = 0;
                    bool present = false;

                    cart_add.Product_quantity.Text = "" + quantity;
                    while (child_count < this.selection_panel.Children.Count)
                    {
                        if (this.selection_panel.Children[child_count] is product_item)
                        {
                            product_panel = this.selection_panel.Children[child_count] as product_item;
                            if (cart_add.Product_Name.Text == product_panel.Product_Name.Text) { present = true; break; }
                        }
                        child_count++;
                    }
                    if (present)
                    {
                        quantity++;
                        string price_text = product_panel.Product_price.Text;
                        string[] separated = price_text.Split(' ');
                        double price = Double.Parse(separated[1]);
                        cart_add.Product_quantity.Text = "" + quantity;
                        product_panel.Product_quantity.Text = "" + quantity;
                        price = quantity * price;
                        cart_add.Product_price.Text = "Rs. " + price;
                        AddQuantityAndOrderAmount(cart_add);

                    }
                    else
                    {
                        string price_text = cart_add.Product_price.Text;
                        string[] separated = price_text.Split(' ');
                        double price = Double.Parse(separated[1]) / quantity;
                        quantity++;
                        price = quantity * price;
                        cart_add.Product_price.Text = "Rs. " + price;
                        cart_add.Product_quantity.Text = "" + quantity;
                        AddQuantityAndOrderAmount(cart_add);
                    }
                }
                else
                {
                    MessageBox.Show("Only 5 products can be dispensed at a time", "Capacity Reached!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Out of stock", "Stock Unavailable!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void All_products_category_panel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.selection_panel.Children.Clear();
                foreach (string cat in categories)
                {
                    AddProductToCategory(cat);
                }
                this.Navigation_Path_category_name.Text = "All Products";
            }
            catch (Exception m) { MessageBox.Show(m.Message, "Error", MessageBoxButton.OK); }
        }


        private void btn_category_back_Click()
        {
            timerCount = 0;
            showCategories();
        }

        private void btn_exit_Click()
        {
            timerStopped();
            this.NavigationService.Navigate(new _1_Idle());
            timerCount = 0;
        }

        private void btn_reset_Click()
        {
            timerCount = 0;
            this.cart.Children.Clear();
            showCategories();
            this.order_amount.Text = "Rs. 0";
            this.order_quantity.Text = "0";
            this.btn_reset.Visibility = Visibility.Hidden;
        }


        private void Btn_sub_Click(object sender, RoutedEventArgs e)
        {
            timerCount = 0;
            Navigation_Buttons subbtn = sender as Navigation_Buttons;
            product_item product_sub = GetParents(subbtn) as product_item;
            SubBtnOfTheProduct(product_sub);
        }

        private void Btn_add_Click(object sender, RoutedEventArgs e)
        {
            timerCount = 0;
            Navigation_Buttons addbtn = sender as Navigation_Buttons;
            product_item product_add = GetParents(addbtn) as product_item;
            AddBtnOfTheProduct(product_add);
        }


        private void Btn_cart_sub_Click(object sender, RoutedEventArgs e)
        {
            timerCount = 0;
            Navigation_Buttons button = sender as Navigation_Buttons;
            cart_item cart_product = GetParents(button) as cart_item;
            SubBtnOfTheProduct(cart_product);
        }

        private void Btn_cart_add_Click(object sender, RoutedEventArgs e)
        {
            timerCount = 0;
            Navigation_Buttons button = sender as Navigation_Buttons;
            cart_item cart_product = GetParents(button) as cart_item;
            AddBtnOfTheProduct(cart_product);
        }

        private void Category_panel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                timerCount = 0;
                category_item category_panel = sender as category_item;
                showCategoryWiseProducts(category_panel);
            }
            catch (Exception m) { MessageBox.Show(m.Message, "Error", MessageBoxButton.OK); }
        }


        private void CheckForCartItems(product_item product_panel)
        {
            cart_item cart_item = new cart_item();
            if (this.cart.Children.Count > 0)
            {
                int child_count = 0; bool present = false;
                while (child_count < this.cart.Children.Count)
                {
                    cart_item = this.cart.Children[child_count] as cart_item;
                    if (cart_item.Product_Name.Text == product_panel.Product_Name.Text) { present = true; break; }
                    child_count++;
                }
                if (present)
                {
                    int product_quantity = Int32.Parse(cart_item.Product_quantity.Text);
                    product_panel.Product_quantity.Text = "" + product_quantity;
                    product_panel.quantity_strip.Visibility = Visibility.Visible;
                    if (MachineMode == "TEST")
                        product_panel.PreviewMouseDown -= Product_panel_PreviewMouseDown;
                    product_panel.PreviewTouchDown -= Product_panel_PreviewTouchDown;
                }
            }
        }

        private Object GetParents(Object element)
        {
            FrameworkElement returnElement = element as FrameworkElement;
            Object returnObject = new Object();
            while (returnElement.Parent != null)
            {
                returnElement = returnElement.Parent as FrameworkElement;
                if (returnElement.Parent is product_item || returnElement.Parent is cart_item)
                {
                    returnObject = returnElement.Parent;
                    break;
                }
            }
            return returnObject;
        }

        private void listAllCategories(List<ProductModel> products, List<string> categories, List<List<ProductModel>> categorywiseProducts)
        {
            int product_count = 0;
            string category = "";
            while (product_count < products.Count)
            {
                category = products[product_count].Pr_Category;
                if (!categories.Contains(category))
                {
                    List<ProductModel> category_products = new List<ProductModel>();
                    categories.Add(category);
                    categorywiseProducts.Add(category_products);
                }
                product_count++;
            }
            categories.Sort();

        }

        private void listProductsCategoryWise(List<ProductModel> products, List<string> categories, List<List<ProductModel>> categorywiseProducts)
        {
            int product_count = 0;
            string category = "";
            while (product_count < products.Count)
            {
                category = products[product_count].Pr_Category;
                if (!categorywiseProducts[categories.IndexOf(category)].Contains(products[product_count]))
                {
                    categorywiseProducts[categories.IndexOf(category)].Add(products[product_count]);
                    categorywiseProducts[categories.IndexOf(category)].OrderBy(x => x.Pr_Name);
                }
                product_count++;
            }
        }

        private void Product_panel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            timerCount = 0;
            product_item product_panel = sender as product_item;
            AddProductToCart(product_panel);
            product_panel.PreviewMouseDown -= Product_panel_PreviewMouseDown;
        }



        private void Product_panel_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            timerCount = 0;
            product_item product_panel = sender as product_item;
            AddProductToCart(product_panel);
            product_panel.PreviewTouchDown -= Product_panel_PreviewTouchDown;
        }




        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            selectionPanelWidth = this.selection_panel.ActualWidth;

            showCategories();
        }

        private void Pay_Page()
        {
            timerCount = 0;
            if (this.cart.Children.Count > 0)
            {
                List<cart_item> cart_items = new List<cart_item>();
                int count = 0;
                while (count < this.cart.Children.Count)
                {
                    cart_item item = this.cart.Children[count] as cart_item;
                   
                    cart_items.Add(item);
                    count++;
                }
                timerStopped();
                this.NavigationService.Navigate(new _14_Payment(this, cart_items, this.order_amount.Text, this.order_quantity.Text));
            }

            else
            {
                MessageBox.Show("Please select atleast one product.");
            }
        }

        private void showCategoryWiseProducts(category_item category_panel)
        {
            try
            {
                string category_name = category_panel.category_name.Text;
                this.selection_panel.Children.Clear();
                AddProductToCategory(category_name);
                this.Navigation_Path_category_name.Text = category_name;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK); }
        }

        private void SubBtnOfTheProduct(product_item product_sub)
        {
            cart_item cart_product = new cart_item();
            bool present = false;
            int child_count = 0;
            int quantity = Int32.Parse(product_sub.Product_quantity.Text);
            quantity--;

            while (child_count < this.cart.Children.Count)
            {
                cart_product = this.cart.Children[child_count] as cart_item;
                if (product_sub.Product_Name.Text == cart_product.Product_Name.Text) { present = true; break; }

                child_count++;
            }
            if (present)
            {
                SubQuantityAndOrderAmount(cart_product);
                if (quantity > 0)
                {
                    product_sub.Product_quantity.Text = "" + quantity;
                    string price_text = product_sub.Product_price.Text;
                    string[] separated = price_text.Split(' ');
                    double price = Double.Parse(separated[1]);
                    cart_product.Product_quantity.Text = "" + quantity;
                    price = quantity * price;
                    cart_product.Product_price.Text = "Rs. " + price;
                }
                else
                {
                    this.cart.Children.RemoveAt(child_count);
                    product_sub.quantity_strip.Visibility = Visibility.Hidden;
                    if (MachineMode == "TEST")
                        product_sub.PreviewMouseDown += Product_panel_PreviewMouseDown;
                    product_sub.PreviewTouchDown += Product_panel_PreviewTouchDown;
                }

            }
        }


        private void SubBtnOfTheProduct(cart_item cart_sub)
        {
            product_item product_panel = new product_item();
            int quantity = Int32.Parse(cart_sub.Product_quantity.Text);
            int child_count = 0;
            bool present = false;

            cart_sub.Product_quantity.Text = "" + quantity;
            while (child_count < this.selection_panel.Children.Count)
            {
                if (this.selection_panel.Children[child_count] is product_item)
                {
                    product_panel = this.selection_panel.Children[child_count] as product_item;
                    if (cart_sub.Product_Name.Text == product_panel.Product_Name.Text) { present = true; break; }
                }
                child_count++;
            }
            SubQuantityAndOrderAmount(cart_sub);
            if (present)
            {
                quantity--;
                if (quantity > 0)
                {
                    string price_text = product_panel.Product_price.Text;
                    string[] separated = price_text.Split(' ');
                    double price = Double.Parse(separated[1]);
                    cart_sub.Product_quantity.Text = "" + quantity;
                    product_panel.Product_quantity.Text = "" + quantity;
                    price = quantity * price;
                    cart_sub.Product_price.Text = "Rs. " + price;
                }
                else
                {
                    product_panel.quantity_strip.Visibility = Visibility.Hidden;
                    int cart_child = 0;
                    bool cart_presence = false;
                    cart_item cart_product = new cart_item();
                    while (cart_child < this.cart.Children.Count)
                    {
                        cart_product = this.cart.Children[cart_child] as cart_item;
                        if (cart_sub.Product_Name.Text == cart_product.Product_Name.Text) { cart_presence = true; break; }
                        cart_child++;
                    }
                    if (cart_presence)
                    {
                        this.cart.Children.RemoveAt(cart_child);
                        product_panel.quantity_strip.Visibility = Visibility.Hidden;
                        if (MachineMode == "TEST")
                            product_panel.PreviewMouseDown += Product_panel_PreviewMouseDown;
                        product_panel.PreviewTouchDown += Product_panel_PreviewTouchDown;
                    }
                }
            }
            else
            {
                string price_text = cart_sub.Product_price.Text;
                string[] separated = price_text.Split(' ');
                double price = Double.Parse(separated[1]) / quantity;
                quantity--;
                if (quantity > 0)
                {
                    price = quantity * price;
                    cart_sub.Product_price.Text = "Rs. " + price;
                    cart_sub.Product_quantity.Text = "" + quantity;
                }
                else
                {
                    int cart_child = 0;
                    bool cart_presence = false;
                    cart_item cart_product = new cart_item();
                    while (cart_child < this.cart.Children.Count)
                    {
                        cart_product = this.cart.Children[cart_child] as cart_item;
                        if (cart_sub.Product_Name.Text == cart_product.Product_Name.Text) { cart_presence = true; break; }
                        cart_child++;
                    }
                    if (cart_presence)
                    {
                        this.cart.Children.RemoveAt(cart_child);
                    }
                }
            }
        }

        private void SubQuantityAndOrderAmount(cart_item Cart_Product)
        {
            int quantity = Int32.Parse(Cart_Product.Product_quantity.Text);
            int product_quantity = Int32.Parse(this.order_quantity.Text);
            product_quantity = product_quantity - 1;
            this.order_quantity.Text = "" + product_quantity;

            string price_text = Cart_Product.Product_price.Text;
            string[] separated = price_text.Split(' ');
            double price = Double.Parse(separated[1]) / quantity;

            string order_amount = this.order_amount.Text;
            string[] separated_1 = order_amount.Split(' ');
            double order_amount_price = Double.Parse(separated_1[1]);
            order_amount_price = order_amount_price - price;
            this.order_amount.Text = "Rs. " + order_amount_price;

            if (order_amount_price == 0 || product_quantity == 0)
            {
                this.btn_reset.Visibility = Visibility.Hidden;
            }
        }

        private void showCategories()
        {
            this.selection_panel.Children.Clear();
            category_header header = new category_header();
            header.Width = selectionPanelWidth;
            header.category_name.Text = "Categories";
            this.selection_panel.Children.Add(header);

            //Adding categories to _17_Category list, initialising _17_Category-wise product list and sorting 
            //the _17_Category list with respect to alphabetical order
            listAllCategories(products, categories, categorywiseProducts);

            //List all products _17_Category wise and arranged alphabetically
            listProductsCategoryWise(products, categories, categorywiseProducts);

            foreach (string cat in categories)
            {

                CategoryModel categoryModel = new CategoryModel();
                List<CategoryModel> Categories = SqliteDataAccess.getCategories();
                foreach (CategoryModel category in Categories)
                {
                    if (cat == category.CT_Name)
                    {
                        categoryModel = category;
                    }
                }

                category_item category_panel = new category_item();
                MemoryStream ms = new MemoryStream(categoryModel.CT_Image);
                System.Drawing.Image image_temp = System.Drawing.Image.FromStream(ms);
                category_panel.category_image.Source = ToWpfImage(image_temp);
                category_panel.category_name.Text = cat;
                if (MachineMode == "TEST")
                    category_panel.PreviewMouseDown += Category_panel_PreviewMouseDown;
                category_panel.PreviewTouchDown += Category_panel_PreviewTouchDown;
                this.selection_panel.Children.Add(category_panel);

            }
            category_item all_products_category_panel = new category_item();
            all_products_category_panel.PreviewMouseDown += All_products_category_panel_PreviewMouseDown;
            all_products_category_panel.PreviewTouchDown += All_products_category_panel_PreviewTouchDown;
            this.selection_panel.Children.Add(all_products_category_panel);
            this.Navigation_Path_category_name.Visibility = Visibility.Hidden;
            this.Navigation_Path_arrow.Visibility = Visibility.Hidden;
            this.btn_category_back.Visibility = Visibility.Hidden;
        }

        private void Category_panel_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            try
            {
                timerCount = 0;
                category_item category_panel = sender as category_item;
                showCategoryWiseProducts(category_panel);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK); }

        }

        private void All_products_category_panel_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            try
            {
                timerCount = 0;
                this.selection_panel.Children.Clear();
                foreach (string cat in categories)
                {
                    AddProductToCategory(cat);
                }
                this.Navigation_Path_category_name.Text = "All Products";
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK); }
        }

        private void ProductDetailsInstruction_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            timerCount = 0;
            FrameworkElement UIelement = sender as FrameworkElement;
            product_item product_panel = GetParents(UIelement) as product_item;
            moreInformation(product_panel);
        }

        private void moreInformation(product_item product_panel)
        {
            ProductModel product = SqliteDataAccess.getProductDetails(product_panel.Product_Name.Text);
            product_information popup = new product_information(product);
            popup.ShowDialog();
        }

        private BitmapImage ToWpfImage(System.Drawing.Image img)
        {
            MemoryStream ms = new MemoryStream();  // no using here! BitmapImage will dispose the stream after loading
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

            BitmapImage ix = new BitmapImage();
            ix.BeginInit();
            ix.CacheOption = BitmapCacheOption.OnLoad;
            ix.StreamSource = ms;
            ix.EndInit();
            return ix;
        }

        private void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            timerCount = 0;
            e.Handled = true;
        }

        private void Page_MouseDown(object sender, MouseButtonEventArgs e)
        {
            timerCount = 0;
        }

        private void Page_TouchDown(object sender, TouchEventArgs e)
        {
            timerCount = 0;
        }
    }


    public class TextInputToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Always test MultiValueConverter inputs for non-null
            // (to avoid crash bugs for views in the designer)
            if (values[0] is bool && values[1] is bool)
            {
                bool hasText = !(bool)values[0];
                bool hasFocus = (bool)values[1];

                if (hasFocus || hasText)
                    return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }


    class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool?)value == true)
                return System.Windows.Visibility.Visible;
            else
                return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
