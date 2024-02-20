using System;

namespace PWSWebApi.Domains
{
    public class Sec
    {
        public int UserID { get; set; }
        public DateTime LoadDateTime { get; internal set; }
    }
}
