using System.IO.Ports;
using sample2.viewModel;
using sample2.remote;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Threading;
using System;
using sample2.User_Controls;
using sample2.helpers;
using sample2.models;
using static sample2.models.DeviceServiceModel;
using static sample2.models.BackgroundWorkerModel;
using System.Text;

namespace sample2.services
{
    public static class DeviceServices
    {
        private static SerialPort devicePort = new SerialPort();
        public static VendScreenViewModel vendViewModel;
        public static string screen_name, response_string = "NR";
        private static Output_Result filter_function;
        private static List<byte> byte_list;
        private static void updateStatus(string status)
        {
            if (screen_name == "Vending Screen")
                vendViewModel.txtVendStatus = status;
        }
        private static List<Currency_cmd_seq_model> currency_CmdSeqFramework = new List<Currency_cmd_seq_model>();
        private static List<PLC_sub_cmd_seq_model> PLC_CmdSeqFramework = new List<PLC_sub_cmd_seq_model>();
        private static List<PLC_CommandSequence_Model> PLC_Command_Sequence = new List<PLC_CommandSequence_Model>();
        private static List<Currency_cmd_seq_model> CD_currency_CmdSeqFramework = new List<Currency_cmd_seq_model>();

        #region INITIALIZING 


        public static void Init_PLC_main_commands(List<cart_item> cart_items)
        {
            PLC_Command_Sequence = new List<PLC_CommandSequence_Model>();
            DeviceServiceModel deviceModel = new DeviceServiceModel();
            int total_quantity_in_bill = 0;

            int quantities = 1;
            foreach (var item in cart_items)
            {

                int total_item_quantity = int.Parse(item.Product_quantity.Text);
                for (int i = 1; i <= total_item_quantity; i++)
                {
                    PLC_CommandSequence_Model plcCommand = new PLC_CommandSequence_Model();
                    CellModel tray_model = SqliteDataAccess.getProductCellDetails(item.Product_Name.Text);
                    int cell_number = routines.row_col_convertion(tray_model.CT_Row_No, tray_model.CT_Col_No);
                    byte[] product_image = SqliteDataAccess.getProductImage(item.Product_Name.Text);
                    total_quantity_in_bill++;


                    plcCommand.hexstring = deviceModel.send_PLC_Coil_Write_request(cell_number);
                    plcCommand.product_name = item.Product_Name.Text;
                    plcCommand.product_image = SqliteChange.byteArrayToImage(product_image);
                    plcCommand.dispensing_order = quantities;
                    plcCommand.sequence_order = total_quantity_in_bill;
                    plcCommand.delivery_quantity = total_item_quantity;
                    plcCommand.cellNo = "R" + tray_model.CT_Row_No + "C" + tray_model.CT_Col_No;
                    plcCommand.balance_before_delivery = tray_model.CT_Balance_Qty;

                    PLC_Command_Sequence.Add(plcCommand);
                    quantities++;
                }
            }
            Init_PLC_sub_commands();

        }

        public static void Init_PLC_sub_commands()
        {
            DeviceServiceModel deviceModel = new DeviceServiceModel();
            PLC_CmdSeqFramework = new List<PLC_sub_cmd_seq_model>();
            PLC_sub_cmd_seq_model addingModel = new PLC_sub_cmd_seq_model();

            addingModel.command_name = "Running Status Check";
            addingModel.hexstring = deviceModel.send_PLC_Coil_Wait_Status_request();
            addingModel.response_reciever = deviceModel.PLC_response_Step1_filter;
            addingModel.sequence_order = 1;
            addingModel.repeat_times = 10;
            PLC_CmdSeqFramework.Add(addingModel);

            addingModel = new PLC_sub_cmd_seq_model();
            addingModel.command_name = "Error Check";
            addingModel.hexstring = deviceModel.send_PLC_Feedback_Status_request();
            addingModel.response_reciever = deviceModel.PLC_response_Step2_Filter;
            addingModel.sequence_order = 2;
            addingModel.repeat_times = 10;
            PLC_CmdSeqFramework.Add(addingModel);

            addingModel = new PLC_sub_cmd_seq_model();
            addingModel.command_name = "MAIN COMMAND";
            addingModel.response_reciever = deviceModel.PLC_response_Step3_Filter;
            addingModel.sequence_order = 3;
            addingModel.repeat_times = 1;
            PLC_CmdSeqFramework.Add(addingModel);

            addingModel = new PLC_sub_cmd_seq_model();
            addingModel.command_name = "Running Status Check 2";
            addingModel.hexstring = deviceModel.send_PLC_Coil_Wait_Status_request();
            addingModel.response_reciever = deviceModel.PLC_response_Step4_filter;
            addingModel.sequence_order = 4;
            addingModel.repeat_times = 18;
            PLC_CmdSeqFramework.Add(addingModel);

            addingModel = new PLC_sub_cmd_seq_model();
            addingModel.command_name = "Stop Command";
            addingModel.hexstring = deviceModel.send_PLC_Stop_Write_request();
            addingModel.response_reciever = deviceModel.PLC_response_Step5_Filter;
            addingModel.sequence_order = 5;
            addingModel.repeat_times = 18;
            PLC_CmdSeqFramework.Add(addingModel);

            addingModel = new PLC_sub_cmd_seq_model();
            addingModel.command_name = "Completing Process...";
            addingModel.hexstring = deviceModel.send_PLC_Coil_Reset_Write_request();
            addingModel.response_reciever = deviceModel.PLC_response_Step6_Filter;
            addingModel.sequence_order = 6;
            addingModel.repeat_times = 10;
            PLC_CmdSeqFramework.Add(addingModel);

            addingModel = new PLC_sub_cmd_seq_model();
           addingModel.command_name = "Completed !!";
            addingModel.hexstring = deviceModel.send_PLC_Register_Reset_Write_request();
           addingModel.response_reciever = deviceModel.PLC_response_Step7_Filter;
            addingModel.sequence_order = 7;
            addingModel.repeat_times = 10;
            PLC_CmdSeqFramework.Add(addingModel);

        }

        public static List<Currency_cmd_seq_model> Init_ND_seq_commands(CurrencyTransactionModel transModel, int dispensing_order, int[] splitted_amt)
        {
            DeviceServiceModel deviceModel = new DeviceServiceModel();
            currency_CmdSeqFramework = new List<Currency_cmd_seq_model>();

            Currency_cmd_seq_model addingModel = new Currency_cmd_seq_model();
            addingModel.command_name = "Status Check";
            addingModel.device_used = "ND";
            addingModel.hexstring = deviceModel.send_ND_request(ND_Payout_Request("S"));
            addingModel.response_reciever = deviceModel.ND_response_Step1_filter;
            addingModel.sequence_order = 1;
            addingModel.dispensing_order = dispensing_order;
            addingModel.repeat_times = 10;
            addingModel.balance_before_delivery = transModel.Cr_Opening_Balance_Qty;
            addingModel.denomination = "" + transModel.Cr_Denomination;
            addingModel.deviceNo = "00";
            currency_CmdSeqFramework.Add(addingModel);

            addingModel = new Currency_cmd_seq_model();
            addingModel.command_name = "Amt Sent Check";
            addingModel.device_used = "ND";
            addingModel.hexstring = deviceModel.send_ND_request(ND_Payout_Request("CHK"));
            addingModel.response_reciever = deviceModel.ND_response_Step2_filter;
            addingModel.sequence_order = 2;
            addingModel.dispensing_order = dispensing_order;
            addingModel.repeat_times = 10;
            addingModel.balance_before_delivery = transModel.Cr_Opening_Balance_Qty;
            addingModel.denomination = "" + transModel.Cr_Denomination;
            addingModel.deviceNo = "00";
            currency_CmdSeqFramework.Add(addingModel);

            addingModel = new Currency_cmd_seq_model();
            addingModel.command_name = "DISPENSING COMMAND";
            addingModel.device_used = "ND";
            addingModel.hexstring = deviceModel.send_ND_request(ND_Payout_Request("B", 0, splitted_amt[0], splitted_amt[1]));
            addingModel.response_reciever = deviceModel.ND_response_Step3_filter;
            addingModel.sequence_order = 3;
            addingModel.dispensing_order = dispensing_order;
            addingModel.repeat_times = 1;
            addingModel.balance_before_delivery = transModel.Cr_Opening_Balance_Qty;
            addingModel.denomination = "" + transModel.Cr_Denomination;
            addingModel.deviceNo = "00";
            currency_CmdSeqFramework.Add(addingModel);

            addingModel = new Currency_cmd_seq_model();
            addingModel.command_name = "Amt Sent Check 2";
            addingModel.device_used = "ND";
            addingModel.hexstring = deviceModel.send_ND_request(ND_Payout_Request("CHK"));
            addingModel.response_reciever = deviceModel.ND_response_Step4_filter;
            addingModel.sequence_order = 4;
            addingModel.dispensing_order = dispensing_order;
            addingModel.repeat_times = 10;
            addingModel.balance_before_delivery = transModel.Cr_Opening_Balance_Qty;
            addingModel.denomination = "" + transModel.Cr_Denomination;
            addingModel.deviceNo = "00";
            currency_CmdSeqFramework.Add(addingModel);

            addingModel = new Currency_cmd_seq_model();
            addingModel.command_name = "Clear Amt";
            addingModel.device_used = "ND";
            addingModel.hexstring = deviceModel.send_ND_request(ND_Payout_Request("CLR"));
            addingModel.response_reciever = deviceModel.ND_response_Step5_filter;
            addingModel.sequence_order = 5;
            addingModel.dispensing_order = dispensing_order;
            addingModel.repeat_times = 10;
            addingModel.balance_before_delivery = transModel.Cr_Opening_Balance_Qty;
            addingModel.denomination = "" + transModel.Cr_Denomination;
            addingModel.deviceNo = "00";
            currency_CmdSeqFramework.Add(addingModel);

            return currency_CmdSeqFramework;
        }

        public static List<Currency_cmd_seq_model> Init_CD_seq_commands(CurrencyTransactionModel transModel, int dispensing_order, int[] splitted_amt)
        {
            DeviceServiceModel deviceModel = new DeviceServiceModel();
            CD_currency_CmdSeqFramework = new List<Currency_cmd_seq_model>();

            Currency_cmd_seq_model addingModel = new Currency_cmd_seq_model();
            addingModel.command_name = "Status Check";
            addingModel.device_used = "CD";
            addingModel.hexstring = deviceModel.send_CD_status_request();
            addingModel.response_reciever = deviceModel.CD_response_Step3_filter;
            addingModel.sequence_order = 1;
            addingModel.dispensing_order = dispensing_order;
            addingModel.repeat_times = 10;
            addingModel.balance_before_delivery = transModel.Cr_Opening_Balance_Qty;
            addingModel.denomination = "" + transModel.Cr_Denomination;
            addingModel.deviceNo = "00";
            CD_currency_CmdSeqFramework.Add(addingModel);

            addingModel = new Currency_cmd_seq_model();
            addingModel.command_name = "Main Command";
            addingModel.device_used = "CD";
            addingModel.hexstring = deviceModel.send_CD_request(CD_Payout_Request(splitted_amt[1]));
            addingModel.response_reciever = deviceModel.CD_response_Step2_filter;
            addingModel.sequence_order = 2;
            addingModel.dispensing_order = dispensing_order;
            addingModel.repeat_times = 1;
            addingModel.balance_before_delivery = transModel.Cr_Opening_Balance_Qty;
            addingModel.denomination = "" + transModel.Cr_Denomination;
            addingModel.deviceNo = "00";
            CD_currency_CmdSeqFramework.Add(addingModel);

            addingModel = new Currency_cmd_seq_model();
            addingModel.command_name = "Status Check 2";
            addingModel.device_used = "CD";
            addingModel.hexstring = deviceModel.send_CD_status_request();
            addingModel.response_reciever = deviceModel.CD_response_Step3_filter;
            addingModel.sequence_order = 3;
            addingModel.dispensing_order = dispensing_order;
            addingModel.repeat_times = 10;
            addingModel.balance_before_delivery = transModel.Cr_Opening_Balance_Qty;
            addingModel.denomination = "" + transModel.Cr_Denomination;
            addingModel.deviceNo = "00";
            CD_currency_CmdSeqFramework.Add(addingModel);

            addingModel = new Currency_cmd_seq_model();
            addingModel.command_name = "Reset";
            addingModel.device_used = "CD";
            addingModel.hexstring = deviceModel.send_CD_Reset_request();
            addingModel.response_reciever = deviceModel.CD_response_Step4_filter;
            addingModel.sequence_order = 4;
            addingModel.dispensing_order = dispensing_order;
            addingModel.repeat_times = 10;
            addingModel.balance_before_delivery = transModel.Cr_Opening_Balance_Qty;
            addingModel.denomination = "" + transModel.Cr_Denomination;
            addingModel.deviceNo = "00";
            CD_currency_CmdSeqFramework.Add(addingModel);

            addingModel = new Currency_cmd_seq_model();
            addingModel.command_name = "Status Check3";
            addingModel.device_used = "CD";
            addingModel.hexstring = deviceModel.send_CD_status_request();
            addingModel.response_reciever = deviceModel.CD_response_Step3_filter;
            addingModel.sequence_order = 5;
            addingModel.dispensing_order = dispensing_order;
            addingModel.repeat_times = 10;
            addingModel.balance_before_delivery = transModel.Cr_Opening_Balance_Qty;
            addingModel.denomination = "" + transModel.Cr_Denomination;
            addingModel.deviceNo = "00";
            CD_currency_CmdSeqFramework.Add(addingModel);

            return CD_currency_CmdSeqFramework;
        }



        #endregion



        public static List<PLC_CommandSequence_Model> getPLC_Command_Sequence()
        {
            return PLC_Command_Sequence;
        }
        public static List<Currency_cmd_seq_model> getND_Command_Sequence()
        {
            return currency_CmdSeqFramework;
        }
        public static List<Currency_cmd_seq_model> getCD_Command_Sequence()
        {
            return CD_currency_CmdSeqFramework;
        }

        public static List<PLC_sub_cmd_seq_model> getPLC_Sub_Command_Sequence()
        {
            return PLC_CmdSeqFramework;
        }

        public static void CloseThePort()
        {
            if (devicePort.IsOpen)
                devicePort.Close();
        }

        #region SEND & RECEIVE

        private delegate void UpdateUiTextDelegate(List<byte> output_data);
        public static void Receive(object sender, SerialDataReceivedEventArgs e)
        {
            // Collecting the characters received to our 'buffer' (string).
            int bytes = devicePort.BytesToRead;
            if (response_string == "NR-NR-No Response.")
                byte_list = new List<byte>();
            byte[] output_data = new byte[bytes];
            devicePort.Read(output_data, 0, bytes);
            foreach (byte item in output_data)
            {
                byte_list.Add(item);
            }

            string deviceName = SqliteChange.getDeviceName(devicePort.PortName);

            response_string = byteToString(byte_list) + "-";
            Inclusive_report_model inProgressModel = BackgroundServices.getReportDetail("In Progress", deviceName);
            if (inProgressModel.Cmd_sent_times > inProgressModel.Cmd_Send_Maxtimes)
                response_string += "TU-Communicaton Failed.";
            else
                response_string += filter_function(byte_list, inProgressModel);
        }
        public static string waitForResponse()
        {
            Thread.Sleep(devicePort.ReadTimeout);
            return response_string;
        }

        private static void ErrorData(string output_data)
        {
            updateStatus("Error!! Call the support: " + SqliteDataAccess.getHelplineNumber() + "Message: "
                + output_data);
        }

        public static string byteToString(List<byte> hexstring)
        {
            string output_val = "";
            foreach (byte hexval in hexstring)
            {
                output_val += hexval.ToString("X2") + " ";
            }
            return output_val;
        }



        private delegate void UpdateUiTextDelegate1(string output_data);
        public static void CmdSend(List<byte> hexstring, Output_Result result_method)
        {
            try
            {
                filter_function = result_method;
                response_string = "NR-NR-No Response.";
                if (!devicePort.IsOpen) devicePort.Open();
                byte_list = new List<byte>();
                foreach (byte hexval in hexstring)
                {
                    byte[] _hexval = new byte[] { hexval };
                    devicePort.Write(_hexval, 0, 1);
                    Thread.Sleep(1);
                }

            }
            catch (Exception ex)
            {
                Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
                dispatcher.BeginInvoke(DispatcherPriority.Send, new UpdateUiTextDelegate1(ErrorData), ex.Message);
            }

        }


        public static byte[] ND_Payout_Request(string request_code, int Hundrenth = 0,
            int Tenth = 0, int oneth = 0)
        {
            byte[] data_content;
            switch (request_code)
            {
                case "B":
                    data_content = Encoding.ASCII.GetBytes("00" + request_code + "00" + Hundrenth + Tenth);
                    break;
                case "CLR":
                    //Clear Accumulated or dispensed numbers
                    data_content = Encoding.ASCII.GetBytes("00I0001");
                    break;
                case "CHK":
                    //Checks dispensed numbers
                    data_content = Encoding.ASCII.GetBytes("00C0000");
                    break;

                default:
                    data_content = Encoding.ASCII.GetBytes("00S0000");
                    break;
            }
            return data_content;
        }

        public static byte CD_Payout_Request(
     int oneth = 0)
        {
            byte data_content = Convert.ToByte(oneth);

           
            return data_content;
        }
        public static void SetReadTimeOut(int Multi_factor = 1)
        {
            devicePort.ReadTimeout = 200;
            devicePort.ReadTimeout = devicePort.ReadTimeout * Multi_factor;
        }

        #endregion

        public static void ND_error_list(byte error_code, string result_data)
        {
            switch (error_code)
            {
                case 0x31:
                    result_data += "-No bills dispensed";
                    break;
                case 0x32:
                    result_data += "-Jam";
                    break;
                case 0x33:
                    result_data += "-Chain";
                    break;
                case 0x34:
                    result_data += "-Half";
                    break;
                case 0x35:
                    result_data += "-Short";
                    break;
                case 0x36:
                    result_data += "-No bills dispensed by start button";
                    break;
                case 0x37:
                    result_data += "-Double";
                    break;
                case 0x38:
                    result_data += "-Over 4000 pcs";
                    break;
                case 0x39:
                    result_data += "-Communication Error";
                    break;
                case 0x41:
                    result_data += "-Encoder Error";
                    break;
                case 0x42:
                    result_data += "-IR LED L Error";
                    break;
                case 0x43:
                    result_data += "-IR LED R Error";
                    break;
                case 0x44:
                    result_data += "-IR Sensor L Error";
                    break;
                case 0x46:
                    result_data += "-IR Sensor R Error";
                    break;
                case 0x47:
                    result_data += "-IR Sensor Different Error";
                    break;
                case 0x48:
                    result_data += "-Bill Low Level Warning";
                    break;
                case 0x49:
                    result_data += "-Low Power Error";
                    break;
                default:
                    result_data += "-0";
                    break;
            }
        }

        public static bool PLC_Modbus_Data_Verify(List<byte> response)
        {

            List<byte> hexstring = new List<byte>();
            for (int i = 0; i < response.Count - 2; i++)
            {
                hexstring.Add(response[i]);
            }
            List<byte> CRC_Result = routines.ModRTU_CRC(hexstring);
            if (response[response.Count - 1] == CRC_Result[0] && response[response.Count - 2] == CRC_Result[1])
            {
                return true;
            }
            else return false;
        }

        public static bool ND_Checksum_Data_Verify(List<byte> response)
        {

            byte hexstring = 0;
            for (int i = 0; i < response.Count - 2; i++)
            {
                hexstring += response[i];
            }
            if (response[response.Count - 2] == hexstring)
            {
                return true;
            }
            else return false;
        }
        public static bool CD_Checksum_Data_Verify(List<byte> response)
        {

            byte hexstring = 0;
            for (int i = 0; i < response.Count - 1; i++)
            {
                hexstring += response[i];
            }
            if (response[response.Count - 1] == hexstring)
            {
                return true;
            }
            else return false;
        }

        #region CONFIGURATIONS

        public static void PLC_Config()
        {
            if (devicePort.IsOpen)
                devicePort.Close();

            devicePort = new SerialPort();
            devicePort.PortName = SqliteDataAccess.getPort("PLC");
            devicePort.BaudRate = 19200; // baud rate - 9600 for both note dispenser, bill validator and coin hopper, 19200 for PLC
            devicePort.Handshake = Handshake.None;
            devicePort.Parity = Parity.None; // for Note Dispenser and PLC
            devicePort.DataBits = 8;
            devicePort.StopBits = StopBits.Two;
            devicePort.ReadTimeout = 1000;
            devicePort.WriteTimeout = 50;
            devicePort.DataReceived += new SerialDataReceivedEventHandler(Receive);
        }

        public static void ND_Config()
        {
            if (devicePort.IsOpen)
                devicePort.Close();

            //Sets up serial port
            devicePort = new SerialPort();
            devicePort.PortName = SqliteDataAccess.getPort("ND");
            devicePort.BaudRate = 9600; // baud rate - 9600 for both note dispenser, bill validator and coin hopper, 19200 for PLC
            devicePort.Handshake = Handshake.None;
            devicePort.Parity = Parity.None; // for Note Dispenser and PLC
            devicePort.DataBits = 8;
            devicePort.StopBits = StopBits.One;
            devicePort.ReadTimeout = 1000;
            devicePort.WriteTimeout = 50;
            devicePort.DataReceived += new SerialDataReceivedEventHandler(Receive);
        }

        public static void BA_Config()
        {
           
        }
        public static void Printer_Config() { }
        public static void CD_Config()
        {

            if (devicePort.IsOpen)
                devicePort.Close();

            //Sets up serial port
            devicePort = new SerialPort();
            devicePort.PortName = SqliteDataAccess.getPort("CD");
            devicePort.BaudRate = 9600; // baud rate - 9600 for both note dispenser, bill validator and coin hopper, 19200 for PLC
            devicePort.Handshake = Handshake.None;
            devicePort.Parity = Parity.Even; // for Note Dispenser and PLC
            devicePort.DataBits = 8;
            devicePort.StopBits = StopBits.One;
            devicePort.ReadTimeout = 1000;
            devicePort.WriteTimeout = 50;
            devicePort.DataReceived += new SerialDataReceivedEventHandler(Receive);
        }
        public static void Card_Config() { }
        public static void Scanner_Config() { }
        #endregion
    }
}

