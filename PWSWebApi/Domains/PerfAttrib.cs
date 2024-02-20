using System;

namespace PWSWebApi.Domains
{
    public class PerfAttrib
    {
        public long? Ranking { get; set; }
        public int PerfAttribID { get; set; }
        public int? PWSRefID { get; set; }
        public int? CCYID { get; set; }
        public int? PerfAttribIDUp { get; set; }
        public bool? DrillDown { get; set; }
        public int? AttribTypeID { get; set; }
        public int? AttribNameID { get; set; }
        public int? HierAttribID { get; set; }
        public int? AssgnStartPt { get; set; }
        public int? AssgnEndPt { get; set; }
        public int? DPID { get; set; }
        public int? LoadSourceID { get; set; }
        public DateTime? LatestContBegDate { get; set; }
        public DateTime? LatestContEndDate { get; set; }
        public int? UserID { get; set; }
        public DateTime? LoadDateTime { get; set; }
        public string Stat { get; set; }
        public DateTime? RetiredDateTime { get; set; }
        public string PerfAttribString { get; set; }
    }
}
