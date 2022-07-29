using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using static sample2.models.ReportModel;

namespace sample2.remote
{
    class ReportData
    {

        public static SQLiteConnection conn;
        public static SQLiteCommand command;
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        private static DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        public static List<StockReportModel> getReportDetails(string fromDateString, string toDateString)
        {


            List<StockReportModel> reportList = new List<StockReportModel>();
            using (conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                command = new SQLiteCommand("SELECT ct.CTT_Product_Name as CTT_Product_Name" +
                        ", (select CTT_Closing_Stock from 'Cell Transaction Table' " +
                        "WHERE CTT_Product_Name = ct.CTT_Product_Name and CTT_DateTime < '" + fromDateString + "' " +
                        "ORDER by CTT_DateTime desc) as Opening_Stock, " +
                         "sum(CASE WHEN CTT_Transaction_Type = 'Buying' and CTT_Payment_Type = 'Office' " +
                         "THEN CTT_Transaction_Qty ELSE 0 END) as LoadStock, " +
                         "sum(CASE WHEN CTT_Transaction_Type = 'Selling' and CTT_Payment_Type != 'Office' " +
                         "THEN CTT_Transaction_Qty ELSE 0 END) as SoldStock, " +
                         "sum(CASE WHEN CTT_Transaction_Type = 'Selling' and CTT_Payment_Type = 'Office' " +
                         "THEN CTT_Transaction_Qty ELSE 0 END) as ReturnStock, " +
                         "(select CTT_Closing_Stock from 'Cell Transaction Table' " +
                         "where CTT_Product_Name = ct.CTT_Product_Name and CTT_DateTime " +
                         "between '" + fromDateString + "' and '" + toDateString + "' " +
                         "Order by CTT_DateTime DESC LIMIT 1) as CTT_Closing_Stock ," +
                         "sum(CASE WHEN CTT_Transaction_Type = 'Selling' and CTT_Payment_Type = 'UPI' " +
                         "THEN CTT_Transaction_Qty ELSE 0 END) as UPI_Count, " +
                         "sum(CASE WHEN CTT_Transaction_Type = 'Selling' and CTT_Payment_Type = 'Cash' " +
                         "THEN CTT_Transaction_Qty ELSE 0 END) as Cash_Count " +
                         "FROM 'Cell Transaction Table' ct where CTT_DateTime " +
                         "between '" + fromDateString + "' and '" + toDateString + "' " +
                         "group by ct.CTT_Product_Name order by ct.CTT_Product_Name COLLATE NOCASE ASC; ", conn);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {

                    if (reader.StepCount != 0)
                        while (reader.Read())
                        {

                            StockReportModel reportDetails = new StockReportModel();
                            reportDetails.ProductName = reader["CTT_Product_Name"].ToString();
                            if (reader["Opening_Stock"] != DBNull.Value)
                                reportDetails.OpeningStock = int.Parse(reader["Opening_Stock"].ToString());
                            if (reader["LoadStock"] != DBNull.Value)
                                reportDetails.LoadStock = int.Parse(reader["LoadStock"].ToString());
                            if (reader["SoldStock"] != DBNull.Value)
                                reportDetails.SoldStock = int.Parse(reader["SoldStock"].ToString());
                            if (reader["ReturnStock"] != DBNull.Value)
                                reportDetails.ReturnStock = int.Parse(reader["ReturnStock"].ToString());
                            if (reader["CTT_Closing_Stock"] != DBNull.Value)
                                reportDetails.ClosingStock = int.Parse(reader["CTT_Closing_Stock"].ToString());
                            if (reader["UPI_Count"] != DBNull.Value)
                                reportDetails.UPICount = int.Parse(reader["UPI_Count"].ToString());
                            if (reader["Cash_Count"] != DBNull.Value)
                                reportDetails.CashCount = int.Parse(reader["Cash_Count"].ToString());
                            reportList.Add(reportDetails);


                        }
                }
            }
            return reportList;
        }

        //Report To Display Cash - Aadarsh 07.10.2021
        public static List<CashReportModel> getCashReportDetails(string fromDateString, string toDateString)
        {

            List<CashReportModel> cashList = new List<CashReportModel>();
            using (conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                command = new SQLiteCommand(" SELECT cr.Cr_Denomination as Cr_Denomination,(select Cr_Opening_Balance_Qty " +
                    " from 'Currency Transaction Table' " +
                    " where Cr_Denomination = cr.Cr_Denomination " +
                    " and Cr_Equipment_Used = 'BA' and Cr_DateTime between '" + fromDateString + "' and '" + toDateString + "' LIMIT 1) as Opening_Balance, " +
                    " sum(CASE WHEN Cr_Transaction_Type = 'Credit' and Cr_Equipment_Used = 'BA' THEN Cr_Transaction_Qty ELSE 0 END) as Cash_Collected, " +
                    " sum(CASE WHEN Cr_Transaction_Type = 'Debit' THEN Cr_Transaction_Qty ELSE 0 END) as Cash_Unloaded, " +
                    " (select Cr_Closing_Balance_Qty " +
                    " from 'Currency Transaction Table' " +
                    " where Cr_Denomination = cr.Cr_Denomination " +
                    " and Cr_Equipment_Used = 'BA' and Cr_DateTime between '" + fromDateString + "' and '" + toDateString + "' Order by Cr_DateTime DESC LIMIT 1) as Closing_Balance " +
                    " FROM " +
                    " 'Currency Transaction Table'  cr " +
                    " where " +
                    " Cr_DateTime between '" + fromDateString + "' and '" + toDateString + "'" +
                    " group by cr.Cr_Denomination order by cr.Cr_Denomination COLLATE NOCASE ASC; ", conn);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {

                    if (reader.StepCount != 0)
                        while (reader.Read())
                        {
                            if (reader["Cash_Unloaded"] != DBNull.Value)
                            {
                                CashReportModel cashreportDetails = new CashReportModel();
                                cashreportDetails.Denomination = int.Parse(reader["Cr_Denomination"].ToString());
                                cashreportDetails.OpeningBalance = int.Parse(reader["Opening_Balance"].ToString());
                                cashreportDetails.CashCollected = int.Parse(reader["Cash_Collected"].ToString());
                                cashreportDetails.CashUnloaded = int.Parse(reader["Cash_Unloaded"].ToString());
                                cashreportDetails.ClosingBalance = int.Parse(reader["Closing_Balance"].ToString());
                                cashList.Add(cashreportDetails);
                            }
                        }
                }
            }
            return cashList;
        }

        //Report To Display Total Cash and UPI Amount Collected - Aadarsh 18.10.2021
        public static List<UPICashTransactionModel> gettotalUPIcashCollected(string fromDateString, string toDateString)
        {

            List<UPICashTransactionModel> UPIList = new List<UPICashTransactionModel>();
            using (conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                command = new SQLiteCommand(" SELECT (SELECT sum (CTT_Transaction_Total_Amount) " +
                    " from 'Cell Transaction Table' " +
                    " where CTT_Transaction_Type='Selling' and CTT_Payment_Type = 'UPI' and CTT_DateTime between '" + fromDateString + "' and '" + toDateString + "' ) as Total_UPI_Amount , " +
                    " (SELECT sum(Cr_Denomination*Cr_Transaction_Qty) " +
                    " from 'Currency Transaction Table'" +
                    " where Cr_Transaction_Type = 'Credit' and Cr_Equipment_Used = 'BA' and Cr_DateTime between '" + fromDateString + "' and '" + toDateString + "') as Total_Cash_Collected" +
                    " from 'Currency Transaction Table' cr " +
                    " LIMIT 1 ;", conn);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {

                    if (reader.StepCount != 0)
                        while (reader.Read())
                        {
                            UPICashTransactionModel cashUPIDetails = new UPICashTransactionModel();
                            if (reader["Total_UPI_Amount"] != DBNull.Value)
                                cashUPIDetails.TotalUPIAmount = int.Parse(reader["Total_UPI_Amount"].ToString());
                            else
                                cashUPIDetails.TotalUPIAmount = 0;

                            if (reader["Total_Cash_Collected"] != DBNull.Value)
                                cashUPIDetails.TotalCashCollected = int.Parse(reader["Total_Cash_Collected"].ToString());
                            else
                                cashUPIDetails.TotalCashCollected = 0;

                            UPIList.Add(cashUPIDetails);
                        }
                }
            }
            return UPIList;
        }
        /* public static List<UPICashTransactionModel> gettotaLUPIcashCollected(string fromDateString, string toDateString)

         {

             List<UPICashTransactionModel> UPIList = new List<UPICashTransactionModel>();
             using (conn = new SQLiteConnection(LoadConnectionString()))
             {
                 conn.Open();
                 command = new SQLiteCommand(" SELECT (SELECT sum (CTT_Transaction_Total_Amount) " +
                     " from 'Cell Transaction Table' " +
                     " where CTT_Transaction_Type='Selling' and CTT_Payment_Type = 'UPI' and CTT_DateTime between '" + fromDateString + "' and '" + toDateString + "' ) as Total_UPI_Amount , " +
                     " (SELECT sum(CTT_Transaction_Total_Amount) " +
                     " from 'Cell Transaction Table'" +
                     " where CTT_Transaction_Type = 'Selling' and CTT_Payment_Type = 'Cash' and CTT_DateTime between '" + fromDateString + "' and '" + toDateString + "') as Total_Cash_Collected" +
                     " from 'Cell Transaction Table' ctt " +
                     " LIMIT 1 ;", conn);
                 using (SQLiteDataReader reader = command.ExecuteReader())
                 {

                     if (reader.StepCount != 0)
                         while (reader.Read())
                         {
                             UPICashTransactionModel cashUPIDetails = new UPICashTransactionModel();
                             /*if (reader["Total_UPI_Amount"] != DBNull.Value)

                                 cashUPIDetails.TotalUPIAmount = int.Parse(reader["Total_UPI_Amount"].ToString());
                             else
                                 cashUPIDetails.TotalUPIAmount = 0;*/

        /* if (reader["Total_Cash_Collected"] != DBNull.Value)
             cashUPIDetails.TotalUPIAmount = int.Parse(reader["Total_Cash_Collected"].ToString());
         else
             cashUPIDetails.TotalUPIAmount = 0;

         UPIList.Add(cashUPIDetails);
     }
}
}
return UPIList;
}*/

        //Report To Display Total Cash Dispensed - Aadarsh 18.10.2021
        public static List<CashDispenseModel> gettotalCashDispensed(string fromDateString, string toDateString)
        {

            List<CashDispenseModel> dispenseList = new List<CashDispenseModel>();
            using (conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                command = new SQLiteCommand(" SELECT (Select sum (Cr_Denomination*Cr_Transaction_Qty) " +
                    " From 'Currency Transaction Table' " +
                    " where Cr_Transaction_Type = 'Debit' and Cr_Denomination = '10' and Cr_Equipment_Used = 'ND' and Cr_DateTime between '" + fromDateString + "' and '" + toDateString + "' ) as Total_Cash_Dispensed " +
                    " From 'Currency Transaction Table' cr " +
                    " LIMIT 1 ;", conn);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {

                    if (reader.StepCount != 0)
                        while (reader.Read())
                        {
                            CashDispenseModel cashDispensedDetails = new CashDispenseModel();
                            if (reader["Total_Cash_Dispensed"] != DBNull.Value)
                                cashDispensedDetails.TotalCashDispensed = int.Parse(reader["Total_Cash_Dispensed"].ToString());
                            else
                                cashDispensedDetails.TotalCashDispensed = 0;
                            dispenseList.Add(cashDispensedDetails);
                        }
                }
            }
            return dispenseList;
        }

        public static List<NewArrangementModel> getNewArrangementReportDetails()
        {

            List<NewArrangementModel> newarrangeList = new List<NewArrangementModel>();
            using (conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                command = new SQLiteCommand(" select CT_Row_No , CT_Col_No, CT_Product_name, CT_Max_Qty , CT_Balance_Qty " +
                    "from 'Cell Table' WHERE CT_Enable_Tag = 1 ", conn);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {

                    if (reader.StepCount != 0)
                        while (reader.Read())
                        {
                            if (reader["CT_Row_No"] != DBNull.Value)
                            {
                                NewArrangementModel arrangementDetails = new NewArrangementModel();
                                arrangementDetails.CT_Row_No = int.Parse(reader["CT_Row_No"].ToString());
                                arrangementDetails.CT_Col_No = int.Parse(reader["CT_Col_No"].ToString());
                                arrangementDetails.CT_Product_name = reader["CT_Product_name"].ToString();
                                arrangementDetails.CT_Max_Qty = int.Parse(reader["CT_Max_Qty"].ToString());
                                arrangementDetails.CT_Balance_Qty = int.Parse(reader["CT_Balance_Qty"].ToString());
                                newarrangeList.Add(arrangementDetails);
                            }
                        }
                }
            }
            return newarrangeList;
        }

        public static List<UnsoldStockModel> UnsoldReportDetails(string fromDateString, string toDateString)
        {

            List<UnsoldStockModel> UnsoldStockList = new List<UnsoldStockModel>();
            using (conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                command = new SQLiteCommand(" select CT_Product_name as CTT_Product_Name , CT_Balance_Qty as Opening_Stock, " +
                    " (0) as Load_Stock, (0) as Sold_Stock, (0) as Return_Stock, (0) as Closing_Stock, (0) as UPI_Count,(0) as Cash_Count" +
                    " From(select *" +
                    " from 'Cell Table'" +
                    " WHERE CT_Enable_Tag = 1 and CT_Product_name NOT in (SELECT CTT_Product_Name" +
                    " FROM" +
                    " (SELECT ct.CTT_Product_Name as CTT_Product_Name," +
                    " (select CTT_Opening_Stock" +
                    " from 'cell transaction table'" +
                    " where CTT_Product_Name = ct.CTT_Product_Name and CTT_DateTime" +
                    " between '" + fromDateString + "' and '" + toDateString + "' LIMIT 1" +
                    " ) as Opening_Stock," +
                    " sum(CASE" +
                    " WHEN" +
                    " CTT_Transaction_Type = 'Buying' and CTT_Payment_Type = 'Office'" +
                    " THEN CTT_Transaction_Qty ELSE 0" +
                    " END) as LoadStock," +
                    " sum(CASE" +
                    " WHEN CTT_Transaction_Type = 'Selling' and CTT_Payment_Type != 'Office'" +
                    " THEN CTT_Transaction_Qty ELSE 0" +
                    " END) as SoldStock," +
                    " sum(CASE" +
                    " WHEN CTT_Transaction_Type = 'Selling' and CTT_Payment_Type = 'Office'" +
                    " THEN CTT_Transaction_Qty ELSE 0" +
                    " END) as ReturnStock," +
                    " (" +
                    " select CTT_Closing_Stock" +
                    " from 'Cell Transaction Table'" +
                    " where CTT_Product_Name = ct.CTT_Product_Name and CTT_DateTime" +
                    " between '" + fromDateString + "' and '" + toDateString + "'" +
                    " Order by CTT_DateTime DESC LIMIT 1) as CTT_Closing_Stock," +
                    " sum(CASE" +
                    " WHEN CTT_Transaction_Type = 'Selling' and CTT_Payment_Type = 'UPI'" +
                    " THEN CTT_Transaction_Qty ELSE 0" +
                    " END) as UPI_Count," +
                    " sum(CASE" +
                    " WHEN CTT_Transaction_Type = 'Selling' and CTT_Payment_Type = 'Cash'" +
                    " THEN CTT_Transaction_Qty ELSE 0" +
                    " END) as Cash_Count" +
                    " FROM 'Cell Transaction Table' ct" +
                    " where CTT_DateTime between '" + fromDateString + "' and '" + toDateString + "'" +
                    " group by ct.CTT_Product_Name" +
                    " order by ct.CTT_Product_Name COLLATE NOCASE ASC) as table_A) ) ", conn);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {

                    if (reader.StepCount != 0)
                        while (reader.Read())
                        {
                            if (reader["Opening_Stock"] != DBNull.Value)
                            {
                                UnsoldStockModel unsoldreportDetails = new UnsoldStockModel();
                                unsoldreportDetails.CTT_Product_Name = reader["CTT_Product_Name"].ToString();
                                unsoldreportDetails.Opening_Stock = int.Parse(reader["Opening_Stock"].ToString());
                                unsoldreportDetails.Load_Stock = int.Parse(reader["Load_Stock"].ToString());
                                unsoldreportDetails.Sold_Stock = int.Parse(reader["Sold_Stock"].ToString());
                                unsoldreportDetails.Return_Stock = int.Parse(reader["Return_Stock"].ToString());
                                unsoldreportDetails.Closing_Stock = int.Parse(reader["Closing_Stock"].ToString());
                                unsoldreportDetails.UPI_Count = int.Parse(reader["UPI_Count"].ToString());
                                unsoldreportDetails.Cash_Count = int.Parse(reader["Cash_Count"].ToString());
                                UnsoldStockList.Add(unsoldreportDetails);
                            }
                        }
                }
            }
            return UnsoldStockList;
        }

        public static string LoadConnectionString(string code = "Default")
        {
            return ConfigurationManager.ConnectionStrings[code].ConnectionString;
        }

        ~ReportData()
        {
            conn.Close();
        }
    }
}
