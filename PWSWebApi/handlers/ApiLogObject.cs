using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PWSWebApi;

namespace PWSWebApi.handlers
{
    public class ApiLogObject
    {
        public int ID { get; set; }

        public string Method { get; set; }

        public string Exception { get; set; }

        public string Query { get; set; }

        public string UserID { get; set; }

        public DateTime UpdatedOn { get; set; }

        public string Params { get; set; }

        public ApiLogObject()
        {
            UpdatedOn = Util.GetEasternDateTime();
        }
    }
}