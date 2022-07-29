using sample2.helpers;
using sample2.models;
using sample2.remote;
using sample2.reports;
using sample2.viewModel;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using static sample2.models.BackgroundWorkerModel;
using static sample2.models.DeviceServiceModel;

namespace sample2.services
{
    public class BackgroundServices
    {
        private static List<Inclusive_report_model> reportModel = new List<Inclusive_report_model>();


        public static bool updateReportModel(Inclusive_report_model updateData, string bill_no)
        {
           
            UpdateDeviceProcessTable(updateData);
           
            if (updateData.Description == "MAIN COMMAND")
            {
                if (bill_no == "test") bill_no = SqliteDataAccess.getBillNumber();
                List<ProductTransactionModel> currentTransactions = SqliteChange.getCurrentCellTransactions(bill_no);
                foreach (var transModel in currentTransactions)
                {
                    if (updateData.Product_Name == transModel.CTT_Product_Name)
                    {
                        if (transModel.CTT_Remarks.Contains("paid"))
                            transModel.CTT_Remarks += "-" + updateData.Remarks;
                        else
                            transModel.CTT_Remarks = updateData.Remarks;

                        transModel.CTT_Status = updateData.Status;


                        SqliteChange.updateCellTransaction(transModel);
                        break;
                    }
                }
            }

            else if(updateData.Description == "DISPENSING COMMAND")
            {
                if (bill_no == "test") bill_no = SqliteDataAccess.getBillNumber();
                List<CurrencyTransactionModel> currentCurrencyTransactions = SqliteChange.getCurrentCurrencyTransactions(bill_no);
                foreach (var transModel in currentCurrencyTransactions)
                {

                    transModel.Cr_Status = updateData.Status;

                    transModel.Cr_Remarks += "-" + updateData.Status;
                    SqliteChange.updateCurrencyTransaction(transModel);
                }
               
            }
            else if (updateData.Description == "Status Check 2")
            {
                if (bill_no == "test") bill_no = SqliteDataAccess.getBillNumber();
                List<CurrencyTransactionModel> currentCurrencyTransactions = SqliteChange.getCurrentCurrencyTransactions(bill_no);
                foreach (var transModel in currentCurrencyTransactions)
                {

                    transModel.Cr_Status = updateData.Status;

                    transModel.Cr_Remarks += "-" + updateData.Status;
                    SqliteChange.updateCurrencyTransaction(transModel);
                }

            }

            else if (updateData.Description == "Running Status Check 2" && updateData.Remarks.Contains("Successfully finished delivery"))
            {
                if (bill_no == "test") bill_no = SqliteDataAccess.getBillNumber();
                List<ProductTransactionModel> currentTransactions = SqliteChange.getCurrentCellTransactions(bill_no);
                foreach (var transModel in currentTransactions)
                {
                    if (updateData.Product_Name == transModel.CTT_Product_Name)
                    {
                        
                        CellModel registerbal = SqliteChange.getCellNumber_Registered_Bal(transModel.CTT_Row_No, transModel.CTT_Col_No);
                        
                        SqliteChange.UpdateCellTable(registerbal.CT_Col_No, registerbal.CT_Row_No, registerbal.CT_Balance_Qty, registerbal.CT_Registered_Balance_Qty - 1);
                        break;
                    }
                }
            }

            bool isUpdated = false;

            int index = reportModel.IndexOf(reportModel.Single(x => x.Steps == updateData.Steps
            && x.Process_Seq == updateData.Process_Seq));

            if (index >= 0)
            {
                reportModel.RemoveAt(index);
                reportModel.Insert(index, updateData);
            }

            if (reportModel.Exists(x => x == updateData))
            {
                isUpdated = true;
            }

            return isUpdated;
        }

        public static bool insertReportModel(Inclusive_report_model insertData)
        {
            bool isInserted = false;

            reportModel.Add(insertData);
            if (reportModel.Exists(x => x == insertData))
            {
                isInserted = true;
            }

            return isInserted;
        }

        public static List<Inclusive_report_model> getReportModel()
        {
            return reportModel;
        }

       
        public static Inclusive_report_model getCommandReportDetail(string commandDescription)
        {
            Inclusive_report_model commandReportEntry = new Inclusive_report_model();
            if (reportModel.Exists(x => x.Description == commandDescription))
            {
                commandReportEntry = reportModel.Single(x => x.Description == commandDescription);
            }

            return commandReportEntry;
        }

        public static Inclusive_report_model getReportDetail(int MainSeq, int StepSeq)
        {
            Inclusive_report_model commandReportEntry = new Inclusive_report_model();
            if (reportModel.Exists(x => x.Process_Seq == MainSeq && x.Steps == StepSeq))
            {
                commandReportEntry = reportModel.Single(x => x.Process_Seq == MainSeq && x.Steps == StepSeq);
            }

            return commandReportEntry;
        }

        public static Inclusive_report_model getReportDetail(string Status, string device_used)
        {
            Inclusive_report_model commandReportEntry = new Inclusive_report_model();
            if (reportModel.Exists(x => x.Status == Status && x.Device_Used == device_used))
            {
                commandReportEntry = reportModel.Single(x => x.Status == Status && x.Device_Used == device_used);
            }

            return commandReportEntry;
        }
        
        public static void Init_ReportModel_PLC()
        {
            reportModel = new List<Inclusive_report_model>();
            TruncateDeviceProcessTable();
            List<PLC_CommandSequence_Model> PLC_Commands = DeviceServices.getPLC_Command_Sequence();
            List<PLC_sub_cmd_seq_model> PLC_sub_Commands = DeviceServices.getPLC_Sub_Command_Sequence();


            for (int i = 0; i < PLC_Commands.Count; i++)
            {
                for (int j = 0; j < PLC_sub_Commands.Count; j++)
                {
                    Inclusive_report_model newEntry = new Inclusive_report_model();
                    newEntry.Process_Seq = PLC_Commands[i].dispensing_order;
                    newEntry.Steps = PLC_sub_Commands[j].sequence_order;
                    if (PLC_sub_Commands[j].sequence_order != 3)
                        newEntry.Cmd_Hexstring_Data = routines.ConvertToHexstring(PLC_sub_Commands[j].hexstring);
                    else
                        newEntry.Cmd_Hexstring_Data = routines.ConvertToHexstring(PLC_Commands[i].hexstring);
                    newEntry.Description = PLC_sub_Commands[j].command_name;
                    newEntry.Cmd_sent_times = 0;
                    newEntry.Response_received_status = "No";
                    newEntry.Response_received_times = 0;
                    newEntry.Response_Hexstring_Data = "";
                    newEntry.Product_Name = PLC_Commands[i].product_name;
                    newEntry.Cell_No = PLC_Commands[i].cellNo;
                    newEntry.Balance_Before_Delivery = PLC_Commands[i].balance_before_delivery;
                    newEntry.Cmd_Sent_TimeStamps = "";
                    newEntry.Response_TimeStamps = "";
                    newEntry.Cmd_Send_Maxtimes = PLC_sub_Commands[j].repeat_times;
                    newEntry.Device_Used = "PLC";
                    newEntry.Status = "Scheduled";
                    newEntry.Remarks = "";
                    InsertIntoDeviceProcessTable(newEntry);
                    reportModel.Add(newEntry);
                }

            }

        }

        public static void Init_ReportModel_Currency(Currency_cmd_seq_model currencyModel)
        {
            Inclusive_report_model newEntry = new Inclusive_report_model();
            newEntry.Process_Seq = currencyModel.dispensing_order;
            newEntry.Steps = currencyModel.sequence_order;
            newEntry.Description = currencyModel.command_name;
            newEntry.Product_Name = currencyModel.denomination;
            newEntry.Cell_No = currencyModel.deviceNo;
            newEntry.Response_Hexstring_Data = "";
            newEntry.Balance_Before_Delivery = currencyModel.balance_before_delivery;
            newEntry.Cmd_Send_Maxtimes = currencyModel.repeat_times;
            newEntry.Device_Used = currencyModel.device_used;
            newEntry.Cmd_Hexstring_Data = routines.ConvertToHexstring(currencyModel.hexstring);
            newEntry.Cmd_sent_times = 0;
            newEntry.Response_received_status = "No";
            newEntry.Response_received_times = 0;
            newEntry.Cmd_Sent_TimeStamps = "";
            newEntry.Response_TimeStamps = "";
            newEntry.Status = "Scheduled";
            newEntry.Remarks = "";
            InsertIntoDeviceProcessTable(newEntry);
            reportModel.Add(newEntry);

        }
     
        public static void updateCmdResendingDetails(Inclusive_report_model firstModel)
        {
            TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

            firstModel.Cmd_sent_times = firstModel.Cmd_sent_times + 1;
            if (firstModel.Cmd_Sent_TimeStamps != "")
                firstModel.Cmd_Sent_TimeStamps += "-";
            firstModel.Cmd_Sent_TimeStamps += indianTime.ToString("yyyy-MM-dd HH:mm:ss");

        }

        public static void updateResponses(Inclusive_report_model given_report_model, string[] remarks, string received_status)
        {
            TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

            if (given_report_model.Response_TimeStamps != "")
                given_report_model.Response_TimeStamps += "-";
            given_report_model.Response_TimeStamps += indianTime.ToString("yyyy-MM-dd HH:mm:ss");

            if (given_report_model.Response_Hexstring_Data != "")
                given_report_model.Response_Hexstring_Data += "-";
            given_report_model.Response_Hexstring_Data += remarks[0];

            given_report_model.Response_received_times = given_report_model.Response_received_times + 1;

            if (given_report_model.Remarks != "")
                given_report_model.Remarks += "-";
            given_report_model.Remarks += remarks[2];

            given_report_model.Response_received_status = received_status;


        }

        public static List<byte> getCommandHexString(Inclusive_report_model firstModel)
        {
            PLC_sub_cmd_seq_model PLC_Sub_Command = DeviceServices.getPLC_Sub_Command_Sequence()[firstModel.Steps-1];

            List<byte> command_send_hex;
            if (PLC_Sub_Command.sequence_order != 3)
                command_send_hex = PLC_Sub_Command.hexstring;
            else
                command_send_hex = DeviceServices.getPLC_Command_Sequence()[firstModel.Process_Seq-1].hexstring;

            return command_send_hex;
        }

        public static void UpdateVendingScreen(VendScreenViewModel vendViewModel, PLC_CommandSequence_Model PLC_Command)
        {
            //setting the product that needs to be delivered.
            vendViewModel.imgProduct = PLC_Command.product_image; // need to put the image
            vendViewModel.txtProductName = PLC_Command.product_name; // need to put the product name.
            vendViewModel.txtDispensingQty = "" + PLC_Command.dispensing_order; //need to update once this is dispensed
            vendViewModel.txtItemOrderQty = "" + PLC_Command.delivery_quantity;
            // need to get the number of products from cart items
        }

        public static void UpdateVendingStatus(VendScreenViewModel vendViewModel, string status, int font_size)
        {
            vendViewModel.txtVendStatus = status;
            vendViewModel.txtVendStatusFontSize = font_size;
        }

        public static void Init_Device_Services(VendScreenViewModel vendViewModel, string screenName)
        {
            DeviceServices.vendViewModel = vendViewModel;

            //we will add the order of command sequence to the PLC_CommandSequence_Model
            DeviceServices.Init_PLC_main_commands(vendViewModel.items);
        }

        public static void ConvertReportModelToExcel(string bill_no)
        {
            DeviceProcessReport deviceReport = new DeviceProcessReport(reportModel, bill_no);
        }

        public static void InsertIntoDeviceProcessTable(Inclusive_report_model model)
        {

            using (SQLiteConnection conn = new SQLiteConnection(SqliteDataAccess.LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("INSERT INTO 'Device Process Table'(DPT_Process_Seq, DPT_Steps, DPT_Description,"
                   + "DPT_Cmd_sent_times, DPT_Cmd_Hexstring_Data, DPT_Response_received_times, DPT_Response_received_status,"
                   + " DPT_Response_Hexstring_Data, DPT_Product_Name, DPT_Cell_No, DPT_Balance_Before_Delivery, DPT_Response_TimeStamps,"
                   + "DPT_Cmd_Sent_TimeStamps, DPT_Cmd_Send_Maxtimes, DPT_Device_Used, DPT_Status, DPT_Remarks) values(" + model.Process_Seq
                   + ", " + model.Steps + ", @description, " + model.Cmd_sent_times + ", @cmd_hex, " + model.Response_received_times
                   + ", @res_status, @res_hex, @product, @cell_no, " + model.Balance_Before_Delivery + ", @res_time, @cmd_time, "
                   + model.Cmd_Send_Maxtimes + ", @device, @status, @remarks)", conn);




                command.Parameters.Add(new SQLiteParameter("@description", model.Description));
                command.Parameters.Add(new SQLiteParameter("@cmd_hex", model.Cmd_Hexstring_Data));
                command.Parameters.Add(new SQLiteParameter("@res_status", model.Response_received_status));
                command.Parameters.Add(new SQLiteParameter("@res_hex", model.Response_Hexstring_Data));
                command.Parameters.Add(new SQLiteParameter("@product", model.Product_Name));
                command.Parameters.Add(new SQLiteParameter("@cell_no", model.Cell_No));
                command.Parameters.Add(new SQLiteParameter("@res_time", model.Response_TimeStamps));
                command.Parameters.Add(new SQLiteParameter("@cmd_time", model.Cmd_Sent_TimeStamps));
                command.Parameters.Add(new SQLiteParameter("@device", model.Device_Used));
                command.Parameters.Add(new SQLiteParameter("@status", model.Status));
                command.Parameters.Add(new SQLiteParameter("@remarks", model.Remarks));

                command.ExecuteNonQuery();


                conn.Close();
            }

        }

        public static void UpdateDeviceProcessTable(Inclusive_report_model model)
        {

            using (SQLiteConnection conn = new SQLiteConnection(SqliteDataAccess.LoadConnectionString()))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand("UPDATE 'Device Process Table' SET  DPT_Description = @description, DPT_Cmd_sent_times = " + model.Cmd_sent_times
                   + ", DPT_Cmd_Hexstring_Data = @cmd_hex, DPT_Response_received_times = " + model.Response_received_times
                   + ", DPT_Response_received_status = @res_status, DPT_Response_Hexstring_Data =  @res_hex, DPT_Product_Name = @product"
                   + ", DPT_Cell_No = @cell_no, DPT_Balance_Before_Delivery = " + model.Balance_Before_Delivery + ", DPT_Response_TimeStamps = @res_time,"
                   + " DPT_Cmd_Sent_TimeStamps = @cmd_time, DPT_Cmd_Send_Maxtimes = " + model.Cmd_Send_Maxtimes + ", DPT_Device_Used = @device,"
                   + " DPT_Status = @status, DPT_Remarks = @remarks WHERE DPT_Process_Seq = " + model.Process_Seq
                    + " AND DPT_Steps = " + model.Steps, conn);




                command.Parameters.Add(new SQLiteParameter("@description", model.Description));
                command.Parameters.Add(new SQLiteParameter("@cmd_hex", model.Cmd_Hexstring_Data));
                command.Parameters.Add(new SQLiteParameter("@res_status", model.Response_received_status));
                command.Parameters.Add(new SQLiteParameter("@res_hex", model.Response_Hexstring_Data));
                command.Parameters.Add(new SQLiteParameter("@product", model.Product_Name));
                command.Parameters.Add(new SQLiteParameter("@cell_no", model.Cell_No));
                command.Parameters.Add(new SQLiteParameter("@res_time", model.Response_TimeStamps));
                command.Parameters.Add(new SQLiteParameter("@cmd_time", model.Cmd_Sent_TimeStamps));
                command.Parameters.Add(new SQLiteParameter("@device", model.Device_Used));
                command.Parameters.Add(new SQLiteParameter("@status", model.Status));
                command.Parameters.Add(new SQLiteParameter("@remarks", model.Remarks));

                command.ExecuteNonQuery();


                conn.Close();
            }

        }

        public static void TruncateDeviceProcessTable()
        {
            using (SQLiteConnection conn = new SQLiteConnection(SqliteDataAccess.LoadConnectionString()))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("DELETE FROM 'Device Process Table'", conn);

                command.ExecuteNonQuery();

                conn.Close();
            }
        }

        public static int[] splited_amount(int total)
        {
            List<int> splitted_amt = new List<int>();
            int bill_denom = Sqlitedatavr.getBillDenomination();
            if (total > 0)
            {

                int bills_count = total / bill_denom;

                splitted_amt.Add(bills_count);

                int remaining_amt = total % bill_denom;
                if (remaining_amt > 0)
                {
                    int coins_count = remaining_amt / Sqlitedatavr.getCoinDenomination();
                    splitted_amt.Add(coins_count);
                }
                else
                {
                    splitted_amt.Add(0);
                }
            }
            else
            {
                splitted_amt.Add(0);
                splitted_amt.Add(0);
            }
            return splitted_amt.ToArray();
        }

        public static int[] InsertCurrencyDetails(int total, string bill_no)
        {

            TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            String formatdate = indianTime.ToString("yyyy-MM-dd HH:mm:ss"); //imthi 20-01-2021
            int[] total_separated = splited_amount(total);


            if (total_separated[0] > 0)
            {
                int saved_successfully = 0;
                int bill_denomination = Sqlitedatavr.getBillDenomination();
                CurrencyTransactionModel lastTransDetails = SqliteChange.getLastCurrencyTransaction(bill_denomination, "ND");
                saved_successfully += SqliteChange.InsertIntoCurrencyTransactionTable(bill_denomination, bill_no, total_separated[0],
                                  formatdate, "Customer", "Debit", lastTransDetails.Cr_Closing_Balance_Qty, //imthi 20-01-2021
                                  lastTransDetails.Cr_Closing_Balance_Qty - total_separated[0], "ND", "Scheduled");
            }
            //Storing the Coin transaction details in the database
            if (total_separated[1] > 0)
            {
                int saved_successfully = 0;
                int coin_denomination = Sqlitedatavr.getCoinDenomination();
                CurrencyTransactionModel lastTransDetails = SqliteChange.getLastCurrencyTransaction(coin_denomination, "CD");
                saved_successfully += SqliteChange.InsertIntoCurrencyTransactionTable(coin_denomination, bill_no, total_separated[1],
                                  formatdate, "Customer", "Debit", lastTransDetails.Cr_Closing_Balance_Qty,     //imthi 20-01-2021
                                  lastTransDetails.Cr_Closing_Balance_Qty - total_separated[1], "CD", "Scheduled");
            }


            return total_separated;
        }

        public static void TryCurrencyInit(VendScreenViewModel vendViewModel, Inclusive_report_model lastReport)
        {
            string[] split = vendViewModel.txtAmtPaid.Split(' ');
            int amtpaid = int.Parse(split[1]);
            split = vendViewModel.txtOrderAmt.Split(' ');
            int bill_amt = int.Parse(split[1]);
            int difference = amtpaid - bill_amt;
            if (difference > 0)
            {
                //putting the value in the table and returning how many bills and coins has to be dispensed if applicable
                int[] currencySeparated = BackgroundServices.InsertCurrencyDetails(difference, vendViewModel.bill_number);
                List<CurrencyTransactionModel> Currency_TransModel = new List<CurrencyTransactionModel>();
                if (currencySeparated[0] > 0)
                    Currency_TransModel.AddRange(SqliteChange.getInProgressCurrencyTransactions(vendViewModel.bill_number, "ND"));
                if (currencySeparated[1] > 0)
                    Currency_TransModel.AddRange(SqliteChange.getInProgressCurrencyTransactions(vendViewModel.bill_number, "CD"));

                foreach (var transModel in Currency_TransModel)
                {
                    //converting into report model so that we add it in the main report model.
                    if (transModel.Cr_Equipment_Used == "ND")
                    {
                        List<Currency_cmd_seq_model> ND_CurrencyModel = DeviceServices.Init_ND_seq_commands(transModel, lastReport.Process_Seq + 1, currencySeparated);
                        foreach (var ND_sequence in ND_CurrencyModel)
                            BackgroundServices.Init_ReportModel_Currency(ND_sequence);
                    }
                    else if (transModel.Cr_Equipment_Used == "CD")
                    {
                        List<Currency_cmd_seq_model> CD_CurrencyModel = DeviceServices.Init_CD_seq_commands(transModel, lastReport.Process_Seq + 2, currencySeparated);
                        foreach (var CD_sequence in CD_CurrencyModel)
                            BackgroundServices.Init_ReportModel_Currency(CD_sequence);
                    }

                }
            }

        }

    }
}
