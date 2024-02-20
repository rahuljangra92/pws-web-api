using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PWSWebApi.ViewModel.Models.Operations
{
    public class LookUps
    {
        [JsonProperty("lookupDepsProps")]
        public List<string> LookupDepsProps { get; set; }
        [JsonProperty("lookupDeptsValues")]
        public List<string> LookupDeptsValues { get; set; }
    }
}