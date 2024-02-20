using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Data;
using PWSWebApi.handlers;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Azure.Core;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using Microsoft.Extensions.Configuration;
using PWSWebApi.Domains.DBContext;

namespace PWSWebApi.Controllers
{
    [ApiController]
    public class ValuesController : ControllerBase
    {        
        private readonly DataClasses1DataContext _dataClasses1DataContext;
        private readonly Helper handlerHelper;

        public ValuesController(IConfiguration configuration, DataClasses1DataContext dataClasses1DataContext)
        {
            _dataClasses1DataContext = dataClasses1DataContext;
            handlerHelper = new Helper(configuration);
        }

        // GET api/values
        public IActionResult Get()
        {
            if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
            {
                return (IActionResult)handlerHelper.ValidateSource(Request);
            }

            if (handlerHelper.ValidateToken(Request) != string.Empty)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
            }

            try
            {
               // var context = new DataClasses1DataContext(connectionString);
                var data = _dataClasses1DataContext.usp_AccountID(95, "").AsQueryable<dynamic>();
                return Ok(data);
            }
            catch (Exception ex)
            {
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }

        public class Data
        {
            public string AcctId { get; set; }

            public string AcctName { get; set; }

            public string BaseCashId { get; set; }

            public string RoleEntity { get; set; }

            public string AcctingCloseDate { get; set; }
        }
    }
}