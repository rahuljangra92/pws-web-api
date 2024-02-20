using System;

namespace PWSWebApi.Domains
{
    public class User
    {
        public int UserID { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public int PWSClientID { get; set; }
        public string UserName { get; internal set; }
        public DateTime? RetiredDateTime { get; internal set; }
        public string UserPassword { get; internal set; }
        public int BrowserTimeoutThreshold { get; internal set; }
    }
}
