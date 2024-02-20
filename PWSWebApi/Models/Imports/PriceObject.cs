using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PWSWebApi.Models.Imports
{
    public class PriceObject
    {
        public string Data_Source { get; set; }
        public string Date { get; set; }
        public string ExcelRowNumber { get; set; }
        public string FIle_Name { get; set; }
        public string Final_or_Estimate { get; set; }
        public string Load_Source { get; set; }
        public string Price { get; set; }
        public string Principal_Currency { get; set; }
        public string Security_Code { get; set; }
        public string User_ID { get; set; }

    }
}