using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sample2.models
{


    public class MachineModel
    {
        public string MM_Machine_No { get; set; }
        public string MM_Open_file { get; set; }
        public string MM_Location { get; set; }
        public string MM_Loc_description { get; set; }
        public byte[] MM_Location_Logo { get; set; }
        public string MM_HelpNo { get; set; }
        public string MM_Company { get; set; }
        public string MM_Company_Address { get; set; }
        public string MM_Phone { get; set; }
        public string MM_GST { get; set; }
        public string MM_Mode { get; set; }
    }


    public class CellModel
    {
        public int CT_Row_No { get; set; }
        public int CT_Col_No { get; set; }
        public int CT_Enable_Tag { get; set; }
        public int CT_Balance_Qty { get; set; }
        public int CT_Max_Qty { get; set; }
        public string CT_Product_name { get; set; }
        public int CT_Registered_Balance_Qty { get; set; }

    }

    
    
}
