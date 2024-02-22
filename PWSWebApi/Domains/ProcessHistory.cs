using System;

namespace PWSWebApi.Domains
{
    public class ProcessHistory
    {
        public int Process_ID { get; internal set; }
        public DateTime RunDateTime { get; internal set; }
        public DateTime UpdateDateTime { get; internal set; }
        public string Error { get; internal set; }
    }
}
