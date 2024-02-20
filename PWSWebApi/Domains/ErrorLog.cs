using System;

namespace PWSWebApi.Domains
{
    public class ErrorLog
    {
        public string errorJSON { get; internal set; }
        public string inputJSON { get; internal set; }
        public DateTime Load_DateTime { get; internal set; }
    }
}
