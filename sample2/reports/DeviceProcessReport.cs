using sample2.remote;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static sample2.models.BackgroundWorkerModel;
using Excel = Microsoft.Office.Interop.Excel;

namespace sample2.reports
{
    class DeviceProcessReport
    {
        List<Inclusive_report_model> fullReportModel;
        Excel.Application excel;
        Excel._Workbook workbook;
        Excel._Worksheet worksheet;
        Excel.Range range;
        string bill_no;
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        private static DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        public DeviceProcessReport(List<Inclusive_report_model> fullReportModel, string bill_no)
        {
            this.bill_no = bill_no;
            this.fullReportModel = fullReportModel;
            try
            {
                excel = new Excel.Application();
                workbook = excel.Workbooks.Add(Missing.Value);
                worksheet = (Excel._Worksheet)workbook.ActiveSheet;
                worksheet = (Excel._Worksheet)workbook.Worksheets["Sheet1"];
                worksheet.Name = "Device Report";

                string ComputerName = Environment.UserName.ToString();
                string filename = "DeviceReport" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xlsx";
                workbook.SaveAs("C:\\Users\\" + ComputerName + "\\Desktop\\Reports\\Device Reports\\" + filename);
                workbook.Saved = true;

                // MERGE CELLS - Developed by Aadarsh - 07.10.2021 
                worksheet.get_Range("A2", "Q3").Merge(false);
                //worksheet.Shapes.AddPicture("F:\\Aadarsh\\25092021\\sample2\\images\\24hrs Yellow with green background.png", Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoCTrue, 50, 50, 300, 45);
                range = worksheet.get_Range("A2", "Q3");
                string Main_Header = "DEVICE   PERFORMANCE   REPORT";
                range.FormulaR1C1 = Main_Header;
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                range.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                range.Font.Color = ColorTranslator.ToOle(Color.Maroon);
                range.Font.Size = 16;

                worksheet.get_Range("A5", "B5").Merge(false);
                //worksheet.Shapes.AddPicture("F:\\Aadarsh\\25092021\\sample2\\images\\24hrs Yellow with green background.png", Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoCTrue, 50, 50, 300, 45);
                range = worksheet.get_Range("A5", "B5");
                range.FormulaR1C1 = "Bill Number:";
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                range.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                range.Font.Bold = true;

                worksheet.get_Range("D5", "E5").Merge(false);
                //worksheet.Shapes.AddPicture("F:\\Aadarsh\\25092021\\sample2\\images\\24hrs Yellow with green background.png", Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoCTrue, 50, 50, 300, 45);
                range = worksheet.get_Range("D5", "E5");
                string dateTime = "Date Time:";
                range.FormulaR1C1 = dateTime;
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                range.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                range.Font.Bold = true;

                worksheet.get_Range("F5", "G5").Merge(false);
                //worksheet.Shapes.AddPicture("F:\\Aadarsh\\25092021\\sample2\\images\\24hrs Yellow with green background.png", Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoCTrue, 50, 50, 300, 45);
                range = worksheet.get_Range("F5", "G5");
                range.FormulaR1C1 = indianTime.ToString();
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                range.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
               

                worksheet.get_Range("A7", "Q7").Font.Bold = true;
                worksheet.get_Range("A7", "Q7").HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                worksheet.get_Range("A7", "Q7").VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                worksheet.Columns.WrapText = true;

                //Set width of each column
                worksheet.get_Range("A7").ColumnWidth = 6.35;
                worksheet.get_Range("B7").ColumnWidth = 5;
                worksheet.get_Range("C7").ColumnWidth = 15;
                worksheet.get_Range("D7").ColumnWidth = 6.5;
                worksheet.get_Range("E7").ColumnWidth = 13;
                worksheet.get_Range("F7").ColumnWidth = 9;
                worksheet.get_Range("G7").ColumnWidth = 9;
                worksheet.get_Range("H7").ColumnWidth = 15;
                worksheet.get_Range("I7").ColumnWidth = 15;
                worksheet.get_Range("J7").ColumnWidth = 7;
                worksheet.get_Range("K7").ColumnWidth = 8;
                worksheet.get_Range("L7").ColumnWidth = 12;
                worksheet.get_Range("M7").ColumnWidth = 11;
                worksheet.get_Range("N7").ColumnWidth = 6.35;
                worksheet.get_Range("O7").ColumnWidth = 6.35;
                worksheet.get_Range("P7").ColumnWidth = 11;
                worksheet.get_Range("Q7").ColumnWidth = 15;


                //Set headings
                //Add table headers going cell by cell. - For Sheet 1
                worksheet.Cells[5, 3] = bill_no;
                worksheet.Cells[7, 1] = "Process Seq";
                worksheet.Cells[7, 2] = "Steps";
                worksheet.Cells[7, 3] = "Description";
                worksheet.Cells[7, 4] = "Cmd sent times";
                worksheet.Cells[7, 5] = "Cmd Hexstring Data";
                worksheet.Cells[7, 6] = "Response received times";
                worksheet.Cells[7, 7] = "Response received status";
                worksheet.Cells[7, 8] = "Response Hexstring Data";
                worksheet.Cells[7, 9] = "Product Name";
                worksheet.Cells[7, 10] = "Cell No";
                worksheet.Cells[7, 11] = "Balance Before Delivery";
                worksheet.Cells[7, 12] = "Response Timestamps";
                worksheet.Cells[7, 13] = "Cmd Sent Timestamps";
                worksheet.Cells[7, 14] = "Cmd Send Max times";
                worksheet.Cells[7, 15] = "Device Used";
                worksheet.Cells[7, 16] = "Status";
                worksheet.Cells[7, 17] = "Remarks";


                int report_count = 7 + fullReportModel.Count;

                if (fullReportModel.Count > 0)
                {

                    int row_count = 0;
                    int[,] Process_Seq = new int[fullReportModel.Count, 1];
                    int[,] Steps = new int[fullReportModel.Count, 1];
                    string[,] Description = new string[fullReportModel.Count, 1];
                    int[,] Cmd_sent_times = new int[fullReportModel.Count, 1];
                    string[,] Cmd_Hexstring_Data = new string[fullReportModel.Count, 1];
                    int[,] Response_received_times = new int[fullReportModel.Count, 1];
                    string[,] Response_received_status = new string[fullReportModel.Count, 1];
                    string[,] Response_Hexstring_Data = new string[fullReportModel.Count, 1];
                    string[,] Product_Name = new string[fullReportModel.Count, 1];
                    string[,] Cell_No = new string[fullReportModel.Count, 1];
                    int[,] Balance_Before_Delivery = new int[fullReportModel.Count, 1];
                    string[,] Response_TimeStamps = new string[fullReportModel.Count, 1];
                    string[,] Cmd_Sent_TimeStamps = new string[fullReportModel.Count, 1];
                    int[,] Cmd_Send_Maxtimes = new int[fullReportModel.Count, 1];
                    string[,] Device_Used = new string[fullReportModel.Count, 1];
                    string[,] Status = new string[fullReportModel.Count, 1];
                    string[,] Remarks = new string[fullReportModel.Count, 1];

                    foreach (var reportDetails in fullReportModel)
                    {
                        Process_Seq[row_count,0] = reportDetails.Process_Seq;
                        Steps[row_count, 0] = reportDetails.Steps;
                        Description[row_count, 0] = reportDetails.Description;
                        Cmd_sent_times[row_count, 0] = reportDetails.Cmd_sent_times;
                        Cmd_Hexstring_Data[row_count, 0] = reportDetails.Cmd_Hexstring_Data;
                        Response_received_times[row_count, 0] = reportDetails.Response_received_times;
                        Response_received_status[row_count, 0] = reportDetails.Response_received_status;
                        Response_Hexstring_Data[row_count, 0] = reportDetails.Response_Hexstring_Data;
                        Product_Name[row_count, 0] = reportDetails.Product_Name;
                        Cell_No[row_count, 0] = reportDetails.Cell_No;
                        Balance_Before_Delivery[row_count, 0] = reportDetails.Balance_Before_Delivery;
                        Response_TimeStamps[row_count, 0] = reportDetails.Response_TimeStamps;
                        Cmd_Sent_TimeStamps[row_count, 0] = reportDetails.Cmd_Sent_TimeStamps;
                        Cmd_Send_Maxtimes[row_count, 0] = reportDetails.Cmd_Send_Maxtimes;
                        Device_Used[row_count, 0] = reportDetails.Device_Used;
                        Status[row_count, 0] = reportDetails.Status;
                        Remarks[row_count, 0] = reportDetails.Remarks;
                        row_count++;
                    }

                    worksheet.get_Range("A8", "A" + report_count).Value2 = Process_Seq;
                    worksheet.get_Range("B8", "B" + report_count).Value2 = Steps;
                    worksheet.get_Range("C8", "C" + report_count).Value2 = Description;
                    worksheet.get_Range("D8", "D" + report_count).Value2 = Cmd_sent_times;
                    worksheet.get_Range("E8", "E" + report_count).Value2 = Cmd_Hexstring_Data;
                    worksheet.get_Range("F8", "F" + report_count).Value2 = Response_received_times;
                    worksheet.get_Range("G8", "G" + report_count).Value2 = Response_received_status;
                    worksheet.get_Range("H8", "H" + report_count).Value2 = Response_Hexstring_Data;
                    worksheet.get_Range("I8", "I" + report_count).Value2 = Product_Name;
                    worksheet.get_Range("J8", "J" + report_count).Value2 = Cell_No;
                    worksheet.get_Range("K8", "K" + report_count).Value2 = Balance_Before_Delivery;
                    worksheet.get_Range("L8", "L" + report_count).Value2 = Response_TimeStamps;
                    worksheet.get_Range("M8", "M" + report_count).Value2 = Cmd_Sent_TimeStamps;
                    worksheet.get_Range("N8", "N" + report_count).Value2 = Cmd_Send_Maxtimes;
                    worksheet.get_Range("O8", "O" + report_count).Value2 = Device_Used;
                    worksheet.get_Range("P8", "P" + report_count).Value2 = Status;
                    worksheet.get_Range("Q8", "Q" + report_count).Value2 = Remarks;
                }
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
