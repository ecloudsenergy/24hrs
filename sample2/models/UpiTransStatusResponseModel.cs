using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sample2.models
{
    class UpiTransStatusResponseModel
    {
        public int status { get; set; }
        public string message { get; set; }
        public TransStatusResponse result { get; set; }
        

    }

    public class TransStatusResponse
    {
        public string terminalId { get; set; }
        public string payerVPAId { get; set; }
        public string amount { get; set; }
        public string rrn { get; set; }
        public string status { get; set; }
        public string dateTime { get; set; }
        public string approvalNo { get; set; }
        public string merchantId { get; set; }
        public string remarks { get; set; }
        public string payeeVPAId { get; set; }
        public string txnId { get; set; }
        public string uniqueId { get; set; }
       
    }
}
