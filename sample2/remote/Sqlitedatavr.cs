using sample2.models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sample2.remote
{
    class Sqlitedatavr
    {
        public static int insertIntoMachineMaster(string col_names, string content, string pr_img = "", byte[] img_byte = null)
        {
            int rowCount = 0;
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                SQLiteCommand command;
                if (getMachineNo().Length == 0)
                {
                    command =
                      new SQLiteCommand("INSERT INTO 'Machine Master Table'(" + col_names + ") values( " + content + ")", conn);
                }
                else
                {
                    List<string> col_names_list = col_names.Split(',').ToList();
                    List<string> values_list = content.Split(',').ToList();
                    string content_update = "";
                    for(int i=0; i < col_names_list.Count; i++)
                    {
                        string _content = col_names_list[i].Trim() + " = " + values_list[i].Trim();

                        if (i == (col_names_list.Count - 1))
                            content_update += _content;
                        else
                            content_update += _content + ", ";
                    }
                    

                    command =
                         new SQLiteCommand("Update 'Machine Master Table' SET "+ content_update, conn);
                }
                if (pr_img != "")
                    command.Parameters.AddWithValue("@img", File.ReadAllBytes(pr_img));
                else if(img_byte.Length > 0)
                    command.Parameters.AddWithValue("@img", img_byte);
                rowCount = command.ExecuteNonQuery();
                conn.Close();
            }

            return rowCount;
        }


        public static int getBillDenomination()
        {

            int denomination = 0;
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("Select * from 'Machine Master Table'", conn);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {

                    if (reader.StepCount != 0)
                        while (reader.Read())
                        {
                            denomination = int.Parse(reader["MM_Bill_Dispenser"].ToString());
                        }

                }
                conn.Close();
            }
            return denomination;
        }

        public static int getCoinDenomination()
        {

            int denomination = 0;
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("Select * from 'Machine Master Table'", conn);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {

                    if (reader.StepCount != 0)
                        while (reader.Read())
                        {
                            denomination = int.Parse(reader["MM_Coin_Dispenser"].ToString());
                        }

                }
                conn.Close();
            }
            return denomination;
        }
       

        public static int get_motor_timer()
        {

            int Timer  = 0;
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("Select * from 'Machine Master Table'", conn);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {

                    if (reader.StepCount != 0)
                        while (reader.Read())
                        {
                            Timer = int.Parse(reader["MM_Motor_time_ms"].ToString());
                        }

                }
                conn.Close();
            }
            return Timer;
        }

        public static int getProductsSold(string fromDateString, string toDateString, string user = "Customer")
        {
            int rowCount = 0;
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("Select * from 'Cell Transaction Table' WHERE CTT_DateTime <= '"
                    + toDateString + "' AND CTT_DateTime >= '" + fromDateString + "' AND CTT_Username = '" + user + "'", conn);
                rowCount = command.ExecuteNonQuery();
                conn.Close();
            }
            return rowCount;
        }

        public static int getCollectedAmount(string fromDateString, string toDateString, string pay_type, string user = "Customer")
        {
            int collectedAmount = 0;
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("Select * from 'Cell Transaction Table' WHERE CTT_DateTime <= '"
                    + toDateString + "' AND CTT_DateTime >= '" + fromDateString + "' AND CTT_Username = '" + user + "' AND CTT_Payment_Type = '"+pay_type+"'", conn);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {

                    if (reader.StepCount != 0)
                        while (reader.Read())
                        {
                            collectedAmount += int.Parse(reader["CTT_Transaction_Price"].ToString());
                        }
                    
                }
                conn.Close();
            }
            return collectedAmount;
        }


        public static List<ProductTransactionModel> getTransactionDetails(string fromDateString, string toDateString, string user = "Customer")
        {
            List<ProductTransactionModel> transactionDetails = new List<ProductTransactionModel>();

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open(); 
                SQLiteCommand command = new SQLiteCommand("Select * from 'Cell Transaction Table' WHERE CTT_DateTime <= '"
                    + toDateString + "' AND CTT_DateTime >= '" + fromDateString + "' AND CTT_Username = '" + user + "'", conn);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {

                    if (reader.StepCount != 0)
                        while (reader.Read())
                        {
                            ProductTransactionModel productDetails = new ProductTransactionModel();
                            productDetails.CTT_DateTime = reader["CTT_DateTime"].ToString();
                            productDetails.CTT_Payment_Type = reader["CTT_Payment_Type"].ToString();
                            productDetails.CTT_Product_Name = reader["CTT_Product_Name"].ToString();
                            productDetails.CTT_Transaction_Qty = int.Parse(reader["CTT_Transaction_Qty"].ToString());
                            productDetails.CTT_Transaction_Total_Amount = int.Parse(reader["CTT_Transaction_Total_Amount"].ToString());
                            productDetails.CTT_Transaction_Price = int.Parse(reader["CTT_Transaction_Price"].ToString());
                            transactionDetails.Add(productDetails);
                        }
                }

                    conn.Close();
            }
            return transactionDetails;
        }

        public static List<CurrencyTransactionModel> getCurrencyDetails(string fromDateString, string toDateString,
            int Denomination, string DeviceUsed, string user = "Customer")
        {
            List<CurrencyTransactionModel> transactionDetails = new List<CurrencyTransactionModel>();

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("Select * from 'Currency Transaction Table' WHERE Cr_DateTime <= '"+ toDateString 
                    + "' AND Cr_DateTime >= '"+ fromDateString +"' AND Cr_Equipment_Used = '" + DeviceUsed 
                    +"' AND Cr_Denomination = '"+ Denomination +"' AND Cr_User = '" + user +"'", conn);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.StepCount != 0)
                        while (reader.Read())
                        {
                            CurrencyTransactionModel currencyDetails = new CurrencyTransactionModel();
                            currencyDetails.Cr_DateTime = reader["Cr_DateTime"].ToString();
                            currencyDetails.Cr_Denomination = reader["Cr_Denomination"].ToString();
                            currencyDetails.Cr_Equipment_Used = reader["Cr_Equipment_Used"].ToString();
                            currencyDetails.Cr_Invoice_Req_No = reader["Cr_Invoice_Req_No"].ToString();
                            currencyDetails.Cr_Transaction_Type = reader["Cr_Transaction_Type"].ToString();
                            currencyDetails.Cr_Transaction_Qty = int.Parse(reader["Cr_Transaction_Qty"].ToString());
                            currencyDetails.Cr_Opening_Balance_Qty = int.Parse(reader["Cr_Opening_Balance_Qty"].ToString());
                            currencyDetails.Cr_Closing_Balance_Qty = int.Parse(reader["Cr_Closing_Balance_Qty"].ToString());
                            transactionDetails.Add(currencyDetails);
                        }
                }

                conn.Close();
            }
            return transactionDetails;
        }

        public static string getMachineNo()
        {
            string Machine_number = "";


            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Machine Master Table'", conn);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    if(reader["MM_Machine_No"] != DBNull.Value)
                        Machine_number = reader["MM_Machine_No"].ToString();
                }
                    

                reader.Close();
                conn.Close();
            }

            return Machine_number;
        }

        public static string getMachineMode()
        {
            string Machine_mode = "";


            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("Select * from 'Machine Master Table'", conn);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    if (reader["MM_Mode"] != DBNull.Value)
                        Machine_mode = reader["MM_Mode"].ToString();
                }


                reader.Close();
                conn.Close();
            }

            return Machine_mode;
        }


        public static string LoadConnectionString(string code = "Default")
        {
            return ConfigurationManager.ConnectionStrings[code].ConnectionString;
        }
    }
}
