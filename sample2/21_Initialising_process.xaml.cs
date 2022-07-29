using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using sample2.remote;
using sample2.models;
using System.IO.Ports;
using System.Windows.Threading;
using System.Threading;
using System.Collections;

namespace sample2
{
    /// <summary>
    /// Interaction logic for _21_Initialising_process.xaml
    /// </summary>
    public partial class _21_Initialising_process : Page, INotifyPropertyChanged
    {
        private BackgroundWorker _bgworker = new BackgroundWorker();
        private int _workerState; int cellCount = 0, cell_number = 0, memory_displacement =0;
        List<CellModel> listOfEnabledCells = SqliteChange.getEnabledCellDetails();
        SerialPort PLC = new SerialPort();
        private delegate string Output_Result(List<byte> output_data);
        Output_Result result_PLC;
        List<byte> response = new List<byte>();
        List<byte> output_data = new List<byte>(), plc_byte_list = new List<byte>();
        Page previous_page;
        bool feedbackReset = false;

        public int WorkerState
        {
            get { return _workerState; }
            set
            {

                _workerState = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("WorkerState"));
            }
        }

        public string FBE_error_status;

        public _21_Initialising_process(Page previous_page)
        {
            this.previous_page = previous_page;
            InitializeComponent();
            Config_PLC();
            PLC.Open();
            progressBar.Maximum = listOfEnabledCells.Count;
            DataContext = this;
            
            _bgworker.DoWork += (s, e) => {

                PLC_CmdSend(send_feedbackBit_reset_Write_request(), response_feedbackBit_reset_write);



                // MessageBox.Show("Work is done =",WorkerState.ToString(),MessageBoxButton.OK);

            };
            _bgworker.RunWorkerAsync();
        }
        #region INotifyPropertyChanged member
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        private void send_Motor_run_request()
        {
            if (cellCount < listOfEnabledCells.Count)
            {
                cell_number = row_col_convertion(listOfEnabledCells[cellCount].CT_Row_No, listOfEnabledCells[cellCount].CT_Col_No);
                
                PLC_CmdSend(send_PLC_Coil_Write_request(), response_Coil_Write_PLC);
                cellCount++;
                WorkerState = cellCount;
            }
            else if(cellCount == listOfEnabledCells.Count)
            {
                cellCount++;
                PLC_CmdSend(send_feedbackBit_reset_Write_request(), response_feedbackBit_reset_write);
            }
            else
            {
                Motor_Error_Status.Text = "Successfully Initialized!";
                Thread.Sleep(2000);
                PLC.Close();
                
            }
            
        }

        #region PLC

        private void Config_PLC()
        {
            //Sets up serial port
            PLC.PortName = SqliteDataAccess.getPort("PLC");
            PLC.BaudRate = 19200; // baud rate - 9600 for both note dispenser, bill validator and coin hopper, 19200 for PLC
            PLC.Handshake = System.IO.Ports.Handshake.None;
            PLC.Parity = Parity.None; // for Note Dispenser and PLC
            PLC.DataBits = 8;
            PLC.StopBits = StopBits.Two;
            
            PLC.DataReceived += new SerialDataReceivedEventHandler(Recieve_PLC);
        }


        #region PLC_Recieving

        private delegate void UpdateUiTextDelegate(List<byte> output_data);
        private void Recieve_PLC(object sender, SerialDataReceivedEventArgs e)
        {
            // Collecting the characters received to our 'buffer' (string).
            int bytes = PLC.BytesToRead;
            byte[] output_data = new byte[bytes];
            PLC.Read(output_data, 0, bytes);
            foreach (byte item in output_data)
            {
                response.Add(item);
            }
            List<byte> hexstring = new List<byte>();
            for (int i = 0; i < response.Count - 2; i++)
            {
                hexstring.Add(response[i]);
            }
            List<byte> CRC_Result = ModRTU_CRC(hexstring);
            if((response.Count - 3) > 0)
            if (response[response.Count - 1] == CRC_Result[0] && response[response.Count - 2] == CRC_Result[1])
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Send, new UpdateUiTextDelegate(WriteData), response);
                response = new List<byte>();
            }
        }


        private void WriteData(List<byte> output_data)
        {
            Display_PLC_Status(result_PLC(output_data));
        }
        #endregion

        #region PLC_Sending
        private delegate void UpdateUiTextDelegate1(string output_data);
        private void PLC_CmdSend(List<byte> hexstring, Output_Result result_method)
        {
            try
            {
                result_PLC = result_method;
                string output_val = "";
                plc_byte_list = hexstring;
                PLC.Write(hexstring.ToArray(), 0, hexstring.Count);

                //foreach (byte hexval in hexstring)
                //{
                //    byte[] _hexval = new byte[] { hexval };
                //    PLC.Write(_hexval, 0, 1);
                //    output_val += hexval.ToString("X2") + " ";
                //    Thread.Sleep(1);
                //}
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Send, new UpdateUiTextDelegate1(ErrorData), ex.Message);
                
                //button_visible();
            }

        }

        private void ErrorData(string output_data)
        {
            Motor_Error_Status.Text = "Error:"+ output_data + "!! Call the support: " + SqliteDataAccess.getHelplineNumber();
        }

        #endregion  // need to add error status


        #region PLC Request Methods

        List<byte> send_PLC_Coil_Wait_Status_request()
        {
            List<byte> hexstring = new List<byte>();

            hexstring.Add(0x01);
            hexstring.Add(0x02);
            hexstring.Add(0x00);
            hexstring.Add((byte)(0x00 + memory_displacement));
            hexstring.Add(0x00);
            hexstring.Add(0x01);
            List<byte> CRC_Result = ModRTU_CRC(hexstring);
            hexstring.Add(CRC_Result[1]); //CRC Big Endian
            hexstring.Add(CRC_Result[0]);//CRC Big Endian
            return hexstring;
        }
        List<byte> send_reset_feedback_write_request()

        {
            List<byte> hexstring = new List<byte>();

            hexstring.Add(0x01);
            hexstring.Add(0x0F);
            hexstring.Add(0x00);
            hexstring.Add(0x05);
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
            List<byte> CRC_Result = ModRTU_CRC(hexstring);
            hexstring.Add(CRC_Result[1]); //CRC Big Endian
            hexstring.Add(CRC_Result[0]);//CRC Big Endian
            return hexstring;
        }
        List<byte> send_PLC_Coil_Write_request()
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
                List<byte> CRC_Result = ModRTU_CRC(hexstring);
                hexstring.Add(CRC_Result[1]); //CRC Big Endian
                hexstring.Add(CRC_Result[0]);//CRC Big Endian
            }

            return hexstring;
        }
        List<byte> send_feedbackBit_reset_Write_request()
        {
            List<byte> hexstring = new List<byte>();

            hexstring.Add(0x01);
            hexstring.Add(0x05);
            hexstring.Add(0x00);
            hexstring.Add(0x4C);
            hexstring.Add(0x00);
            hexstring.Add(0x00);
            List<byte> CRC_Result = ModRTU_CRC(hexstring);
            hexstring.Add(CRC_Result[1]);//CRC Big Endian
            hexstring.Add(CRC_Result[0]);//CRC Big Endian

            return hexstring;
        }


        List<byte> send_PLC_Feedback_Status_request()
        {
            List<byte> hexstring = new List<byte>();
            hexstring.Add(0x01);
            hexstring.Add(0x01);
            hexstring.Add(0x00);
            hexstring.Add(0x05);
            hexstring.Add(0x00);
            hexstring.Add(0x40);
            List<byte> CRC_Result = ModRTU_CRC(hexstring);
            hexstring.Add(CRC_Result[1]); //CRC Big Endian
            hexstring.Add(CRC_Result[0]);//CRC Big Endian

            return hexstring;
        }

        #endregion

        #region PLC Response Methods


        private string response_feedback_status_PLC(List<byte> output_data)
        {
            int row = 0, col = 0;
            string result_data = "";
            FBE_error_status = "";
            byte[] _byte = new byte[8];
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

            if (FBE_error_status.Length == 0) result_data = "Run";
            else result_data = "FBE";
            return result_data;
        }

        private string response_PLC_Coil_Wait_status_PLC(List<byte> output_data)
        {
            string result_data = "";
            if (output_data[3] == 0)
                result_data = "R2";
            else
                result_data = "R1";
            return result_data;
        }

        private string response_Coil_Write_PLC(List<byte> output_data)
        {
            string result_data = "";
            if (output_data[1] != 0x06)
                result_data = "E1";
            else result_data = "R1";
            return result_data;
        }

        private string response_feedbackBit_reset_write(List<byte> output_data)
        {
            string result_data = "";
            if (output_data[4] != 0x00)
                result_data = "E1";
            else result_data = "R2";
            return result_data;
        }

        private string response_reset_feedback(List<byte> output_data)
        {
            string result_data = "R2";
            feedbackReset = true;
            return result_data;
        }

        #endregion


      
        private void Display_PLC_Status(string display_data)
        {
            switch (display_data)
            {
                case "Run":
                    Motor_Error_Status.Text = "Run";
                    send_Motor_run_request();
                    break;

                case "R1":
                    Motor_Error_Status.Text = "R1";
                    Thread.Sleep(1000);
                   
                    PLC_CmdSend(send_PLC_Coil_Wait_Status_request(), response_PLC_Coil_Wait_status_PLC);
                    break;

                case "R2":
                    Motor_Error_Status.Text = "R2";
                    if (feedbackReset) PLC_CmdSend(send_PLC_Feedback_Status_request(), response_feedback_status_PLC);
                    else
                        PLC_CmdSend(send_reset_feedback_write_request(), response_reset_feedback);
                    break;

                case "R3":
                    Motor_Error_Status.Text = "R3";
                    PLC_CmdSend(send_feedbackBit_reset_Write_request(), response_feedbackBit_reset_write);
                    break;

                case "FBE":
                    Motor_Error_Status.Text = "Continous Motor Error! FBE problem motors:" + FBE_error_status + ". Contact the support team: "
                        + SqliteDataAccess.getHelplineNumber();
                    //button_visible();
                    //SqliteDataAccess.disableAllProducts();
                    break;

                case "E1":
                    Motor_Error_Status.Text = "PLC command Error! Contact the support team: " + SqliteDataAccess.getHelplineNumber();
                    //button_visible();
                    break;

                default:
                    Motor_Error_Status.Text = "Error -- " + display_data;
                    break;
            }


        }

        private static List<byte> ModRTU_CRC(List<byte> data)
        {
            List<byte> checksum = new List<byte>();
            ushort crc = 0xFFFF;

            for (int i = 0; i < data.Count; i++)
            {
                crc ^= data[i];          // XOR byte into least sig. byte of crc

                for (int j = 0; j < 8; j++)
                {    // Loop over each bit
                    if ((crc & 0x01) == 1)
                        crc = (ushort)((crc >> 1) ^ 0xA001);                    // Shift right and XOR 0xA001
                    else                            // Else LSB is not set
                        crc = (ushort)(crc >> 1);                    // Just shift right
                }
            }

            checksum.Add((byte)((crc >> 8) & 0xFF));
            checksum.Add((byte)(crc & 0xFF));
            return checksum;
        }

        private int row_col_convertion(int row, int col)
        {
            int coil = ((row - 1) * 8) + col;
            return coil;
        }


        #endregion

        private void Back(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(previous_page);
            PLC.Close();
        }

    }
}

