using sample2.models;
using sample2.remote;
using sample2.User_Controls;
using sample2.windows;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;


namespace sample2
{
    /// <summary>
    /// Interaction logic for _18_Vending_Screen.xaml
    /// </summary>
    public partial class _18_Vending_Screen : Page
    {
        Page previous_page;

        SerialPort PLC = new SerialPort();
        private delegate string Output_Result(List<byte> output_data);
      
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        Output_Result result_PLC, result_ND, result_CD, PLC_response_temp;
        List<byte> response = new List<byte>(), PLC_command_temp;
        int product_quantities = 0, delivery_command_count = 0;
        List<cart_item> cart_items;
        int cell_number = 0, /*TODO: Initialize to zero*/ item_count = 0, rounds = 0, product_quantities_remaining = 0, PLC_pause_limit = 0;
        int[] total_separated;
        SerialPort ND = new SerialPort();
        SerialPort CD = new SerialPort();
        byte coin_qty = 0x00;
        //bool coin_hopper_check = false;
        List<byte> coin_byte_list = new List<byte>(), plc_byte_list = new List<byte>();
        string class_name = ""; string bill_number = "";
        DispatcherTimer dt = new DispatcherTimer();
        List<product_count_ledger> product_ledger = new List<product_count_ledger>();
        CellModel tray_model = new CellModel();
        string FBE_error_status = "";
        bool PLC_repeat = false, delivery_command = false;




        public _18_Vending_Screen(Page previous_page, List<cart_item> cart_items, string order_amount,
            string order_quantity, string received_amount, string class_name, string bill_number)
        {
            InitializeComponent();
            //Intializing all the devices - PLC, Note Dispenser and Coin Dispenser
            Config_PLC();
            Config_ND();
            Config_CD();
            product_vending_status.FontSize = 24;
            //Intialising the bill number, previous page, class name, order amount, order quantity, amount paid and cart items from previous page
            this.bill_number = bill_number;
            this.previous_page = previous_page;
            this.order_amount.Text = order_amount;
            this.order_quantity.Text = order_quantity;
            this.amount_paid.Text = received_amount;
            this.cart_items = cart_items;
            this.class_name = class_name;
            //Converting the amount paid and bill_amount from text to int
            int amt_paid = Int32.Parse(split_amount(this.amount_paid.Text));
            int bill_amount = Int32.Parse(split_amount(this.order_amount.Text));

            //finding total value need to be returned
            int total = amt_paid - bill_amount;
            //Splitting the total value to be returned in coins and cash
            total_separated = splited_amount(total);

            //converting coin quantity to be return into byte
            coin_qty = (byte)total_separated[2];

            //Intialising the cart items in the diplay screen
            foreach (cart_item item in cart_items)
            {
                cart_item addItem = new cart_item(item);
                this.cart.Children.Add(addItem);
            }

            //Classifying the storing transaction details process depending on whether it is a cash or coin or both
            if (class_name == "cash")
            {
                //Initialising the date to be stored
                String formatdate = indianTime.ToString("yyyy-MM-dd HH:mm:ss"); //imthi 20-01-2021

                //Storing the Cash transaction details in the database
                if (total_separated[1] > 0)
                {
                    int saved_successfully = 0;
                    CurrencyTransactionModel lastTransDetails = SqliteChange.getLastCurrencyTransaction(10, "ND");
                    saved_successfully += SqliteChange.InsertIntoCurrencyTransactionTable(10, bill_number, total_separated[1],
                                      formatdate, "Customer", "Debit", lastTransDetails.Cr_Closing_Balance_Qty,	//imthi 20-01-2021
                                      lastTransDetails.Cr_Closing_Balance_Qty - total_separated[1], "ND", "Scheduled");
                }
                //Storing the Coin transaction details in the database
                if (total_separated[2] > 0)
                {
                    int saved_successfully = 0;
                    CurrencyTransactionModel lastTransDetails = SqliteChange.getLastCurrencyTransaction(1, "CD");
                    saved_successfully += SqliteChange.InsertIntoCurrencyTransactionTable(1, bill_number, total_separated[2],
                                      formatdate, "Customer", "Debit", lastTransDetails.Cr_Closing_Balance_Qty,		//imthi 20-01-2021
                                      lastTransDetails.Cr_Closing_Balance_Qty - total_separated[2], "CD", "Scheduled");
                }
            }

            //Starting the product delivery
            start_next_product_delivery();
        }

        void start_next_product_delivery()
        {

            //As this function is recursive, we count the items until we reach the total number of items present in the cart.
            if (item_count < cart_items.Count)
            {
                //If the quantity of each product to be delivered is zero or lesser than zero, we go through the below process to 
                //check and initialise next product
                if (product_quantities <= 0)
                {
                    //Intialising the next product
                    cart_item product = cart_items[item_count];
                    //Intialising the product image, product name and ordered quantity in the screen
                    byte[] product_image = SqliteDataAccess.getProductImage(product.Product_Name.Text);
                    this.product_image.Source = SqliteChange.byteArrayToImage(product_image);
                    this.product_name.Text = product.Product_Name.Text;
                    this.order_qty.Text = product.Product_quantity.Text;
                    //Initialising the product quantities by converting it from text to int
                    if (product_quantities_remaining == 0) product_quantities_remaining = Int32.Parse(this.order_qty.Text);

                    // Intialising the cell number - row and col
                    tray_model = SqliteDataAccess.getProductCellDetails(product.Product_Name.Text);
                    if (product_quantities_remaining <= tray_model.CT_Balance_Qty)
                    {
                        if (rounds == 0) this.dispensed_qty.Text = "0";
                        //Storing the cell transaction details in the database
                        SqliteChange.UpdateCellTable(tray_model.CT_Col_No, tray_model.CT_Row_No,
                            tray_model.CT_Balance_Qty - product_quantities_remaining, tray_model.CT_Balance_Qty - product_quantities_remaining);
                        product_quantities = product_quantities_remaining;
                        product_quantities_remaining = 0;
                        rounds = 0;
                    }
                    else
                    {
                        if (rounds == 0) this.dispensed_qty.Text = "0";
                        //Storing the cell transaction details in the database
                        product_quantities_remaining = product_quantities_remaining - tray_model.CT_Balance_Qty;
                        SqliteChange.UpdateCellTable(tray_model.CT_Col_No, tray_model.CT_Row_No, 0, 0);
                        product_quantities = tray_model.CT_Balance_Qty;
                        rounds++;
                    }

                    //Registering the product details such as name and quantity for billing purpose
                    product_count_ledger new_product = new product_count_ledger();
                    new_product.Pr_Name = product.Product_Name.Text;
                    new_product.Pr_Qty = product_quantities;
                    product_ledger.Add(new_product);
                }

                cell_number = row_col_convertion(tray_model.CT_Row_No, tray_model.CT_Col_No);
                //Sending the Feedback status request to know the status of all the motors.
                if (cell_number > 0 && cell_number <= 64)
                {
                    //We can directly put the required command to send, but it might not repond to it, thus to give a booster,we start
                    // with continous communication.
                    PLC_command_temp = send_feedbackBit_reset_Write_request();
                    PLC_response_temp = response_feedbackBit_reset_write;
                    PLC_CmdSend(send_PLC_Coil_Wait_Status_request(), response_repeat_Coil_status_PLC);
                }
                //only if the remaining quantities are dispensed, the program will vend next product
                if (product_quantities_remaining == 0) item_count++;

            }

            else if (item_count == cart_items.Count)
            {
                item_count++;
                PLC_command_temp = send_feedbackBit_reset_Write_request();
                PLC_response_temp = response_feedbackBit_reset_write;
                PLC_CmdSend(send_PLC_Coil_Wait_Status_request(), response_repeat_Coil_status_PLC);
            }

            else
            {
                if (class_name == "cash")
                {
                    if (total_separated[1] > 0)
                    {
                        this.product_vending_status.Text = "Dispensing Cash...";
                        Thread.Sleep(2000);
                        ND_CmdSend(send_ND_request(get_request("S")), response_ND_status);
                    }
                    else if (coin_qty > 0)
                    {
                        Thread.Sleep(2000);
                        CD_CmdSend(send_CD_request(0x12, 0x00), reset_response_CD);
                    }
                    else
                    {

                        Thread.Sleep(3000);
                        sendToNextScreen();
                    }
                }

                else if (class_name == "card" || class_name == "upi")
                {
                    Thread.Sleep(3000);

                    sendToNextScreen();
                }
                else this.product_vending_status.Text = "Error: class name not mentioned. class name: " + class_name;
            }
        }

        void sendToNextScreen()
        {
            if (ND.IsOpen)
                ND.Close();
            if (CD.IsOpen)
                CD.Close();
            if (PLC.IsOpen)
                PLC.Close();
            PLC_timerStopped_process();
            _19_Bill_Printing next_page = new _19_Bill_Printing();

            this.NavigationService.Navigate(next_page);
            next_page.Bill_Printing(product_ledger, bill_number);
            
        }

        //private void timerStopped_process()
        //{
        //    dt.Stop();
        //    dt = new DispatcherTimer();
        //    process_timeout_time = 0;
        //}

        //private void timerStarted_process()
        //{
        //    dt.Tick += delayCounter_process;
        //    dt.Interval = new TimeSpan(0, 0, 1);
        //    dt.Start();
        //}

        //private void delayCounter_process(object sender, EventArgs e)
        //{
        //   if(process_timeout_time == 10)
        //    {
        //        //timeOutError(process_name);
        //        process_timeout_time++;
        //    }
        //   else if (process_timeout_time < 10)
        //    {
        //        process_timeout_time++;
        //    }
        //}


        #region Coin Hopper


        //private void timerStopped_Coin()
        //{
        //    dt.Stop();
        //    dt = new DispatcherTimer();
        //}

        //private void timerStarted_Coin()
        //{
        //    dt.Tick += delayCounter_coin;
        //    dt.Interval = new TimeSpan(0, 0, 0,0,700);
        //    dt.Start();

        //}

        //private void delayCounter_coin(object sender, EventArgs e)
        //{
        //    if (coin_hopper_check)
        //        CD_Cmd();
        //    else         
        //        timerStopped_Coin();
        //}

        private void Config_CD()
        {
            //Sets up serial port
            CD.PortName = SqliteDataAccess.getPort("CD");
            CD.BaudRate = 9600; // baud rate - 9600 for note dispenser, bill validator and coin hopper, 19200 for PLC
            CD.Handshake = Handshake.None;
            CD.Parity = Parity.Even;
            CD.DataBits = 8;
            CD.StopBits = StopBits.One;
            CD.ReadTimeout = 1000;
            CD.WriteTimeout = 50;
            CD.DataReceived += new SerialDataReceivedEventHandler(Recieve_CD);
        }

        #region CD_Recieving
        bool start_bit = false;
        private void Recieve_CD(object sender, SerialDataReceivedEventArgs e)
        {

            // Collecting the characters received to our 'buffer' (string).
            //if (coin_byte_list[3] == 0x12) coin_hopper_check = false;

            int bytes = CD.BytesToRead;
            byte[] output_data = new byte[bytes];
            CD.Read(output_data, 0, bytes);
            foreach (byte item in output_data)
            {
                if (item == 0x05) start_bit = true;
                if (start_bit)
                    response.Add(item);
            }

            byte checksum = new byte();
            if (response.Count >= 6)
            {
                for (int i = 0; i < 5; i++)
                {
                    checksum += response[i];
                }

                if (checksum == response[5])
                {
                    //coin_hopper_check = false;
                    response.RemoveRange(6, response.Count - 6);
                    Dispatcher.BeginInvoke(DispatcherPriority.Send, new UpdateUiTextDelegate(WriteData_CD), response);
                    response = new List<byte>();
                    start_bit = false;
                }
            }
            else if (result_CD == reset_response_CD)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Send, new UpdateUiTextDelegate(WriteData_CD), response);
                response = new List<byte>();
                start_bit = false;
            }
            else
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Send, new UpdateUiTextDelegate(WriteErrorData_CD), response);

            }
        }

        private void WriteData_CD(List<byte> output_data)
        {
            Display_CD_Status(result_CD(output_data));
        }

        private void WriteErrorData_CD(List<byte> output_data)
        {
            string respo = "";
            foreach (byte item in output_data)
            {
                respo += item + "-";
            }
            this.product_vending_status.Text = respo + "---";
        }

        #endregion

        #region CD_Sending
        private void CD_CmdSend(List<byte> hexstring, Output_Result result_method)
        {
            //if (!coin_hopper_check)
            //{
            try
            {
                // response = new List<byte>();
                result_CD = result_method;
                coin_byte_list = hexstring;
                if (!CD.IsOpen) CD.Open();
                // timerStarted_Coin();

                //string output_val = "";
                // CD.Write(hexstring.ToArray(), 0, hexstring.Count);
                foreach (byte hexval in hexstring)
                {
                    byte[] _hexval = new byte[] { hexval };
                    CD.Write(_hexval, 0, 1);
                    //output_val += hexval.ToString("X2") + " ";
                    Thread.Sleep(1);

                }
                //coin_hopper_check = true;
            }
            catch (Exception ex)
            {
                product_vending_status.Text = "CD Error - " + ex.Message + "!! Call the support: " + SqliteDataAccess.getHelplineNumber();
                //button_visible();
            }
            //}
        }

        private void CD_Cmd()
        {
            try
            {
                // response = new List<byte>();
                if (!CD.IsOpen) CD.Open();
                //string output_val = "";
                CD.Write(coin_byte_list.ToArray(), 0, coin_byte_list.Count);
                //foreach (byte hexval in coin_byte_list)
                //{
                //    byte[] _hexval = new byte[] { hexval };
                //    CD.Write(_hexval, 0, 1);
                //    //output_val += hexval.ToString("X2") + " ";
                //    Thread.Sleep(1);
                //}

            }
            catch (Exception ex)
            {
                product_vending_status.Text = "CD Error - " + ex.Message + "!! Call the support: " + SqliteDataAccess.getHelplineNumber();
                //button_visible();
            }
        }

        #endregion

        #region Coin Dispsenser Request methods
        List<byte> send_CD_request(byte command, byte data)
        {
            List<byte> hexstring = new List<byte>();
            hexstring.Add(0x05);
            hexstring.Add(0x10);
            hexstring.Add(0x00);
            hexstring.Add(command);
            hexstring.Add(data);
            byte checksum = new byte();
            foreach (byte item in hexstring) checksum += item;
            hexstring.Add(checksum);
            return hexstring;
        }
        #endregion

        #region Coin Dispsenser Response methods

        private string response_CD(List<byte> output_data)
        {
            string data = "";
            byte checksum = new byte();
            for (int i = 0; i < output_data.Count - 1; i++) checksum += output_data[i];
            if (output_data[output_data.Count - 1] == checksum)
            {
                byte command, data_byte;
                if (output_data.Count == 6)
                {
                    command = output_data[3];
                    data_byte = output_data[4];
                }
                else
                {
                    command = output_data[2];
                    data_byte = output_data[3];
                }


                switch (command)
                {
                    case 0x04:
                        data = display_error_CD(data_byte);
                        break;
                    case 0x07:
                        data = "SS";
                        break;
                    case 0x08:
                        data = "S";
                        break;
                    case 0xAA:
                        data = "ACK";
                        break;
                    case 0xBB:
                        data = "B";
                        break;
                    default:
                        data = "a: " + command;
                        break;
                }
            }
            else
            {
                foreach (byte item in output_data)
                    data += item;
            }

            return data;
        }

        private string reset_response_CD(List<byte> output_data)
        {

            return "RESET";
        }

        #endregion


        private string display_error_CD(byte data_byte)
        {
            string return_data = "";
            switch (data_byte)
            {
                case 0x80:
                    return_data = "NP";
                    break;

                case 0x40:
                    return_data = "NP";
                    break;

                default:
                    return_data = "NP";
                    break;
            }

            return return_data;
        }

        private void Display_CD_Status(string display_data)
        {
            switch (display_data)
            {
                case "NP":
                    this.product_vending_status.Text = "Dispensing Coin...";
                    CD_CmdSend(send_CD_request(0x10, coin_qty), response_CD);
                    break;
                case "RESET":
                    this.product_vending_status.Text = "Dispensing Coin...";
                    Thread.Sleep(2000);
                    CD_CmdSend(send_CD_request(0x11, 0x00), response_CD);
                    break;
                case "B":
                    this.product_vending_status.Text = "Dispensing Coin...";
                    Thread.Sleep(1000);
                    CD_CmdSend(send_CD_request(0x10, coin_qty), response_CD);
                    break;

                case "ACK":
                    this.product_vending_status.Text = "Coins Dispensed Successfully...";
                    coin_qty = 0;
                    start_next_product_delivery();
                    break;
                default:
                    this.product_vending_status.Text = "Unknown Response: " + display_data + ". Contact Support: " + SqliteDataAccess.getHelplineNumber();
                    break;
            }
        }

        #endregion

        #region Note Dispenser

        private int[] splited_amount(int total_amount)
        {
            int[] separated = new int[3];
            separated[0] = total_amount / 100;
            total_amount -= (separated[0] * 100);
            separated[1] = total_amount / 10;
            total_amount -= (separated[1] * 10);
            separated[2] = total_amount / 1;
            return separated;
        }

        private string split_amount(string amount)
        {
            string[] split_amount = amount.Split(' ');
            return split_amount[1];
        }

        private void Config_ND()
        {
            
            //Sets up serial port
            ND.PortName = SqliteDataAccess.getPort("ND");
            ND.BaudRate = 9600; // baud rate - 9600 for both note dispenser, bill validator and coin hopper, 19200 for PLC
            ND.Handshake = Handshake.None;
            ND.Parity = Parity.None; // for Note Dispenser and PLC
            ND.DataBits = 8;
            ND.StopBits = StopBits.One;
            ND.ReadTimeout = 1000;
            ND.WriteTimeout = 50;
            ND.DataReceived += new SerialDataReceivedEventHandler(Recieve_ND);
        }

        #region ND_Recieving

        private void Recieve_ND(object sender, SerialDataReceivedEventArgs e)
        {
            // Collecting the characters received to our 'buffer' (string).
            int bytes = ND.BytesToRead;
            byte[] output_data = new byte[bytes];
            ND.Read(output_data, 0, bytes);
            foreach (byte item in output_data)
            {
                response.Add(item);
            }
            if (response.Count == 10)
            {
                byte checksum = new byte();
                for (int i = 0; i < response.Count - 2; i++)
                {
                    checksum += response[i];
                }

                if (checksum == response[response.Count - 2])
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Send, new UpdateUiTextDelegate(WriteData_ND), response);
                    response = new List<byte>();
                }
            }
            else if (response.Count == 1)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Send, new UpdateUiTextDelegate(WriteData_ND), response);
                response = new List<byte>();
            }

        }


        private void WriteData_ND(List<byte> output_data)
        {
            Display_ND_Status(result_ND(output_data));
        }
        #endregion

        #region Note Dispsenser Request methods
        List<byte> send_ND_request(byte[] data_content)
        {
            List<byte> hexstring = new List<byte>();
            hexstring.Add(0x02);
            for (int i = 1; i <= data_content.Length; i++) hexstring.Add(data_content[i - 1]);
            byte checksum = new byte();
            foreach (byte item in hexstring) checksum += item;
            hexstring.Add(checksum);
            hexstring.Add(0x03);

            return hexstring;
        }
        #endregion

        #region Note Dispsenser Response methods

        private string response_ND_Dispense_status(List<byte> output_data)
        {
            string data = "";
            if (output_data.Count == 1)
            {
                if (output_data[0] == 0x06)
                    data = "B1";
                else data = "EB1 " + output_data[0];
            }
            else
            {
                if (output_data[3] == 0x62)
                    data = "B1";
                else if (output_data[3] == 0x45)
                {
                    data = "e";
                    error_list(output_data[4], data);
                }
                else data = "EB2";
            }
            return data;
        }
        private string response_ND_status(List<byte> output_data)
        {
            string result_data = "";
            if (output_data.Count == 10)
                if (output_data[3] == 0x73)
                {
                    switch (output_data[4])
                    {
                        case 0x77:
                            result_data = "w";
                            break;
                        case 0x72:
                            result_data = "r";
                            break;
                        case 0x65:
                            result_data = "e";
                            break;
                        case 0x74:
                            result_data = "t";
                            break;
                    }
                    if (result_data == "e")
                        error_list(output_data[5], result_data);
                }
                else
                {
                    foreach (var byte_data in output_data)
                    {
                        result_data += byte_data;
                    }
                }
            return result_data;
        }

        #endregion

        #region ND_Sending
        private void ND_CmdSend(List<byte> hexstring, Output_Result result_method)
        {

            try
            {
                result_ND = result_method;
                if (!ND.IsOpen) ND.Open();

                //string output_val = "";
                //ND.Write(hexstring.ToArray(), 0, hexstring.Count);
                foreach (byte hexval in hexstring)
                {
                    byte[] _hexval = new byte[] { hexval };
                    ND.Write(_hexval, 0, 1);
                    //output_val += hexval.ToString("X2") + " ";
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                product_vending_status.Text = "ND Error!!" + ex.Message + " Call the support: " + SqliteDataAccess.getHelplineNumber();
                // button_visible();
            }

        }
        #endregion


        private byte[] get_request(string request_code, int Hundrenth = 0, int Tenth = 0, int oneth = 0)
        {
            byte[] data_content;
            switch (request_code)
            {
                case "B":
                    data_content = Encoding.ASCII.GetBytes("00" + request_code + "00" + Hundrenth + Tenth);
                    break;

                default:
                    data_content = Encoding.ASCII.GetBytes("00S0000");
                    break;
            }
            return data_content;
        }

        private void Display_ND_Status(string display_data)
        {

            if (display_data.Contains("e"))
            {
                string[] separated = display_data.Split('-');
                display_data = separated[1];
                this.product_vending_status.Text = error_display(display_data);
                //button_visible();
            }
            else
                switch (display_data)
                {
                    case "B1":
                        this.product_vending_status.Text = "Cash Dispensed Successfully!!";

                        start_next_product_delivery();
                        break;

                    case "EB1":
                        this.product_vending_status.Text = "EB1 Unknown Error. Contact support: " + SqliteDataAccess.getHelplineNumber();
                        //button_visible();
                        break;
                    case "EB2":
                        this.product_vending_status.Text += "EB2 Unknown Error. Contact support: " + SqliteDataAccess.getHelplineNumber();
                        //button_visible();
                        break;
                    case "r":
                        ND_CmdSend(send_ND_request(get_request("B", 0, total_separated[1], total_separated[2])), response_ND_Dispense_status);
                        total_separated[1] = 0; total_separated[2] = 0;
                        break;
                    case "w":
                        Thread.Sleep(1000);
                        ND_CmdSend(send_ND_request(get_request("S")), response_ND_status);
                        break;
                    case "t":
                        this.product_vending_status.Text = "Test Mode. Contact support: " + SqliteDataAccess.getHelplineNumber();
                        break;
                    default:
                        this.product_vending_status.Text = "Unknown Error" + display_data + ". Contact support: " + SqliteDataAccess.getHelplineNumber();
                        //button_visible();
                        break;
                }
        }

        private void error_list(byte error_code, string result_data)
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

        private string error_display(string result_data)
        {
            string display_string = "";
            string support = "Please Call the support number: " + SqliteDataAccess.getHelplineNumber();
            switch (result_data)
            {
                case "No bills dispensed":
                    display_string = "No bills dispensed. " + support;
                    break;
                case "Jam":
                    result_data += "Currency Jammed. " + support;
                    break;
                case "Chain":
                    result_data += "Chain Currency. " + support;
                    break;
                case "Half":
                    result_data += "Half Currency. " + support;
                    break;
                case "Short":
                    result_data += "Short Currency. " + support;
                    break;
                case "No bills dispensed by start button":
                    result_data += "No bills dispensed by start button. " + support;
                    break;
                case "Double":
                    result_data += "Double notes. " + support;
                    break;
                case "Over 4000 pcs":
                    result_data += "Over 4000 pcs. " + support;
                    break;
                case "Communication Error":
                    result_data += "Communication Error. " + support;
                    break;
                case "Encoder Error":
                    result_data += "Encoder Error. " + support;
                    break;
                case "IR LED L Error":
                    result_data += "IR LED L Error. " + support;
                    break;
                case "IR LED R Error":
                    result_data += "IR LED R Error. " + support;
                    break;
                case "IR Sensor L Error":
                    result_data += "IR Sensor L Error. " + support;
                    break;
                case "IR Sensor R Error":
                    result_data += "IR Sensor R Error. " + support;
                    break;
                case "IR Sensor Different Error":
                    result_data += "IR Sensor Different Error. " + support;
                    break;
                case "Bill Low Level Warning":
                    result_data += "Bill Low Level Warning. " + support;
                    break;
                case "Low Power Error":
                    result_data += "Low Power Error. " + support;
                    break;
                default:
                    result_data += "Unknown Error. " + support;
                    break;
            }
            return display_string;
        }



        #endregion

        #region PLC

        private void Config_PLC()
        {
            //Sets up serial port
            PLC.PortName = SqliteDataAccess.getPort("PLC");
            PLC.BaudRate = 19200; // baud rate - 9600 for both note dispenser, bill validator and coin hopper, 19200 for PLC
            PLC.Handshake = Handshake.None;
            PLC.Parity = Parity.None; // for Note Dispenser and PLC
            PLC.DataBits = 8;
            PLC.StopBits = StopBits.Two;
            PLC.ReadTimeout = 1000;
            PLC.WriteTimeout = 50;
            PLC.DataReceived += new SerialDataReceivedEventHandler(Recieve_PLC);
        }


        #region PLC_Timer
        private void PLC_timerStopped_process()
        {
            dt.Stop();
            dt = new DispatcherTimer();
            PLC_pause_limit = 0;
            PLC_repeat = false;
        }

        private void PLC_timerStarted_process()
        {
            dt.Interval = new TimeSpan(0, 0, 0, 0, 700);
            dt.Tick += PLC_delayCounter_process;
            dt.Start();
            PLC_repeat = true;
        }

        private void PLC_delayCounter_process(object sender, EventArgs e)
        {
            PLC_CmdSend(send_PLC_Coil_Wait_Status_request(), response_repeat_Coil_status_PLC);
            PLC_pause_limit++;
            if (PLC_pause_limit > 35)
            {
                PLC_timerStopped_process();
                throwErrorPopUp();
                //TODO: Show error and send to next screen with a bill printed cancelled.
            }
        }

        public void throwErrorPopUp()
        {
            //TODO: send to next screen function
            TimeoutError errorWindow = new TimeoutError();
            errorWindow.Retry.Visibility = Visibility.Collapsed;
            errorWindow.gotoPage = sendToNextScreen;
            errorWindow.ShowDialog();
        }

        #endregion

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
            if ((response.Count - 3) > 0)
                if (response[response.Count - 1] == CRC_Result[0] && response[response.Count - 2] == CRC_Result[1])
                {

                    if (delivery_command)
                    {
                        delivery_command_count = 0;
                        delivery_command = false;
                    }
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
                if (!PLC.IsOpen) PLC.Open();
                string output_val = "";
                plc_byte_list = hexstring;
                //PLC.Write(hexstring.ToArray(), 0, hexstring.Count);
                foreach (byte hexval in hexstring)
                {
                    byte[] _hexval = new byte[] { hexval };
                    PLC.Write(_hexval, 0, 1);
                    output_val += hexval.ToString("X2") + " ";
                    Thread.Sleep(1);
                }
                if (!PLC_repeat)
                    PLC_timerStarted_process();
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Send, new UpdateUiTextDelegate1(ErrorData), ex.Message);

                //button_visible();
            }

        }

        private void ErrorData(string output_data)
        {
            product_vending_status.Text = "Error!! Call the support: " + SqliteDataAccess.getHelplineNumber() + "Messeage: "
                + output_data;
        }

        #endregion  // need to add error status

        #region PLC Request Methods

        /*List<byte> send_PLC_Register_Status_request()
        {
            List<byte> hexstring = new List<byte>();
            hexstring.Add(0x01);
            hexstring.Add(0x03);
            hexstring.Add(0x00);
            hexstring.Add(0x02);
            hexstring.Add(0x00);
            hexstring.Add(0x01);
            List<byte> CRC_Result = ModRTU_CRC(hexstring);
            hexstring.Add(CRC_Result[1]); //CRC Big Endian
            hexstring.Add(CRC_Result[0]);//CRC Big Endian

            return hexstring;
        }
    */
        /* List<byte> send_PLC_Coil_Stop_Write_request(int coil_number)
        {
            previous_cell_number = coil_number;

            List<byte> hexstring = new List<byte>();
            if (coil_number > 0 && coil_number <= 64)
            {
                hexstring.Add(0x01);
                hexstring.Add(0x05);
                hexstring.Add(0x00);
                hexstring.Add((byte)(0x04 + coil_number));
                hexstring.Add(0x00);
                hexstring.Add(0x00);
                List<byte> CRC_Result = ModRTU_CRC(hexstring);
                hexstring.Add(CRC_Result[1]); //CRC Big Endian
                hexstring.Add(CRC_Result[0]);//CRC Big Endian
            }

                return hexstring;
        }*/

        List<byte> send_PLC_Coil_Wait_Status_request()
        {

            List<byte> hexstring = new List<byte>();

            hexstring.Add(0x01);
            hexstring.Add(0x02);
            hexstring.Add(0x00);
            hexstring.Add((byte)(0x00));
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

        List<byte> send_PLC_Feedback_Status_request()
        {
            //timerStarted_process();
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

        List<byte> send_feedbackBit_reset_Write_request()
        {
            //timerStarted_process();
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

        List<byte> send_PLC_light_Write_request()
        {
            //timerStarted_process();
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

        #endregion

        #region PLC Response Methods

        /*   private string response_register_status_PLC(List<byte> output_data)
           {
               string result_data = "";
               if (output_data[4] == 0) result_data = "R1";
               else result_data = "R2";
               return result_data;
           }*/

        private string response_feedback_status_PLC(List<byte> output_data)
        {
            //timerStopped_process();
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
            //timerStopped_process();
            string result_data = "";
            if (output_data[3] == 0)
                result_data = "R3";
            else
                result_data = "R1";
            return result_data;
        }

        private string response_Coil_Write_PLC(List<byte> output_data)
        {
            //timerStopped_process();
            string result_data = "";
            if (output_data[1] != 0x06)
                result_data = "E1";
            else result_data = "R1";
            return result_data;
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

        private string response_previous_Coil_status_PLC(List<byte> output_data)
        {
            //timerStopped_process();
            string result_data = "";
            if (output_data[3] == 0)
                result_data = "R2";
            else
                result_data = "R1";
            return result_data;
        }

        private string response_repeat_Coil_status_PLC(List<byte> output_data)
        {

            return "N1";
        }

        private string response_feedbackBit_reset_write(List<byte> output_data)
        {
            //timerStopped_process();
            string result_data = "";
            if (output_data[4] != 0x00)
                result_data = "E1";
            else result_data = "R2";
            return result_data;
        }

        #endregion

        int dispensed_item_quant = 0;
        private void Display_PLC_Status(string display_data)
        {
            switch (display_data)
            {
                case "Run":
                    if (PLC_repeat)
                        PLC_timerStopped_process();
                    if (product_quantities > 0)
                    {
                        dispensed_item_quant = int.Parse(this.dispensed_qty.Text);
                        this.dispensed_qty.Text = "" + dispensed_item_quant;
                        product_vending_status.Text = "Vending...";
                        delivery_command = true;
                        PLC_command_temp = send_PLC_Coil_Write_request();
                        PLC_response_temp = response_Coil_Write_PLC;
                        PLC_CmdSend(send_PLC_Coil_Wait_Status_request(), response_repeat_Coil_status_PLC);
                        dispensed_item_quant++;
                        product_quantities--;
                    }
                    else { start_next_product_delivery(); dispensed_item_quant = 0; }
                    break;

                case "R1":
                    if (PLC_repeat)
                        PLC_timerStopped_process();
                    Thread.Sleep(1000);
                    product_vending_status.Text = "Vending... Please Wait";
                    PLC_command_temp = send_PLC_Coil_Wait_Status_request();
                    PLC_response_temp = response_previous_Coil_status_PLC;
                    PLC_CmdSend(send_PLC_Coil_Wait_Status_request(), response_repeat_Coil_status_PLC);
                    break;

                case "R2":
                    if (PLC_repeat)
                        PLC_timerStopped_process();
                    product_vending_status.Text = "Initializing.";
                    PLC_command_temp = send_PLC_Feedback_Status_request();
                    PLC_response_temp = response_feedback_status_PLC;
                    PLC_CmdSend(send_PLC_Coil_Wait_Status_request(), response_repeat_Coil_status_PLC);
                    break;

                case "R3":
                    if (PLC_repeat)
                        PLC_timerStopped_process();
                    product_vending_status.Text = "Initializing..";
                    PLC_command_temp = send_feedbackBit_reset_Write_request();
                    PLC_response_temp = response_feedbackBit_reset_write;
                    PLC_CmdSend(send_PLC_Coil_Wait_Status_request(), response_repeat_Coil_status_PLC);
                    break;

                case "N1":
                    if (PLC_repeat)
                        PLC_timerStopped_process();
                    product_vending_status.Text = "Initializing...";
                    if (delivery_command_count < 2)
                        PLC_CmdSend(PLC_command_temp, PLC_response_temp);
                    else
                        throwErrorPopUp();

                    if (delivery_command)
                        delivery_command_count++;

                    break;

                case "FBE":
                    if (PLC_repeat)
                        PLC_timerStopped_process();
                    product_vending_status.FontSize = 12;
                    product_vending_status.Text = "Continous Motor Error! FBE problem motors:" + FBE_error_status + ". Contact the support team: "
                        + SqliteDataAccess.getHelplineNumber();

                    //button_visible();
                    //SqliteDataAccess.disableAllProducts();
                    break;

                case "E1":
                    if (PLC_repeat)
                        PLC_timerStopped_process();
                    product_vending_status.Text = "PLC command Error! Contact the support team: " + SqliteDataAccess.getHelplineNumber();
                    //button_visible();
                    break;

                default:

                    product_vending_status.Text = "Error -- " + display_data;
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

        void button_visible()
        {
            btn_category_back.Visibility = Visibility.Visible;
            btn_exit.Visibility = Visibility.Visible;
        }

        private void btn_exit_Click(object sender, TouchEventArgs e)
        {
            this.NavigationService.Navigate(new _1_Idle());
            ND.Close();
            PLC.Close();
        }

        private void btn_category_back_Click(object sender, TouchEventArgs e)
        {
            this.NavigationService.Navigate(new _2_Product_Selection());
            ND.Close();
            PLC.Close();
        }


    }
}
