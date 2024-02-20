using System;

namespace PWSWebApi.Domains
{
    public class ApiLog
    {
        public string Exception { get; internal set; }
        public string Method { get; internal set; }
        public string Query { get; internal set; }
        public string UserID { get; internal set; }
        public string Params { get; internal set; }
        public DateTime UpdatedOn { get; internal set; }
    }
}
