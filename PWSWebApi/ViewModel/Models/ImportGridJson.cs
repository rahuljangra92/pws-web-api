using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PWSWebApi.ViewModel.Models
{
    public class ImportGridJson
    {
       [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("gridColumnTitle")]
        public string Column { get; set; }

        [JsonProperty("required")]
        public string Required { get; set; }

        [JsonProperty("rowIndex")]
        public string RowIndex { get; set; }

    }
}