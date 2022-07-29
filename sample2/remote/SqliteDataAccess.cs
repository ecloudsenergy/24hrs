using sample2.models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using ToastNotifications;
using ToastNotifications.Position;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using System.Windows;
using System.Data;

namespace sample2.remote
{
    class SqliteDataAccess
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        private static DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        private static Notifier noti = new Notifier(cfg =>
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

        public static string getPort(string device_name)
        {
            string device_port = "";


            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Port Table' where Device_Name = '" + device_name + "'", conn);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                    device_port = reader["Port_Name"].ToString();


                reader.Close();
                conn.Close();
            }

            return device_port;
        }


        public static MachineModel getMachineMasterDetails()
        {
            MachineModel machineDetails = new MachineModel();
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                String query = "Select * from 'Machine Master Table'";
                cnn.Open();
                using (var cmd = new SQLiteCommand(query, cnn))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            machineDetails.MM_Machine_No = reader["MM_Machine_No"].ToString();
                            machineDetails.MM_Open_file = reader["MM_Open_file"].ToString();
                            machineDetails.MM_Location = reader["MM_Location"].ToString();
                            machineDetails.MM_Loc_description = reader["MM_Loc_description"].ToString();
                            machineDetails.MM_HelpNo = reader["MM_HelpNo"].ToString();
                            machineDetails.MM_Company = reader["MM_Company"].ToString();
                            machineDetails.MM_Company_Address = reader["MM_Company_Address"].ToString();
                            machineDetails.MM_Phone = reader["MM_Phone"].ToString();
                            machineDetails.MM_GST = reader["MM_GST"].ToString();
                            machineDetails.MM_Mode = reader["MM_Mode"].ToString();

                            machineDetails.MM_Location_Logo = getMachineLogoImage();


                        }
                    }
                }

            }
            return machineDetails;
        }


        public static List<ProductModel> getProducts()
        {
            List<ProductModel> listProductModel = new List<ProductModel>();
            List<string> products = new List<string>();

            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Open();
                string query2 = "Select * from 'Cell Table' WHERE CT_Enable_Tag = 1";
                using (var cmd = new SQLiteCommand(query2, cnn))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            if (!products.Contains(reader["CT_Product_name"].ToString()))
                                products.Add(reader["CT_Product_name"].ToString());
                        }
                    }
                }

                for (int i = 0; i < products.Count; i++)
                {
                    string query = "Select * from 'Product Table' WHERE Pr_Name = '" + products[i] + "'";
                    using (var cmd = new SQLiteCommand(query, cnn))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ProductModel product = new ProductModel();
                                product.Pr_Name = reader["Pr_Name"].ToString();
                                product.Pr_image = getProductImage(product.Pr_Name);
                                product.Pr_Description = reader["Pr_Description"].ToString();
                                product.Pr_HSN = reader["Pr_HSN"].ToString();
                                product.Pr_Category = reader["Pr_Category"].ToString();
                                product.Pr_Buying_Price = reader["Pr_Buying_Price"].ToString();
                                product.Pr_SGST = reader["Pr_SGST"].ToString();
                                product.Pr_CGST = reader["Pr_CGST"].ToString();
                                product.Pr_IGST = reader["Pr_IGST"].ToString();
                                product.Pr_Selling_Price = reader["Pr_Selling_Price"].ToString();
                                product.Pr_Min_Qty = reader["Pr_Min_Qty"].ToString();
                                product.Pr_Notify_Before = reader["Pr_Notify_Before"].ToString();
                                listProductModel.Add(product);

                            }
                        }
                    }
                }
            }
            return listProductModel;
        }

        public static List<CategoryModel> getCategories()
        {
            List<CategoryModel> listCategoryModel = new List<CategoryModel>();
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                String query = "Select * from 'Category Table'";
                cnn.Open();
                using (var cmd = new SQLiteCommand(query, cnn))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CategoryModel category = new CategoryModel();

                            category.CT_Name = reader["CT_Name"].ToString();
                            category.CT_Image = getCategoryImage(category.CT_Name);

                            listCategoryModel.Add(category);
                        }
                    }
                }

            }
            return listCategoryModel;
        }


        public static void insertError(string errorType, string errorDescription)
        {
            indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            string formattedTime = indianTime.ToString("yyyy-MM-dd HH:mm:ss");
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {

                SQLiteCommand command = new SQLiteCommand("INSERT INTO 'Error Log Table'(EL_Date_Time, EL_Error_Type, EL_Error_Description) values('" + formattedTime + "', '" + errorType + "', '" + errorDescription + "')", conn);

                command.ExecuteNonQuery();
            }

        }


        public static byte[] getProductImage(string name)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select Pr_image from 'Product Table' WHERE Pr_Name = '" + name + "'", conn);


                const int CHUNK_SIZE = 2 * 1024;
                byte[] buffer = new byte[CHUNK_SIZE];
                byte[] Imagebytes = new List<byte>().ToArray();
                try
                {
                    IDataReader rdr = command.ExecuteReader();

                    try
                    {
                        while (rdr.Read())
                        {
                            Imagebytes = (System.Byte[])rdr[0];

                        }
                    }
                    catch (Exception exc) { MessageBox.Show(exc.Message); }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
                conn.Close();
                return Imagebytes;
            }
        }
   

        public static byte[] getMachineLogoImage()
        {
            

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select MM_Location_Logo from 'Machine Master Table'", conn);
                const int CHUNK_SIZE = 2 * 1024;
                byte[] buffer = new byte[CHUNK_SIZE];
                byte[] Imagebytes = new List<byte>().ToArray();
                try
                {
                    IDataReader rdr = command.ExecuteReader();

                    try
                    {
                        while (rdr.Read())
                        {
                            Imagebytes = (System.Byte[])rdr[0];

                        }
                    }
                    catch (Exception exc) { MessageBox.Show(exc.Message); }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
                conn.Close();
                return Imagebytes;
            }


        }

        public static byte[] getCategoryImage(string CT_Name)
        {
            

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select CT_Image from 'Category Table' WHERE CT_Name = '" + CT_Name + "'", conn);
                const int CHUNK_SIZE = 2 * 1024;
                byte[] buffer = new byte[CHUNK_SIZE];
                byte[] Imagebytes = new List<byte>().ToArray();
                try
                {
                    IDataReader rdr = command.ExecuteReader();

                    try
                    {
                        while (rdr.Read())
                        {
                            Imagebytes = (System.Byte[])rdr[0];

                        }
                    }
                    catch (Exception exc) { MessageBox.Show(exc.Message); }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
                conn.Close();
                return Imagebytes;

            }


        }





        public static ProductModel getProductDetails(string name)
        {
            ProductModel productDetails = new ProductModel();


            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Product Table' WHERE Pr_Name = '" + name + "'", conn);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    productDetails.Pr_Name = reader["Pr_Name"].ToString();
                    productDetails.Pr_image = getProductImage(productDetails.Pr_Name);
                    productDetails.Pr_Description = reader["Pr_Description"].ToString();
                    productDetails.Pr_HSN = reader["Pr_HSN"].ToString();
                    productDetails.Pr_Category = reader["Pr_Category"].ToString();
                    productDetails.Pr_Buying_Price = reader["Pr_Buying_Price"].ToString();
                    productDetails.Pr_SGST = reader["Pr_SGST"].ToString();
                    productDetails.Pr_CGST = reader["Pr_CGST"].ToString();
                    productDetails.Pr_IGST = reader["Pr_IGST"].ToString();
                    productDetails.Pr_Selling_Price = reader["Pr_Selling_Price"].ToString();
                    productDetails.Pr_Min_Qty = reader["Pr_Min_Qty"].ToString();
                    productDetails.Pr_Notify_Before = reader["Pr_Notify_Before"].ToString();
                    productDetails.Pr_Expiry_Date = reader["Pr_Expiry_Date"].ToString();
                }

                reader.Close();
                conn.Close();
            }

            return productDetails;
        }

        public static int insertProductDetails(string pr_name, string pr_Description, string pr_Category, string pr_HSN,
            float pr_Rate, float pr_GST_Rate, float pr_Final_Rate, int pr_Min_Qty, string pr_ExpiryDate = "2020-08-25 17:21:12",
            string pr_image_path = "../../images/eClouds Logo.png")
        {
            int rowCount = 0;
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command =
                    new SQLiteCommand("INSERT INTO 'Product Table'(Pr_Name,Pr_image,Pr_Description,Pr_HSN,Pr_Category,Pr_ExpDate,"
                    + "Pr_Rate,Pr_GST_Rate,Pr_Final_Rate,Pr_Min_Qty) values( '" + pr_name + "', @img, '" + pr_Description + "', "
                    + "'" + pr_HSN + "', '" + pr_Category + "', '" + pr_ExpiryDate + "', " + pr_Rate + ", "
                    + pr_GST_Rate + ", " + pr_Final_Rate + ", " + pr_Min_Qty + ")", conn);

                command.Parameters.AddWithValue("@img", File.ReadAllBytes(pr_image_path));

                rowCount = command.ExecuteNonQuery();

                conn.Close();
            }

            return rowCount;
        }

        public static int insertCategoryDetails(string categoryName, string imagePath = "../../images/all products.png")
        {
            int rowCount = 0;
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("INSERT INTO 'Category Table' values( '" + categoryName + "', @img )", conn);
                command.Parameters.AddWithValue("@img", File.ReadAllBytes(imagePath));

                rowCount = command.ExecuteNonQuery();

                conn.Close();
            }

            return rowCount;
        }

        public static string getHelplineNumber()
        {
            string helplineNo = "";


            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Machine Master Table'", conn);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                    helplineNo = reader["MM_HelpNo"].ToString();


                reader.Close();
                conn.Close();
            }

            return helplineNo;
        }

        public static string getMachineNumber()
        {
            string machineNo = "";


            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Machine Master Table'", conn);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                    machineNo = reader["MM_Machine_No"].ToString();
                reader.Close();
                conn.Close();
            }

            return machineNo;
        }

        public static string getMachineVideo()
        {
            string machineVideoFilePath = "";


            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Machine Master Table'", conn);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                    machineVideoFilePath = reader["MM_Open_file"].ToString();
                reader.Close();
                conn.Close();
            }

            return machineVideoFilePath;
        }

        public static string getUsertype(string username)
        {
            string usertype = "";


            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Admin Table' WHERE Ad_User_Name = '" + username + "'", conn);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                    usertype = reader["Ad_UserType"].ToString();
                reader.Close();
                conn.Close();
            }

            return usertype;
        }

        public static List<string> getUsernames()
        {
            List<string> usernames = new List<string>();


            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Admin Table'", conn);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    usernames.Add(reader["Ad_User_Name"].ToString());
                }
                conn.Close();
            }

            return usernames;
        }

        public static LogModel getLastLogEvent()
        {
            LogModel logModel = new LogModel();


            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("SELECT * FROM 'Logging Table' ORDER BY LT_DateTime desc, LT_Id desc LIMIT 1", conn);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    logModel.LT_Event_code = Int32.Parse(reader["LT_Event_code"].ToString());
                    string[] separator = reader["LT_Event_ref"].ToString().Split('-');
                    logModel.LT_username = separator[0];
                    logModel.LT_eventState = separator[1];
                    logModel.LT_usertype = separator[2];
                }

                reader.Close();
                conn.Close();
            }

            return logModel;
        }

        public static string settingBillNumber()
        {
            string bill_no = "T";
            string machine_no = SqliteDataAccess.getMachineNumber();
            int machine_year = int.Parse(machine_no.Substring(1, 2));
            int machine_month = int.Parse(machine_no.Substring(3, 2));
            string machine_number = machine_no.Substring(5, 2);
            if (machine_year >= 20 && machine_year <= 29)
            {
                machine_year = machine_year - 20;
                bill_no += "" + machine_year;
            }
            else if (machine_year >= 30 && machine_year <= 55)
            {
                machine_year = machine_year + 35;
                char charac = (char)machine_year;
                bill_no += charac.ToString();
            }
            else if (machine_year >= 56 && machine_year <= 81)
            {
                machine_year = machine_year + 41;
                char charac = (char)machine_year;
                bill_no += charac.ToString();
            }
            else { noti.ShowError("Bill number is not proper. - y"); }

            if (machine_month > 0 && machine_month < 10)
            {
                bill_no += "" + machine_month;
            }
            else if (machine_month >= 10 && machine_month < 13)
            {
                machine_month = machine_month + 55;
                char charac = (char)machine_month;
                bill_no += charac.ToString();
            }
            else { noti.ShowError("Bill number is not proper. - m"); }

            bill_no += machine_number;

            return bill_no;
        }

        public static bool findBillNumber(string bill_no)
        {
            int row_count = 0;
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("SELECT * FROM 'Bills Table' WHERE BT_Bill_No = '" + bill_no + "'", conn);
                SQLiteDataReader reader = command.ExecuteReader();

                row_count = reader.StepCount;
                reader.Close();
                conn.Close();
            }
            return (row_count > 0) ? true : false;
        }

        public static string getBillNumber()
        {
            string bill_no = "";
            int lastNumber = 0;
            string timeString = settingBillNumber() + "-" + indianTime.ToString("yy");
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("SELECT * FROM 'Bills Table' ORDER BY BT_Bill_No desc LIMIT 1", conn);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {

                    if (reader["BT_Bill_No"].ToString().Contains(timeString))
                    {
                        lastNumber = int.Parse(reader["BT_Bill_No"].ToString().Substring(8));
                    }
                    else lastNumber = 0;
                }
                lastNumber++;
                for (int i = lastNumber.ToString().Length; i < 6; i++)
                {
                    timeString += "0";
                }

                bill_no = timeString + lastNumber;

                reader.Close();
                conn.Close();
            }

            bill_no = recursiveBillAdding(lastNumber, timeString, bill_no);

            if (bill_no.Length > 14)
            {
                noti.ShowError("Bill number is not proper. - m");
                bill_no = "Mstk"+ bill_no;
            }
           

            return bill_no;
        }

        public static string recursiveBillAdding(int lastNumber, string timeString, string bill_no)
        {
            if (findBillNumber(bill_no))
            {
                lastNumber++;
                for (int i = lastNumber.ToString().Length; i < 6; i++)
                {
                    timeString += "0";
                }
                bill_no = timeString + lastNumber;
                bill_no = recursiveBillAdding(lastNumber, timeString, bill_no);
            }

            return bill_no;
        }

        public static DenominationRecords getAllDenominationDetails(List<int> denominations)
        {
            List<int> balance_nos_all = new List<int>();

            int total_balance = 0;

            for (int i = 0; i < denominations.Count; i++)
            {
                using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
                {
                    conn.Open();

                    SQLiteCommand command = new SQLiteCommand("SELECT Cr_Closing_Balance_Qty FROM 'Currency Transaction Table' WHERE Cr_Denomination = " + denominations[i]
                        + " AND Cr_Equipment_Used = 'CA' ORDER BY Cr_DateTime desc LIMIT 1", conn);
                    SQLiteDataReader reader = command.ExecuteReader();

                    if (reader.StepCount == 0)
                    {
                        int quantity = 0;
                        balance_nos_all.Add(quantity);
                    }

                    while (reader.Read())
                    {
                        total_balance += Int32.Parse(reader["Cr_Closing_Balance_Qty"].ToString()) * denominations[i];
                        int quantity = Int32.Parse(reader["Cr_Closing_Balance_Qty"].ToString());
                        balance_nos_all.Add(quantity);
                    }

                    reader.Close();
                    conn.Close();
                }

            }


            DenominationRecords records = new DenominationRecords();
            records.Balance_Nos = balance_nos_all;
            records.Total_Balance = total_balance;
            return records;
        }

        public static CellModel getProductCellDetails(string TR_Product_name)
        {
            SQLiteCommand command = new SQLiteCommand();
            CellModel tray_model = new CellModel();
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                command = new SQLiteCommand("Select * from 'Cell Table' where CT_Product_name = '" + TR_Product_name + "' and CT_Registered_Balance_Qty > 0 Order by CT_Registered_Balance_Qty asc Limit 1", conn);

                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    tray_model.CT_Product_name = TR_Product_name;
                    tray_model.CT_Balance_Qty = Int32.Parse(reader["CT_Balance_Qty"].ToString());
                    tray_model.CT_Col_No = Int32.Parse(reader["CT_Col_No"].ToString());
                    tray_model.CT_Row_No = Int32.Parse(reader["CT_Row_No"].ToString());
                    tray_model.CT_Max_Qty = Int32.Parse(reader["CT_Max_Qty"].ToString());
                    tray_model.CT_Enable_Tag = Int32.Parse(reader["CT_Enable_Tag"].ToString());
                }
                conn.Close();
            }

            return tray_model;
        }

        public static int insertLogDetails(string username, int state, string machineNo, string usertype)
        {
            int rowCount = 0;
            string eventState = "";
            if (state == 1) eventState = "Logged In";
            else if (state == 2) eventState = "Logged Out";

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("INSERT INTO 'Logging Table'(LT_Number,LT_DateTime,LT_Event_code,LT_Event_ref) values( '" + machineNo + "', datetime('now','localtime'), " + state + ", '" + username + "-" + eventState + "-" + usertype + "' )", conn);


                rowCount = command.ExecuteNonQuery();

                conn.Close();
            }

            return rowCount;
        }

        public static int updateAdminTable(string username, string password, string userrole, string machineNo)
        {
            int rowCount = 0;
            int usernameCheck = getUsernameVerification(username);

            if (usernameCheck == 0)
            {
                using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
                {
                    conn.Open();

                    SQLiteCommand command = new SQLiteCommand("INSERT INTO 'Admin Table' values( '" + machineNo + "', '" + username + "','" + password + "', '" + userrole + "' )", conn);
                    rowCount = command.ExecuteNonQuery();

                    conn.Close();
                }
            }

            else
            {
                using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
                {
                    conn.Open();
                    SQLiteCommand command;

                    if (password.Length > 0)
                        command = new SQLiteCommand("UPDATE 'Admin Table' SET Ad_Password = '" + password + "', Ad_UserType = '" + userrole + "' WHERE Ad_User_Name = '" + username + "';", conn);
                    else
                        command = new SQLiteCommand("UPDATE 'Admin Table' SET Ad_UserType = '" + userrole + "' WHERE Ad_User_Name = '" + username + "';", conn);
                    rowCount = command.ExecuteNonQuery();
                    conn.Close();
                }
            }

            return rowCount;
        }

        public static int disableAllProducts()
        {
            int rowCount = 0;
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                SQLiteCommand command;

                command = new SQLiteCommand("UPDATE 'Cell Table' SET CT_Enable_Tag = 0", conn);
                rowCount = command.ExecuteNonQuery();
                conn.Close();
            }
            

            return rowCount;
        }

        public static int deleteEntryInAdminTable(string username, string machineNo)
        {
            int rowCount = 0;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("DELETE from 'Admin Table' WHERE Ad_User_Name = '" + username + "'", conn);
                rowCount = command.ExecuteNonQuery();
                conn.Close();
            }


            return rowCount;
        }

        public static int getLoginVerification(string username, string password)
        {
            int rowCount = 0;


            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Admin Table' where Ad_User_Name = '" + username
                    + "' and Ad_Password = '" + password + "';", conn);
                SQLiteDataReader reader = command.ExecuteReader();

                rowCount = reader.StepCount;


                reader.Close();
                conn.Close();
            }

            return rowCount;
        }

        public static int getEnabledCellCount()
        {
            int EnabledCellCount = 0;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                
                SQLiteCommand command = new SQLiteCommand("select count(CT_Enable_Tag) as count FROM 'Cell Table' WHERE CT_Enable_Tag = 1", conn);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    EnabledCellCount = Int32.Parse(reader["count"].ToString());
                }


                    reader.Close();
                conn.Close();
            }

            return EnabledCellCount;
        }

        public static int getUsernameVerification(string username)
        {
            int rowCount = 0;


            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Admin Table' where Ad_User_Name = '" + username
                   + "';", conn);
                SQLiteDataReader reader = command.ExecuteReader();

                rowCount = reader.StepCount;


                reader.Close();
                conn.Close();
            }

            return rowCount;
        }

        public static string LoadConnectionString(string code = "Default")
        {
            return ConfigurationManager.ConnectionStrings[code].ConnectionString;
        }

        public static int insertBill(string bill_number, string bill_path, string machineMode)
        {
            int rowCount = 0;
            SQLiteCommand command = new SQLiteCommand();
            if (!findBillNumber(bill_number))
            {
                using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
                {
                    conn.Open();
                    indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                    String formattedTime = indianTime.ToString("yyyy-MM-dd HH:mm:ss");

                    command = new SQLiteCommand("INSERT INTO 'Bills Table'(BT_DateTime,BT_Bill_No,BT_Bill_Image, BT_Mode) values('" + formattedTime + "', '" + bill_number + "', @img, @mode)", conn);
                    command.Parameters.AddWithValue("@img", File.ReadAllBytes(bill_path));
                    command.Parameters.Add(new SQLiteParameter("@mode", machineMode));
                    rowCount = command.ExecuteNonQuery();

                    conn.Close();
                }
            }
            else
            {
                //TODO: Error log
            }
           
            return rowCount;

        }
    }
}
