using sample2.models;
using sample2.remote;
using sample2.services;
using sample2.User_Controls;
using sample2.viewModel;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static sample2.models.BackgroundWorkerModel;
using static sample2.models.DeviceServiceModel;


namespace sample2.helpers
{
    class Port_BackgroundWorker
    {
        VendScreenViewModel vendViewModel;

        public Port_BackgroundWorker(VendScreenViewModel vendViewModel)
        {
            BackgroundServices.Init_Device_Services(vendViewModel, "Vending Screen");
            BackgroundServices.Init_ReportModel_PLC();
            List<Inclusive_report_model> reportContent = BackgroundServices.getReportModel();
            BackgroundServices.TryCurrencyInit(vendViewModel, reportContent.Last());

            this.vendViewModel = vendViewModel;

            Execute_PLC();

        }

        private async void CommunicationProcess(List<byte> hex_bytes, Output_Result response_reciever,
            Inclusive_report_model reportModel)
        {

            DeviceServices.CmdSend(hex_bytes, response_reciever);

            Task<string> task = new Task<string>(DeviceServices.waitForResponse);

            task.Start();

            string response = await task;

            switch (reportModel.Device_Used)
            {
                case "PLC":

                    PLC_NextCommand(response, reportModel);
                    break;
                case "ND":
                    ND_NextCommand(response, reportModel);
                    break;
                case "CD":
                    CD_NextCommand(response, reportModel);
                    break;
            }

        }

        private async void OnlyResponseProcess(Inclusive_report_model reportModel)
        {
            Task<string> task = new Task<string>(DeviceServices.waitForResponse);

            task.Start();

            string response = await task;

            switch (reportModel.Device_Used)
            {
                case "PLC":
                    PLC_NextCommand(response, reportModel);
                    break;
                case "ND":
                    ND_NextCommand(response, reportModel);
                    break;
                case "CD":
                    CD_NextCommand(response, reportModel);

                    break;
            }

        }


        public void NextDeviceOrScreen(Inclusive_report_model reportModel)
        {
            DeviceServices.CloseThePort();
            List<Inclusive_report_model> ReportDetails = BackgroundServices.getReportModel();
            if (ReportDetails.Exists(x => x.Process_Seq == (reportModel.Process_Seq + 1)))
            {
                Inclusive_report_model nextReportModel = BackgroundServices.getReportDetail((reportModel.Process_Seq + 1), 1);
                if (nextReportModel.Device_Used == "ND")
                    Execute_ND(nextReportModel);
                else if (nextReportModel.Device_Used == "CD")
                    Execute_CD(nextReportModel);
            }
            else if(ReportDetails.Exists(x => x.Process_Seq == (reportModel.Process_Seq + 2)))
            {
                Inclusive_report_model nextReportModel = BackgroundServices.getReportDetail((reportModel.Process_Seq + 2), 1);
                if (nextReportModel.Device_Used == "ND")
                    Execute_ND(nextReportModel);
                else if (nextReportModel.Device_Used == "CD")
                    Execute_CD(nextReportModel);

            }
            else
            {
                List<product_count_ledger> product_ledger = new List<product_count_ledger>();

                foreach (var product in vendViewModel.items)
                {
                    product_count_ledger new_product = new product_count_ledger();
                    new_product.Pr_Name = product.Product_Name.Text;
                    new_product.Pr_Qty = int.Parse(product.Product_quantity.Text);
                    product_ledger.Add(new_product);
                }
                BackgroundServices.ConvertReportModelToExcel(vendViewModel.bill_number);

                List<Inclusive_report_model> progressReport = BackgroundServices.getReportModel();
                string error = "";

                foreach (var record in progressReport)
                {
                    if (record.Description == "MAIN COMMAND" && record.Response_received_times > 4)
                    {
                        error = "Continous rotation at: " + record.Cell_No;
                    }
                }


                _19_Bill_Printing next_page = new _19_Bill_Printing(error);

                vendViewModel.currentPage.NavigationService.Navigate(next_page);
                next_page.Bill_Printing(product_ledger, vendViewModel.bill_number);

            }
        }

        private void Get_and_Send_Next_Process(Inclusive_report_model reportModel)
        {
            switch (reportModel.Device_Used)
            {
                case "PLC":

                    Next_PLC_Command(reportModel);
                    break;

                case "ND":
                    Next_ND_Command(reportModel);
                    break;

                case "CD":
                    Next_CD_Command(reportModel);
                    break;
            }
        }

        #region PLC

        private void Execute_PLC()
        {

            Inclusive_report_model firstModel = BackgroundServices.getReportModel()[0];
            firstModel.Status = "In Progress";
            BackgroundServices.updateCmdResendingDetails(firstModel);
            BackgroundServices.updateReportModel(firstModel, vendViewModel.bill_number);
            PLC_CommandSequence_Model command = DeviceServices.getPLC_Command_Sequence().Single(x => x.dispensing_order == firstModel.Process_Seq);
            BackgroundServices.UpdateVendingScreen(vendViewModel, command);

            DeviceServices.PLC_Config();

            PLC_sub_cmd_seq_model PLC_Sub_Command = DeviceServices.getPLC_Sub_Command_Sequence().Single(x => x.sequence_order == firstModel.Steps);
            // updating the vend screen status
            BackgroundServices.UpdateVendingStatus(vendViewModel, firstModel.Description, 24);
            CommunicationProcess(PLC_Sub_Command.hexstring, PLC_Sub_Command.response_reciever, firstModel);
        }

        private void Next_PLC_Command(Inclusive_report_model reportModel)
        {
            List<PLC_CommandSequence_Model> PlC_command_details_All = DeviceServices.getPLC_Command_Sequence();

            if (PlC_command_details_All.Exists(x => x.dispensing_order == (reportModel.Process_Seq + 1))
                 || reportModel.Steps < 7)
            {
                Inclusive_report_model nextTaskInReport = new Inclusive_report_model();
                //getting the next report
                if (reportModel.Steps < 7)
                {
                    nextTaskInReport = BackgroundServices.getReportDetail(reportModel.Process_Seq,
                          (reportModel.Steps + 1));
                }
                else if (PlC_command_details_All.Exists(x => x.dispensing_order == (reportModel.Process_Seq + 1)))
                {
                    nextTaskInReport = BackgroundServices.getReportDetail((reportModel.Process_Seq + 1),
                          1);
                    BackgroundServices.UpdateVendingScreen
                       (
                           vendViewModel,
                           DeviceServices.getPLC_Command_Sequence()[nextTaskInReport.Process_Seq - 1]
                       );
                }



                //updating the next report
                nextTaskInReport.Status = "In Progress";
                BackgroundServices.updateCmdResendingDetails(nextTaskInReport);
                BackgroundServices.updateReportModel(nextTaskInReport, vendViewModel.bill_number);
                // updating the vend screen status
                BackgroundServices.UpdateVendingStatus(vendViewModel, nextTaskInReport.Description, 24);
                //Sending the command to PLC
                List<byte> command_send_hex = BackgroundServices.getCommandHexString(nextTaskInReport);
                Output_Result response_receiver = DeviceServices.getPLC_Sub_Command_Sequence()[nextTaskInReport.Steps - 1].response_reciever;

                CommunicationProcess(command_send_hex, response_receiver, nextTaskInReport);
            }
            else
            {
                NextDeviceOrScreen(reportModel);
            }

        }

        private void PLC_NextCommand(string response, Inclusive_report_model reportModel)
        {
            DeviceServiceModel deviceModel = new DeviceServiceModel();
            string[] responses = response.Split('-');
            //Resends the same command for Step 1,2 and 4 only.
            if (responses[1] == "S1Re" || responses[1] == "S2Re" || responses[1] == "S4ReN"
                || responses[1] == "S4ReP" || responses[1] == "S5Re")
            {
                reportModel.Status = "In Progress";
                BackgroundServices.updateCmdResendingDetails(reportModel);
                BackgroundServices.updateResponses(reportModel, responses, "Yes");
                BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);
                if (responses[1] == "S4ReP")
                    BackgroundServices.UpdateVendingStatus(vendViewModel, responses[2], 18);


                //Sending the command to PLC
                List<byte> command_send_hex = BackgroundServices.getCommandHexString(reportModel);
                Output_Result response_receiver = DeviceServices.getPLC_Sub_Command_Sequence()[reportModel.Steps - 1].response_reciever;
                CommunicationProcess(command_send_hex, response_receiver, reportModel);
            }
            //Doesnt care if the received response for step 3 is successful or not, just direct it to next part
            else if (responses[1] == "S3Skip" )
            {
                //updating the current report model
                reportModel.Status = "Pending";
                BackgroundServices.updateResponses(reportModel, responses, "Yes");
                BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);

                Get_and_Send_Next_Process(reportModel);

            }
            else if (responses[1] == "S4ErCR")
            {
                //updating the current report model
                reportModel.Status = "Failed";
                BackgroundServices.updateResponses(reportModel, responses, "Yes");
                BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);

                Get_and_Send_Next_Process(reportModel);
            }

        

            //Next actions for step 1 and step 2 successful responses
            else if (responses[1] == "S1Ne" || responses[1] == "S2Ne" )
            {
                //updating the current report model
                reportModel.Status = "Completed";
                BackgroundServices.updateResponses(reportModel, responses, "Yes");
                BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);
                Get_and_Send_Next_Process(reportModel);
            }
            //error Responses
            else if (responses[1] == "S1ErR" || responses[1] == "S2ErR"
                || responses[1] == "S4ErNR" || responses[1] == "S7ErR"  ||  responses[1] == "S5error")
            {
                reportModel.Status = "Failed";
                BackgroundServices.updateResponses(reportModel, responses, "Yes");
                BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);

                // updating the vend screen status
                BackgroundServices.UpdateVendingStatus(vendViewModel, (responses[2]
                    + "\n Contact Helpline: " + SqliteDataAccess.getHelplineNumber()), 18);
              

                BackgroundServices.ConvertReportModelToExcel(vendViewModel.bill_number);
            }

            else if (responses[1] == "S4Ne")
            {
                reportModel.Status = "Completed";
                BackgroundServices.updateResponses(reportModel, responses, "Yes");
                BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);

                Inclusive_report_model mainCmdReportModel = BackgroundServices.getReportDetail("Pending", "PLC");
                mainCmdReportModel.Status = "Completed";
                BackgroundServices.updateReportModel(mainCmdReportModel, vendViewModel.bill_number);

                Get_and_Send_Next_Process(reportModel);

            }
            else if (responses[1] == "S5Ne" || responses[1] == "S6Ne" || responses[1] == "S7Ne" )
            {
                //updating the current report model
                reportModel.Status = "Completed";
                BackgroundServices.updateResponses(reportModel, responses, "Yes");
                BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);
                Get_and_Send_Next_Process(reportModel);
            }


            else if (responses[1] == "NR")
            {
                if (reportModel.Cmd_sent_times <= reportModel.Cmd_Send_Maxtimes)
                {
                    reportModel.Status = "In Progress";
                    BackgroundServices.updateCmdResendingDetails(reportModel);
                    BackgroundServices.updateResponses(reportModel, responses, "No");
                    BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);

                    //Sending the command to PLC
                    List<byte> command_send_hex = BackgroundServices.getCommandHexString(reportModel);
                    Output_Result response_receiver = DeviceServices.getPLC_Sub_Command_Sequence()[reportModel.Steps - 1].response_reciever;
                    CommunicationProcess(command_send_hex, response_receiver, reportModel);
                }
                else
                {
                    reportModel.Status = "Connection Failed";
                    BackgroundServices.updateResponses(reportModel, responses, "No");
                    BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);
                    BackgroundServices.UpdateVendingStatus(vendViewModel, ("Connection Failed"
                        + "\n Contact Helpline: " + SqliteDataAccess.getHelplineNumber()), 18);
                    BackgroundServices.ConvertReportModelToExcel(vendViewModel.bill_number);
                }
            }
            else if (responses[1] == "TU")
            {
                reportModel.Status = "Connection Failed";
                BackgroundServices.updateResponses(reportModel, responses, "No");
                BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);
                BackgroundServices.UpdateVendingStatus(vendViewModel, ("Connection Failed Time Out"
                    + "\n Contact Helpline: " + SqliteDataAccess.getHelplineNumber()), 18);
                BackgroundServices.ConvertReportModelToExcel(vendViewModel.bill_number);
            }
            else
            {
                BackgroundServices.UpdateVendingStatus(vendViewModel, ("Unknown Response: " + responses[1]
                    + "\n Contact Helpline: " + SqliteDataAccess.getHelplineNumber()), 18);
                BackgroundServices.ConvertReportModelToExcel(vendViewModel.bill_number);
            }


        }

        #endregion

        #region Note Dispenser

        private void Execute_ND(Inclusive_report_model reportModel)
        {
            reportModel.Status = "In Progress";
            BackgroundServices.updateCmdResendingDetails(reportModel);
            BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);
            BackgroundServices.UpdateVendingStatus(vendViewModel, "Initializing Currency Dispenser", 24);


            List<Currency_cmd_seq_model> currencyDeliveryReports = DeviceServices.getND_Command_Sequence();

            Currency_cmd_seq_model command = currencyDeliveryReports.Single(x => x.sequence_order == reportModel.Steps);
            DeviceServices.ND_Config();

            CommunicationProcess(command.hexstring, command.response_reciever, reportModel);
        }

        private void ND_NextCommand(string response, Inclusive_report_model reportModel)
        {
            string[] responses = response.Split('-');

            //Resends the same command for Step 1,2 and 4 only.
            if (responses[1] == "S1Re" || responses[1] == "S2Re" || responses[1] == "S5Re"
                || responses[1] == "S4Re")
            {
                reportModel.Status = "In Progress";
                BackgroundServices.updateCmdResendingDetails(reportModel);
                BackgroundServices.updateResponses(reportModel, responses, "Yes");
                BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);



                //Sending the command to ND
                List<Currency_cmd_seq_model> currencyDeliveryReports = DeviceServices.getND_Command_Sequence();

                Currency_cmd_seq_model command = currencyDeliveryReports.Single(x => x.sequence_order == reportModel.Steps);
                CommunicationProcess(command.hexstring, command.response_reciever, reportModel);
            }
            //Doesnt care if the received response for step 3 is successful or not, just direct it to next part
            else if (responses[1] == "S3Skip")
            {
                //updating the current report model
                reportModel.Status = "Pending";
                BackgroundServices.updateResponses(reportModel, responses, "Yes");
                BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);

                Get_and_Send_Next_Process(reportModel);

            }
            //Next actions for step 1, step 2 and step 4 successful responses
            else if (responses[1] == "S1Ne" || responses[1] == "S2Ne" || responses[1] == "S4Ne")
            {
                //updating the current report model
                reportModel.Status = "Completed";
                BackgroundServices.updateResponses(reportModel, responses, "Yes");
                BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);
                Get_and_Send_Next_Process(reportModel);
            }
            //Next actions for step 5 successful responses
            else if (responses[1] == "S5Ne")
            {
                //updating the current report model
                reportModel.Status = "Completed";
                Inclusive_report_model inPendingModel = BackgroundServices.getReportDetail("Pending", "ND");
                inPendingModel.Status = "Completed";
                BackgroundServices.updateResponses(reportModel, responses, "Yes");
                BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);
                BackgroundServices.updateReportModel(inPendingModel, vendViewModel.bill_number);
                Get_and_Send_Next_Process(reportModel);
            }
            //error Responses
            else if (responses[1] == "S1ErR" || responses[1] == "S2ErR"
                || responses[1] == "S4ErR" || responses[1] == "S5ErR" || responses[1] == "S3ErR")
            {
                reportModel.Status = "Failed";
                BackgroundServices.updateResponses(reportModel, responses, "Yes");
                BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);

                // updating the vend screen status
                BackgroundServices.UpdateVendingStatus(vendViewModel, (responses[2]
                    + "\n Contact Helpline: " + SqliteDataAccess.getHelplineNumber()), 18);

                BackgroundServices.ConvertReportModelToExcel(vendViewModel.bill_number);
            }
            else if (responses[1] == "NR")
            {
                if (reportModel.Cmd_sent_times <= reportModel.Cmd_Send_Maxtimes)
                {
                    reportModel.Status = "In Progress";
                    BackgroundServices.updateCmdResendingDetails(reportModel);
                    BackgroundServices.updateResponses(reportModel, responses, "No");
                    BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);

                    //Sending the command to ND
                    List<Currency_cmd_seq_model> currencyDeliveryReports = DeviceServices.getND_Command_Sequence();

                    Currency_cmd_seq_model command = currencyDeliveryReports.Single(x => x.sequence_order == reportModel.Steps);
                    CommunicationProcess(command.hexstring, command.response_reciever, reportModel);
                }
                else
                {
                    reportModel.Status = "Connection Failed";
                    BackgroundServices.updateResponses(reportModel, responses, "No");
                    BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);
                    BackgroundServices.UpdateVendingStatus(vendViewModel, ("Connection Failed"
                        + "\n Contact Helpline: " + SqliteDataAccess.getHelplineNumber()), 18);
                    BackgroundServices.ConvertReportModelToExcel(vendViewModel.bill_number);
                }
            }
            else if (responses[1] == "TU")
            {
                reportModel.Status = "Connection Failed";
                BackgroundServices.updateResponses(reportModel, responses, "No");
                BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);
                BackgroundServices.UpdateVendingStatus(vendViewModel, ("Connection Failed"
                    + "\n Contact Helpline: " + SqliteDataAccess.getHelplineNumber()), 18);
                BackgroundServices.ConvertReportModelToExcel(vendViewModel.bill_number);
            }
            else
            {
                reportModel.Status = "Failed";
                BackgroundServices.updateResponses(reportModel, responses, "No");
                BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);
                BackgroundServices.UpdateVendingStatus(vendViewModel, ("Unknown Response: " + responses[1]
                    + "\n Contact Helpline: " + SqliteDataAccess.getHelplineNumber()), 18);
                BackgroundServices.ConvertReportModelToExcel(vendViewModel.bill_number);
            }


        }

        private void Next_ND_Command(Inclusive_report_model reportModel)
        {

            List<Currency_cmd_seq_model> reportDetailsAll = DeviceServices.getND_Command_Sequence();

            if (reportDetailsAll.Exists(x => x.dispensing_order == (reportModel.Process_Seq + 1))
                 || reportModel.Steps < 5)
            {
                Inclusive_report_model nextTaskInReport = new Inclusive_report_model();
                //getting the next report
                if (reportModel.Steps < 5)
                {
                    nextTaskInReport = BackgroundServices.getReportDetail(reportModel.Process_Seq,
                          (reportModel.Steps + 1));
                }
                else if (reportDetailsAll.Exists(x => x.dispensing_order == (reportModel.Process_Seq + 1)))
                {
                    nextTaskInReport = BackgroundServices.getReportDetail((reportModel.Process_Seq + 1),
                          1);
                    BackgroundServices.UpdateVendingScreen
                       (
                           vendViewModel,
                           DeviceServices.getPLC_Command_Sequence()[nextTaskInReport.Process_Seq - 1]
                       );
                }

                //updating the next report
                nextTaskInReport.Status = "In Progress";
                BackgroundServices.updateCmdResendingDetails(nextTaskInReport);
                BackgroundServices.updateReportModel(nextTaskInReport, vendViewModel.bill_number);
                // updating the vend screen status
                BackgroundServices.UpdateVendingStatus(vendViewModel, nextTaskInReport.Description, 24);
                List<Currency_cmd_seq_model> currencyDeliveryReports = DeviceServices.getND_Command_Sequence();

                Currency_cmd_seq_model command = currencyDeliveryReports.Single(x => x.sequence_order == nextTaskInReport.Steps);
                //Sending the command to ND
                CommunicationProcess(command.hexstring, command.response_reciever, nextTaskInReport);
            }
            else
            {
                NextDeviceOrScreen(reportModel);
            }
        }



        #endregion
        private void Execute_CD(Inclusive_report_model reportModel)
        {
            reportModel.Status = "In Progress";
            BackgroundServices.updateCmdResendingDetails(reportModel);
            BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);
            BackgroundServices.UpdateVendingStatus(vendViewModel, "Initializing Coin hopper", 24);


            List<Currency_cmd_seq_model> currencyDeliveryReports = DeviceServices.getCD_Command_Sequence();

            Currency_cmd_seq_model command = currencyDeliveryReports.Single(x => x.sequence_order == reportModel.Steps);
            DeviceServices.CD_Config();

            CommunicationProcess(command.hexstring, command.response_reciever, reportModel);
        }

        int countForCoinS4 = 0;

        private void CD_NextCommand(string response, Inclusive_report_model reportModel)
        {
            string[] responses = response.Split('-');

            //Resends the same command for Step 1,2 and 4 only.
            if (responses[1] == "S3Re" || responses[1] == "S2Re"
                )
            {
                reportModel.Status = "In Progress";
                BackgroundServices.updateCmdResendingDetails(reportModel);
                BackgroundServices.updateResponses(reportModel, responses, "Yes");
                BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);



                //Sending the command to CD
                List<Currency_cmd_seq_model> currencyDeliveryReports = DeviceServices.getCD_Command_Sequence();

                Currency_cmd_seq_model command = currencyDeliveryReports.Single(x => x.sequence_order == reportModel.Steps);
                CommunicationProcess(command.hexstring, command.response_reciever, reportModel);
            }
            //Doesnt care if the received response for step 3 is successful or not, just direct it to next part
            else if (responses[1] == "S4Ne")
            {
                //updating the current report model
                reportModel.Status = "Completed";
                BackgroundServices.updateResponses(reportModel, responses, "Yes");
                BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);

                Get_and_Send_Next_Process(reportModel);

            }
            
            else if (responses[1] == "S4ErR" || responses[1] == "S4")
            {
                countForCoinS4++;

                if (countForCoinS4 < 30)
                { 
                    //updating the current report model
                    reportModel.Status = "Pending";
                    Inclusive_report_model inPendingModel = BackgroundServices.getReportDetail("Pending", "CD");
                    inPendingModel.Status = "Completed";
                    BackgroundServices.updateResponses(reportModel, responses, "Yes");
                    BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);
                    OnlyResponseProcess(reportModel);

                }
            
                else
                {
                    // close all the device ports
                    // change the coin read timeout to 1
                    // Record all the values as failed
                    // display the error in the screen\
                    reportModel.Status = "In Progress";
                    BackgroundServices.updateCmdResendingDetails(reportModel);
                    BackgroundServices.updateResponses(reportModel, responses, "Yes");
                    BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);



                    //Sending the command to CD
                    List<Currency_cmd_seq_model> currencyDeliveryReports = DeviceServices.getCD_Command_Sequence();

                    Currency_cmd_seq_model command = currencyDeliveryReports.Single(x => x.sequence_order == reportModel.Steps);
                    CommunicationProcess(command.hexstring, command.response_reciever, reportModel);


                }

            }
           
            //Next actions for step 1, step 2 and step 4 successful responses
            else if (responses[1] == "S2Ne" || responses[1] == "S3Ne")
            {
                //updating the current report model
                reportModel.Status = "Completed";
                Inclusive_report_model inPendingModel = BackgroundServices.getReportDetail("Pending", "CD");
                inPendingModel.Status = "Completed";
                BackgroundServices.updateResponses(reportModel, responses, "Yes");
                BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);
                Get_and_Send_Next_Process(reportModel);
            }
            else if ( responses[1] == "S3NE")
            {
                //updating the current report model
                reportModel.Status = "Completed";
                Inclusive_report_model inPendingModel = BackgroundServices.getReportDetail("Pending", "CD");
                inPendingModel.Status = "Completed";
                BackgroundServices.updateResponses(reportModel, responses, "Yes");
                BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);
                Get_and_Send_Next_Process(reportModel);
            }

            //error Responses
            else if (responses[1] == "S2ErR"
                || responses[1] == "S3ErR")
            {
                reportModel.Status = "Failed";
                BackgroundServices.updateResponses(reportModel, responses, "Yes");
                BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);

                // updating the vend screen status
                BackgroundServices.UpdateVendingStatus(vendViewModel, (responses[2]
                    + "\n Contact Helpline: " + SqliteDataAccess.getHelplineNumber()), 18);

                BackgroundServices.ConvertReportModelToExcel(vendViewModel.bill_number);
            }
            else if (responses[1] == "NR")
            {
                if (reportModel.Cmd_sent_times <= reportModel.Cmd_Send_Maxtimes)
                {
                    reportModel.Status = "In Progress";
                    BackgroundServices.updateCmdResendingDetails(reportModel);
                    BackgroundServices.updateResponses(reportModel, responses, "No");
                    BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);

                    //Sending the command to ND
                    List<Currency_cmd_seq_model> currencyDeliveryReports = DeviceServices.getCD_Command_Sequence();

                    Currency_cmd_seq_model command = currencyDeliveryReports.Single(x => x.sequence_order == reportModel.Steps);
                    CommunicationProcess(command.hexstring, command.response_reciever, reportModel);
                }
                else
                {
                    reportModel.Status = "Connection Failed";
                    BackgroundServices.updateResponses(reportModel, responses, "No");
                    BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);
                    BackgroundServices.UpdateVendingStatus(vendViewModel, ("Connection Failed"
                        + "\n Contact Helpline: " + SqliteDataAccess.getHelplineNumber()), 18);
                    BackgroundServices.ConvertReportModelToExcel(vendViewModel.bill_number);
                }
            }
            else if (responses[1] == "TU")
            {
                reportModel.Status = "Connection Failed";
                BackgroundServices.updateResponses(reportModel, responses, "No");
                BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);
                BackgroundServices.UpdateVendingStatus(vendViewModel, ("Connection Failed"
                    + "\n Contact Helpline: " + SqliteDataAccess.getHelplineNumber()), 18);
                BackgroundServices.ConvertReportModelToExcel(vendViewModel.bill_number);
            }
            else
            {
                reportModel.Status = "Failed";
                BackgroundServices.updateResponses(reportModel, responses, "No");
                BackgroundServices.updateReportModel(reportModel, vendViewModel.bill_number);
                BackgroundServices.UpdateVendingStatus(vendViewModel, ("Unknown Response: " + responses[1]
                    + "\n Contact Helpline: " + SqliteDataAccess.getHelplineNumber()), 18);
                BackgroundServices.ConvertReportModelToExcel(vendViewModel.bill_number);
            }


        }
        private void Next_CD_Command(Inclusive_report_model reportModel)
        {

            List<Currency_cmd_seq_model> reportDetailsAll = DeviceServices.getND_Command_Sequence();

            if (reportDetailsAll.Exists(x => x.dispensing_order == (reportModel.Process_Seq + 1))
                 || reportModel.Steps < 5)
            {
                Inclusive_report_model nextTaskInReport = new Inclusive_report_model();
                //getting the next report
                if (reportModel.Steps < 5)
                {
                    nextTaskInReport = BackgroundServices.getReportDetail(reportModel.Process_Seq,
                          (reportModel.Steps + 1));
                }
                else if (reportDetailsAll.Exists(x => x.dispensing_order == (reportModel.Process_Seq + 1)))
                {
                    nextTaskInReport = BackgroundServices.getReportDetail((reportModel.Process_Seq + 1),
                          1);
                    BackgroundServices.UpdateVendingScreen
                       (
                           vendViewModel,
                           DeviceServices.getPLC_Command_Sequence()[nextTaskInReport.Process_Seq - 1]
                       );
                }

                //updating the next report
                nextTaskInReport.Status = "In Progress";
                BackgroundServices.updateCmdResendingDetails(nextTaskInReport);
                BackgroundServices.updateReportModel(nextTaskInReport, vendViewModel.bill_number);
                // updating the vend screen status
                BackgroundServices.UpdateVendingStatus(vendViewModel, nextTaskInReport.Description, 24);
                List<Currency_cmd_seq_model> currencyDeliveryReports = DeviceServices.getCD_Command_Sequence();

                Currency_cmd_seq_model command = currencyDeliveryReports.Single(x => x.sequence_order == nextTaskInReport.Steps);
                //Sending the command to ND
                CommunicationProcess(command.hexstring, command.response_reciever, nextTaskInReport);
            }
            else
            {
                NextDeviceOrScreen(reportModel);
            }
        }

    }
}


