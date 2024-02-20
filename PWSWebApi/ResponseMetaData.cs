using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PWSWebApi
{
    public class ResponseMetaData<T>
    {
        public int status { get; set; }
        public bool isError { get; set; }
        public object errorDetails { get; set; }
        public string message { get; set; }
        public T result { get; set; }
    }
    public class TokenMeta
    {
        public int id { get; set; }
        public string token { get; set; }
        public DateTime expires { get; set; }
        public bool isExpired { get; set; }
        public DateTime created { get; set; }
        public string createdByIp { get; set; }
        public object revoked { get; set; }
        public object revokedByIp { get; set; }
        public object replacedByToken { get; set; }
        public bool isActive { get; set; }
        public string accessCode { get; set; }

    }

    public class Claims
    {
        public string UserId { get; set; }
        public string Username { get; set; }
    }
}