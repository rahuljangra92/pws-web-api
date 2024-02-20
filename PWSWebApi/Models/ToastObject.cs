using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PWSWebApi.Models
{
    public  class ToastObject
    {
        [JsonProperty("message")]
        public string Message { get; set; } = "Validation Error";
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("duration")]
        public int Duration { get; set; } = 10;
        [JsonProperty("type")]
        public string Type { get; set; } = "error";
    }

}