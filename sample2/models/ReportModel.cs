using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sample2.models
{
    class ReportModel
    {
        //Aadarsh - 07-10-2021
        public class StockReportModel
        {
            public string ProductName { get; set; }
            public int OpeningStock { get; set; }
            public int LoadStock { get; set; }
            public int SoldStock { get; set; }
            public int ReturnStock { get; set; }
            public int ClosingStock { get; set; }
            public int UPICount { get; set; }
            public int CashCount { get; set; }
        }

        //Aadarsh - 07-10-2021
        public class CashReportModel
        {
            public int Denomination { get; set; }
            public int OpeningBalance { get; set; }
            public int CashCollected { get; set; }
            public int CashUnloaded { get; set; }
            public int ClosingBalance { get; set; }
        }

        public class UPICashTransactionModel //Aadarsh - 18-10-2021
        {
            public int TotalUPIAmount { get; set; }
            public int TotalCashCollected { get; set; }
        }

        public class CashDispenseModel  //Aadarsh - 18-10-2021
        {
            public int TotalCashDispensed { get; set; }
        }

        //Aadarsh 03-11-2021
        public class NewArrangementModel
        {
            public int CT_Row_No { get; set; }
            public int CT_Col_No { get; set; }
            public String CT_Product_name { get; set; }
            public int CT_Max_Qty { get; set; }
            public int CT_Balance_Qty { get; set; }
        }

        public class UnsoldStockModel
        {
            public string CTT_Product_Name { get; set; }
            public int Opening_Stock { get; set; }
            public int Load_Stock { get; set; }
            public int Sold_Stock { get; set; }
            public int Return_Stock { get; set; }
            public int Closing_Stock { get; set; }
            public int UPI_Count { get; set; }
            public int Cash_Count { get; set; }
        }
    }
}
