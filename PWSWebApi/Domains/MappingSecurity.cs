using System.ComponentModel;
using System;

namespace PWSWebApi.Domains
{
    public class MappingSecurity
    {
        public int SecurityMappingID { get; set; }
        public int? DPID { get; set; }
        public int? LoadSourceID { get; set; }
        public int? SecID { get; set; }
        public string DPSecurityID { get; set; }
        public string DPSecurityName { get; set; }
        public string MappedUsing { get; set; }
        public int? AcctID { get; set; }
        public bool? BlockRecon { get; set; }
        public int? AdjustShares { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? LoadDateTime { get; set; }

        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
