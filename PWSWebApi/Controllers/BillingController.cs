using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PWSWebApi.Domains.DBContext;
using PWSWebApi.handlers;
using PWSWebApi.Models.Billing;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace PWSWebApi.Controllers
{
    [Route("billing")]
    [ApiController]
    public class BillingController : ControllerBase
    {       
        
        private readonly DataClasses1DataContext _dataClasses1DataContext;
        private readonly string connectionString;
        private readonly Helper handlerHelper;
        public BillingController(IConfiguration configuration, DataClasses1DataContext dataClasses1DataContext)
        {
            _dataClasses1DataContext = dataClasses1DataContext;
            connectionString = _dataClasses1DataContext.Database.GetConnectionString();
            handlerHelper = new Helper(configuration);
        }

        [HttpPost]
        [Route("searchfeesdetailsbyportfolio")]
        public IActionResult SearchFeesDetailsByPortfolio(ObjectBilling requestBody) //(int UserID, string FeeStructureAssignmentIDsBYDates)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, requestBody.UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, requestBody.UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", requestBody.UserID);
                pars.Add("@FeeStructureAssignmentIDs", requestBody.FeeStructureAssignmentIDsBYDates);
                var data = Helper.callProcedure("PWS.[adm].[usp_FeeDetailsByPortfolio_Search]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (System.Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = requestBody.UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("searchfeesbycategory")]
        public IActionResult SearchFeesByCategory(ObjectBilling requestBody) //(int UserID, DateTime StartDate, DateTime EndDate, string UserGroupIDs, string Category = "acct", string RptPortIDs = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, requestBody.UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, requestBody.UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", requestBody.UserID);
                pars.Add("@UserGroupIDs", requestBody.UserGroupIDs);
                pars.Add("@EffectivedateStarting", requestBody.StartDate);
                pars.Add("@EffectivedateEnding", requestBody.EndDate);
                if (requestBody.RptPortIDs != null && requestBody.RptPortIDs != "null")
                    pars.Add("@RptPortIDs", requestBody.RptPortIDs);
                var procToBeCalled = requestBody.Category.Equals("acct", StringComparison.OrdinalIgnoreCase) ? "pws.[adm].[usp_FeeByAcct_Search]" : "pws.[adm].usp_FeeByPortfolio_Search";
                var data = Helper.callProcedure(procToBeCalled, pars);
                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = requestBody.UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("movefeestoprod")]
        public IActionResult MoveFeesToProd([FromBody] ObjectBilling requestBody)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, requestBody.UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, requestBody.UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", requestBody.UserID);
                pars.Add("@FeeStructureAssignmentIDs", requestBody.FeeStructureAssignmentIDs);
                var data = Helper.callProcedure("pws_rec.[feeproc].[usp_Fee_ToProduction]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = requestBody.UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("createmanagementfees")]
        public IActionResult CreateManagementFees([FromBody] ObjectBilling requestBody) //(int UserID, DateTime EndDate, string UserGroupIDs, string RptPortIDs = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, requestBody.UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, requestBody.UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", requestBody.UserID);
                pars.Add("@UserGroupIDs", requestBody.UserGroupIDs);
                pars.Add("@EndDate", requestBody.EndDate);
                if (requestBody.RptPortIDs == null)
                    requestBody.RptPortIDs = "";
                pars.Add("@RptPortIDs", requestBody.RptPortIDs);
                var data = Helper.callProcedure("pws_rec.[feeproc].[usp_Create_MgtFees]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = requestBody.UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }
    }
}