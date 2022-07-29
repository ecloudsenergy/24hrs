using sample2.helpers;
using sample2.services;
using System;
using sample2.remote;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static sample2.models.BackgroundWorkerModel;

namespace sample2.models
{
    public class DeviceServiceModel
    {
        public delegate string Output_Result(List<byte> output_data, Inclusive_report_model ReportModel);

        public class PLC_CommandSequence_Model
        {
            public List<byte> hexstring { get; set; }
            public BitmapImage product_image { get; set; }
            public string product_name { get; set; }
            /** dispensing quantity order starts from zero and its just for displaying
            * the number in the main screen. it is not used for anything else. **/
            public int dispensing_order { get; set; }
            public int sequence_order { get; set; }
            public string cellNo { get; set; }
            public int balance_before_delivery { get; set; }
            public int delivery_quantity { get; set; }
        }

        public class PLC_sub_cmd_seq_model
        {

            public List<byte> hexstring { get; set; }
            public string command_name { get; set; }
            public Output_Result response_reciever { get; set; }
            public int sequence_order { get; set; }
            public int repeat_times { get; set; }
        }

        public class Currency_cmd_seq_model
        {
            public string device_used { get; set; }
            public List<byte> hexstring { get; set; }
            public string command_name { get; set; }
            public Output_Result response_reciever { get; set; }
            public int sequence_order { get; set; }
            public int repeat_times { get; set; }
            public string denomination { get; set; }
            public string deviceNo { get; set; }
            public int balance_before_delivery { get; set; }
            public int delivery_quantity { get; set; }
            public int dispensing_order { get; set; }
        }

        #region ND REQUESTS
        public List<byte> send_ND_request(byte[] data_content)
        {
            List<byte> hexstring = new List<byte>();
            hexstring.Add(0x02);
            //Adding each byte into hexstring
            for (int i = 0; i < data_content.Length; i++) hexstring.Add(data_content[i]);
            hexstring.Add(routines.Checksum(hexstring));
            hexstring.Add(0x03);

            return hexstring;
        }

        #endregion

        #region ND RESPONSES
        public string ND_response_Step1_filter(List<byte> output_data, Inclusive_report_model ReportModel)
        {
            string response = "S1";
            if (output_data.Count > 4)
            {
                if (DeviceServices.ND_Checksum_Data_Verify(output_data))
                {
                    switch (output_data[3])
                    {
                        case 0x45:
                        case 0x65:
                            response += "ErR";
                            DeviceServices.ND_error_list(output_data[4], response);
                            break;
                        case 0x73:
                            response += "Ne-Successful!";
                            break;
                        default:
                            response += "Re-Wrong Response.";
                            break;
                    }
                }
                else
                {
                    response += "Re-No or Wrong Response.";
                }
            }
            else
            {
                response += "Re-No or Wrong Response.";
            }
            return response;
        }

        public string ND_response_Step2_filter(List<byte> output_data, Inclusive_report_model ReportModel)
        {
            string response = "S2";
            if (output_data.Count > 4)
            {
                if (DeviceServices.ND_Checksum_Data_Verify(output_data))
                {
                    switch (output_data[3])
                    {
                        case 0x45:
                        case 0x65:
                            response += "ErR";
                            DeviceServices.ND_error_list(output_data[4], response);
                            break;
                        case 0x63:
                            if (output_data[5] == 0x30 && output_data[6] == 0x30 && output_data[7] == 0x30)
                                response += "Ne-Successful!";
                            else
                                response += "NeZero-Make Zero in the Cash Dispenser.";
                            break;
                        default:
                            response += "Re-Wrong Response.";
                            break;
                    }
                }
                else
                {
                    response += "Re-No or Wrong Response.";
                }
            }
            else
            {
                response += "Re-No or Wrong Response.";
            }
            return response;
        }

        public string ND_response_Step3_filter(List<byte> output_data, Inclusive_report_model ReportModel)
        {
            string response = "S3";

            switch (output_data[0])
            {
                case 0x06:
                    response += "Skip-Successful!";
                    break;
                case 0x0a:
                    response += "Skip-Not Acknowledged!";
                    break;
                default:
                    response += "Skip-No or Wrong Response.";
                    break;
            }
            return response;
        }

        public string ND_response_Step4_filter(List<byte> output_data, Inclusive_report_model ReportModel)
        {
            string response = "S4";
            if (output_data.Count > 4)
            {
                if (DeviceServices.ND_Checksum_Data_Verify(output_data))
                {
                    switch (output_data[3])
                    {
                        case 0x45:
                        case 0x65:
                            response += "ErR";
                            DeviceServices.ND_error_list(output_data[4], response);
                            break;
                        case 0x63:
                            if (output_data[5] != 0x30 || output_data[6] != 0x30 || output_data[7] != 0x30)
                                response += "Ne-Successful!";
                            else
                                response += "ErR-No amount has been dispensed.";
                            break;

                        default:
                            response += "Re-Wrong Response.";
                            break;
                    }
                }
                else
                {
                    response += "Re-No or Wrong Response.";
                }
            }
            else
            {
                response += "Re-No or Wrong Response.";
            }
            return response;
        }

        public string ND_response_Step5_filter(List<byte> output_data, Inclusive_report_model ReportModel)
        {
            string response = "S5";

            switch (output_data[0])
            {
                case 0x06:
                    response += "Ne-Successful!";
                    break;
                case 0x0a:
                    response += "Re-Not Acknowledged!";
                    break;
                default:
                    response += "No or Wrong Response.";
                    break;
            }

            return response;
        }

        #endregion

        #region PLC REQUESTS

        public List<byte> send_PLC_Coil_Wait_Status_request()
        {

            List<byte> hexstring = new List<byte>();

            hexstring.Add(0x01);
            hexstring.Add(0x02);
            hexstring.Add(0x00);
            hexstring.Add((byte)(0x00));
            hexstring.Add(0x00);
            hexstring.Add(0x01);
            List<byte> CRC_Result = routines.ModRTU_CRC(hexstring);
            hexstring.Add(CRC_Result[1]); //CRC Big Endian
            hexstring.Add(CRC_Result[0]);//CRC Big Endian
            return hexstring;
        }
        public List<byte> send_reset_feedback_write_request()

        {

            List<byte> hexstring = new List<byte>();

            hexstring.Add(0x01);
            hexstring.Add(0x0F);
            hexstring.Add(0x00);
            hexstring.Add((byte)(0x05));
            hexstring.Add(0x00);
            hexstring.Add(0x40);
            hexstring.Add(0x08);
            hexstring.Add(0x00);
            hexstring.Add(0x00);
            hexstring.Add(0x00);
            hexstring.Add(0x00);
            hexstring.Add(0x00);
            hexstring.Add(0x00);
            hexstring.Add(0x00);
            hexstring.Add(0x00);
            List<byte> CRC_Result = routines.ModRTU_CRC(hexstring);
            hexstring.Add(CRC_Result[1]); //CRC Big Endian
            hexstring.Add(CRC_Result[0]);//CRC Big Endian
            return hexstring;
        }
        public List<byte> send_PLC_Coil_Write_request(int cell_number)
        {
            List<byte> hexstring = new List<byte>();
            if (cell_number > 0 && cell_number <= 64)
            {
                hexstring.Add(0x01);
                hexstring.Add(0x06);
                hexstring.Add(0x00);
                hexstring.Add(0x21);
                hexstring.Add(0x00);
                hexstring.Add((byte)(cell_number));
                List<byte> CRC_Result = routines.ModRTU_CRC(hexstring);
                hexstring.Add(CRC_Result[1]); //CRC Big Endian
                hexstring.Add(CRC_Result[0]);//CRC Big Endian
            }

            return hexstring;
        }
        public List<byte> send_PLC_Feedback_Status_request()
        {
            //timerStarted_process();
            List<byte> hexstring = new List<byte>();
            hexstring.Add(0x01);
            hexstring.Add(0x01);
            hexstring.Add(0x00);
            hexstring.Add(0x05);
            hexstring.Add(0x00);
            hexstring.Add(0x40);
            List<byte> CRC_Result = routines.ModRTU_CRC(hexstring);
            hexstring.Add(CRC_Result[1]); //CRC Big Endian
            hexstring.Add(CRC_Result[0]);//CRC Big Endian

            return hexstring;
        }
        public List<byte> send_feedbackBit_reset_Write_request()
        {
            //timerStarted_process();
            List<byte> hexstring = new List<byte>();

            hexstring.Add(0x01);
            hexstring.Add(0x05);
            hexstring.Add(0x00);
            hexstring.Add(0x4C);
            hexstring.Add(0x00);
            hexstring.Add(0x00);
            List<byte> CRC_Result = routines.ModRTU_CRC(hexstring);
            hexstring.Add(CRC_Result[1]);//CRC Big Endian
            hexstring.Add(CRC_Result[0]);//CRC Big Endian

            return hexstring;
        }
        /*List<byte> send_PLC_Register_Write_request()
        {
            List<byte> hexstring = new List<byte>();
            hexstring.Add(0x01);
            hexstring.Add(0x06);
            hexstring.Add(0x00);
            hexstring.Add(0x01);
            hexstring.Add(0x00);
            hexstring.Add(0x00);
            List<byte> CRC_Result = ModRTU_CRC(hexstring);
            hexstring.Add(CRC_Result[1]); //CRC Big Endian
            hexstring.Add(CRC_Result[0]);//CRC Big Endian

            return hexstring;
        }
        */
        public List<byte> send_PLC_light_Write_request()
        {
            //timerStarted_process();
            List<byte> hexstring = new List<byte>();
            hexstring.Add(0x01);
            hexstring.Add(0x06);
            hexstring.Add(0x00);
            hexstring.Add(0x01);
            hexstring.Add(0x00);
            hexstring.Add(0x00);
            List<byte> CRC_Result = routines.ModRTU_CRC(hexstring);
            hexstring.Add(CRC_Result[1]); //CRC Big Endian
            hexstring.Add(CRC_Result[0]);//CRC Big Endian

            return hexstring;
        }
        public List<byte> send_PLC_Stop_Write_request()
        {
            DeviceServices.SetReadTimeOut();
            //timerStarted_process();
            List<byte> hexstring = new List<byte>();
            hexstring.Add(0x01);
            hexstring.Add(0x05);
            hexstring.Add(0x00);
            hexstring.Add(0x63);
            hexstring.Add(0xFF);
            hexstring.Add(0x00);
            List<byte> CRC_Result = routines.ModRTU_CRC(hexstring);
            hexstring.Add(CRC_Result[1]); //CRC Big Endian
            hexstring.Add(CRC_Result[0]);//CRC Big Endian

            return hexstring;
        }
        public List<byte> send_PLC_Register_Reset_Write_request()
        {
            //timerStarted_process();
            List<byte> hexstring = new List<byte>();
            hexstring.Add(0x01);
            hexstring.Add(0x06);
            hexstring.Add(0x00);
            hexstring.Add(0x21);
            hexstring.Add(0x00);
            hexstring.Add(0x00);
            List<byte> CRC_Result = routines.ModRTU_CRC(hexstring);
            hexstring.Add(CRC_Result[1]); //CRC Big Endian
            hexstring.Add(CRC_Result[0]);//CRC Big Endian

            return hexstring;
        }
        public List<byte> send_PLC_Coil_Reset_Write_request()
        {
            //timerStarted_process();
            List<byte> hexstring = new List<byte>();
            hexstring.Add(0x01);
            hexstring.Add(0x0F);
            hexstring.Add(0x00);
            hexstring.Add(0x45);
            hexstring.Add(0x00);
            hexstring.Add(0x1E);
            hexstring.Add(0x04);
            hexstring.Add(0x00);
            hexstring.Add(0x00);
            hexstring.Add(0x00);
            hexstring.Add(0x00);
            List<byte> CRC_Result = routines.ModRTU_CRC(hexstring);
            hexstring.Add(CRC_Result[1]); //CRC Big Endian
            hexstring.Add(CRC_Result[0]);//CRC Big Endian

            return hexstring;
        }

        #endregion

        #region PLC RESPONSES
        /*   private string response_register_status_PLC(List<byte> output_data)
           {
               string result_data = "";
               if (output_data[4] == 0) result_data = "R1";
               else result_data = "R2";
               return result_data;
           }*/
        public string PLC_response_Step1_filter(List<byte> output_data, Inclusive_report_model ReportModel)
        {
            
            string response = "S1";
            
            if (output_data.Count > 3)
            {
                if (DeviceServices.PLC_Modbus_Data_Verify(output_data))
                {
                    switch (output_data[3])
                    {
                        case 1:
                            response += "ErR-Motor Live Error";
                            break;
                        case 0:
                            response += "Ne-Successful!";
                            break;
                        default:
                            response += "Re-Wrong Response.";
                            break;
                    }
                }
                else
                {
                    response += "Re-No or Wrong Response.";
                }
            }
            else
            {
                response += "Re-No or Wrong Response.";
            }
            return response;
        }
        public string PLC_response_Step2_Filter(List<byte> output_data, Inclusive_report_model ReportModel)
        {
            string response = "S2";
            
            response += PLC_feedback_filter(output_data);
            return response;
        }


        public string PLC_response_Step3_Filter(List<byte> output_data, Inclusive_report_model ReportModel)
        {
            DeviceServices.SetReadTimeOut(1);
            string response = "S3";
     
            string hexstring = DeviceServices.byteToString(output_data);
            if (output_data.Count > 3)
            {
                if (DeviceServices.PLC_Modbus_Data_Verify(output_data))
                {
                    if (hexstring == ReportModel.Cmd_Hexstring_Data)
                        response += "Skip-Successful!";
                    else
                        response += "Skip-Wrong Response.";
                }
                else
                {
                    response += "Skip-No or Wrong Response.";
                }
            }
            else
            {
                response += "Skip-No or Wrong Response.";
            }
            return response;
        }

        int Operation = 1;
      
        public string PLC_response_Step4_filter(List<byte> output_data, Inclusive_report_model ReportModel)
        {
            int b = 0;
            int Timer = Sqlitedatavr.get_motor_timer() - 1000;
            if(Timer == 2000)
            {
                b = 5 ;
            }
            else if (Timer == 2500)
            {
                b = 5;
            }
            else if (Timer == 3000)
            {
                b = 5;
            }
            else if(Timer == 1500 )
            {
                b = 4;
            }
            else if(Timer == 1000)
            {
                b = 2;
            }
            else if (Timer == 500)
            {
                b = 0;
            }
            else if (Timer == 0)
            {
                b = 0;
            }

            int Time =  Timer / 200;
           
            int a = Time - b;

            string response = "S4";
            
            if (output_data.Count > 3)
            {
                if (DeviceServices.PLC_Modbus_Data_Verify(output_data))
                {
                    switch (output_data[3])
                    {
                        case 1:
                            if (ReportModel.Response_received_times <= a)     
                            {
                                response += "ReP-Motor Running.";  
                            }
                            else
                            {
                               response += "ErCR-Motor Continuous Running Error.";
                                Operation += 1;
                            }
                            break;
                        case 0:
                            if (ReportModel.Response_received_times >= 1 && ReportModel.Cmd_sent_times >= 2)
                            {
                                response += "Ne-Successfully finished delivery!";
                                Operation += 2;
                            }
                            else
                            {
                                response += "ErNR-Motor Not Running Error.";
                            }
                            break;
                        default:
                            response += "ReN-Wrong Response.";
                            break;
                    }
                }
                else
                {
                    response += "ReN-No or Wrong Response.";
                }
            }
            else
            {
                response += "ReN-No or Wrong Response.";
            }
           
            return response;
        }

        public string PLC_response_Step5_Filter(List<byte> output_data, Inclusive_report_model ReportModel)
        {
            
            string response = "S5";

            if (Operation == 3 || Operation == 5 || Operation == 7 || Operation == 9 || Operation == 11 )
            {
                response += "Ne-Successful!";
            }
            else if (Operation == 2)
            {
                response += "error-Successful!";
            }
            else
            {
                response +="Re-No or Wrong Response.";
            }

            return response;
            

        }
        public string PLC_response_Step6_Filter(List<byte> output_data, Inclusive_report_model ReportModel)
        {

            string response = "S6";
       
            response += "Ne-Successful!";
            return response;
        }

        public string PLC_response_Step7_Filter(List<byte> output_data, Inclusive_report_model ReportModel)
        {
            string response = "S7";
            response += "Ne-Successful!";

            return response;
        }


        public string PLC_feedback_filter(List<byte> output_data)

        {
            string response = "";
            if (output_data.Count > 3)
            {

                int row = 0, col = 0;

                string FBE_error_status = "";
                byte[] _byte = new byte[8];
                if (DeviceServices.PLC_Modbus_Data_Verify(output_data))
                {
                    for (int i = 0; i < _byte.Length; i++)
                    {
                        _byte[i] = output_data[i + 3];
                        if (_byte[i] > 0)
                        {
                            row = i + 1;
                            string bits = Convert.ToString(_byte[i], 2).PadLeft(8, '0');
                            char[] bits_array = bits.ToCharArray();
                            Array.Reverse(bits_array);
                            for (int j = 0; j < bits.Length; j++)
                            {
                                col = 1 + j;
                                if (bits_array[j] != '0') FBE_error_status += "- R" + row + ", C" + col;
                            }
                        }
                    }

                    switch (FBE_error_status.Length)
                    {
                        case 0:
                            response += "Ne-Successful!";
                            break;
                        default:
                            response += "ErR-Continuous Motor Error: " + FBE_error_status + ". ";
                            break;
                    }
                }
                else
                {
                    response += "Re-No or Wrong Response.";
                }
            }
            else
            {
                response += "Re-No or Wrong Response.";
            }
            return response;
        }
        /*  private string response_register_write_PLC(List<byte> output_data)
          {
              string result_data = "";
              if (output_data[4] == 0 && output_data[5] == 0) result_data = "R1";
              else result_data = "E1";
              return result_data;
          }*/
        /*  private string response_Coil_status_PLC(List<byte> output_data)
          {
              string result_data = "";
              if (output_data[3] == 0)
                  result_data = "FBC";
              else
                  result_data = "W";
              return result_data;
          }*/
        //public string response_previous_Coil_status_PLC(List<byte> output_data)
        //{
        //    //timerStopped_process();
        //    string result_data = "";
        //    if (output_data[3] == 0)
        //        result_data = "R2";
        //    else
        //        result_data = "R1";
        //    return result_data;
        //}
        public string PLC_response_repeat_Coil_status(List<byte> output_data)
        {

            return "N1";
        }
        public string PLC_response_feedbackBit_reset_write(List<byte> output_data)
        {
            //timerStopped_process();
            string result_data = "";
            if (output_data[4] != 0x00)
                result_data = "E1";
            else result_data = "R2";
            return result_data;
        }


        #endregion

        #region CD REQUESTS

        public List<byte> send_CD_request(byte coin_qty)
        {
            DeviceServices.SetReadTimeOut(75);
            List<byte> hexstring = new List<byte>();
            hexstring.Add(0x05);
            hexstring.Add(0x10);
            hexstring.Add(0x00);
            hexstring.Add(0x14);
            hexstring.Add(coin_qty);
            byte checksum = new byte();
            foreach (byte item in hexstring) checksum += item;
            hexstring.Add(checksum);
            return hexstring;


        }
        public List<byte> send_CD_Reset_request()
        {

            DeviceServices.SetReadTimeOut(40);
            List<byte> hexstring = new List<byte>();
            hexstring.Add(0x05);
            hexstring.Add(0x10);
            hexstring.Add(0x00);
            hexstring.Add(0x12);
            hexstring.Add(0x00);
            byte checksum = new byte();
            foreach (byte item in hexstring) checksum += item;
            hexstring.Add(checksum);


            return hexstring;
        }

        public List<byte> send_CD_status_request()
        {

            List<byte> hexstring = new List<byte>();
            hexstring.Add(0x05);
            hexstring.Add(0x10);
            hexstring.Add(0x00);
            hexstring.Add(0x11);
            hexstring.Add(0x00);
            byte checksum = new byte();
            foreach (byte item in hexstring) checksum += item;
            hexstring.Add(checksum);


            return hexstring;
        }
        #endregion

        #region CD Response

        public string CD_response_Step2_filter(List<byte> output_data, Inclusive_report_model ReportModel)
        {


            string response = "S4";
            if (output_data.Count > 5)
            {


                byte last = output_data[output_data.Count - 1];
                byte last1 = output_data[output_data.Count - 2];
                byte last2 = output_data[output_data.Count - 3];
                byte last3 = output_data[output_data.Count - 4];
                byte last4 = output_data[output_data.Count - 5];
                byte last5 = output_data[output_data.Count - 6];
                if (last == 0x0E & last1 == 0x00 & last2 == 0x08 & last3 == 0x00 & last4 == 0x01 & last5 == 0x05)
                {
                    response += "Ne-Successful!";
                    DeviceServices.SetReadTimeOut(1);
                }
                else if (last == 0x0D & last1 == 0x00 & last2 == 0x07 & last3 == 0x00 & last4 == 0x01 & last5 == 0x05)
                {
                    response += "ErR-No Coins.";
                    DeviceServices.SetReadTimeOut(1);

                }
                else if (last2 == 0xAA & last3 == 0x00 & last4 == 0x01 & last5 == 0x05)
                {
                    response += "ErR-Dispensing...";
                    DeviceServices.SetReadTimeOut(1);

                }
            }
            else
            {
                response += "ErR-Dispensing...";
            }


            return response;
        }

        public string CD_response_Step3_filter(List<byte> output_data, Inclusive_report_model ReportModel)
        {
            string response = "S3";
            DeviceServices.SetReadTimeOut(24);
            if (DeviceServices.CD_Checksum_Data_Verify(output_data))
            {

                switch (output_data[4])
                {

                    case 0xC0:
                        response += "Ne-Successful!";
                        break;
                    case 0x01:
                        response += "ErR-Motor Problem";
                        break;
                    case 0x02:
                        response += "ErR-Hopper low level detected";
                        break;
                    case 0x04:
                        response += "Ne-Successful!";
                        break;
                    case 0x08:
                        response += "ErR-Prism sensor failure";
                        break;
                    case 0x10:
                        response += "ErR-Shaft sensor failure";
                        break;
                    case 0x20:
                        response += "ErR-Hopper busy";
                        break;
                    case 0x40:
                        response += "Ne-Successful!";
                        break;
                    case 0x80:
                        response += "Ne-Successful!";
                        break;
                    default:
                        response += "Re-No or Wrong Response.";
                        break;
                }

            }
            else
            {
                response += "Re-No or Wrong Response.";
            }


            return response;

        }
        public string CD_response_Step4_filter(List<byte> output_data, Inclusive_report_model ReportModel)
        {
            string response = "S2";


            if (output_data.Count > 0)
            {


                response += "Ne-Successfull!";

            }
            else
            {
                response += "Re-No or Wrong Response.";

            }

            return response;

        }
        #endregion
    }
}

