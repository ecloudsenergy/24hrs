using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace sample2.models
{
    public class BackgroundWorkerModel
    {
       
        public class Inclusive_report_model
        {
            public int Process_Seq { get; set; }
            public int Steps { get; set; }
            public string Description { get; set; }
            public int Cmd_sent_times { get; set; }
            public string Cmd_Hexstring_Data { get; set; }
            public int Response_received_times { get; set; }
            public string Response_received_status { get; set; }
            public string Response_Hexstring_Data { get; set; }
            public string Product_Name { get; set; }
            public string Cell_No { get; set; }
            public int Balance_Before_Delivery { get; set; }
            public string Response_TimeStamps { get; set; }
            public string Cmd_Sent_TimeStamps { get; set; }
            public int Cmd_Send_Maxtimes { get; set; }
            public string Device_Used { get; set; }
            public string Status { get; set; }
            public string Remarks { get; set; }
        }
    }
}
