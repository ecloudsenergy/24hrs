using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using sample2.models;

namespace sample2.remote
{
    class SqliteChange
    {
        static Notifier noti = new Notifier(cfg =>
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

        public static int getEnableStatus(int Row_No, int Col_No)
        {
            int enable_status = 0;


            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Cell Table' WHERE CT_Row_No = '" + Row_No + "' AND  CT_Col_No = '" + Col_No + "'", conn);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    if(reader["CT_Enable_Tag"].ToString() != "")
                        enable_status = Int32.Parse(reader["CT_Enable_Tag"].ToString());
                }
                   
                reader.Close();
                conn.Close();
            }

            return enable_status;
        }

        public static int getMaxQuantity(string Product, int Row_No, int Col_No)
        {
            int Balance = 0;


            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Cell Table' WHERE CT_Product_name = @product AND CT_Row_No = " + Row_No
                    + " AND CT_Col_No = "+ Col_No, conn);
                command.Parameters.Add(new SQLiteParameter("@product", Product));
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    if(reader["CT_Max_Qty"].ToString() != "")
                    Balance = Int32.Parse(reader["CT_Max_Qty"].ToString());
                }
                reader.Close();
                conn.Close();
            }

            return Balance;
        }

        public static int Trayinsert(int CT_Row_No, int CT_Col_No, int CT_Enable_Tag, int CT_Balance_Qty, int CT_Max_Qty, string CT_Product_name)
        {
            int rowCount = 0;
            SQLiteCommand command = new SQLiteCommand();
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                command = new SQLiteCommand("UPDATE 'Cell Table' SET CT_Enable_Tag = '" + CT_Enable_Tag + "',CT_Balance_Qty = '" + CT_Balance_Qty + "',CT_Max_Qty = '" + CT_Max_Qty 
                    + "',CT_Product_name = @product WHERE CT_Row_No = '" + CT_Row_No + "' AND CT_Col_No = '" + CT_Col_No + "';", conn);
                command.Parameters.Add(new SQLiteParameter("@product", CT_Product_name));
                rowCount = command.ExecuteNonQuery();

                conn.Close();
            }
            
            return rowCount;
        }

        public static int UpdateProductTable(string pr_name, int selling_price, int buying_price, double SGST,
            double CGST, double IGST, string expiry_date)
        {
            int rowCount = 0;
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("UPDATE 'Product Table' SET Pr_Selling_Price = " + selling_price + ", Pr_Buying_Price=" + buying_price + ", Pr_SGST=" + SGST
                    + ", Pr_CGST=" + CGST + ", Pr_IGST = " + IGST + ", Pr_Expiry_Date = @expiry WHERE Pr_Name = @product;", conn);
                command.Parameters.Add(new SQLiteParameter("@product", pr_name));
                command.Parameters.Add(new SQLiteParameter("@expiry", expiry_date));
                rowCount = command.ExecuteNonQuery();
                conn.Close();
            }
            if (rowCount > 0)
            {
                noti.ShowSuccess("Successfully Updated!");

            }
            else
            {
                noti.ShowError("Product Update Failed.");
            }

            return rowCount;
        }

        public static int UpdateCellTable(int col, int row, int balance, int registered_balance)
        {
            int rowCount = 0;
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("UPDATE 'Cell Table' SET CT_Balance_Qty = " + balance 
                    + ", CT_Registered_Balance_Qty = "+ registered_balance + " WHERE CT_Row_No = " + row + " AND CT_Col_No = "+ col+" ; ", conn);
                rowCount = command.ExecuteNonQuery();
                conn.Close();
            }
         return rowCount;
        }

        public static int getExistingQty(string Product, int row_no, int col_no)
        {
            int Balance = 0;


            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select CTT_Closing_Stock from 'Cell Transaction Table' WHERE CTT_Product_Name = @product AND CTT_Row_No = " +
                  row_no + " AND CTT_Col_No = "+ col_no + " ORDER BY CTT_DateTime desc LIMIT 1 ", conn);
                command.Parameters.Add(new SQLiteParameter("@product", Product));
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    if(reader["CTT_Closing_Stock"].ToString() != "")
                    Balance = Int32.Parse(reader["CTT_Closing_Stock"].ToString());
                }
                    
                reader.Close();
                conn.Close();
            }

            return Balance;
        }

        public static int InsertIntoTrayTransaction(int Row, int Col,string Product,int Qty,string transactionType, int CTT_Transaction_Price,
            double CTT_SGST, double CTT_CGST, double CTT_IGST, string dateTime, string CTT_Payment_Type, string CTT_Invoice_Req_No, int CTT_Opening_Stock, 
            int CTT_Closing_Stock, int CTT_Transaction_Total_Amount, string CTT_Remarks, string username, string CTT_ExpiryDate, string status, string mode, string extras = "")
        {
            int rowCount = 0;
            SQLiteCommand command = new SQLiteCommand();
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                command =
                    new SQLiteCommand("INSERT INTO 'Cell Transaction Table' (CTT_Row_No, CTT_Col_No,"
                    + "CTT_Product_Name, CTT_Transaction_Qty, CTT_Transaction_Type, CTT_Transaction_Price,CTT_SGST,CTT_CGST,CTT_IGST, CTT_DateTime,"
                    + "CTT_Payment_Type, CTT_Invoice_Req_No,CTT_Opening_Stock,CTT_Closing_Stock,CTT_Transaction_Total_Amount,CTT_Remarks, CTT_Extras,"
                    + "CTT_Username, CTT_ExpiryDate, CTT_Status, CTT_Mode) values" + "(" + Row + ", " + Col + ", @product," + Qty + ", @transType, " 
                    + CTT_Transaction_Price + ", "+ CTT_SGST + ", "+ CTT_CGST + ", " + CTT_IGST + ", @dateTime, @payType, @Inv_No ," 
                    + CTT_Opening_Stock + "," + CTT_Closing_Stock + "," + CTT_Transaction_Total_Amount + ", @remarks, @extras, @username, @expiry, @status, @mode)", conn);

                command.Parameters.Add(new SQLiteParameter("@transType", transactionType));
                command.Parameters.Add(new SQLiteParameter("@product", Product));
                command.Parameters.Add(new SQLiteParameter("@dateTime", dateTime));
                command.Parameters.Add(new SQLiteParameter("@payType", CTT_Payment_Type));
                command.Parameters.Add(new SQLiteParameter("@Inv_No", CTT_Invoice_Req_No));
                command.Parameters.Add(new SQLiteParameter("@remarks", CTT_Remarks));
                command.Parameters.Add(new SQLiteParameter("@username", username));
                command.Parameters.Add(new SQLiteParameter("@expiry", CTT_ExpiryDate));
                command.Parameters.Add(new SQLiteParameter("@extras", extras));
                command.Parameters.Add(new SQLiteParameter("@status", status));
                command.Parameters.Add(new SQLiteParameter("@mode", mode));


                rowCount = command.ExecuteNonQuery();

                conn.Close();
            }
           
            return rowCount;
        }

        public static List<CellModel> getTrayDetails()
        {
            List<CellModel> listTrayModel = new List<CellModel>();
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                String query = "Select * from 'Cell Table'";
                cnn.Open();
                using (var cmd = new SQLiteCommand(query, cnn))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CellModel Tray = new CellModel();
                            if (reader["CT_Row_No"].ToString() != "")
                            {
                                Tray.CT_Row_No = Int32.Parse(reader["CT_Row_No"].ToString());
                                Tray.CT_Col_No = Int32.Parse(reader["CT_Col_No"].ToString());
                                Tray.CT_Enable_Tag = Int32.Parse(reader["CT_Enable_Tag"].ToString());
                                Tray.CT_Balance_Qty = Int32.Parse(reader["CT_Balance_Qty"].ToString());
                                Tray.CT_Max_Qty = Int32.Parse(reader["CT_Max_Qty"].ToString());
                                Tray.CT_Product_name = reader["CT_Product_name"].ToString();
                                listTrayModel.Add(Tray);
                            }
                            
                        }
                    }
                }

            }
            return listTrayModel;
        }


        public static List<ProductTransactionModel> getCurrentCellTransactions(string bill_no)
        {
            List<ProductTransactionModel> currentTransactions = new List<ProductTransactionModel>();
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Open();
                string query = "select * from 'Cell Transaction Table' where CTT_Invoice_Req_No = '"+ bill_no +"' ORDER by CTT_DateTime DESC";

                SQLiteCommand command = new SQLiteCommand(query, cnn);
                command.Parameters.Add(new SQLiteParameter("@bill_no", bill_no));
                SQLiteDataReader reader = command.ExecuteReader();


                while (reader.Read())
                {
                    if (currentTransactions.Count > 0)
                    {
                        if (currentTransactions[0].CTT_DateTime == reader["CTT_DateTime"].ToString())
                        {
                            ProductTransactionModel transDetail = new ProductTransactionModel();

                            transDetail.CTT_Row_No = Int32.Parse(reader["CTT_Row_No"].ToString());
                            transDetail.CTT_Col_No = Int32.Parse(reader["CTT_Col_No"].ToString());
                            transDetail.CTT_Product_Name = reader["CTT_Product_Name"].ToString();
                            transDetail.CTT_Transaction_Price = Int32.Parse(reader["CTT_Transaction_Price"].ToString());
                            transDetail.CTT_Transaction_Qty = Int32.Parse(reader["CTT_Transaction_Qty"].ToString());
                            transDetail.CTT_Transaction_Total_Amount = Int32.Parse(reader["CTT_Transaction_Total_Amount"].ToString());
                            transDetail.CTT_Closing_Stock = Int32.Parse(reader["CTT_Closing_Stock"].ToString());
                            transDetail.CTT_Opening_Stock = Int32.Parse(reader["CTT_Opening_Stock"].ToString());
                            transDetail.CTT_CGST = double.Parse(reader["CTT_CGST"].ToString());
                            transDetail.CTT_IGST = double.Parse(reader["CTT_IGST"].ToString());
                            transDetail.CTT_SGST = double.Parse(reader["CTT_SGST"].ToString());
                            transDetail.CTT_DateTime = reader["CTT_DateTime"].ToString();
                            transDetail.CTT_Invoice_Req_No = reader["CTT_Invoice_Req_No"].ToString();
                            transDetail.CTT_Payment_Type = reader["CTT_Payment_Type"].ToString();
                            transDetail.CTT_Remarks = reader["CTT_Remarks"].ToString();
                            transDetail.CTT_Username = reader["CTT_Username"].ToString();
                            transDetail.CTT_Status = reader["CTT_Status"].ToString();
                            transDetail.CTT_ExpiryDate = reader["CTT_ExpiryDate"].ToString();
                            transDetail.CTT_Extras = reader["CTT_Extras"].ToString();
                            transDetail.CTT_Mode = reader["CTT_Mode"].ToString();
                            transDetail.CTT_Transaction_Type = reader["CTT_Transaction_Type"].ToString();

                            currentTransactions.Add(transDetail);
                        }
                    }
                    else
                    {
                        ProductTransactionModel transDetail = new ProductTransactionModel();

                        transDetail.CTT_Row_No = Int32.Parse(reader["CTT_Row_No"].ToString());
                        transDetail.CTT_Col_No = Int32.Parse(reader["CTT_Col_No"].ToString());
                        transDetail.CTT_Product_Name = reader["CTT_Product_Name"].ToString();
                        transDetail.CTT_Transaction_Price = Int32.Parse(reader["CTT_Transaction_Price"].ToString());
                        transDetail.CTT_Transaction_Qty = Int32.Parse(reader["CTT_Transaction_Qty"].ToString());
                        transDetail.CTT_Transaction_Total_Amount = Int32.Parse(reader["CTT_Transaction_Total_Amount"].ToString());
                        transDetail.CTT_Closing_Stock = Int32.Parse(reader["CTT_Closing_Stock"].ToString());
                        transDetail.CTT_Opening_Stock = Int32.Parse(reader["CTT_Opening_Stock"].ToString());
                        transDetail.CTT_CGST = double.Parse(reader["CTT_CGST"].ToString());
                        transDetail.CTT_IGST = double.Parse(reader["CTT_IGST"].ToString());
                        transDetail.CTT_SGST = double.Parse(reader["CTT_SGST"].ToString());
                        transDetail.CTT_DateTime = reader["CTT_DateTime"].ToString();
                        transDetail.CTT_Invoice_Req_No = reader["CTT_Invoice_Req_No"].ToString();
                        transDetail.CTT_Payment_Type = reader["CTT_Payment_Type"].ToString();
                        transDetail.CTT_Remarks = reader["CTT_Remarks"].ToString();
                        transDetail.CTT_Username = reader["CTT_Username"].ToString();
                        transDetail.CTT_Status = reader["CTT_Status"].ToString();
                        transDetail.CTT_ExpiryDate = reader["CTT_ExpiryDate"].ToString();
                        transDetail.CTT_Extras = reader["CTT_Extras"].ToString();
                        transDetail.CTT_Mode = reader["CTT_Mode"].ToString();
                        transDetail.CTT_Transaction_Type = reader["CTT_Transaction_Type"].ToString();

                        currentTransactions.Add(transDetail);
                    }
                }
                 
            }
            return currentTransactions;
        }

        public static List<CurrencyTransactionModel> getCurrentCurrencyTransactions(string bill_no)
        {
            List<CurrencyTransactionModel> currentTransactions = new List<CurrencyTransactionModel>();
            

            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {

                cnn.Open();
                string query = "select * from 'Currency Transaction Table' where Cr_Invoice_Req_No = @bill_no ORDER by Cr_DateTime DESC";

                SQLiteCommand command = new SQLiteCommand(query, cnn);
                command.Parameters.Add(new SQLiteParameter("@bill_no", bill_no));
                SQLiteDataReader reader = command.ExecuteReader();


                while (reader.Read())
                {
                    if (currentTransactions.Count > 0)
                    {
                        if (currentTransactions[0].Cr_DateTime == reader["Cr_DateTime"].ToString())
                        {
                            CurrencyTransactionModel transDetail = new CurrencyTransactionModel();

                            transDetail.Cr_Opening_Balance_Qty = Int32.Parse(reader["Cr_Opening_Balance_Qty"].ToString());
                            transDetail.Cr_Transaction_Qty = Int32.Parse(reader["Cr_Transaction_Qty"].ToString());
                            transDetail.Cr_Denomination = reader["Cr_Denomination"].ToString();
                            transDetail.Cr_Closing_Balance_Qty = Int32.Parse(reader["Cr_Closing_Balance_Qty"].ToString());
                            transDetail.Cr_Equipment_Used = reader["Cr_Equipment_Used"].ToString();
                            transDetail.Cr_Invoice_Req_No = reader["Cr_Invoice_Req_No"].ToString();
                            transDetail.Cr_Remarks = reader["Cr_Remarks"].ToString();
                            transDetail.Cr_Status = reader["Cr_Status"].ToString();
                            transDetail.Cr_User = reader["Cr_User"].ToString();
                            transDetail.Cr_Transaction_Type = reader["Cr_Transaction_Type"].ToString();
                            transDetail.Cr_DateTime = reader["Cr_DateTime"].ToString();

                            currentTransactions.Add(transDetail);
                        }
                    }
                    else
                    {
                        CurrencyTransactionModel transDetail = new CurrencyTransactionModel();

                        transDetail.Cr_Opening_Balance_Qty = Int32.Parse(reader["Cr_Opening_Balance_Qty"].ToString());
                        transDetail.Cr_Transaction_Qty = Int32.Parse(reader["Cr_Transaction_Qty"].ToString());
                        transDetail.Cr_Denomination = reader["Cr_Denomination"].ToString();
                        transDetail.Cr_Closing_Balance_Qty = Int32.Parse(reader["Cr_Closing_Balance_Qty"].ToString());
                        transDetail.Cr_Equipment_Used = reader["Cr_Equipment_Used"].ToString();
                        transDetail.Cr_Invoice_Req_No = reader["Cr_Invoice_Req_No"].ToString();
                        transDetail.Cr_Remarks = reader["Cr_Remarks"].ToString();
                        transDetail.Cr_Status = reader["Cr_Status"].ToString();
                        transDetail.Cr_User = reader["Cr_User"].ToString();
                        transDetail.Cr_Transaction_Type = reader["Cr_Transaction_Type"].ToString();
                        transDetail.Cr_DateTime = reader["Cr_DateTime"].ToString();

                        currentTransactions.Add(transDetail);
                    }
                }
                cnn.Close();
            }
            return currentTransactions;
        }

        public static List<CellModel> getEnabledCellDetails()
        {
            List<CellModel> listTrayModel = new List<CellModel>();
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                string query = "Select * from 'Cell Table' WHERE CT_Enable_Tag = 1";
                cnn.Open();
                using (var cmd = new SQLiteCommand(query, cnn))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CellModel Tray = new CellModel();
                            if (reader["CT_Row_No"].ToString() != "")
                            {
                                Tray.CT_Row_No = Int32.Parse(reader["CT_Row_No"].ToString());
                                Tray.CT_Col_No = Int32.Parse(reader["CT_Col_No"].ToString());
                                Tray.CT_Enable_Tag = Int32.Parse(reader["CT_Enable_Tag"].ToString());
                                Tray.CT_Balance_Qty = Int32.Parse(reader["CT_Balance_Qty"].ToString());
                                Tray.CT_Max_Qty = Int32.Parse(reader["CT_Max_Qty"].ToString());
                                Tray.CT_Product_name = reader["CT_Product_name"].ToString();
                                listTrayModel.Add(Tray);
                            }

                        }
                    }
                }

            }
            return listTrayModel;
        }


        public static CellModel getCellNumber(string product_name)
        {
            CellModel tDetails = new CellModel();

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Cell Table' WHERE CT_Product_name = @product AND CT_Balance_Qty > 0 ORDER BY CT_Balance_Qty ASC LIMIT 1", conn);
                command.Parameters.Add(new SQLiteParameter("@product", product_name));

                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    if (reader["CT_Row_No"].ToString() != "")
                    {
                        tDetails.CT_Row_No = Int32.Parse(reader["CT_Row_No"].ToString());
                        tDetails.CT_Col_No = Int32.Parse(reader["CT_Col_No"].ToString());
                        tDetails.CT_Enable_Tag = Int32.Parse(reader["CT_Enable_Tag"].ToString());
                        tDetails.CT_Balance_Qty = Int32.Parse(reader["CT_Balance_Qty"].ToString());
                        tDetails.CT_Max_Qty = Int32.Parse(reader["CT_Max_Qty"].ToString());
                        tDetails.CT_Product_name = reader["CT_Product_name"].ToString();
                    }
                }

                reader.Close();
                conn.Close();
            }

            return tDetails;
        }


        public static CellModel getCellNumber_Registered_Bal(int row , int col)
        {
            CellModel tDetails = new CellModel();

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Cell Table' WHERE CT_Row_No = "+ row + " AND CT_Col_No = " + col, conn);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    if (reader["CT_Row_No"].ToString() != "")
                    {
                        tDetails.CT_Row_No = Int32.Parse(reader["CT_Row_No"].ToString());
                        tDetails.CT_Col_No = Int32.Parse(reader["CT_Col_No"].ToString());
                        tDetails.CT_Enable_Tag = Int32.Parse(reader["CT_Enable_Tag"].ToString());
                        tDetails.CT_Balance_Qty = Int32.Parse(reader["CT_Balance_Qty"].ToString());
                        tDetails.CT_Max_Qty = Int32.Parse(reader["CT_Max_Qty"].ToString());
                        tDetails.CT_Product_name = reader["CT_Product_name"].ToString();
                        tDetails.CT_Registered_Balance_Qty = Int32.Parse(reader["CT_Registered_Balance_Qty"].ToString());
                    }
                }

                reader.Close();
                conn.Close();
            }

            return tDetails;
        }

        /*----------------------------- Imtiyaz balance quantity from cell table----------------------------------------------------------- */

        public static int getbalanceqty(string product_name)
        {
            int bal_qty =0;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select CT_Balance_Qty from 'Cell Table' WHERE CT_Product_name = @product", conn);
                command.Parameters.Add(new SQLiteParameter("@product", product_name));
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    bal_qty += int.Parse(reader["CT_Balance_Qty"].ToString());
                }

                reader.Close();
                conn.Close();
            }

            return bal_qty;
        }
        /* -------------------------------------------------------------------------------------------------------------------------------------------*/
        public static CellModel getPrductofCell(int Row,int Col)
        {
            CellModel tDetails = new CellModel();


            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Cell Table' WHERE CT_Row_No = '" + Row + "' AND CT_Col_No = '" + Col +"'", conn);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    if (reader["CT_Row_No"].ToString() != "")
                    {
                        tDetails.CT_Row_No = Int32.Parse(reader["CT_Row_No"].ToString());
                        tDetails.CT_Col_No = Int32.Parse(reader["CT_Col_No"].ToString());
                        tDetails.CT_Enable_Tag = Int32.Parse(reader["CT_Enable_Tag"].ToString());
                        tDetails.CT_Balance_Qty = Int32.Parse(reader["CT_Balance_Qty"].ToString());
                        tDetails.CT_Max_Qty = Int32.Parse(reader["CT_Max_Qty"].ToString());
                        tDetails.CT_Product_name = reader["CT_Product_name"].ToString();
                    }
                }

                reader.Close();
                conn.Close();
            }

            return tDetails;
        }

        public static List<string> getProductNames()
        {
            List<string> ProductNames = new List<string>();


            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Product Table'", conn);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ProductNames.Add(reader["Pr_Name"].ToString());
                }
                conn.Close();
            }

            return ProductNames;
        }
       
        public static int getBalance(string deviceName, int denomination)
        {
            int Balance = 0;


            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Currency Transaction Table' where Cr_Equipment_Used = @deviceName AND Cr_Denomination=" 
                    + denomination + " ORDER BY Cr_DateTime desc LIMIT 1", conn);
                command.Parameters.Add(new SQLiteParameter("@deviceName", deviceName));
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                    Balance = Int32.Parse(reader["Cr_Closing_Balance_Qty"].ToString());
                reader.Close();
                conn.Close();
            }

            return Balance;
        }

        public static string getDeviceName(string portName)
        {
            string deviceName = "";


            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Port Table' where Port_Name = @portName", conn);
                command.Parameters.Add(new SQLiteParameter("@portName", portName));
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                    deviceName = reader["Device_Name"].ToString();
                reader.Close();
                conn.Close();
            }

            return deviceName;
        }

        public static int InsertIntoCurrencyTransactionTable(int denomination, string Invoice_Req_No, int TransCount, string datetime,
            string username, string transtype, int opening_balance_qty, int closing_balance_qty, string equipment_used, string status, string remarks = "")
        {
            int rowCount = 0;
            SQLiteCommand command = new SQLiteCommand();
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                command =
                    new SQLiteCommand("INSERT INTO 'Currency Transaction Table' (Cr_DateTime,Cr_Invoice_Req_No,Cr_Denomination,Cr_Opening_Balance_Qty,Cr_Transaction_Qty,Cr_Transaction_Type,Cr_Closing_Balance_Qty,Cr_Equipment_Used,Cr_User, Cr_Status, Cr_Remarks)"
                    + " values(@datetime, @Inv_No, " + denomination + ","+ opening_balance_qty + ","+ TransCount 
                    + ", @transtype," + closing_balance_qty + ", @equipment_used, @username, @status, @remarks)", conn);

                command.Parameters.Add(new SQLiteParameter("@datetime", datetime));
                command.Parameters.Add(new SQLiteParameter("@Inv_No", Invoice_Req_No));
                command.Parameters.Add(new SQLiteParameter("@username", username));
                command.Parameters.Add(new SQLiteParameter("@transtype", transtype));
                command.Parameters.Add(new SQLiteParameter("@equipment_used", equipment_used));
                command.Parameters.Add(new SQLiteParameter("@status", status));
                command.Parameters.Add(new SQLiteParameter("@remarks", remarks));


                rowCount = command.ExecuteNonQuery();

                conn.Close();
            }

            return rowCount;
        }

        
        public static List<CurrencyTransactionModel> getInProgressCurrencyTransactions(string bill_no, string device_used)
        {
            List<CurrencyTransactionModel> currencyTransactions = new List<CurrencyTransactionModel>();
            SQLiteCommand command = new SQLiteCommand();
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                command =
                    new SQLiteCommand("Select * from 'Currency Transaction Table' WHERE (Cr_Status = 'In Progress' OR Cr_Status = 'Scheduled' OR "
                    + "Cr_Status = 'Pending') AND Cr_Equipment_Used = @device And Cr_Invoice_Req_No = @bill_no ORDER BY Cr_DateTime desc", conn);
                command.Parameters.Add(new SQLiteParameter("@bill_no", bill_no));
                command.Parameters.Add(new SQLiteParameter("@device", device_used));
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {

                    if (currencyTransactions.Count == 0)
                    {
                        CurrencyTransactionModel transModel = new CurrencyTransactionModel();
                        transModel.Cr_Closing_Balance_Qty = int.Parse(reader["Cr_Closing_Balance_Qty"].ToString());
                        transModel.Cr_Denomination = reader["Cr_Denomination"].ToString();
                        transModel.Cr_Opening_Balance_Qty = int.Parse(reader["Cr_Opening_Balance_Qty"].ToString());
                        transModel.Cr_Transaction_Qty = int.Parse(reader["Cr_Transaction_Qty"].ToString());
                        transModel.Cr_Transaction_Type = reader["Cr_Transaction_Type"].ToString();
                        transModel.Cr_Status = reader["Cr_Status"].ToString();
                        transModel.Cr_Remarks = reader["Cr_Remarks"].ToString();
                        transModel.Cr_DateTime = reader["Cr_DateTime"].ToString();
                        transModel.Cr_User = reader["Cr_User"].ToString();
                        transModel.Cr_Invoice_Req_No = reader["Cr_Invoice_Req_No"].ToString();
                        transModel.Cr_Equipment_Used = reader["Cr_Equipment_Used"].ToString();
                        currencyTransactions.Add(transModel);
                    }
                    else if(currencyTransactions[currencyTransactions.Count - 1].Cr_DateTime == reader["Cr_DateTime"].ToString())
                    {
                        CurrencyTransactionModel transModel = new CurrencyTransactionModel();
                        transModel.Cr_Closing_Balance_Qty = int.Parse(reader["Cr_Closing_Balance_Qty"].ToString());
                        transModel.Cr_Denomination = reader["Cr_Denomination"].ToString();
                        transModel.Cr_Opening_Balance_Qty = int.Parse(reader["Cr_Opening_Balance_Qty"].ToString());
                        transModel.Cr_Transaction_Qty = int.Parse(reader["Cr_Transaction_Qty"].ToString());
                        transModel.Cr_Transaction_Type = reader["Cr_Transaction_Type"].ToString();
                        transModel.Cr_Status = reader["Cr_Status"].ToString();
                        transModel.Cr_Remarks = reader["Cr_Remarks"].ToString();
                        transModel.Cr_User = reader["Cr_User"].ToString();
                        transModel.Cr_DateTime = reader["Cr_DateTime"].ToString();
                        transModel.Cr_Invoice_Req_No = reader["Cr_Invoice_Req_No"].ToString();
                        transModel.Cr_Equipment_Used = reader["Cr_Equipment_Used"].ToString();
                        currencyTransactions.Add(transModel);
                    }
                }
                    conn.Close();
            }

            return currencyTransactions;

        }

        public static List<string> getCategories()
        {
            List<string> CtNames = new List<string>();


            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select CT_Name from 'Category Table'", conn);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    CtNames.Add(reader["CT_Name"].ToString());
                }
                conn.Close();
            }

            return CtNames;
        }

        public static int ProductNamecombo(string ProductName)
        {
            int rowCount = 0;


            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Product Table' where Pr_Name = @product;", conn);
                command.Parameters.Add(new SQLiteParameter("@product", ProductName));
                SQLiteDataReader reader = command.ExecuteReader();

                rowCount = reader.StepCount;


                reader.Close();
                conn.Close();
            }

            return rowCount;
        }

        public static int CategoryNamecombo(string CategoryName)
        {
            int rowCount = 0;


            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Category Table' where CT_Name = @category;", conn);
                command.Parameters.Add(new SQLiteParameter("@category", CategoryName));
                SQLiteDataReader reader = command.ExecuteReader();

                rowCount = reader.StepCount;


                reader.Close();
                conn.Close();
            }

            return rowCount;
        }

        public static CategoryModel getCategoryDetails(string name)
        {
            CategoryModel categoryDetail = new CategoryModel();


            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Category Table' WHERE CT_Name = @category", conn);
                command.Parameters.Add(new SQLiteParameter("@category", name));
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    categoryDetail.CT_Name = reader["CT_Name"].ToString();
                    categoryDetail.CT_Image = SqliteDataAccess.getCategoryImage(categoryDetail.CT_Name);
                   
                }

                reader.Close();
                conn.Close();
            }

            return categoryDetail;
        }

        public static int insertCategory(string Cat_name, string Cat_img = "", byte[] image_bytes = null)
        {
            int rowCount = 0;
            SQLiteCommand command = new SQLiteCommand();
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                command = new SQLiteCommand("SELECT * FROM 'Category Table' WHERE CT_Name = @category", conn);
                command.Parameters.Add(new SQLiteParameter("@category", Cat_name));
                SQLiteDataReader reader = command.ExecuteReader();

                rowCount = reader.StepCount;

                if (rowCount == 0)
                {
                    command =
                   new SQLiteCommand("INSERT INTO 'Category Table'(CT_Name,CT_Image) values( @category, @img)", conn);
                    if (Cat_img != null)
                        command.Parameters.AddWithValue("@img", File.ReadAllBytes(Cat_img));
                    else command.Parameters.AddWithValue("@img", image_bytes);
                    command.Parameters.Add(new SQLiteParameter("@category", Cat_name));
                }

                else
                {
                    command =
                   new SQLiteCommand("Update 'Category Table' SET CT_Image = @img where CT_Name = @category", conn);
                    if(Cat_img != null)
                    command.Parameters.AddWithValue("@img", File.ReadAllBytes(Cat_img));
                    else command.Parameters.AddWithValue("@img", image_bytes);
                    command.Parameters.Add(new SQLiteParameter("@category", Cat_name));
                }


                rowCount = command.ExecuteNonQuery();

                conn.Close();
            }
            if (rowCount > 0)
            {
                noti.ShowSuccess("Successfully Added!");

            }
            else
            {
                noti.ShowError("Something went wrong! Please contact Admin!");
            }


            return rowCount;
        }

        public static int insertProductDetails(string pr_name, string pr_Description,
            string pr_Category, string pr_HSN, float pr_Rate, float pr_GST_Rate, float pr_Final_Rate,
            int pr_Min_Qty, string pr_img, string pr_ExpiryDate)
        {
            int rowCount = 0;
            SQLiteCommand command = new SQLiteCommand();
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                command =
                    new SQLiteCommand("INSERT INTO 'Product Table'(Pr_Name,Pr_image,Pr_Description,Pr_HSN,Pr_Category,Pr_ExpDate,Pr_Rate,Pr_GST_Rate,Pr_Final_Rate,Pr_Min_Qty) values( "
                    + " @product, @img, @description, @hsn, @category, @expiry, " + pr_Rate + ", "
                    + pr_GST_Rate + ", " + pr_Final_Rate + ", " + pr_Min_Qty + ")", conn);

                command.Parameters.AddWithValue("@img", File.ReadAllBytes(pr_img));
                command.Parameters.Add(new SQLiteParameter("@product", pr_name));
                command.Parameters.Add(new SQLiteParameter("@description", pr_Description));
                command.Parameters.Add(new SQLiteParameter("@hsn", pr_HSN));
                command.Parameters.Add(new SQLiteParameter("@category", pr_Category));
                command.Parameters.Add(new SQLiteParameter("@expiry", pr_ExpiryDate));
               
                ///INSERT INTO 'Product Table'(Pr_Name,Pr_image,Pr_Description,Pr_HSN,Pr_Category,Pr_ExpDate,
                ///Pr_Rate,Pr_GST_Rate,Pr_Final_Rate,Pr_Min_Qty) values("Dart" , "sample" , "tablet for headache" ,
                ///0001, "Category1", "05-12-2020", 19.00, 0.50, 20.00,1)
                rowCount = command.ExecuteNonQuery();

                conn.Close();
            }
            if (rowCount > 0)
            {
                noti.ShowSuccess("Successfully Added!");

            }
            else
            {
                noti.ShowError("Failed! please contact Admin!");
            }

            return rowCount;
        }

        public static int getPrnameVerification(string Pr_name)
        {
            int rowCount = 0;


            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Product Table' where Pr_Name = @product;", conn);
                command.Parameters.Add(new SQLiteParameter("@product", Pr_name));
                SQLiteDataReader reader = command.ExecuteReader();

                rowCount = reader.StepCount;


                reader.Close();
                conn.Close();
            }

            return rowCount;
        }

        public static int updateCellTransaction(ProductTransactionModel transDetail)
        {
            int rowCount = 0;
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("UPDATE 'Cell Transaction Table' SET CTT_Status = @status, CTT_Remarks = @remarks"
                       + " WHERE CTT_DateTime = @datetime AND CTT_Invoice_Req_No = @billNo;", conn);
                
                command.Parameters.Add(new SQLiteParameter("@status", transDetail.CTT_Status));
                command.Parameters.Add(new SQLiteParameter("@remarks", transDetail.CTT_Remarks));
                command.Parameters.Add(new SQLiteParameter("@datetime", transDetail.CTT_DateTime));
                command.Parameters.Add(new SQLiteParameter("@billNo", transDetail.CTT_Invoice_Req_No));
                rowCount = command.ExecuteNonQuery();
                conn.Close();
            }


                return rowCount;
        }

        public static int updateCurrencyTransaction(CurrencyTransactionModel transDetail)
        {
            int rowCount = 0;
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("UPDATE 'Currency Transaction Table' SET Cr_Status = @status, Cr_Remarks = @remarks"
                       + " WHERE Cr_DateTime = @datetime AND Cr_Invoice_Req_No = @billNo;", conn);

                command.Parameters.Add(new SQLiteParameter("@status", transDetail.Cr_Status));
                command.Parameters.Add(new SQLiteParameter("@remarks", transDetail.Cr_Remarks));
                command.Parameters.Add(new SQLiteParameter("@datetime", transDetail.Cr_DateTime));
                command.Parameters.Add(new SQLiteParameter("@billNo", transDetail.Cr_Invoice_Req_No));
                rowCount = command.ExecuteNonQuery();
                conn.Close();
            }


            return rowCount;
        }

        public static int updateProductTable(string pr_name, string pr_Description,
            string pr_Category, string pr_HSN, float selling_price, float buying_price, float IGST, float CGST, float SGST,
            int pr_Min_Qty, int notify_before, string pr_img="", byte[] img_byte = null )
        {
            int rowCount = 0;
            int PrnameCheck = getPrnameVerification(pr_name);

            if (PrnameCheck == 0 && pr_img != "")
            {
                using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
                {
                    conn.Open();

                    SQLiteCommand command = new SQLiteCommand("INSERT INTO 'Product Table'(Pr_Name,Pr_image,Pr_Description,Pr_HSN,Pr_Category,Pr_Selling_Price,Pr_Buying_Price,Pr_SGST,Pr_CGST,Pr_IGST,Pr_Notify_Before,Pr_Min_Qty) "
                        + "values( @product, @img, @description, @hsn, @category, " + selling_price + ", " + buying_price + ", " 
                        + SGST + ", " + CGST +", " + IGST + ", " + notify_before + ", " + pr_Min_Qty + ")", conn);

                    command.Parameters.AddWithValue("@img", File.ReadAllBytes(pr_img));
                    command.Parameters.Add(new SQLiteParameter("@product", pr_name));
                    command.Parameters.Add(new SQLiteParameter("@description", pr_Description));
                    command.Parameters.Add(new SQLiteParameter("@hsn", pr_HSN));
                    command.Parameters.Add(new SQLiteParameter("@category", pr_Category));
                    rowCount = command.ExecuteNonQuery();

                    conn.Close();
                }
                if (rowCount > 0)
                {
                    noti.ShowSuccess("Successfully Added!");

                }
                else
                {
                    noti.ShowError("Failed! please contact Admin!");
                }
            }

            else
            {
                using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
                {
                    conn.Open();
                    SQLiteCommand command = new SQLiteCommand("UPDATE 'Product Table' SET Pr_image = @img, Pr_Description = @description,"
                        + "Pr_HSN = @hsn,Pr_Category = @category, "
                        + "Pr_Selling_Price = " + selling_price+ ", Pr_Buying_Price=" + buying_price + ", Pr_SGST=" + SGST
                        + ", Pr_CGST=" + CGST + "," + "Pr_IGST = " + IGST+", Pr_Min_Qty = "+pr_Min_Qty + ", Pr_Notify_Before = " + notify_before
                        + " WHERE Pr_Name = @product;", conn);
                    if (pr_img != "")
                    {
                        command.Parameters.AddWithValue("@img", File.ReadAllBytes(pr_img));
                    }
                    else
                        command.Parameters.AddWithValue("@img", img_byte);
                    command.Parameters.Add(new SQLiteParameter("@product", pr_name));
                    command.Parameters.Add(new SQLiteParameter("@description", pr_Description));
                    command.Parameters.Add(new SQLiteParameter("@hsn", pr_HSN));
                    command.Parameters.Add(new SQLiteParameter("@category", pr_Category));
                    rowCount = command.ExecuteNonQuery();
                    conn.Close();
                }
                if (rowCount > 0)
                {
                    noti.ShowSuccess("Successfully Updated!");

                }
                else
                {
                    noti.ShowError("Check All Fields!");
                }
            }

            return rowCount;
        }

        public static int deleteEntryInProductTable(string username)
        {
            int rowCount = 0;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("DELETE from 'Product Table' WHERE Pr_name = @username", conn);
                command.Parameters.Add(new SQLiteParameter("@username", username));
                rowCount = command.ExecuteNonQuery();
                conn.Close();
            }


            return rowCount;
        }

        public static int deleteEntryInCategoryTable(string name)
        {
            int rowCount = 0;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("DELETE from 'Category Table' WHERE CT_Name = @category", conn);
                command.Parameters.Add(new SQLiteParameter("@category", name));
                rowCount = command.ExecuteNonQuery();
                conn.Close();
            }


            return rowCount;
        }

        public static BitmapImage byteArrayToImage(byte[] byteArrayIn)
        {
           
            MemoryStream ms = new MemoryStream(byteArrayIn);
            System.Drawing.Image image_temp = System.Drawing.Image.FromStream(ms); 
            BitmapImage returnImage = ToWpfImage(image_temp);

            return returnImage;
        }

        private static BitmapImage ToWpfImage(System.Drawing.Image img)
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

        public static string LoadConnectionString(string code = "Default")
        {
            return ConfigurationManager.ConnectionStrings[code].ConnectionString;
        }
      
      
      
        public static ProductTransactionModel getLastProductTransaction(string productName, int row, int col)
        {
            ProductTransactionModel LastTransaction = new ProductTransactionModel();

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Cell Transaction Table' WHERE CTT_Product_Name = @product"
                    +" AND CTT_Row_No = " + row + " AND CTT_Col_No = " + col + " ORDER BY CTT_DateTime desc LIMIT 1", conn);
                command.Parameters.Add(new SQLiteParameter("@product", productName));
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    if (reader["CTT_Closing_Stock"].ToString() != "")
                    {
                        LastTransaction.CTT_Closing_Stock = int.Parse(reader["CTT_Closing_Stock"].ToString());
                        LastTransaction.CTT_ExpiryDate = reader["CTT_ExpiryDate"].ToString();
                    }
                }

                reader.Close();
                conn.Close();
            }

            return LastTransaction;
        }

        public static CurrencyTransactionModel getLastCurrencyTransaction(int denomination, string Device)
        {
            CurrencyTransactionModel LastTransaction = new CurrencyTransactionModel();

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Currency Transaction Table' WHERE Cr_Denomination = " 
                    + denomination + " AND Cr_Equipment_Used = @device ORDER BY Cr_DateTime desc LIMIT 1", conn);
                command.Parameters.Add(new SQLiteParameter("@device", Device));
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    if (reader["Cr_Closing_Balance_Qty"].ToString() != "")
                    {
                        LastTransaction.Cr_Closing_Balance_Qty = int.Parse(reader["Cr_Closing_Balance_Qty"].ToString());
                    }
                }



                reader.Close();
                conn.Close();
            }

            return LastTransaction;
        }
    }
}
