using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sample2.models
{
    class UpiTransStatusRequestModel
    {
        public string accessToken { get; set; }
        public string merchantId { get; set; }
        public string terminalId { get; set; }
        public string amount { get; set; }
        public string requestKey { get; set; }
        public string qrRequestType { get; set; }
        public string vpaId { get; set; }
        public string uniqueId { get; set; }
    }
}
