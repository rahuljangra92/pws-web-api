using System;

namespace PWSWebApi.Domains
{
    public class Perf_Static
    {
        public int PerfStaticID { get; set; }
        public string PerfAttribDescr { get; set; }
        public string PWSRefString { get; set; }
        public int PerfAttribID { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? BMV { get; set; }
        public decimal? EMV { get; set; }
        public decimal? BegFlow { get; set; }
        public decimal? NetFlow { get; set; }
        public decimal? MgtFee { get; set; }
        public decimal? GRet { get; set; }
        public decimal? NRet { get; set; }
        public bool? StaticData { get; set; }
        public int? DPID { get; set; }
        public int? UserID { get; set; }
        public DateTime? LoadDateTime { get; set; }
        public string Stat { get; set; }
        public DateTime? RetiredDateTime { get; set; }
        public int? LoadSourceID { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }
}
