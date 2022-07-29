using sample2.models;
using sample2.remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.Windows;
using static sample2.models.ReportModel;
using System.Threading;

namespace sample2.reports
{
    class StockReport
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        private static DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        public void reportContent(string fromDateString, string toDateString)
        {
            List<StockReportModel> reportList = ReportData.getReportDetails(fromDateString, toDateString);
            List<UnsoldStockModel> UnsoldStockList = ReportData.UnsoldReportDetails(fromDateString, toDateString);
            List<CashReportModel> cashList = ReportData.getCashReportDetails(fromDateString, toDateString);
            List<UPICashTransactionModel> UPIList = ReportData.gettotalUPIcashCollected(fromDateString, toDateString);
            List<CashDispenseModel> dispenseList = ReportData.gettotalCashDispensed(fromDateString, toDateString);
            printReport(reportList, UnsoldStockList, cashList, UPIList, dispenseList, fromDateString, toDateString);
            MessageBox.Show("Report Downloaded Successfully!");
        }

        public void printReport(List<StockReportModel> reportList, List<UnsoldStockModel> UnsoldStockList, List<CashReportModel> cashList, List<UPICashTransactionModel> UPIList, List<CashDispenseModel> dispenseList, string fromDateString, string toDateString)
        {
            Excel.Application excel;
            Excel._Workbook workbook;
            Excel._Worksheet worksheet;
            Excel._Worksheet worksheet2;
            Excel.Range range;

            try
            {
                //Start Excel and get Application object.
                excel = new Excel.Application();
                
                
                //Get a new workbook. Author : Aadarsh K Last Updated - 12-10-2021
                workbook = (Excel._Workbook)(excel.Workbooks.Add(Missing.Value));
                worksheet = (Excel._Worksheet)workbook.ActiveSheet;
                worksheet2 = (Excel._Worksheet)workbook.ActiveSheet;
                workbook.Sheets.Add(After: worksheet);
                worksheet = (Excel._Worksheet)workbook.Worksheets["Sheet1"];
                worksheet.Name = "Stock Report";
                worksheet2 = (Excel._Worksheet)workbook.Worksheets["Sheet2"];
                worksheet2.Name = "Cash Report";
                string ComputerName = Environment.UserName.ToString();
                string filename = "StockReport" + DateTime.Now.ToString("ddMMyyyyhhmm") + ".xlsx";
                workbook.SaveAs("C:\\Users\\" + ComputerName + "\\Desktop\\Reports\\Stock Reports\\" + filename);
                workbook.Saved = true;
                // NOTE: Before Closing the excel application, kindly save the file and close.


                //Add table headers going cell by cell. - For Sheet 1
                worksheet.Cells[3, 2] = "From";
                worksheet.Cells[3, 3] = fromDateString;
                worksheet.Cells[3, 4] = "To";
                worksheet.Cells[3, 5] = toDateString;
                worksheet.Cells[3, 6] = "Report Date";
                worksheet.Cells[3, 7] = indianTime.ToString();
                worksheet.Cells[3, 8] = "Machine No:";
                worksheet.Cells[3, 9] = Sqlitedatavr.getMachineNo();
                worksheet.Cells[4, 1] = "Product Name";
                worksheet.Cells[4, 2] = "Opening Stock";
                worksheet.Cells[4, 3] = "Load Stock";
                worksheet.Cells[4, 4] = "Sold Stock";
                worksheet.Cells[4, 5] = "Return Stock";
                worksheet.Cells[4, 6] = "Closing Stock";
                worksheet.Cells[4, 7] = "Pay Count : UPI";
                worksheet.Cells[4, 8] = "Pay Count : Cash";

                //Add Table Headers Going Cell By Cell  - For Sheet 2
                worksheet2.Cells[3, 1] = "From";
                worksheet2.Cells[3, 2] = fromDateString;
                worksheet2.Cells[3, 3] = "To";
                worksheet2.Cells[3, 4] = toDateString;
                worksheet2.Cells[3, 5] = "Report Date";
                worksheet2.Cells[3, 6] = indianTime.ToString();
                worksheet.Cells[3, 8] = "Machine No:";
                worksheet.Cells[3, 9] = Sqlitedatavr.getMachineNo();
                worksheet2.Cells[4, 1] = "Denomination";
                worksheet2.Cells[4, 2] = "Opening_Balance";
                worksheet2.Cells[4, 3] = "Cash_Collected";
                worksheet2.Cells[4, 4] = "Cash_Unloaded";
                worksheet2.Cells[4, 5] = "Closing Balance";
                worksheet2.Cells[12, 1] = "Total_UPI_Amount";
                worksheet2.Cells[13, 1] = "Total_Cash_Collected";
                worksheet2.Cells[14, 1] = "Total_Cash_Dispensed";
                worksheet2.Cells[15, 1] = "Gross Amount";
                worksheet2.Cells[16, 1] = "Net Amount";


                // MERGE CELLS - Developed by Aadarsh - 07.10.2021 
                worksheet.get_Range("A1", "H2").Merge(false);
                //worksheet.Shapes.AddPicture("F:\\Aadarsh\\25092021\\sample2\\images\\24hrs Yellow with green background.png", Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoCTrue, 50, 50, 300, 45);
                range = worksheet.get_Range("A1", "H2");
                string Name = "24 HRS STOCK INVENTORY REPORT";
                range.FormulaR1C1 = Name;
                range.HorizontalAlignment = 3;
                range.VerticalAlignment = 3;
                range.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Green);
                range.Font.Size = 20;
                worksheet.Cells[1, 2] = "Green";

                //Merge Cells for Sheet 2
                worksheet2.get_Range("A1", "F2").Merge(false);
                //worksheet.Shapes.AddPicture("F:\\Aadarsh\\25092021\\sample2\\images\\24hrs Yellow with green background.png", Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoCTrue, 50, 50, 300, 45);
                range = worksheet2.get_Range("A1", "F2");
                string Name2 = "24 HRS CASH COLLECTION REPORT";
                range.FormulaR1C1 = Name2;
                range.HorizontalAlignment = 3;
                range.VerticalAlignment = 3;
                range.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Green);
                range.Font.Size = 20;
                worksheet2.Cells[1, 2] = "Green";

                //Format A1:D1 as bold, vertical alignment = center.
                worksheet.get_Range("A4", "H4").Font.Bold = true;
                worksheet.get_Range("B3").Font.Bold = true;
                worksheet.get_Range("D3").Font.Bold = true;
                worksheet.get_Range("F3").Font.Bold = true;
                worksheet.get_Range("A4", "H4").VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                worksheet.Columns.WrapText = true;
                worksheet.Columns.ColumnWidth = 20;

                //Format for Sheet 2 as Bold

                worksheet2.get_Range("A4", "G4").Font.Bold = true;
                worksheet2.get_Range("A3").Font.Bold = true;
                worksheet2.get_Range("C3").Font.Bold = true;
                worksheet2.get_Range("E3").Font.Bold = true;
                worksheet2.get_Range("A4", "G4").VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                worksheet2.Columns.WrapText = true;
                worksheet2.Columns.ColumnWidth = 20;


                //A1 - Product Name - Changed by Aadarsh - 27.09.2021 for Displaying Products that are recently got Transactioned
                int row_count = 0;
                string[,] productname = new string[reportList.Count, 1];
                int[,] Opening_Stock = new int[reportList.Count, 1];
                int[,] Load_Stock = new int[reportList.Count, 1];
                int[,] Sold_Stock = new int[reportList.Count, 1];
                int[,] Return_Stock = new int[reportList.Count, 1];
                int[,] Closing_Stock = new int[reportList.Count, 1];
                int[,] UPI_Count = new int[reportList.Count, 1];
                int[,] Cash_Count = new int[reportList.Count, 1];

                 
                //Excel Row positioning
                foreach (var reportDetails in reportList)
                {
                    // Create an array to multiple values at once.
                    productname[row_count, 0] = reportDetails.ProductName;
                    Opening_Stock[row_count, 0] = reportDetails.OpeningStock;
                    Load_Stock[row_count, 0] = reportDetails.LoadStock;
                    Sold_Stock[row_count, 0] = reportDetails.SoldStock;
                    Return_Stock[row_count, 0] = reportDetails.ReturnStock;
                    Closing_Stock[row_count, 0] = reportDetails.ClosingStock;
                    UPI_Count[row_count, 0] = reportDetails.UPICount;
                    Cash_Count[row_count, 0] = reportDetails.CashCount;
                    row_count++;
                }

                int report_count = reportList.Count + 4;

                //UnsoldStockReport  - in sheet 1 17.11.2021 - By Aadarsh
                row_count = 0;
                string[,] ProductName = new string[UnsoldStockList.Count, 1];
                int[,] OpeningStock = new int[UnsoldStockList.Count, 1];
                int[,] LoadStock = new int[UnsoldStockList.Count, 1];
                int[,] SoldStock = new int[UnsoldStockList.Count, 1];
                int[,] ReturnStock = new int[UnsoldStockList.Count, 1];
                int[,] ClosingStock = new int[UnsoldStockList.Count, 1];
                int[,] UPICount = new int[UnsoldStockList.Count, 1];
                int[,] CashCount = new int[UnsoldStockList.Count, 1];


                //Excel Row positioning
                foreach (var unsoldreportDetails in UnsoldStockList)
                {
                    // Create an array to multiple values at once.
                    ProductName[row_count, 0] = unsoldreportDetails.CTT_Product_Name;
                    OpeningStock[row_count, 0] = unsoldreportDetails.Opening_Stock;
                    LoadStock[row_count, 0] = unsoldreportDetails.Load_Stock;
                    SoldStock[row_count, 0] = unsoldreportDetails.Sold_Stock;
                    ReturnStock[row_count, 0] = unsoldreportDetails.Return_Stock;
                    ClosingStock[row_count, 0] = unsoldreportDetails.Closing_Stock;
                    UPICount[row_count, 0] = unsoldreportDetails.UPI_Count;
                    CashCount[row_count, 0] = unsoldreportDetails.Cash_Count;
                    row_count++;
                }

                int unsoldcount = report_count;
                int unsold_Stock_report_count = UnsoldStockList.Count + unsoldcount - 1;


                row_count = 0;
                int[,] Denomination = new int[cashList.Count, 1];
                int[,] Opening_Balance = new int[cashList.Count, 1];
                int[,] Cash_Collected = new int[cashList.Count, 1];
                int[,] Cash_Unloaded = new int[cashList.Count, 1];
                int[,] Closing_Balance = new int[cashList.Count, 1];

                foreach (var cashDetails in cashList)
                {
                    Denomination[row_count, 0] = cashDetails.Denomination;
                    Opening_Balance[row_count, 0] = cashDetails.OpeningBalance;
                    Cash_Collected[row_count, 0] = cashDetails.CashCollected;
                    Cash_Unloaded[row_count, 0] = cashDetails.CashUnloaded;
                    Closing_Balance[row_count, 0] = cashDetails.ClosingBalance;
                    row_count++;
                }

                //Excel Row Positioning on sheet 2 

                int cash_count = cashList.Count + 4;

                row_count = 0;
                int[,] Total_Cash_Collected = new int[UPIList.Count, 1];
                int[,] Total_UPI_Amount = new int[UPIList.Count, 1];

                foreach (var cashUPIDetails in UPIList)
                {
                    Total_UPI_Amount[row_count, 0] = cashUPIDetails.TotalUPIAmount;
                    Total_Cash_Collected[row_count, 0] = cashUPIDetails.TotalCashCollected;
                    row_count++;
                }

               


                row_count = 0;
                int[,] Total_Cash_Dispensed = new int[dispenseList.Count, 1];
                foreach (var dispenseDetails in dispenseList)
                {
                    Total_Cash_Dispensed[row_count, 0] = dispenseDetails.TotalCashDispensed;
                    row_count++;
                }

                int dispense_count = dispenseList.Count + 4;

                int total_count = unsold_Stock_report_count + 2; // this count helps to Print Total count after 1 cell dynamically




                //Fill A5:E5 with an array of values (First and Last Names).
                worksheet.get_Range("A5", "A" + report_count).Value2 = productname;
                worksheet.get_Range("B5", "B" + report_count).Value2 = Opening_Stock;
                worksheet.get_Range("C5", "C" + report_count).Value2 = Load_Stock;
                worksheet.get_Range("D5", "D" + report_count).Value2 = Sold_Stock;
                worksheet.get_Range("E5", "E" + report_count).Value2 = Return_Stock;
                worksheet.get_Range("F5", "F" + report_count).Value2 = Closing_Stock;
                worksheet.get_Range("G5", "G" + report_count).Value2 = UPI_Count;
                worksheet.get_Range("H5", "H" + report_count).Value2 = Cash_Count;

                // Fill A5:B14  - Aadarsh
                worksheet2.get_Range("A5", "A" + cash_count).Value2 = Denomination;
                worksheet2.get_Range("B5", "B" + cash_count).Value2 = Opening_Balance;
                worksheet2.get_Range("C5", "C" + cash_count).Value2 = Cash_Collected;
                worksheet2.get_Range("D5", "D" + cash_count).Value2 = Cash_Unloaded;
                worksheet2.get_Range("E5", "E" + cash_count).Value2 = Closing_Balance;
                worksheet2.get_Range("B12").Value2 = Total_UPI_Amount;
                worksheet2.get_Range("B13").Value2 = Total_Cash_Collected;
                worksheet2.get_Range("B14").Value2 = Total_Cash_Dispensed;
                worksheet2.get_Range("B15").Formula = "=SUM(B12:B13)";//Net Amount
                worksheet2.get_Range("B16").Formula = "=B15-B14";//Gross Amount
                worksheet.get_Range("A" + total_count).Formula = "TOTAL";
                worksheet.get_Range("B" + total_count).Formula = "=sum(B5:B" + unsold_Stock_report_count + ")";
                worksheet.get_Range("C" + total_count).Formula = "=sum(C5:C" + unsold_Stock_report_count + ")";
                worksheet.get_Range("D" + total_count).Formula = "=sum(D5:D" + unsold_Stock_report_count + ")";
                worksheet.get_Range("E" + total_count).Formula = "=sum(E5:E" + unsold_Stock_report_count + ")";
                worksheet.get_Range("F" + total_count).Formula = "=sum(F5:F" + unsold_Stock_report_count + ")";

                worksheet.get_Range("A" + unsoldcount, "A" + unsold_Stock_report_count).Value2 = ProductName;
                worksheet.get_Range("B" + unsoldcount, "B" + unsold_Stock_report_count).Value2 = OpeningStock;
                worksheet.get_Range("C" + unsoldcount, "C" + unsold_Stock_report_count).Value2 = LoadStock;
                worksheet.get_Range("D" + unsoldcount, "D" + unsold_Stock_report_count).Value2 = SoldStock;
                worksheet.get_Range("E" + unsoldcount, "E" + unsold_Stock_report_count).Value2 = ReturnStock;
                worksheet.get_Range("F" + unsoldcount, "F" + unsold_Stock_report_count).Value2 = ClosingStock;
                worksheet.get_Range("G" + unsoldcount, "G" + unsold_Stock_report_count).Value2 = UPICount;
                worksheet.get_Range("H" + unsoldcount, "H" + unsold_Stock_report_count).Value2 = CashCount;

                excel.UserControl = true;
                workbook.Save();

                workbook.Close(0);
                excel.Quit();
            }
            catch (Exception theException)
            {
                String errorMessage;
                errorMessage = "Error: ";
                errorMessage = String.Concat(errorMessage, theException.Message);
                errorMessage = String.Concat(errorMessage, " Line: ");
                errorMessage = String.Concat(errorMessage, theException.Source);

                MessageBox.Show(errorMessage, "Error");
            }
        }
    }
}
