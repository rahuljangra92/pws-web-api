using System;

namespace PWSWebApi.Domains
{
    public class SiteMonitor
    {
        public DateTime? LoadTimeStamp { get; set; }
        public int UserID { get; set; }
        public string Misc { get; set; }
        public string HREF { get; set; }
        public string Scope { get; set; }
        public string Site { get; set; }
        public string Session { get; set; }
        public bool AutomatedPing { get; set; }
    }
}
