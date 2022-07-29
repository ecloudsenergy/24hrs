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

namespace sample2.reports
{
    class NewArrangementReport
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        private static DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        public void reportContent(string fromDateString, string toDateString)
        {
            List<NewArrangementModel> newarrangeList = ReportData.getNewArrangementReportDetails();
            printReport(newarrangeList, fromDateString, toDateString);
            MessageBox.Show("New Arrangement Report Downloaded Successfully!");
        }

        public void printReport(List<NewArrangementModel> newarrangeList, string fromDateString, string toDateString)
        {
            Excel.Application excel;
            Excel._Workbook workbook;
            Excel._Worksheet worksheet;
            Excel.Range range;

            try
            {
                //Start Excel and get Application object.
                excel = new Excel.Application();
                
                //Get a new workbook
                workbook = (Excel._Workbook)(excel.Workbooks.Add(Missing.Value));
                worksheet = (Excel._Worksheet)workbook.Worksheets["Sheet1"];
                worksheet.Name = "New Arrangement Report";
                string ComputerName = Environment.UserName.ToString();
                string filename = "NewArrangementReport" + DateTime.Now.ToString("ddMMyyyyhhmm") + ".xlsx";
                workbook.SaveAs("C:\\Users\\" + ComputerName + "\\Desktop\\Reports\\New Arrangement Reports\\" + filename);
                workbook.Saved = true;

                worksheet.Cells[3, 5] = "Report Date";
                worksheet.Cells[3, 6] = DateTime.Now;
                worksheet.Cells[4, 1] = "Row No";
                worksheet.Cells[4, 2] = "Column No";
                worksheet.Cells[4, 3] = "Product Name";
                worksheet.Cells[4, 4] = "Max Quantity";
                worksheet.Cells[4, 5] = "Balance Quantity";
                worksheet.Cells[4, 6] = "Loading Quantity";

                //Merge Cells for Sheet  - 10.11.2021 Aadarsh
                worksheet.get_Range("A1", "F2").Merge(false);
                //worksheet.Shapes.AddPicture("F:\\Aadarsh\\25092021\\sample2\\images\\24hrs Yellow with green background.png", Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoCTrue, 50, 50, 300, 45);
                range = worksheet.get_Range("A1", "F2");
                string Name3 = "24 HRS NEW ARRANGEMENT REPORT";
                range.FormulaR1C1 = Name3;
                range.HorizontalAlignment = 3;
                range.VerticalAlignment = 3;
                range.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Green);
                range.Font.Size = 20;
                worksheet.Cells[1, 2] = "Green";

                //Format for Sheet as Bold - 07.11.2021 Aadarsh

                worksheet.get_Range("A4", "G4").Font.Bold = true;
                worksheet.get_Range("A3").Font.Bold = true;
                worksheet.get_Range("C3").Font.Bold = true;
                worksheet.get_Range("E3").Font.Bold = true;
                worksheet.get_Range("A4", "G4").VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                worksheet.Columns.WrapText = true;
                worksheet.Columns.ColumnWidth = 20;

                int row_count = 0;
                int[,] CT_Row_No = new int[newarrangeList.Count, 1];
                int[,] CT_Col_No = new int[newarrangeList.Count, 1];
                string[,] CT_Product_name = new string[newarrangeList.Count, 1];
                int[,] CT_Max_Qty = new int[newarrangeList.Count, 1];
                int[,] CT_Balance_Qty = new int[newarrangeList.Count, 1];
                int[,] CT_Loading_Qty = new int[newarrangeList.Count, 1];

                foreach (var arrangementdetails in newarrangeList)
                {
                    CT_Row_No[row_count, 0] = arrangementdetails.CT_Row_No;
                    CT_Col_No[row_count, 0] = arrangementdetails.CT_Col_No;
                    CT_Product_name[row_count, 0] = arrangementdetails.CT_Product_name;
                    CT_Max_Qty[row_count, 0] = arrangementdetails.CT_Max_Qty;
                    CT_Balance_Qty[row_count, 0] = arrangementdetails.CT_Balance_Qty;
                    CT_Loading_Qty[row_count, 0] = arrangementdetails.CT_Max_Qty - arrangementdetails.CT_Balance_Qty;
                    row_count++;
                }

                //Excel Row Positioning on workbook 2 sheet 1 - 08.11.2021 Aadarsh
                int arrangement_count = newarrangeList.Count + 4;
                worksheet.get_Range("A5", "A" + arrangement_count).Value2 = CT_Row_No;
                worksheet.get_Range("B5", "B" + arrangement_count).Value2 = CT_Col_No;
                worksheet.get_Range("C5", "C" + arrangement_count).Value2 = CT_Product_name;
                worksheet.get_Range("D5", "D" + arrangement_count).Value2 = CT_Max_Qty;
                worksheet.get_Range("E5", "E" + arrangement_count).Value2 = CT_Balance_Qty;
                worksheet.get_Range("F5", "F" + arrangement_count).Value2 = CT_Loading_Qty;
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
