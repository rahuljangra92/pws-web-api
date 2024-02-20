using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PWSWebApi.Models.Imports
{
    public class PositionObject
    {
        public string Account_Code { get; set; }
        public string Accrued_Interest_Base { get; set; }
        public string Accrued_Interest_Local { get; set; }
        public string Base { get; set; }
        public string Base_Currency { get; set; }
        public string Data_Source { get; set; }
        public string Date { get; set; }
        public string ExcelRowNumber { get; set; }
        public string FIle_Name { get; set; }
        public string Load_Source { get; set; }
        public string Local { get; set; }
        public string Local_Currency { get; set; }
        public string Original_Cost_Base { get; set; }
        public string Original_Cost_Local { get; set; }
        public string Position_ID { get; set; }
        public string Price { get; set; }
        public string Quantity { get; set; }
        public string Security_Code { get; set; }
        public string Security_Code_Type { get; set; }
        public string Security_Name { get; set; }
        public string Security_Type { get; set; }
        public string Trade_to_Base_FX_Rate { get; set; }
        public string User_ID { get; set; }

    }
}