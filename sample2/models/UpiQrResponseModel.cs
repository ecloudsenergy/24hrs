using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sample2.models
{
    public class UpiQrResponseModel
    {
        public string message { get; set; }
        public QrResponse result { get; set; }
    }

    public class QrResponse
    {
        public string qrCodeURL { get; set; }
        public string merchantVpa { get; set; }
        public string description { get; set; }
        public string dynamicUniqueId { get; set; }
        public string transactionStatus { get; set; }
    }
}
