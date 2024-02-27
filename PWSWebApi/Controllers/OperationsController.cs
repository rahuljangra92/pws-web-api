using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PWSWebApi.Controllers;
using PWSWebApi.Domains;
using PWSWebApi.Domains.DBContext;
using PWSWebApi.handlers;
using PWSWebApi.Models.Imports;
using PWSWebApi.ViewModel;
using PWSWebApi.ViewModel.Helper;
using PWSWebApi.ViewModel.Models;
using PWSWebApi.ViewModel.Models.Operations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Http;
using FromBodyAttribute = Microsoft.AspNetCore.Mvc.FromBodyAttribute;
using HttpGetAttribute = Microsoft.AspNetCore.Mvc.HttpGetAttribute;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace PWSWebApi.Controllers
{

    public class ResponseError
    {
        public string UIId { get; set; }
        public string Column { get; set; }
        public string ErrorMessage { get; set; }

        public string ErrorCodeID { get; set; }

        public string New_AccountsID { get; set; }

        public string New_Accounts_tmpID { get; set; }


    }


    public class ComparePerfStats
    {
        public List<Perf_Static> Perms { get; set; }
        public List<Perf_Static_tmp> Temps { get; set; }
    }

    public class Monitoring
    {
        public string UserID { get; set; }
        public string Scope { get; set; }

        public string Misc { get; set; }

        public bool AutomatedPing { get; set; }

        public string Route { get; set; }

        public string ClientInfo { get; set; }

        public string Site { get; set; }

        public DateTime LoadTimeStamp { get; set; }

        public double TimeSpent { get; set; }

        public string Session { get; set; }
        public string PingToken { get; set; }

    }

    public class SampleTran
    {
        public int UserID { get; set; }
    }



    [Route("ops")]
    public class OperationsController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly DataClasses1DataContext dataClasses1DataContext;
        private readonly PWSRecDataContext pWSRecDataContext;
        private readonly string connectionString;
        //private readonly string pwsRectConnectionString;
        private HelperOperations helperOps;
        private Helper handlerHelper;
        public OperationsController(IConfiguration configuration, DataClasses1DataContext dataClasses1DataContext, PWSRecDataContext pWSRecDataContext)
        {
            this.configuration = configuration;
            this.dataClasses1DataContext = dataClasses1DataContext;
            this.pWSRecDataContext = pWSRecDataContext;
            this.connectionString = configuration.GetConnectionString("PWSProd");
            //this.pwsRectConnectionString= configuration.GetConnectionString("PWSRec");
            this.helperOps = new HelperOperations(configuration);
            handlerHelper = new Helper(configuration);
        }

        [HttpPost]
        [Route("SendTransactionInfo")]
        public IActionResult SendTransactionInfo(TransactionImportObject importObject)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult sourceValidationResult)
            {
                return sourceValidationResult;
            }

            if (handlerHelper.ValidateToken(Request, importObject.UserID.ToString()) != string.Empty)
            {
                return Unauthorized(handlerHelper.ValidateToken(Request, importObject.UserID.ToString()));
            }


            try
            {
                importObject.Errors = new List<ErrorExcelObject>
            {
                new ErrorExcelObject { SheetName = "Portfolios", WorkbookName = "test.xlsx", RowNumber = 2, Error = "Must add dfksdf" }
            };

                return Ok(new { data = importObject });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = importObject.UserID.ToString()
                });

                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception });
            }
        }
        [Route("monitorinfo")]
        public IActionResult GetMonitorInfo([FromQuery] string fromDate = null, [FromQuery] string toDate = null)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult sourceValidationResult)
            {
                return sourceValidationResult;
            }

            var tokenValidationResult = handlerHelper.ValidateToken(Request);
            if (!string.IsNullOrEmpty(tokenValidationResult))
            {
                return Unauthorized(tokenValidationResult);
            }

            try
            {
                //var _context = new DataClasses1DataContext(connectionString);
                List<SiteMonitor> query = new List<SiteMonitor>();
                List<Monitoring> monitoringList = new List<Monitoring>();

                if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
                {
                    var fDate = Convert.ToDateTime(fromDate);
                    var tDate = Convert.ToDateTime(toDate);

                    query = dataClasses1DataContext.SiteMonitors.Where(f => f.LoadTimeStamp.Value.Date >= fDate.Date &&
                                                             f.LoadTimeStamp.Value.Date <= tDate.Date)
                                                 .ToList();
                }
                else
                {
                    query = dataClasses1DataContext.SiteMonitors.Where(f => f.LoadTimeStamp.Value >= DateTime.Now.AddMonths(-4)).ToList();
                }

                query = query.OrderBy(f => f.UserID).ThenBy(f => f.LoadTimeStamp).ToList();

                var pwsClients = dataClasses1DataContext.PWSClients.ToList();
                var users = dataClasses1DataContext.Users.ToList();

                for (int i = 0; i < query.Count; i++)
                {
                    var d = query[i];
                    var newRow = new Monitoring();

                    newRow.LoadTimeStamp = Convert.ToDateTime(d.LoadTimeStamp);
                    newRow.Misc = d.Misc;
                    newRow.Route = d.HREF;
                    newRow.Scope = d.Scope;
                    newRow.Site = d.Site;
                    newRow.UserID = Convert.ToString(d.UserID);
                    newRow.Session = Convert.ToString(d.Session);
                    newRow.AutomatedPing = Convert.ToBoolean(d.AutomatedPing);
                    newRow.Session = d.Session;

                    var userInfo = users.SingleOrDefault(f => f.UserID == d.UserID);

                    var nextRowWithSameSession = i < query.Count - 1 &&
                                                 (!string.IsNullOrEmpty(query[i + 1].Session) && !string.IsNullOrEmpty(d.Session)) &&
                                                 query[i + 1].Session.Equals(d.Session);

                    if (nextRowWithSameSession)
                    {
                        newRow.TimeSpent = query[i + 1].LoadTimeStamp.Value.Subtract(Convert.ToDateTime(d.LoadTimeStamp)).Seconds;
                    }

                    if (userInfo != null)
                    {
                        var clientInfoSoFar = userInfo.UserFirstName + " " + userInfo.UserLastName;
                        var clientInfo = pwsClients.SingleOrDefault(f => f.PWSClientID == userInfo.PWSClientID);

                        if (clientInfo != null)
                        {
                            clientInfoSoFar += Environment.NewLine + clientInfo.PWSClientName;
                        }

                        newRow.ClientInfo += clientInfoSoFar;
                    }
                    else
                    {
                        newRow.ClientInfo = "";
                    }

                    monitoringList.Add(newRow);
                }

                return Ok(monitoringList);
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api"
                });

                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception });
            }
        }

        [HttpGet]
        [Route("FindAddedTran")]
        public IActionResult FindAddedTran(string UserID)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult sourceValidationResult)
            {
                return sourceValidationResult;
            }

            var tokenValidationResult = handlerHelper.ValidateToken(Request, UserID);
            if (!string.IsNullOrEmpty(tokenValidationResult))
            {
                return Unauthorized(tokenValidationResult);
            }

            try
            {
                // var _context = new PWSRecDataContext(pwsRectConnectionString);

                var data = pWSRecDataContext.NewEditTrans.Where(f => f.UserID == Convert.ToInt32(UserID))
                                               .OrderByDescending(f => f.LoadDateTime)
                                               .Take(1);

                var returnedRow = data.FirstOrDefault();

                return Ok(new { success = true, data = returnedRow });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID.ToString()
                });

                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception });
            }
        }

        [HttpGet]
        [Route("FindAddedSec")]
        public IActionResult FindAddedSec(string UserID)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult sourceValidationResult)
            {
                return sourceValidationResult;
            }

            if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
            {
                return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
            }


            try
            {
                //var _context = new DataClasses1DataContext(connectionString);

                var data = dataClasses1DataContext.Secs.Where(f => f.UserID == Convert.ToInt32(UserID))
                                        .OrderByDescending(f => f.LoadDateTime)
                                        .Take(1);

                var returnedRow = data.FirstOrDefault();

                return Ok(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID.ToString()
                });

                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception });
            }
        }


        [HttpPost]
        [Route("monitor")]
        public IActionResult MonitorSite([FromBody] Monitoring body)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult sourceValidationResult)
            {
                return sourceValidationResult;
            }

            if (handlerHelper.ValidateToken(Request, body.UserID) != string.Empty)
            {
                return Unauthorized(handlerHelper.ValidateToken(Request, body.UserID));
            }


            try
            {
                //var _context = new DataClasses1DataContext(connectionString);

                var url = Request.Headers["Referer"].ToString();
                var scope = body.Scope;

                var timeUtc = DateTime.UtcNow;
                TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);
                body.Misc = string.IsNullOrEmpty(body.Misc) ? "" : body.Misc;

                var pars = new Dictionary<string, dynamic>
            {
                { "@UserID", body.UserID },
                { "@HREF", url },
                { "@Site", "React" },
                { "@Scope", body.Scope },
                { "@LoadTimeStamp", easternTime },
                { "@Misc", body.Misc },
                { "@AutomatedPing", body.AutomatedPing },
                { "@Session", body.Session },

            };

                var d = "";// callProcedure("[dbo].usp_Monitor_Site", pars);
                return Ok(new { d = HttpContext.Request.Headers["Referer"].ToString() });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = body.UserID.ToString()
                });

                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception });
            }
        }


        [HttpGet]
        [Route("acctsubacct")]
        public IActionResult accountSubAccounts([FromQuery] string userId, [FromQuery] string EntityIDs = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult sourceValidationResult)
                {
                    return sourceValidationResult;
                }

                if (handlerHelper.ValidateToken(Request, userId) != string.Empty)
                {
                    return Unauthorized(handlerHelper.ValidateToken(Request, userId));
                }


                var pars = new Dictionary<string, dynamic>
            {
                { "@UserID", userId }
            };

                if (EntityIDs != null)
                {
                    pars.Add("@EntityIDs", EntityIDs);
                }

                var d = "";//callProcedure("DD.usp_AcctID_SubAcctID", pars);

                return Ok(new { messsage = "", errors = false, success = true, data = d });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = userId.ToString()
                });

                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception });
            }
        }


        [HttpGet]
        [Route("attrtypes")]
        public IActionResult attribtypeids(string UserID)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult sourceValidationResult)
            {
                return sourceValidationResult;
            }

            var tokenValidationResult = handlerHelper.ValidateToken(Request, UserID);
            if (!string.IsNullOrEmpty(tokenValidationResult))
            {
                return Unauthorized(new { error = true, tokenValidationResult });
            }

            try
            {
                var pars = new Dictionary<string, dynamic>
            {
                { "@UserID", UserID }
            };

                var data = "";// callProcedure("DD.usp_AttribTypeID", pars);
                return Ok(new { messsage = "", errors = false, success = true, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID.ToString()
                });

                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception });
            }
        }

        [HttpGet]
        [Route("attrnames")]
        public IActionResult attribnameids(string UserID)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult sourceValidationResult)
            {
                return sourceValidationResult;
            }

            var tokenValidationResult = handlerHelper.ValidateToken(Request, UserID);
            if (!string.IsNullOrEmpty(tokenValidationResult))
            {
                return Unauthorized(new { error = true, tokenValidationResult });
            }

            try
            {
                var pars = new Dictionary<string, dynamic>
            {
                { "@UserID", UserID }
            };

                var data = "";// callProcedure("DD.usp_AttribNameID", pars);
                return Ok(new { messsage = "", errors = false, success = true, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID.ToString()
                });

                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception });
            }
        }

        [HttpGet]
        [Route("securities")]
        public IActionResult GetSecurities([FromQuery] string userId, [FromQuery] string Matching = "0", [FromQuery] string Match = "", [FromQuery] string Top = "1000")
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult sourceValidationResult)
            {
                return sourceValidationResult;
            }

            var tokenValidationResult = handlerHelper.ValidateToken(Request);
            if (!string.IsNullOrEmpty(tokenValidationResult))
            {
                return Unauthorized(new { error = true, tokenValidationResult });
            }

            try
            {
                var pars = new Dictionary<string, dynamic>
            {
                { "@UserID", userId }
            };

                if (Matching != "0")
                {
                    pars.Add("@Match", Match);
                    pars.Add("@Top", Top);
                }

                var d = Matching == "0" ? callProcedure("DD.usp_SecID", pars) : callProcedure("DD.usp_SecID_Match", pars);

                return Ok(new { messsage = "", errors = false, success = true, data = d });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = userId.ToString()
                });

                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception });
            }
        }

        [HttpPost]
        [Route("transactions")]
        public IActionResult TransSearchResults(string UserID, string Pending, string Processed, string TransIDs,
                                                string AcctIDs, string SecIDs, string UserGroupIDs, string LoadSourceIDs,
                                                string DPIDs, string BaseCCYIDs, string TradeCCYIDs, string TxnBaseAmtLow,
                                                string TxnBaseAmtHigh, string TxnAmtLow, string TxnAmtHigh, string NetTxnBaseAmtLow,
                                                string NetTxnBaseAmtHigh, string NetTxnAmtLow, string NetTxnAmtHigh, string EffectiveDateStart,
                                                string EffectiveDateEnd, string TradeDateStart, string TradeDateEnd, string SettleDateStart,
                                                string SettleDateEnd, string AcquisitionTradeDateStart, string AcquisitionTradeDateEnd,
                                                string AcquisitionSettleDateStart, string AcquisitionSettleDateEnd, string TCodeIDs,
                                                string Comment, string ShareAmtLow, string ShareAmtHigh, string SettleCCYIDs,
                                                string RMs, string TxnSettleAmtLow, string TxnSettleAmtHigh, string NetTxnSettleAmtLow,
                                                string NetTxnSettleAmtHigh, string Final, string DPTransIDs)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult sourceValidationResult)
            {
                return sourceValidationResult;
            }

            var tokenValidationResult = handlerHelper.ValidateToken(Request);
            if (!string.IsNullOrEmpty(tokenValidationResult))
            {
                return Unauthorized(new { error = true, tokenValidationResult });
            }

            try
            {
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@Pending", Pending);
                pars.Add("@Processed", Processed);
                pars.Add("@TransIDs", TransIDs);
                pars.Add("@AcctIDs", AcctIDs);
                pars.Add("@SecIDs", SecIDs);
                pars.Add("@UserGroupIDs", UserGroupIDs);
                pars.Add("@LoadSourceIDs", LoadSourceIDs);
                pars.Add("@DPIDs", DPIDs);
                pars.Add("@BaseCCYIDs", BaseCCYIDs);
                pars.Add("@TradeCCYIDs", TradeCCYIDs);
                pars.Add("@TxnBaseAmtLow", TxnBaseAmtLow);
                pars.Add("@TxnBaseAmtHigh", TxnBaseAmtHigh);
                pars.Add("@TxnAmtLow", TxnAmtLow);
                pars.Add("@TxnAmtHigh", TxnAmtHigh);
                pars.Add("@NetTxnBaseAmtLow", NetTxnBaseAmtLow);
                pars.Add("@NetTxnBaseAmtHigh", NetTxnBaseAmtHigh);
                pars.Add("@NetTxnAmtLow", NetTxnAmtLow);
                pars.Add("@NetTxnAmtHigh", NetTxnAmtHigh);
                pars.Add("@EffectiveDateStart", EffectiveDateStart);
                pars.Add("@EffectiveDateEnd", EffectiveDateEnd);
                pars.Add("@TradeDateStart", TradeDateStart);
                pars.Add("@TradeDateEnd", TradeDateEnd);
                pars.Add("@SettleDateStart", SettleDateStart);
                pars.Add("@SettleDateEnd", SettleDateEnd);
                pars.Add("@AcquisitionTradeDateStart", AcquisitionTradeDateStart);
                pars.Add("@AcquisitionTradeDateEnd", AcquisitionTradeDateEnd);
                pars.Add("@AcquisitionSettleDateStart", AcquisitionSettleDateStart);
                pars.Add("@AcquisitionSettleDateEnd", AcquisitionSettleDateEnd);
                pars.Add("@TCodeIDs", TCodeIDs);
                pars.Add("@Comment", Comment);
                pars.Add("@ShareAmtLow", ShareAmtLow);
                pars.Add("@ShareAmtHigh", ShareAmtHigh);
                pars.Add("@SettleCCYIDs", SettleCCYIDs);
                pars.Add("@RMs", RMs);
                pars.Add("@TxnSettleAmtLow", TxnSettleAmtLow);
                pars.Add("@TxnSettleAmtHigh", TxnSettleAmtHigh);
                pars.Add("@NetTxnSettleAmtLow", NetTxnSettleAmtLow);
                pars.Add("@NetTxnSettleAmtHigh", NetTxnSettleAmtHigh);
                pars.Add("@DPTransIDs", DPTransIDs);
                if (!Helper.IgnoreAsNullParameter(Final))
                    pars.Add("@Final", Final);

                var d = callProcedure("adm.usp_Trans_Search", pars);

                return Ok(new { messsage = "", errors = false, success = true, data = d });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID.ToString()
                });

                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception });
            }
        }

        [HttpGet]
        [Route("DDGridCols")]
        public IActionResult GetDDForGridColumns()
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult sourceValidationResult)
            {
                return sourceValidationResult;
            }

            var tokenValidationResult = handlerHelper.ValidateToken(Request);
            if (!string.IsNullOrEmpty(tokenValidationResult))
            {
                return Unauthorized(new { error = true, tokenValidationResult });
            }

            try
            {
                //var context = new DataClasses1DataContext(connectionString);
                var data = dataClasses1DataContext.usp_GridColumns().AsQueryable();

                return Ok(data);
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = nameof(GetDDForGridColumns),
                    Exception = ex.Message,
                    Params = "web api"
                });

                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception });
            }
        }


        [HttpPost]
        [Route("clearimportdatatemptable")]
        public IActionResult ClearTempDataForImportTable(string gridType, string userId)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult)
            {
                return (IActionResult)handlerHelper.ValidateSource(Request);
            }

            var tokenValidationResult = handlerHelper.ValidateToken(Request, userId);
            if (!string.IsNullOrEmpty(tokenValidationResult))
            {
                return Unauthorized(tokenValidationResult);
            }

            try
            {
                //var _contextPWS = new DataClasses1DataContext(connectionString);
                dataClasses1DataContext.Database.SetCommandTimeout(0);

                var tableName = "";

                if (gridType == "New_Accounts")
                {
                    tableName = "[PWS].[dbo].[New_Accounts_tmp]";
                }

                if (!string.IsNullOrEmpty(tableName))
                {
                    dataClasses1DataContext.Database.ExecuteSqlRaw("delete from " + tableName + " where UserId=" + userId);
                    return Ok(new { success = true, gridType = gridType });
                }

                return Ok(new { success = false, gridType = gridType });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = userId.ToString()
                });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception });
            }
        }

        [HttpGet]
        [Route("checkuserlock")]
        public IActionResult CheckUserLock(string gridType, string predicates, string userId, string row)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult)
            {
                return (IActionResult)handlerHelper.ValidateSource(Request);
            }


            var tokenValidationResult = handlerHelper.ValidateToken(Request, userId);
            if (!string.IsNullOrEmpty(tokenValidationResult))
            {
                return Unauthorized(tokenValidationResult);
            }

            try
            {
                //var _contextPWS = new DataClasses1DataContext(connectionString);           

                dataClasses1DataContext.Database.SetCommandTimeout(0);

                if (gridType == "New_Accounts")
                {
                    var newAccountIdTemp = predicates.Split(',')[0];
                    var newAccountId = predicates.Split(',')[1];
                    var rowObject = JsonConvert.DeserializeObject<NewAccount>(row);
                    var user = Convert.ToInt32(userId);

                    //if editing is canceled by the user
                    if (rowObject.EditMode)
                    {
                        var record = dataClasses1DataContext.New_Accounts_tmps
                                .Where(f => f.New_AccountsID == rowObject.New_AccountsID && f.UserID == user)
                                .AsQueryable();

                        dataClasses1DataContext.New_Accounts_tmps.RemoveRange(record);
                        dataClasses1DataContext.SaveChanges();
                        rowObject.EditMode = false;
                        return Ok(new { userLock = false, error = false, success = true, deactivateEdit = true, data = rowObject });
                    }

                    var doesRecordExistsForThisUser = dataClasses1DataContext.New_Accounts_tmps
                        .Where(f => f.New_AccountsID == rowObject.New_AccountsID && rowObject.New_AccountsID != 0 && f.UserID == user)
                        .Any();

                    var doesRecordExistsForAnotherUser = dataClasses1DataContext.New_Accounts_tmps
                        .Where(f => f.New_AccountsID == rowObject.New_AccountsID && rowObject.New_AccountsID != 0 && f.UserID != user)
                        .Any();

                    // if record does not exist in newAccounts_temp, add it to temp table
                    if (!doesRecordExistsForThisUser && !doesRecordExistsForAnotherUser)
                    {
                        var newRow = new New_Accounts_tmp
                        {
                            New_AccountsID = rowObject.New_AccountsID,
                            AcctCode = rowObject.AcctCode,
                            AcctName = rowObject.AcctName,
                            AcctNickNamePrimary = rowObject.AcctNickNamePrimary,
                            AcctNum = rowObject.AcctNum,
                            BaseCurr = rowObject.BaseCurr,
                            EndingDate = !string.IsNullOrEmpty(rowObject.EndingDate) ? Convert.ToDateTime(rowObject.EndingDate) : new DateTime(2200, 12, 31),
                            EntityCode = rowObject.EntityCode,
                            LoadSourceID = !string.IsNullOrEmpty(rowObject.LoadSourceID) ? Convert.ToInt32(rowObject.LoadSourceID) : (int?)null,
                            OwnedPct = !string.IsNullOrEmpty(rowObject.OwnedPct) ? Convert.ToDecimal(rowObject.OwnedPct) : (decimal?)null,
                            PortfolioCode = rowObject.PortfolioCode,
                            ReliefMethodID = !string.IsNullOrEmpty(rowObject.TaxLotReliefMethodID) ? Convert.ToInt32(rowObject.TaxLotReliefMethodID) : (int?)null,
                            StartingDate = !string.IsNullOrEmpty(rowObject.StartingDate) ? Convert.ToDateTime(rowObject.StartingDate) : new DateTime(1900, 1, 1),
                            UserGroupID = !string.IsNullOrEmpty(rowObject.UserGroupID) ? Convert.ToInt32(rowObject.UserGroupID) : (int?)null,
                            UserID = Convert.ToInt32(userId)
                        };

                        dataClasses1DataContext.New_Accounts_tmps.Add(newRow);
                        dataClasses1DataContext.SaveChanges();

                        IQueryable newlyAddedRow = newRow.New_AccountsID != 0
                            ? dataClasses1DataContext.New_Accounts_tmps
                                .Where(f => f.New_AccountsID == rowObject.New_AccountsID && f.UserID == user)
                                .Take(1)
                            : dataClasses1DataContext.New_Accounts_tmps
                                .Where(f => f.New_AccountsID == 0 && f.UserID == user)
                                .OrderByDescending(f => f.New_Accounts_tmpID)
                                .Take(1);

                        return Ok(new { newlyAddeedRow = newRow.New_AccountsID == 0, userLock = false, error = false, success = true, data = newlyAddedRow });
                    }
                    else
                    {
                        IQueryable<New_Accounts_tmp> existingRow = dataClasses1DataContext.New_Accounts_tmps.Where(f => f.New_AccountsID == rowObject.New_AccountsID);

                        if (existingRow.Any())
                        {
                            var currentlyLockingUserId = Convert.ToString(existingRow.FirstOrDefault().UserID);

                            // if record exists in newAccounts_temp, is it locked by another user
                            if (!userId.Equals(currentlyLockingUserId))
                            {
                                //means it locked by someone else
                                IEnumerable<User> lockingUser = dataClasses1DataContext.Users.Where(p => p.UserID == Convert.ToInt32(currentlyLockingUserId));
                                return Ok(new { userLock = true, error = false, success = true, message = "The record is currently being edited by " + lockingUser.FirstOrDefault().UserName });
                            }
                            else
                            {
                                //first delete the row from temp
                                dataClasses1DataContext.New_Accounts_tmps.RemoveRange(existingRow);

                                // then add the row again so its a fresh update
                                var newRow = new New_Accounts_tmp
                                {
                                    New_AccountsID = rowObject.New_AccountsID,
                                    AcctCode = rowObject.AcctCode,
                                    AcctName = rowObject.AcctName,
                                    AcctNickNamePrimary = rowObject.AcctNickNamePrimary,
                                    AcctNum = rowObject.AcctNum,
                                    BaseCurr = rowObject.BaseCurr,
                                    EndingDate = !string.IsNullOrEmpty(rowObject.EndingDate) ? Convert.ToDateTime(rowObject.EndingDate) : new DateTime(2200, 12, 31),
                                    EntityCode = rowObject.EntityCode,
                                    LoadSourceID = !string.IsNullOrEmpty(rowObject.LoadSourceID) ? Convert.ToInt32(rowObject.LoadSourceID) : (int?)null,
                                    OwnedPct = !string.IsNullOrEmpty(rowObject.OwnedPct) ? Convert.ToDecimal(rowObject.OwnedPct) : (decimal?)null,
                                    PortfolioCode = rowObject.PortfolioCode,
                                    ReliefMethodID = !string.IsNullOrEmpty(rowObject.TaxLotReliefMethodID) ? Convert.ToInt32(rowObject.TaxLotReliefMethodID) : (int?)null,
                                    StartingDate = !string.IsNullOrEmpty(rowObject.StartingDate) ? Convert.ToDateTime(rowObject.StartingDate) : new DateTime(1900, 1, 1),
                                    UserGroupID = !string.IsNullOrEmpty(rowObject.UserGroupID) ? Convert.ToInt32(rowObject.UserGroupID) : (int?)null,
                                    UserID = Convert.ToInt32(userId)
                                };

                                dataClasses1DataContext.New_Accounts_tmps.Add(newRow);
                                dataClasses1DataContext.SaveChanges();

                                IQueryable newlyAddedRow = dataClasses1DataContext.New_Accounts_tmps
                                    .Where(f => f.New_AccountsID == rowObject.New_AccountsID)
                                    .Take(1);

                                return Ok(new { userLock = false, error = false, success = true, data = newlyAddedRow });
                            }
                        }
                    }
                }

                return Ok(new { success = false });

            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = userId.ToString()
                });

                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception });
            }
        }


        private IActionResult SubmitNewAccountsNonExcel(VMImpotData vm)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult)
                {
                    return (IActionResult)handlerHelper.ValidateSource(Request);
                }

                var tokenValidationResult = handlerHelper.ValidateToken(Request, vm.UserID);
                if (!string.IsNullOrEmpty(tokenValidationResult))
                {
                    return Unauthorized(tokenValidationResult);
                }

                var dataTobeSaved = new List<New_Accounts_tmp>();
                var responseError = new List<ResponseError>();

                //var _contextPWS = new DataClasses1DataContext(connectionString);
                dataClasses1DataContext.Database.SetCommandTimeout(0);

                //var _contextPWSRec = new PWSRecDataContext(pwsRectConnectionString);
                pWSRecDataContext.Database.SetCommandTimeout(0);

                vm.newAccts.ForEach(acct =>
                {
                    var rowToUpdate = dataClasses1DataContext.New_Accounts_tmps
                        .FirstOrDefault(r => r.New_Accounts_tmpID == acct.New_Accounts_tmpID);

                    if (rowToUpdate != null) // existing in temp table
                    {
                        rowToUpdate.AcctCode = acct.AcctCode;
                        rowToUpdate.AcctName = acct.AcctName;
                        rowToUpdate.AcctNickNamePrimary = acct.AcctNickNamePrimary;
                        rowToUpdate.AcctNum = acct.AcctNum;
                        rowToUpdate.BaseCurr = acct.BaseCurr;
                        rowToUpdate.EndingDate = !string.IsNullOrEmpty(acct.EndingDate) ? Convert.ToDateTime(acct.EndingDate) : new DateTime(2200, 12, 31);
                        if (!string.IsNullOrEmpty(acct.EntityCode)) rowToUpdate.EntityCode = acct.EntityCode;
                        rowToUpdate.LoadSourceID = !string.IsNullOrEmpty(acct.LoadSourceID) ? Convert.ToInt32(acct.LoadSourceID) : (int?)null;
                        rowToUpdate.OwnedPct = !string.IsNullOrEmpty(acct.OwnedPct) ? Convert.ToDecimal(acct.OwnedPct) : (decimal?)null;
                        rowToUpdate.PortfolioCode = acct.PortfolioCode;
                        rowToUpdate.ReliefMethodID = !string.IsNullOrEmpty(acct.TaxLotReliefMethodID) ? Convert.ToInt32(acct.TaxLotReliefMethodID) : (int?)null;
                        rowToUpdate.StartingDate = !string.IsNullOrEmpty(acct.StartingDate) ? Convert.ToDateTime(acct.StartingDate) : new DateTime(1900, 1, 1);
                        rowToUpdate.UserGroupID = !string.IsNullOrEmpty(acct.UserGroupID) ? Convert.ToInt32(acct.UserGroupID) : (int?)null;
                        rowToUpdate.UserID = Convert.ToInt32(vm.UserID);
                        rowToUpdate.UpdateDateTime = DateTime.Now;
                    }
                    else // new entry in temp table
                    {
                        rowToUpdate = new New_Accounts_tmp
                        {
                            AcctCode = acct.AcctCode,
                            AcctName = acct.AcctName,
                            AcctNickNamePrimary = acct.AcctNickNamePrimary,
                            AcctNum = acct.AcctNum,
                            BaseCurr = acct.BaseCurr,
                            EndingDate = !string.IsNullOrEmpty(acct.EndingDate) ? Convert.ToDateTime(acct.EndingDate) : new DateTime(2200, 12, 31),
                            EntityCode = acct.EntityCode,
                            LoadSourceID = !string.IsNullOrEmpty(acct.LoadSourceID) ? Convert.ToInt32(acct.LoadSourceID) : (int?)null,
                            OwnedPct = !string.IsNullOrEmpty(acct.OwnedPct) ? Convert.ToDecimal(acct.OwnedPct) : (decimal?)null,
                            PortfolioCode = acct.PortfolioCode,
                            ReliefMethodID = !string.IsNullOrEmpty(acct.TaxLotReliefMethodID) ? Convert.ToInt32(acct.TaxLotReliefMethodID) : (int?)null,
                            StartingDate = !string.IsNullOrEmpty(acct.StartingDate) ? Convert.ToDateTime(acct.StartingDate) : new DateTime(1900, 1, 1),
                            UserGroupID = !string.IsNullOrEmpty(acct.UserGroupID) ? Convert.ToInt32(acct.UserGroupID) : (int?)null,
                            UserID = Convert.ToInt32(vm.UserID),
                            UpdateDateTime = DateTime.Now
                        };

                        dataClasses1DataContext.New_Accounts_tmps.Add(rowToUpdate);
                    }
                });

                dataClasses1DataContext.SaveChanges();

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", vm.UserID);

                var d = "";//callProcedure("adm.usp_New_Accounts_Validate", pars);

                responseError = JsonConvert.DeserializeObject<List<ResponseError>>(d);
                var responseErrorRevised = responseError.Where(f => f.Column != "AcctCode").ToList();

                responseError.Where(f => f.Column == "AcctCode").ToList().ForEach(errObj =>
                {
                    if (Convert.ToInt32(errObj.New_AccountsID) > 0) // for existing record
                    {
                        var acctCodeOfOriginalRecord = dataClasses1DataContext.New_Accounts
                            .FirstOrDefault(g => Convert.ToString(g.New_AccountsID) == errObj.New_AccountsID)?.AcctCode?.Trim();

                        var acctCodeOfIncomingRecord = vm.newAccts
                            .FirstOrDefault(g => Convert.ToString(g.New_AccountsID) == errObj.New_AccountsID)?.AcctCode?.Trim();

                        if (!string.Equals(acctCodeOfIncomingRecord, acctCodeOfOriginalRecord, StringComparison.OrdinalIgnoreCase))
                        {
                            responseErrorRevised.Add(errObj);
                        }
                    }
                    else // for new records
                    {
                        var acctCodeOfIncomingRecord = vm.newAccts
                            .FirstOrDefault(g => Convert.ToString(g.New_Accounts_tmpID) == errObj.New_Accounts_tmpID)?.AcctCode?.Trim();

                        if (acctCodeOfIncomingRecord != null)
                        {
                            var doesAcctCodeExistsInNewAccountLAlready = dataClasses1DataContext.New_Accounts
                                .Any(f => f.AcctCode.Trim() == acctCodeOfIncomingRecord);

                            if (doesAcctCodeExistsInNewAccountLAlready)
                            {
                                responseErrorRevised.Add(errObj);
                            }
                        }
                    }
                });

                var newAcctTempIdsWithError = responseErrorRevised.Select(r => r.New_Accounts_tmpID).Distinct().ToList();
                var newAccountsDataTobeSaved = new List<New_Account>();

                vm.newAccts.ForEach(acct =>
                {
                    var rowHasErrors = newAcctTempIdsWithError.IndexOf(Convert.ToString(acct.New_Accounts_tmpID)) > -1;
                    if (!rowHasErrors)
                    {
                        if (acct.New_AccountsID > 0)
                        {
                            var rowToUpdate = dataClasses1DataContext.New_Accounts.FirstOrDefault(r => r.New_AccountsID == acct.New_AccountsID);
                            if (rowToUpdate != null)
                            {
                                rowToUpdate.AcctCode = acct.AcctCode;
                                rowToUpdate.AcctName = acct.AcctName;
                                rowToUpdate.AcctNickNamePrimary = acct.AcctNickNamePrimary;
                                rowToUpdate.AcctNum = acct.AcctNum;
                                rowToUpdate.BaseCurr = acct.BaseCurr;
                                rowToUpdate.EndingDate = !string.IsNullOrEmpty(acct.EndingDate) ? Convert.ToDateTime(acct.EndingDate) : new DateTime(2200, 12, 31);
                                rowToUpdate.EntityCode = acct.EntityCode;
                                rowToUpdate.LoadSourceID = !string.IsNullOrEmpty(acct.LoadSourceID) ? Convert.ToInt32(acct.LoadSourceID) : (int?)null;
                                rowToUpdate.OwnedPct = !string.IsNullOrEmpty(acct.OwnedPct) ? Convert.ToDecimal(acct.OwnedPct) : (decimal?)null;
                                rowToUpdate.PortfolioCode = acct.PortfolioCode;
                                rowToUpdate.TaxLotReliefMethodID = !string.IsNullOrEmpty(acct.TaxLotReliefMethodID) ? Convert.ToInt32(acct.TaxLotReliefMethodID) : (int?)null;
                                rowToUpdate.StartingDate = !string.IsNullOrEmpty(acct.StartingDate) ? Convert.ToDateTime(acct.StartingDate) : new DateTime(1900, 1, 1);
                                rowToUpdate.UserGroupID = !string.IsNullOrEmpty(acct.UserGroupID) ? Convert.ToInt32(acct.UserGroupID) : (int?)null;
                                rowToUpdate.UserID = Convert.ToInt32(vm.UserID);
                                rowToUpdate.UpdateDateTime = DateTime.Now;

                                dataClasses1DataContext.SaveChanges();
                            }
                        }
                        else
                        {
                            var newRow = new New_Account
                            {
                                AcctCode = acct.AcctCode,
                                AcctName = acct.AcctName,
                                AcctNickNamePrimary = acct.AcctNickNamePrimary,
                                AcctNum = acct.AcctNum,
                                BaseCurr = acct.BaseCurr,
                                EndingDate = !string.IsNullOrEmpty(acct.EndingDate) ? Convert.ToDateTime(acct.EndingDate) : new DateTime(2200, 12, 31),
                                EntityCode = acct.EntityCode,
                                LoadSourceID = !string.IsNullOrEmpty(acct.LoadSourceID) ? Convert.ToInt32(acct.LoadSourceID) : (int?)null,
                                OwnedPct = !string.IsNullOrEmpty(acct.OwnedPct) ? Convert.ToDecimal(acct.OwnedPct) : (decimal?)null,
                                PortfolioCode = acct.PortfolioCode,
                                TaxLotReliefMethodID = !string.IsNullOrEmpty(acct.TaxLotReliefMethodID) ? Convert.ToInt32(acct.TaxLotReliefMethodID) : (int?)null,
                                StartingDate = !string.IsNullOrEmpty(acct.StartingDate) ? Convert.ToDateTime(acct.StartingDate) : new DateTime(1900, 1, 1),
                                UserGroupID = !string.IsNullOrEmpty(acct.UserGroupID) ? Convert.ToInt32(acct.UserGroupID) : (int?)null,
                                UserID = Convert.ToInt32(vm.UserID),
                                UpdateDateTime = DateTime.Now,
                                LoadDateTime = DateTime.Now
                            };

                            newAccountsDataTobeSaved.Add(newRow);
                        }
                    }
                });

                if (newAccountsDataTobeSaved.Any())
                {
                    dataClasses1DataContext.New_Accounts.AddRange(newAccountsDataTobeSaved);
                    dataClasses1DataContext.SaveChanges();
                }

                return Ok(new { message = "", errors = responseErrorRevised, success = true, nonExcel = true });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = nameof(SubmitNewAccountsNonExcel),
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = vm.UserID
                });

                var exception = "Unknown Error Occurred";
                return Ok(new { message = exception, errors = new List<ResponseError>(), exception = true, success = false, nonExcel = true });
            }
        }


        public List<Perf_Static_tmp> DetectDiscontinuityDataFromTamp(VMImpotData vm, DataClasses1DataContext _contextPWS, Dictionary<int, List<Perf_Static>> dataCache = null, List<Perf_Static_tmp> tempsToBeAdded = null, List<Perf_Static> permsToBeDeleted = null)
        {
            try
            {
                var tempsWithDiscontinuity = new List<Perf_Static_tmp>();

                if (permsToBeDeleted == null)
                {
                    permsToBeDeleted = new List<Perf_Static>();
                }

                if (tempsToBeAdded == null)
                {
                    tempsToBeAdded = new List<Perf_Static_tmp>();
                    tempsToBeAdded = _contextPWS.Perf_Static_tmps.Where(f => f.UserID.Equals(vm.UserID)).ToList();
                }

                if (dataCache == null)
                {
                    dataCache = new Dictionary<int, List<Perf_Static>>();
                }

                var perfAttribIds = tempsToBeAdded.Select(f => f.PerfAttribID).Distinct();

                perfAttribIds.ToList().ForEach(perfAttriId =>
                {
                    var data = _contextPWS.Perf_Statics.Where(f => f.PerfAttribID == perfAttriId);
                    dataCache.Add(perfAttriId, data.ToList());
                });

                foreach (var temp in tempsToBeAdded)
                {
                    var beginDate = temp.BeginDate;
                    var endDate = temp.EndDate;

                    var perStatsUnflitered = dataCache.Where(f => f.Key == temp.PerfAttribID).FirstOrDefault();
                    var perfStats = new List<Perf_Static>();

                    perStatsUnflitered.Value.ForEach(item =>
                    {
                        if (!permsToBeDeleted.Any(f => f.PerfStaticID == item.PerfStaticID))
                        {
                            perfStats.Add(item);
                        }
                    });

                    var minEndDateInPerm = perfStats.Where(f => f.PerfAttribID == temp.PerfAttribID)
                                                    .Select(f => f.EndDate).Min();

                    var maxBeginDateInPerm = perfStats.Where(f => f.PerfAttribID == temp.PerfAttribID)
                                                      .Select(f => f.BeginDate).Max();

                    var recordAvailableBefore = perfStats.Where(f => f.EndDate == beginDate.Value.AddDays(-1) &&
                                                                     temp.PerfAttribID == f.PerfAttribID);

                    var recordsAvailableAfter = perfStats.Where(f => f.BeginDate == endDate.Value.AddDays(1) &&
                                                                     temp.PerfAttribID == f.PerfAttribID);

                    if (recordAvailableBefore.Any() && recordsAvailableAfter.Any())
                    {
                        continue;
                    }
                    else
                    {
                        var tempBefore = tempsToBeAdded.Where(f => f.EndDate == beginDate.Value.AddDays(-1));

                        var tempAfter = tempsToBeAdded.Where(f => f.BeginDate.Value == endDate.Value.AddDays(1));

                        if (perfStats.Any())
                        {
                            if ((!tempAfter.Any() && temp.EndDate < maxBeginDateInPerm.Value.AddDays(-1)) ||
                                (!tempBefore.Any() && temp.BeginDate > minEndDateInPerm.Value.AddDays(1)))
                            {
                                tempsWithDiscontinuity.Add(temp);
                            }
                        }
                    }
                }

                return tempsWithDiscontinuity;
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = nameof(DetectDiscontinuityDataFromTamp),
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = vm.UserID
                });

                return new List<Perf_Static_tmp>();
            }
        }

        public IActionResult HandlePerfStaticObjectDeletions(VMImpotData vm, DataClasses1DataContext _contextPWS)
        {
            try
            {
                var tempsWithDiscontinuity = new List<Perf_Static_tmp>();
                var permsToBeDeleted = vm.visualGroupingPermToTemp.SelectMany(f => f.Perms).ToList();
                var tempsToBeAdded = _contextPWS.Perf_Static_tmps.Where(f => f.UserID.Equals(vm.UserID)).ToList();

                Dictionary<int, List<Perf_Static>> dataCache = new Dictionary<int, List<Perf_Static>>();

                // First, let's check for discontinuity if it's Excel to keep
                if (vm.PerfStaticKeepAction.Equals("temp"))
                {
                    tempsWithDiscontinuity = DetectDiscontinuityDataFromTamp(vm, _contextPWS, dataCache, tempsToBeAdded, permsToBeDeleted);
                }

                if (!tempsWithDiscontinuity.Any() || vm.PerfStaticKeepAction.Equals("forcesave"))
                {
                    // First, delete the records from perm table
                    if (permsToBeDeleted.Any())
                    {
                        foreach (var item in permsToBeDeleted)
                        {
                            var itemInTable = _contextPWS.Perf_Statics.FirstOrDefault(f => f.PerfStaticID == item.PerfStaticID);
                            if (itemInTable != null)
                            {
                                _contextPWS.Perf_Statics.Remove(itemInTable);
                            }
                        }
                    }

                    // Now, add the ones from Excel
                    foreach (var tempItemToKeep in tempsToBeAdded)
                    {
                        var permToInsert = new Perf_Static
                        {
                            PerfAttribID = tempItemToKeep.PerfAttribID,
                            BeginDate = tempItemToKeep.BeginDate,
                            EndDate = tempItemToKeep.EndDate,
                            BMV = tempItemToKeep.BMV,
                            EMV = tempItemToKeep.EMV,
                            BegFlow = tempItemToKeep.BegFlow,
                            NetFlow = tempItemToKeep.NetFlow,
                            MgtFee = tempItemToKeep.MgtFee,
                            GRet = tempItemToKeep.GRet != null ? tempItemToKeep.GRet * 0.01m : tempItemToKeep.GRet,
                            NRet = tempItemToKeep.NRet != null ? tempItemToKeep.NRet * 0.01m : tempItemToKeep.NRet,
                            UserID = Convert.ToInt32(vm.UserID),
                            Stat = tempItemToKeep.Stat,
                            RetiredDateTime = tempItemToKeep.RetiredDateTime,
                            UpdateDateTime = tempItemToKeep.UpdateDateTime,
                            LoadDateTime = DateTime.Now,
                            PerfAttribDescr = Convert.ToString(tempItemToKeep.PerfAttribDescr),
                            LoadSourceID = Convert.ToInt32(tempItemToKeep.LoadSourceID),
                            DPID = Convert.ToInt32(tempItemToKeep.DPID)
                        };

                        _contextPWS.Perf_Statics.Add(permToInsert);
                    }

                    _contextPWS.SaveChanges();

                    vm.visualGroupingPermToTemp.Clear();
                    vm.visualGroupingTempToPerm.Clear();
                }

                return Ok(new
                {
                    messsage = "",
                    errors = "",
                    visualGroupingTempToPerm = vm.visualGroupingTempToPerm,
                    visualGroupingPermToTemp = vm.visualGroupingPermToTemp,
                    success = !tempsWithDiscontinuity.Any(),
                    hasMissingRecordPrior = tempsWithDiscontinuity.Any(),
                    tempsWithDiscontinuityWarning = tempsWithDiscontinuity,
                    gridType = "Perf_Static"
                });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = vm.UserID
                });

                var exception = "Unknown Error Occurred";
                return Ok(new { messsage = exception, errors = "A service error has occurred while saving the data", ex = ex, exception = true, success = false, gridType = "Perf_Static" });
            }
        }

        [HttpPost]
        [Route("SubmitGridData")]
        public IActionResult SubmitGridData([FromBody] VMImpotData vm)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult)
            {
                return (IActionResult)handlerHelper.ValidateSource(Request);
            }

            var tokenValidationResult = handlerHelper.ValidateToken(Request, vm.UserID);
            if (!string.IsNullOrEmpty(tokenValidationResult))
            {
                return Unauthorized(tokenValidationResult);
            }

            try
            {
                //var _contextPWS = new DataClasses1DataContext(connectionString);
                dataClasses1DataContext.Database.SetCommandTimeout(0);
                //var _contextPWSRec = new PWSRecDataContext(pwsRectConnectionString);
                pWSRecDataContext.Database.SetCommandTimeout(0);

                if (vm.notExcelImport)
                {
                    if (vm.GridName == "New_Accounts")
                    {
                        return SubmitNewAccountsNonExcel(vm);
                    }
                }

                if (!string.IsNullOrEmpty(vm.PerfStaticKeepAction) && vm.GridName == "Perf_Static")
                {
                    if (vm.visualGroupingPermToTemp == null)
                    {
                        vm.visualGroupingPermToTemp = new List<ComparePerfStats>();
                    }
                    if (vm.visualGroupingTempToPerm == null)
                    {
                        vm.visualGroupingTempToPerm = new List<ComparePerfStats>();
                    }
                    return HandlePerfStaticObjectDeletions(vm, dataClasses1DataContext);
                }

                List<ResponseError> responseError = new List<ResponseError>();

                //Perf_Static
                if (vm.GridName == "Perf_Static")
                {
                    try
                    {
                        dataClasses1DataContext.Database.ExecuteSqlRaw("delete from [PWS].[dbo].[Perf_Static_tmp] where UserID={0}", vm.UserID);
                        responseError = helperOps.UIValidatePerfStatic(vm.GridColumnSId, vm.UserID, vm.Cols, vm.perfStatic);
                        List<Perf_Static_tmp> perfStaticsTamps = new List<Perf_Static_tmp>();
                        List<Perf_Static> perfStaticsPermanents = new List<Perf_Static>();
                        if (responseError.Any())
                        {
                            return Ok(new { message = "", errors = responseError, success = false, gridType = "Perf_Static" });
                        }
                        else
                        {
                            //lets save in temp table
                            vm.perfStatic.ForEach(row =>
                            {
                                var newRow = new Perf_Static_tmp();

                                newRow.BegFlow = !string.IsNullOrEmpty(row.BegFlow) ? Convert.ToDecimal(row.BegFlow) : newRow.BegFlow;
                                newRow.NetFlow = !string.IsNullOrEmpty(row.NetFlow) ? Convert.ToDecimal(row.NetFlow) : newRow.NetFlow;
                                newRow.MgtFee = !string.IsNullOrEmpty(row.MgtFee) ? Convert.ToDecimal(row.MgtFee) : newRow.MgtFee;
                                newRow.BMV = !string.IsNullOrEmpty(row.BMV) ? Convert.ToDecimal(row.BMV) : newRow.BMV;
                                newRow.EMV = !string.IsNullOrEmpty(row.EMV) ? Convert.ToDecimal(row.EMV) : newRow.EMV;
                                newRow.GRet = !string.IsNullOrEmpty(row.GRet) ? Convert.ToDecimal(row.GRet) : newRow.GRet;
                                newRow.NRet = !string.IsNullOrEmpty(row.NRet) ? Convert.ToDecimal(row.NRet) : newRow.NRet;
                                newRow.PerfAttribDescr = Convert.ToString(row.PerfAttribDescr);
                                int num;
                                if (int.TryParse(row.BeginDate, out num) && row.BeginDate.Length == 5)
                                {
                                    newRow.BeginDate = Convert.ToDateTime(DateTime.FromOADate(Convert.ToDouble(row.BeginDate))).Date.AddDays(1); //
                                }
                                if (row.BeginDate.Length > 7)
                                {
                                    newRow.BeginDate = !string.IsNullOrEmpty(row.BeginDate) ? Convert.ToDateTime(row.BeginDate).Date.AddDays(1) : newRow.BeginDate; //
                                }

                                if (int.TryParse(row.EndDate, out num) && row.EndDate.Length == 5)
                                {
                                    newRow.EndDate = Convert.ToDateTime(DateTime.FromOADate(Convert.ToDouble(row.EndDate)).Date);
                                }
                                if (row.EndDate.Length > 7)
                                {
                                    newRow.EndDate = !string.IsNullOrEmpty(row.EndDate) ? Convert.ToDateTime(row.EndDate).Date : newRow.EndDate;
                                }

                                row.EndDateFormatted = Convert.ToDateTime(newRow.EndDate);
                                row.BeginDateFormatted = Convert.ToDateTime(newRow.BeginDate);

                                newRow.Stat = Convert.ToString(row.Stat).ToUpper();
                                newRow.CreateDateTime = DateTime.Now;
                                if (newRow.Stat == "R")
                                {
                                    newRow.RetiredDateTime = DateTime.Now;
                                }
                                newRow.PerfAttribID = !string.IsNullOrEmpty(row.PerfAttribID) ? Convert.ToInt32(row.PerfAttribID) : newRow.PerfAttribID;
                                newRow.UserID = Convert.ToInt32(vm.UserID);

                                newRow.DPID = Convert.ToInt32(row.DPID);
                                newRow.LoadSourceID = Convert.ToInt32(row.LoadSourceID);

                                perfStaticsTamps.Add(newRow);
                            });

                            perfStaticsTamps = perfStaticsTamps.OrderBy(f => f.BeginDate).ThenBy(f => f.EndDate).ToList();

                            dataClasses1DataContext.Perf_Static_tmps.AddRange(perfStaticsTamps);
                            dataClasses1DataContext.SaveChanges();
                        }

                        // let's check the datarange problem in the temp table within hte same perfAttribeID and force a UI validation
                        if (!responseError.Any())
                        {
                            responseError = helperOps.UIValidatePerfStatic(vm.GridColumnSId, vm.UserID, vm.Cols, vm.perfStatic);
                            if (responseError.Any())
                            {
                                return Ok(new { messsage = "", errors = responseError, success = false, gridType = "Perf_Static" });
                            }
                        }
                        // now check if there are overlapping dates
                        //for each row lets find the min and max dates and match it with Perf_Static table for data range conflicts
                        var tempItems = dataClasses1DataContext.Perf_Static_tmps.Where(f => f.UserID == Convert.ToInt32(vm.UserID)).ToList();

                        var perfAttribIds = vm.perfStatic.Select(f => f.PerfAttribID).Distinct();

                        var visualGroupingPermToTemp = new List<ComparePerfStats>();
                        var visualGroupingTempToPerm = new List<ComparePerfStats>();

                        foreach (var perfAttibID in perfAttribIds)
                        {
                            var temps = perfStaticsTamps.Where(f => f.PerfAttribID.ToString().Equals(perfAttibID)).ToList();
                            var perms = dataClasses1DataContext.Perf_Statics.Where(f => f.PerfAttribID.ToString().Equals(perfAttibID)).ToList();

                            var tempsComparedOnlyOnce = new List<int>();
                            foreach (var perm in perms)
                            {
                                var startDateOverlapped = new List<Perf_Static_tmp>();
                                var endDateOverlapped = new List<Perf_Static_tmp>();

                                var newComp1 = new ComparePerfStats();
                                newComp1.Perms = new List<Perf_Static>();
                                newComp1.Temps = new List<Perf_Static_tmp>();


                                foreach (var temp in temps)
                                {
                                    if (perm.BeginDate == temp.BeginDate && perm.EndDate == temp.EndDate)
                                    {
                                        startDateOverlapped.Add(temp);
                                        newComp1.Temps.Add(temp);
                                        tempsComparedOnlyOnce.Add(temp.PerfStatic_tmpID);
                                        break;
                                    }

                                    var ignoreTemp = tempsComparedOnlyOnce.IndexOf(temp.PerfStatic_tmpID) > -1;

                                    if (perm.BeginDate >= temp.BeginDate && perm.BeginDate.Value.AddDays(-1) < temp.EndDate && !ignoreTemp) //
                                    {
                                        startDateOverlapped.Add(temp);
                                        newComp1.Temps.Add(temp);
                                    }
                                    else if (perm.EndDate > temp.BeginDate.Value.AddDays(-1) && perm.EndDate <= temp.EndDate && !ignoreTemp) //
                                    {
                                        endDateOverlapped.Add(temp);
                                        newComp1.Temps.Add(temp);
                                    }
                                    else if (perm.BeginDate < temp.BeginDate && perm.EndDate > temp.EndDate && !ignoreTemp)
                                    {
                                        endDateOverlapped.Add(temp);
                                        newComp1.Temps.Add(temp);
                                    }
                                }

                                if (newComp1.Temps.Any() && !newComp1.Perms.Any())
                                {
                                    newComp1.Perms.Add(perm);
                                }

                                if (newComp1.Perms.Any() && newComp1.Temps.Any())
                                {
                                    visualGroupingPermToTemp.Add(newComp1);
                                }
                            }

                            var permsComparedOnlyOnce = new List<int>();
                            foreach (var temp in temps)
                            {
                                var startDateOverlapped = new List<Perf_Static>();
                                var endDateOverlapped = new List<Perf_Static>();

                                var newComp2 = new ComparePerfStats();
                                newComp2.Perms = new List<Perf_Static>();
                                newComp2.Temps = new List<Perf_Static_tmp>();

                                foreach (var perm in perms)
                                {
                                    if (perm.BeginDate == temp.BeginDate && perm.EndDate == temp.EndDate)
                                    {
                                        startDateOverlapped.Add(perm);
                                        newComp2.Perms.Add(perm);
                                        permsComparedOnlyOnce.Add(perm.PerfStaticID);
                                        break;
                                    }

                                    var ignorePerm = permsComparedOnlyOnce.IndexOf(perm.PerfStaticID) > -1;

                                    if (temp.BeginDate >= perm.BeginDate && temp.BeginDate.Value.AddDays(-1) < perm.EndDate && !ignorePerm) //
                                    {
                                        startDateOverlapped.Add(perm);
                                        newComp2.Perms.Add(perm);
                                    }
                                    else if (temp.EndDate > perm.BeginDate.Value.AddDays(-1) && temp.EndDate <= perm.EndDate && !ignorePerm) //
                                    {
                                        endDateOverlapped.Add(perm);
                                        newComp2.Perms.Add(perm);
                                    }
                                    else if (temp.BeginDate < perm.BeginDate && temp.EndDate > perm.EndDate && !ignorePerm)
                                    {
                                        endDateOverlapped.Add(perm);
                                        newComp2.Perms.Add(perm);
                                    }
                                }

                                if (newComp2.Perms.Any() && !newComp2.Temps.Any())
                                {
                                    newComp2.Temps.Add(temp);
                                }

                                if (newComp2.Perms.Any() && newComp2.Temps.Any())
                                {
                                    visualGroupingTempToPerm.Add(newComp2);
                                }
                            }

                        }
                        // this is when there are conflicts between excel and existing perm table data                   

                        if (visualGroupingTempToPerm.Any() || visualGroupingPermToTemp.Any())
                        {
                            return Ok(new
                            {
                                message = "",
                                errors = "",
                                visualGroupingTempToPerm = visualGroupingTempToPerm,
                                visualGroupingPermToTemp = visualGroupingPermToTemp,
                                dateRangeConflict = true,
                                success = false,
                                gridType = "Perf_Static"
                            });
                        }

                        var tempsWithDiscontinuity = new List<Perf_Static_tmp>();

                        tempItems = tempItems.OrderBy(f => f.BeginDate).ToList();

                        for (int i = 0; i < tempItems.Count; i++)
                        {
                            if (i > 0)
                            {
                                if (tempItems[i].BeginDate.Value.AddDays(-1) != tempItems[i - 1].EndDate) //
                                {
                                    if (!tempsWithDiscontinuity.Where(f => f.PerfStatic_tmpID == tempItems[i - 1].PerfStatic_tmpID).Any())
                                    {
                                        tempsWithDiscontinuity.Add(tempItems[i - 1]);
                                    }

                                    tempsWithDiscontinuity.Add(tempItems[i]);
                                }
                            }
                        }

                        if (tempsWithDiscontinuity.Any()) // extra protection for brand new unconflicted temp data that may have discontinuity
                        {
                            return Ok(new
                            {
                                message = "",
                                errors = "",
                                visualGroupingTempToPerm = vm.visualGroupingTempToPerm,
                                visualGroupingPermToTemp = vm.visualGroupingPermToTemp,
                                success = !tempsWithDiscontinuity.Any(),
                                hasMissingRecordPrior = tempsWithDiscontinuity.Any(),
                                tempsWithDiscontinuityWarning = tempsWithDiscontinuity,
                                gridType = "Perf_Static"
                            });
                        }
                        else if (vm.validateOnly)
                        {
                            return Ok(new
                            {
                                message = "",
                                errors = "",
                                validateOnly = vm.validateOnly,
                                gridType = "Perf_Static"
                            });
                        }


                        //now save to permanent table because server side validation has passed already
                        vm.perfStatic.ForEach(row =>
                        {
                            var newRow = new Perf_Static();
                            newRow.BegFlow = !string.IsNullOrEmpty(row.BegFlow) ? Convert.ToDecimal(row.BegFlow) : newRow.BegFlow;
                            newRow.NetFlow = !string.IsNullOrEmpty(row.NetFlow) ? Convert.ToDecimal(row.NetFlow) : newRow.NetFlow;
                            newRow.MgtFee = !string.IsNullOrEmpty(row.MgtFee) ? Convert.ToDecimal(row.MgtFee) : newRow.MgtFee;
                            newRow.BMV = !string.IsNullOrEmpty(row.BMV) ? Convert.ToDecimal(row.BMV) : newRow.BMV;
                            newRow.EMV = !string.IsNullOrEmpty(row.EMV) ? Convert.ToDecimal(row.EMV) : newRow.EMV;
                            newRow.GRet = !string.IsNullOrEmpty(row.GRet) ? Convert.ToDecimal(row.GRet) * 0.01m : newRow.GRet;
                            newRow.NRet = !string.IsNullOrEmpty(row.NRet) ? Convert.ToDecimal(row.NRet) * 0.01m : newRow.NRet;
                            newRow.PerfAttribID = !string.IsNullOrEmpty(row.PerfAttribID) ? Convert.ToInt32(row.PerfAttribID) : newRow.PerfAttribID;
                            newRow.PerfAttribDescr = Convert.ToString(row.PerfAttribDescr);
                            int num;
                            if (int.TryParse(row.BeginDate, out num) && row.BeginDate.Length == 5)
                            {
                                newRow.BeginDate = Convert.ToDateTime(DateTime.FromOADate(Convert.ToDouble(row.BeginDate))).AddDays(1); //
                            }
                            if (row.BeginDate.Length > 7)
                            {
                                newRow.BeginDate = !string.IsNullOrEmpty(row.BeginDate) ? Convert.ToDateTime(row.BeginDate).AddDays(1) : newRow.BeginDate;
                            }

                            if (int.TryParse(row.EndDate, out num) && row.EndDate.Length == 5)
                            {
                                newRow.EndDate = Convert.ToDateTime(DateTime.FromOADate(Convert.ToDouble(row.EndDate)));
                            }
                            if (row.EndDate.Length > 7)
                            {
                                newRow.EndDate = !string.IsNullOrEmpty(row.EndDate) ? Convert.ToDateTime(row.EndDate) : newRow.EndDate;
                            }

                            newRow.Stat = Convert.ToString(row.Stat).ToUpper();
                            newRow.LoadDateTime = DateTime.Now;
                            if (newRow.Stat == "R")
                            {
                                newRow.RetiredDateTime = DateTime.Now;
                            }
                            newRow.UserID = Convert.ToInt32(vm.UserID);
                            newRow.DPID = Convert.ToInt32(row.DPID);
                            newRow.LoadSourceID = Convert.ToInt32(row.LoadSourceID);
                            newRow.StaticData = true;
                            perfStaticsPermanents.Add(newRow);
                        });

                        dataClasses1DataContext.Perf_Statics.AddRange(perfStaticsPermanents);
                        dataClasses1DataContext.SaveChanges();



                        dataClasses1DataContext.Database.ExecuteSqlRaw("delete from [PWS].[dbo].[Perf_Static_tmp] where UserID={0}", vm.UserID);

                        return Ok(new { messsage = "", errors = responseError, success = true, gridType = "Perf_Static" });

                    }
                    catch (Exception ex)
                    {
                        Helper.AddApiLogs(connectionString, new ApiLogObject
                        {
                            Method = MethodInfo.GetCurrentMethod().Name,
                            Exception = ex.Message,
                            Params = "web api",
                            UserID = vm.UserID
                        });
                        dataClasses1DataContext.Database.ExecuteSqlRaw("delete from [PWS].[dbo].[Perf_Static_tmp] where UserID={0}", vm.UserID);
                        return Ok(new { messsage = "Exception has occured", exception = true, success = false, ex = ex, gridType = "Perf_Static" });
                    }
                }

                //RptModel
                if (vm.GridName == "RptModel")
                {
                    try
                    {
                        dataClasses1DataContext.Database.ExecuteSqlRaw("delete from [PWS].[dbo].[RptModel_tmp] where UserID=" + vm.UserID);
                        responseError = helperOps.UIValidateRptModel(vm.GridColumnSId, vm.UserID, vm.Cols, vm.rptModels);
                        if (responseError.Any())
                        {
                            return Ok(new { messsage = "", errors = responseError, success = false, gridType = "RptModel" });
                        }
                        var tempDataTobeSaved = new List<RptModel_tmp>();
                        // insert data to temp table
                        vm.rptModels.ForEach(rptModel =>
                        {
                            var newRow = new RptModel_tmp();
                            newRow.RowOrder = Convert.ToInt32(rptModel.RowOrder);
                            newRow.ParentRowOrder = Convert.ToInt32(rptModel.ParentRowOrder);
                            newRow.Levels = Convert.ToInt32(rptModel.Levels);
                            newRow.Title = rptModel.ParentRowOrder;
                            newRow.Description = rptModel.Description;
                            newRow.AttribTypeID = Convert.ToInt32(rptModel.AttribTypeID);
                            newRow.AttribNameID_ = Convert.ToInt32(rptModel.AttribNameID);
                            newRow.AttribOrder = Convert.ToInt32(rptModel.AttribOrder);
                            newRow.ModelTitle = rptModel.ModelTitle;
                            newRow.UserGroupID = Convert.ToInt32(rptModel.UserGroupID);
                            newRow.UserID = Convert.ToInt32(vm.UserID);
                            newRow.ParentRowOrder = Convert.ToInt32(rptModel.ParentRowOrder);

                            tempDataTobeSaved.Add(newRow);
                        });

                        if (tempDataTobeSaved.Any())
                        {
                            dataClasses1DataContext.RptModel_tmps.AddRange(tempDataTobeSaved);
                            dataClasses1DataContext.SaveChanges();
                        }

                        // do server side validation

                        // insert data to RptModel table

                        return Ok(new { messsage = "", exception = true, errors = responseError, gridType = "RptModel" });
                    }
                    catch (Exception ex)
                    {
                        Helper.AddApiLogs(connectionString, new ApiLogObject
                        {
                            Method = MethodInfo.GetCurrentMethod().Name,
                            Exception = ex.Message,
                            Params = "web api",
                            UserID = vm.UserID
                        });
                        return Ok(new { messsage = "Exception has occured", exception = true, ex = ex, success = false, gridType = "RptModel" });
                    }
                }

                // trans
                if (vm.GridName == "Trans")
                {
                    try
                    {
                        responseError = helperOps.UIValidateTrans(vm.GridColumnSId, vm.UserID, vm.Cols, vm.trans);
                        if (responseError.Any())
                        {
                            return Ok(new { messsage = "", errors = responseError, success = false });
                        }
                        else if (vm.validateOnly)
                        {
                            return Ok(new { messsage = "", errors = responseError, success = true, validationOnly = true });
                        }
                        else
                        {
                            pWSRecDataContext.Database.ExecuteSqlRaw("delete from [PWS_Rec].[dbo].[NewEditTrans_tmp] where UserId=" + vm.UserID);
                            var dataTobeSaved = new List<NewEditTrans_tmp>();
                            vm.trans.ForEach(tran =>
                            {
                                var newRow = new NewEditTrans_tmp();
                                newRow.TransID = !string.IsNullOrEmpty(tran.TransID) ? Convert.ToInt32(tran.TransID) : Convert.ToInt32(null);
                                newRow.TransProcessCodeID = !string.IsNullOrEmpty(tran.TransProcessCodeID) ? Convert.ToInt32(tran.TransProcessCodeID) : Convert.ToInt32(null);
                                newRow.ProcessOrd = !string.IsNullOrEmpty(tran.ProcessOrd) ? Convert.ToDecimal(tran.ProcessOrd) : Convert.ToDecimal(null);
                                newRow.StaticData = !string.IsNullOrEmpty(tran.StaticData) ? tran.StaticData.Trim().ToLower() == "y" || tran.StaticData.Trim().ToLower() == "1" : Convert.ToBoolean(null);
                                newRow.DPTransID = tran.DPTransID; // 5
                                newRow.DPLotID = tran.DPLotID;
                                newRow.AcctID = !string.IsNullOrEmpty(tran.AcctID) ? Convert.ToInt32(tran.AcctID) : Convert.ToInt32(null);
                                newRow.SubAcctID = !string.IsNullOrEmpty(tran.SubAcctID) ? Convert.ToInt32(tran.SubAcctID) : Convert.ToInt32(null);
                                newRow.ReliefMethod = !string.IsNullOrEmpty(tran.ReliefMethod) ? Convert.ToInt32(tran.ReliefMethod) : Convert.ToInt32(null);
                                newRow.SecID = !string.IsNullOrEmpty(tran.SecID) ? Convert.ToInt32(tran.SecID) : Convert.ToInt32(null);  // 10
                                newRow.TCodeID = !string.IsNullOrEmpty(tran.TCodeID) ? Convert.ToInt32(tran.TCodeID) : Convert.ToInt32(null);
                                newRow.DPTransCodeName = tran.DPTransCodeName;
                                newRow.EffectiveDate = !string.IsNullOrEmpty(tran.EffectiveDate) ? Convert.ToDateTime(tran.EffectiveDate) : Convert.ToDateTime(null);
                                newRow.TradeDate = !string.IsNullOrEmpty(tran.TradeDate) ? Convert.ToDateTime(tran.TradeDate) : Convert.ToDateTime(null);
                                newRow.SettleDate = !string.IsNullOrEmpty(tran.SettleDate) ? Convert.ToDateTime(tran.SettleDate) : Convert.ToDateTime(null);  // 15
                                newRow.AcquisitionTradeDate = !string.IsNullOrEmpty(tran.AcquisitionTradeDate) ? Convert.ToDateTime(tran.AcquisitionTradeDate) : Convert.ToDateTime(null);
                                newRow.AcquisitionSettleDate = !string.IsNullOrEmpty(tran.AcquisitionSettleDate) ? Convert.ToDateTime(tran.AcquisitionSettleDate) : Convert.ToDateTime(null);
                                newRow.Shrs = !string.IsNullOrEmpty(tran.Shrs) ? Convert.ToDecimal(tran.Shrs) : Convert.ToDecimal(null);
                                newRow.OFShrs = !string.IsNullOrEmpty(tran.OFShrs) ? Convert.ToDecimal(tran.OFShrs) : Convert.ToDecimal(null);
                                newRow.Price = !string.IsNullOrEmpty(tran.Price) ? Convert.ToDecimal(tran.Price) : Convert.ToDecimal(null); // 20
                                newRow.PriceMult = !string.IsNullOrEmpty(tran.PriceMult) ? Convert.ToDecimal(tran.PriceMult) : Convert.ToDecimal(null);
                                newRow.TradeCCYID = !string.IsNullOrEmpty(tran.TradeCCYID) ? Convert.ToInt32(tran.TradeCCYID) : Convert.ToInt32(null);
                                newRow.SettleCCYID = !string.IsNullOrEmpty(tran.SettleCCYID) ? Convert.ToInt32(tran.SettleCCYID) : Convert.ToInt32(null);
                                newRow.BaseCCYID = !string.IsNullOrEmpty(tran.BaseCCYID) ? Convert.ToInt32(tran.BaseCCYID) : Convert.ToInt32(null);
                                newRow.TradeFXRate = !string.IsNullOrEmpty(tran.TradeFXRate) ? Convert.ToDecimal(tran.TradeFXRate) : Convert.ToDecimal(null); // 25
                                newRow.SettleFXRate = !string.IsNullOrEmpty(tran.SettleFXRate) ? Convert.ToDecimal(tran.SettleFXRate) : Convert.ToDecimal(null);
                                newRow.BaseFXRate = !string.IsNullOrEmpty(tran.BaseFXRate) ? Convert.ToDecimal(tran.BaseFXRate) : Convert.ToDecimal(null);
                                newRow.GrossLocal = !string.IsNullOrEmpty(tran.GrossLocal) ? Convert.ToDecimal(tran.GrossLocal) : Convert.ToDecimal(null);
                                newRow.GrossBase = !string.IsNullOrEmpty(tran.GrossBase) ? Convert.ToDecimal(tran.GrossBase) : Convert.ToDecimal(null);
                                newRow.Fees = !string.IsNullOrEmpty(tran.Fees) ? Convert.ToDecimal(tran.Fees) : Convert.ToDecimal(null); // 30
                                newRow.ForeignWithhold = !string.IsNullOrEmpty(tran.ForeignWithhold) ? Convert.ToDecimal(tran.ForeignWithhold) : Convert.ToDecimal(null);
                                newRow.NetLocal = !string.IsNullOrEmpty(tran.NetLocal) ? Convert.ToDecimal(tran.NetLocal) : Convert.ToDecimal(null);
                                newRow.NetBase = !string.IsNullOrEmpty(tran.NetBase) ? Convert.ToDecimal(tran.NetBase) : Convert.ToDecimal(null);
                                newRow.AILocal = !string.IsNullOrEmpty(tran.AILocal) ? Convert.ToDecimal(tran.AILocal) : Convert.ToDecimal(null);
                                newRow.AIBase = !string.IsNullOrEmpty(tran.AIBase) ? Convert.ToDecimal(tran.AIBase) : Convert.ToDecimal(null); // 35
                                newRow.AALocal = !string.IsNullOrEmpty(tran.AALocal) ? Convert.ToDecimal(tran.AALocal) : Convert.ToDecimal(null);
                                newRow.AABase = !string.IsNullOrEmpty(tran.AABase) ? Convert.ToDecimal(tran.AABase) : Convert.ToDecimal(null);
                                newRow.OrigCostLocal = !string.IsNullOrEmpty(tran.OrigCostLocal) ? Convert.ToDecimal(tran.OrigCostLocal) : Convert.ToDecimal(null);
                                newRow.OrigCostBase = !string.IsNullOrEmpty(tran.OrigCostBase) ? Convert.ToDecimal(tran.OrigCostBase) : Convert.ToDecimal(null);
                                newRow.BookCostLocal = !string.IsNullOrEmpty(tran.BookCostLocal) ? Convert.ToDecimal(tran.BookCostLocal) : Convert.ToDecimal(null); //40
                                newRow.BookCostBase = !string.IsNullOrEmpty(tran.BookCostBase) ? Convert.ToDecimal(tran.BookCostBase) : Convert.ToDecimal(null);
                                newRow.RecordCostLocal = !string.IsNullOrEmpty(tran.RecordCostLocal) ? Convert.ToDecimal(tran.RecordCostLocal) : Convert.ToDecimal(null);
                                newRow.RecordCostBase = !string.IsNullOrEmpty(tran.RecordCostBase) ? Convert.ToDecimal(tran.RecordCostBase) : Convert.ToDecimal(null);
                                newRow.PurYld = !string.IsNullOrEmpty(tran.PurYld) ? Convert.ToDecimal(tran.PurYld) : Convert.ToDecimal(null);
                                newRow.Comment = tran.Comment;  //45
                                newRow.Final = !string.IsNullOrEmpty(tran.Final) ? tran.Final.Trim().ToLower() == "y" || tran.Final.Trim().ToLower() == "1" : Convert.ToBoolean(null);
                                newRow.Stat = tran.Stat;
                                newRow.LoadSourceID = !string.IsNullOrEmpty(tran.LoadSourceID) ? Convert.ToInt32(tran.LoadSourceID) : Convert.ToInt32(null);
                                newRow.DPID = !string.IsNullOrEmpty(tran.DPID) ? Convert.ToInt32(tran.DPID) : Convert.ToInt32(null);
                                newRow.OriginalTransID = !string.IsNullOrEmpty(tran.OriginalTransID) ? Convert.ToInt32(tran.OriginalTransID) : Convert.ToInt32(null); //50
                                newRow.TransTypeID = !string.IsNullOrEmpty(tran.TransTypeID) ? Convert.ToInt32(tran.TransTypeID) : Convert.ToInt32(null);
                                newRow.TransSubTypeID = !string.IsNullOrEmpty(tran.TransSubTypeID) ? Convert.ToInt32(tran.TransSubTypeID) : Convert.ToInt32(null);
                                newRow.LinkAcctID = !string.IsNullOrEmpty(tran.LinkAcctID) ? Convert.ToInt32(tran.LinkAcctID) : Convert.ToInt32(null);
                                newRow.LinkSubAcctID = !string.IsNullOrEmpty(tran.LinkSubAcctID) ? Convert.ToInt32(tran.LinkSubAcctID) : Convert.ToInt32(null);
                                newRow.UserDefined1 = tran.UserDefined1; //55
                                newRow.UserDefined2 = tran.UserDefined2;
                                newRow.UserID = Convert.ToInt32(vm.UserID);
                                newRow.UIID = vm.trans.IndexOf(tran);
                                dataTobeSaved.Add(newRow);
                            });

                            pWSRecDataContext.NewEditTrans_tmps.AddRange(dataTobeSaved);
                            pWSRecDataContext.SaveChanges();
                            return Ok(new { messsage = "", errors = responseError, success = true });
                        }
                    }
                    catch (Exception ex)
                    {
                        Helper.AddApiLogs(connectionString, new ApiLogObject
                        {
                            Method = MethodInfo.GetCurrentMethod().Name,
                            Exception = ex.Message,
                            Params = "web api",
                            UserID = vm.UserID
                        });
                        return Ok(new { messsage = "Exception has occured", ex = ex, exception = true, success = false });
                    }

                }

                // new accounts
                if (vm.GridName == "New_Accounts")
                {
                    try
                    {
                        // do the validation/insert with Joanne's proc
                        Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();

                        responseError = helperOps.UIValidateNewAccounts(vm.GridColumnSId, vm.UserID, vm.Cols, vm.newAccts);

                        if (responseError.Any())
                        {
                            return Ok(new { messsage = "", errors = responseError, success = false });
                        }

                        else
                        {
                            dataClasses1DataContext.Database.ExecuteSqlRaw("delete from [PWS].[dbo].[New_Accounts_tmp] where UserId=" + vm.UserID);
                            var dataTobeSaved = new List<New_Accounts_tmp>();
                            int num;
                            vm.newAccts.ForEach(acct =>
                            {
                                var newRow = new New_Accounts_tmp();
                                newRow.AcctCode = acct.AcctCode;
                                newRow.AcctName = acct.AcctName;
                                newRow.AcctNickNamePrimary = acct.AcctNickNamePrimary;
                                newRow.AcctNum = acct.AcctNum;
                                newRow.BaseCurr = acct.BaseCurr;
                                if (int.TryParse(acct.EndingDate, out num) && acct.EndingDate.Length == 5)
                                {
                                    acct.EndingDate = Convert.ToString(DateTime.FromOADate(Convert.ToDouble(acct.EndingDate)));
                                }
                                newRow.EndingDate = !string.IsNullOrEmpty(acct.EndingDate) ? Convert.ToDateTime(acct.EndingDate) : Convert.ToDateTime(new DateTime(2200, 12, 31));
                                newRow.EntityCode = acct.EntityCode;
                                newRow.LoadSourceID = !string.IsNullOrEmpty(acct.LoadSourceID) ? Convert.ToInt32(acct.LoadSourceID) : Convert.ToInt32(null);
                                newRow.OwnedPct = !string.IsNullOrEmpty(acct.OwnedPct) ? Convert.ToDecimal(acct.OwnedPct) : Convert.ToDecimal(null);
                                newRow.PortfolioCode = acct.PortfolioCode;
                                newRow.ReliefMethodID = !string.IsNullOrEmpty(acct.ReliefMethodID) ? Convert.ToInt32(acct.ReliefMethodID) : Convert.ToInt32(null);
                                if (int.TryParse(acct.StartingDate, out num) && acct.StartingDate.Length == 5)
                                {
                                    acct.StartingDate = Convert.ToString(DateTime.FromOADate(Convert.ToDouble(acct.StartingDate)));
                                }
                                newRow.StartingDate = !string.IsNullOrEmpty(acct.StartingDate) ? Convert.ToDateTime(acct.StartingDate) : Convert.ToDateTime(new DateTime(1900, 1, 1));
                                newRow.UserGroupID = !string.IsNullOrEmpty(acct.UserGroupID) ? Convert.ToInt32(acct.UserGroupID) : Convert.ToInt32(null);
                                newRow.UserID = Convert.ToInt32(vm.UserID);
                                newRow.UpdateDateTime = DateTime.Now;
                                newRow.LoadDateTime = DateTime.Now;
                                newRow.UIID = vm.newAccts.IndexOf(acct);
                                dataTobeSaved.Add(newRow);
                            });

                            dataClasses1DataContext.New_Accounts_tmps.AddRange(dataTobeSaved);
                            dataClasses1DataContext.SaveChanges();

                            pars.Clear();
                            pars.Add("@UserID", vm.UserID);

                            var d = "";// callProcedure("adm.usp_New_Accounts_Validate", pars);
                            responseError = JsonConvert.DeserializeObject<List<ResponseError>>(d);
                            if (responseError.Any())
                            {
                                return Ok(new { messsage = "", errors = responseError, success = false });
                            }
                            else if (vm.validateOnly)
                            {
                                return Ok(new { messsage = "", errors = responseError, validateOnly = vm.validateOnly, success = false });
                            }

                            // no validation errors so just insert the rows to new account table
                            var newAccountDataTobeInserted = new List<New_Account>();

                            vm.newAccts.ToList().ForEach(acct =>
                            {
                                var newRow = new New_Account();
                                newRow.AcctCode = acct.AcctCode;
                                newRow.AcctName = acct.AcctName;
                                newRow.AcctNickNamePrimary = acct.AcctNickNamePrimary;
                                newRow.AcctNum = acct.AcctNum;
                                newRow.BaseCurr = acct.BaseCurr;
                                newRow.EndingDate = !string.IsNullOrEmpty(acct.EndingDate) ? Convert.ToDateTime(acct.EndingDate) : Convert.ToDateTime(new DateTime(2200, 12, 31));
                                newRow.EntityCode = acct.EntityCode;
                                newRow.LoadSourceID = !string.IsNullOrEmpty(acct.LoadSourceID) ? Convert.ToInt32(acct.LoadSourceID) : Convert.ToInt32(null);
                                newRow.OwnedPct = !string.IsNullOrEmpty(acct.OwnedPct) ? Convert.ToDecimal(acct.OwnedPct) : Convert.ToDecimal(null);
                                newRow.PortfolioCode = acct.PortfolioCode;
                                newRow.TaxLotReliefMethodID = !string.IsNullOrEmpty(acct.TaxLotReliefMethodID) ? Convert.ToInt32(acct.TaxLotReliefMethodID) : Convert.ToInt32(null);
                                newRow.StartingDate = !string.IsNullOrEmpty(acct.StartingDate) ? Convert.ToDateTime(acct.StartingDate) : Convert.ToDateTime(new DateTime(1900, 1, 1));
                                newRow.UserGroupID = !string.IsNullOrEmpty(acct.UserGroupID) ? Convert.ToInt32(acct.UserGroupID) : Convert.ToInt32(null);
                                newRow.UserID = Convert.ToInt32(vm.UserID);
                                newRow.UpdateDateTime = DateTime.Now;
                                newRow.LoadDateTime = DateTime.Now;

                                newAccountDataTobeInserted.Add(newRow);
                            });

                            if (newAccountDataTobeInserted.Any())
                            {
                                dataClasses1DataContext.New_Accounts.AddRange(newAccountDataTobeInserted);
                                dataClasses1DataContext.SaveChanges();

                            }

                            return Ok(new { messsage = "", errors = responseError, success = true });
                        }

                    }
                    catch (Exception ex)
                    {
                        Helper.AddApiLogs(connectionString, new ApiLogObject
                        {
                            Method = MethodInfo.GetCurrentMethod().Name,
                            Exception = ex.Message,
                            Params = "web api",
                            UserID = vm.UserID
                        });
                        return Ok(new { messsage = "Exception has occured: " + ex.Message, exception = true, success = false });
                    }
                }

                return Ok(new { messsage = "", errors = responseError, success = true });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = vm.UserID
                });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }

        }

        [HttpPost]
        [Route("TransactionsSearch")]
        public IActionResult TransactionsSearch(
            string UserID,
            string Pending,
            string Processed,
            string TransIDs,
            string Final,
            string CreatedDateStart,
            string CreatedDateEnd,
            string RMs,
            string TCodeIDs,
            string AcctIDs,
            string SecIDs,
            string UserGroupIDs,
            string LoadSourceIDs,
            string DPIDs,
            string DPTransIDs,
            string TradeDateStart,
            string TradeDateEnd,
            string SettleDateStart,
            string SettleDateEnd,
            string EffectiveDateStart,
            string EffectiveDateEnd,
            string AcquisitionTradeDateStart,
            string AcquisitionTradeDateEnd,
            string AcquisitionSettleDateStart,
            string AcquisitionSettleDateEnd,
            string TradeCCYIDs,
            string SettleCCYIDs,
            string BaseCCYIDs,
            string TxnAmtLow,
            string TxnAmtHigh,
            string NetTxnAmtLow,
            string NetTxnAmtHigh,
            string TxnSettleAmtLow,
            string TxnSettleAmtHigh,
            string NetTxnSettleAmtLow,
            string NetTxnSettleAmtHigh,
            string TxnBaseAmtLow,
            string TxnBaseAmtHigh,
            string NetTxnBaseAmtLow,
            string NetTxnBaseAmtHigh,
            string ShareAmtLow,
            string ShareAmtHigh,
            string Comment
        )

        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult)
                {
                    return (IActionResult)handlerHelper.ValidateSource(Request);
                }
                if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
                {
                    return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@Pending", Pending);
                pars.Add("@Processed", Processed);
                pars.Add("@TransIDs", TransIDs);
                pars.Add("@RMs", RMs);
                pars.Add("@TCodeIDs", TCodeIDs);
                pars.Add("@AcctIDs", AcctIDs);
                pars.Add("@SecIDs", SecIDs);
                pars.Add("@UserGroupIDs", UserGroupIDs);
                pars.Add("@LoadSourceIDs", LoadSourceIDs);
                pars.Add("@DPIDs", DPIDs);
                pars.Add("@DPTransIDs", DPTransIDs);
                pars.Add("@TradeDateStart", TradeDateStart);
                pars.Add("@TradeDateEnd", TradeDateEnd);
                pars.Add("@SettleDateStart", SettleDateStart);
                pars.Add("@SettleDateEnd", SettleDateEnd);
                pars.Add("@EffectiveDateStart", EffectiveDateStart);
                pars.Add("@EffectiveDateEnd", EffectiveDateEnd);
                pars.Add("@CreatedDateStart", CreatedDateStart);
                pars.Add("@CreatedDateEnd", CreatedDateEnd);
                pars.Add("@AcquisitionTradeDateStart", AcquisitionTradeDateStart);
                pars.Add("@AcquisitionTradeDateEnd", AcquisitionTradeDateEnd);
                pars.Add("@AcquisitionSettleDateStart", AcquisitionSettleDateStart);
                pars.Add("@AcquisitionSettleDateEnd", AcquisitionSettleDateEnd);
                pars.Add("@TradeCCYIDs", TradeCCYIDs);
                pars.Add("@SettleCCYIDs", SettleCCYIDs);
                pars.Add("@BaseCCYIDs", BaseCCYIDs);
                pars.Add("@TxnAmtLow", TxnAmtLow);
                pars.Add("@TxnAmtHigh", TxnAmtHigh);
                pars.Add("@NetTxnAmtLow", NetTxnAmtLow);
                pars.Add("@NetTxnAmtHigh", NetTxnAmtHigh);
                pars.Add("@TxnSettleAmtLow", TxnSettleAmtLow);
                pars.Add("@TxnSettleAmtHigh", TxnSettleAmtHigh);
                pars.Add("@NetTxnSettleAmtLow", NetTxnSettleAmtLow);
                pars.Add("@NetTxnSettleAmtHigh", NetTxnSettleAmtHigh);
                pars.Add("@TxnBaseAmtLow", TxnBaseAmtLow);
                pars.Add("@TxnBaseAmtHigh", TxnBaseAmtHigh);
                pars.Add("@NetTxnBaseAmtLow", NetTxnBaseAmtLow);
                pars.Add("@NetTxnBaseAmtHigh", NetTxnBaseAmtHigh);
                pars.Add("@ShareAmtLow", ShareAmtLow);
                pars.Add("@ShareAmtHigh", ShareAmtHigh);
                pars.Add("@Comment", Comment);
                if (!Helper.IgnoreAsNullParameter(Final))
                    pars.Add("@Final", Final);

                var data = Helper.callProcedure("[adm].[usp_Trans_Search]", pars);
                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID.ToString()
                });
                var exception = "Unknown Error Occurred";

                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("TransactionsEdit")]
        public IActionResult TransactionsEdit(
            string UserID,
            string Trial_TransIDs,
            string ProcessOrd,
            string AcctID,
            string ReliefMethod,
            string SecID,
            string TCodeID,
            string EffectiveDate,
            string TradeDate,
            string SettleDate,
            string AcquisitionTradeDate,
            string AcquisitionSettleDate,
            string Shrs,
            string OFShrs,
            string Price,
            string SettleCCYID,
            string TradeFXRate,
            string SettleFXRate,
            string BaseFXRate,
            string GrossLocal,
            string GrossSettle,
            string GrossBase,
            string Fees,
            string FeesSettle,
            string FeesBase,
            string ForeignWithhold,
            string ForeignWithholdSettle,
            string ForeignWithholdBase,
            string NetLocal,
            string NetSettle,
            string NetBase,
            string AILocal,
            string AIBase,
            string AISettle,
            string RecordCostLocal,
            string RecordCostBase,
            string Comment,
            string LinkAcctID,
            string SecEntAllocID,
            string Final,
            string UserDefined1,
            string UserDefined2,
            string TotalTradeAmtLocal,
            string TotalTradeAmtSettle,
            string TotalTradeAmtBase,
            string Stat
        )

        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult)
                {
                    return (IActionResult)handlerHelper.ValidateSource(Request);
                }
                if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
                {
                    return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@Trial_TransIDs", Trial_TransIDs);
                pars.Add("@ProcessOrd", ProcessOrd);
                pars.Add("@AcctID", AcctID);
                pars.Add("@ReliefMethod", ReliefMethod);
                pars.Add("@SecID", SecID);
                pars.Add("@TCodeID", TCodeID);
                pars.Add("@EffectiveDate", EffectiveDate);
                pars.Add("@TradeDate", TradeDate);
                pars.Add("@SettleDate", SettleDate);
                if (!Helper.IgnoreAsNullParameter(AcquisitionTradeDate))
                    pars.Add("@AcquisitionTradeDate", AcquisitionTradeDate);
                if (!Helper.IgnoreAsNullParameter(AcquisitionSettleDate))
                    pars.Add("@AcquisitionSettleDate", AcquisitionSettleDate);
                pars.Add("@Shrs", Shrs);
                if (!Helper.IgnoreAsNullParameter(OFShrs))
                    pars.Add("@OFShrs", OFShrs);
                pars.Add("@Price", Price);
                pars.Add("@SettleCCYID", SettleCCYID);
                pars.Add("@TradeFXRate", TradeFXRate);
                pars.Add("@SettleFXRate", SettleFXRate);
                pars.Add("@BaseFXRate", BaseFXRate);
                pars.Add("@GrossLocal", GrossLocal);
                pars.Add("@GrossSettle", GrossSettle);
                pars.Add("@GrossBase", GrossBase);
                if (!Helper.IgnoreAsNullParameter(Fees))
                    pars.Add("@Fees", Fees);
                if (!Helper.IgnoreAsNullParameter(FeesSettle))
                    pars.Add("@FeesSettle", FeesSettle);
                if (!Helper.IgnoreAsNullParameter(FeesBase))
                    pars.Add("@FeesBase", FeesBase);
                if (!Helper.IgnoreAsNullParameter(ForeignWithhold))
                    pars.Add("@ForeignWithhold", ForeignWithhold);
                if (!Helper.IgnoreAsNullParameter(ForeignWithholdSettle))
                    pars.Add("@ForeignWithholdSettle", ForeignWithholdSettle);
                if (!Helper.IgnoreAsNullParameter(ForeignWithholdBase))
                    pars.Add("@ForeignWithholdBase", ForeignWithholdBase);
                pars.Add("@NetLocal", NetLocal);
                pars.Add("@NetSettle", NetSettle);
                pars.Add("@NetBase", NetBase);
                if (!Helper.IgnoreAsNullParameter(AILocal))
                    pars.Add("@AILocal", AILocal);
                if (!Helper.IgnoreAsNullParameter(AIBase))
                    pars.Add("@AIBase", AIBase);
                if (!Helper.IgnoreAsNullParameter(AISettle))
                    pars.Add("@AISettle", AISettle);
                if (!Helper.IgnoreAsNullParameter(RecordCostLocal))
                    pars.Add("@RecordCostLocal", RecordCostLocal);
                if (!Helper.IgnoreAsNullParameter(RecordCostBase))
                    pars.Add("@RecordCostBase", RecordCostBase);
                if (!Helper.IgnoreAsNullParameter(Comment))
                    pars.Add("@Comment", Comment);
                if (!Helper.IgnoreAsNullParameter(LinkAcctID))
                    pars.Add("@LinkAcctID", LinkAcctID);
                if (!Helper.IgnoreAsNullParameter(SecEntAllocID))
                    pars.Add("@SecEntAllocID", SecEntAllocID);
                if (!Helper.IgnoreAsNullParameter(Final))
                    pars.Add("@Final", Final);
                if (!Helper.IgnoreAsNullParameter(UserDefined1))
                    pars.Add("@UserDefined1", UserDefined1);
                if (!Helper.IgnoreAsNullParameter(UserDefined2))
                    pars.Add("@UserDefined2", UserDefined2);
                pars.Add("@TotalTradeAmtLocal", TotalTradeAmtLocal);
                pars.Add("@TotalTradeAmtSettle", TotalTradeAmtSettle);
                pars.Add("@TotalTradeAmtBase", TotalTradeAmtBase);
                pars.Add("@Stat", Stat);

                var data = Helper.callProcedure("[adm].[usp_Trans_Edit]", pars);
                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID.ToString()
                });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("TransactionsInsert")]
        public IActionResult TransactionsInsert(
            string UserID,
            string AcctID,
            string SecID,
            string TCodeID,
            string EffectiveDate,
            string Price,
            string Shrs,
            string GrossLocal,
            string SettleCCYID
        )

        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult)
                {
                    return (IActionResult)handlerHelper.ValidateSource(Request);
                }
                if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
                {
                    return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@AcctID", AcctID);
                pars.Add("@SecID", SecID);
                pars.Add("@TCodeID", TCodeID);
                pars.Add("@EffectiveDate", EffectiveDate);

                if (!Helper.IgnoreAsNullParameter(SettleCCYID))
                    pars.Add("@SettleCCYID", SettleCCYID);

                if (!Helper.IgnoreAsNullParameter(Shrs))
                    pars.Add("@Shrs", Shrs);

                if (!Helper.IgnoreAsNullParameter(Price))
                    pars.Add("@Price", Price);

                if (!Helper.IgnoreAsNullParameter(GrossLocal))
                    pars.Add("@GrossLocal", GrossLocal);

                var data = Helper.callProcedure("[adm].[usp_Trans_Insert]", pars);
                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID.ToString()
                });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }
        [HttpPost]
        [Route("TransTrialSave")]
        public IActionResult TransTrialSave(
            string UserID,
            string Trial_TransIDs
        )

        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult)
                {
                    return (IActionResult)handlerHelper.ValidateSource(Request);
                }
                if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
                {
                    return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@Trial_TransIDs", Trial_TransIDs);

                var data = Helper.callProcedure("[adm].[usp_Trans_Trial_Save]", pars);
                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID.ToString()
                });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("TransTrialDelete")]
        public IActionResult TransTrialDelete(
            string UserID,
            string Trial_TransIDs
        )

        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult)
                {
                    return (IActionResult)handlerHelper.ValidateSource(Request);
                }
                if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
                {
                    return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@Trial_TransIDs", Trial_TransIDs);

                var data = Helper.callProcedure("[adm].[usp_Trans_Trial_Delete]", pars);
                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID.ToString()
                });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("removeimporteddata")]
        public IActionResult RemoveImportedData([FromQuery] string userId, [FromQuery] string gridType, [FromQuery] string predicates)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult)
            {
                return (IActionResult)handlerHelper.ValidateSource(Request);
            }

            if (handlerHelper.ValidateToken(Request, userId) != string.Empty)
            {
                return Unauthorized(handlerHelper.ValidateToken(Request, userId));
            }

            try
            {
                //var context = new DataClasses1DataContext(connectionString);
                dataClasses1DataContext.Database.SetCommandTimeout(0);
                if (gridType == "New_Accounts")
                {
                    var newAccountsTempId = predicates.Split(',')[0];
                    var newAccountsId = predicates.Split(',')[1];

                    if (!string.IsNullOrEmpty(newAccountsTempId) && newAccountsTempId != "null" && newAccountsTempId != "undefined")
                    {
                        //delete from the temp table
                        dataClasses1DataContext.Database.ExecuteSqlRaw("delete from [PWS].[dbo].[New_Accounts_tmp] where New_Accounts_tmpID=" + newAccountsTempId);
                    }

                    if (!string.IsNullOrEmpty(newAccountsId) && newAccountsId != "null" && newAccountsId != "undefined")
                    {
                        //delete from the main table
                        dataClasses1DataContext.Database.ExecuteSqlRaw("delete from [PWS].[dbo].[New_Accounts] where New_AccountsID=" + newAccountsId);
                    }
                }
                return Ok(new { messsage = "", errors = "", success = true });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = userId
                });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }


        [Route("lookup")]
        [HttpPost]
        public IActionResult GetLookUpByID([FromQuery] string userId, [FromQuery] string id, [FromBody] LookUps lookups)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult)
            {
                return (IActionResult)handlerHelper.ValidateSource(Request);
            }

            if (handlerHelper.ValidateToken(Request, userId) != string.Empty)
            {
                return Unauthorized(handlerHelper.ValidateToken(Request, userId));
            }

            try
            {
                //var context = new DataClasses1DataContext(connectionString);
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", userId);
                pars.Add("@LookupID", id);
                for (int i = 0; i < lookups.LookupDepsProps.Count; i++)
                {
                    pars.Add(lookups.LookupDepsProps[i], lookups.LookupDeptsValues[i]);
                }

                //var data = context.usp_LookUp_Values(Convert.ToInt32(userId),Convert.ToInt32(id)).AsQueryable();
                var data = "";// callProcedure("[DD].[usp_LookUp_Values]", pars);
                return Ok(data);
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = userId
                });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("searchLot")]
        public IActionResult SearchTaxLot([FromQuery] string userId, [FromQuery] string editMode = "0", [FromQuery] string transId = "null", [FromQuery] string newEditTransId = "null", [FromQuery] string processed = "0",
                                                [FromQuery] double acctId = 0, [FromQuery] double subAcctId = 0, [FromQuery] double secId = 0, [FromQuery] DateTime? effectiveDate = null, bool explicitSearch = false)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult)
            {
                return (IActionResult)handlerHelper.ValidateSource(Request);
            }

            if (handlerHelper.ValidateToken(Request, userId) != string.Empty)
            {
                return Unauthorized(handlerHelper.ValidateToken(Request, userId));
            }

            try
            {
                //var context = new DataClasses1DataContext(connectionString);
                dataClasses1DataContext.Database.SetCommandTimeout(0);
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", userId);
                pars.Add("@Edit", editMode);

                if (transId != null && transId != "null" && !explicitSearch)
                {
                    pars.Add("@TransID", transId);
                }
                if (newEditTransId != null && newEditTransId != "null" && !explicitSearch)
                {
                    pars.Add("@NewEditTransID", newEditTransId);
                }

                if (explicitSearch)
                {
                    pars.Add("@AcctID", acctId);
                    pars.Add("@SubAcctID", subAcctId);
                    pars.Add("@Effectivedate", effectiveDate);
                    pars.Add("@SecID", secId);
                }
                pars.Add("@Processed", Convert.ToInt32(processed));
                var data = "";// callProcedure("[adm].[usp_TaxLot_Search]", pars);
                return Ok(new { lots = data, completed = true, message = "loading finished with " + data.Count() + " results" });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = userId
                });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("insertLot")]
        public IActionResult AssignOrInsertLot([FromBody] VmOperations ops, string userId)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult)
            {
                return (IActionResult)handlerHelper.ValidateSource(Request);
            }

            if (handlerHelper.ValidateToken(Request, userId) != string.Empty)
            {
                return Unauthorized(handlerHelper.ValidateToken(Request, userId));
            }

            try
            {

                //var context = new DataClasses1DataContext(connectionString);
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                var itemCount = 0;
                foreach (var lotItem in ops.lots)
                {
                    itemCount++;
                    try
                    {
                        pars.Clear();
                        pars.Add("@UserID", userId);

                        if (lotItem.TransID != null)
                        {
                            pars.Add("@TransID", lotItem.TransID);
                        }
                        if (lotItem.NewEditTransID != null)
                        {
                            pars.Add("@NewEditTransID", Convert.ToInt32(lotItem.NewEditTransID));
                        }


                        var editShares = Convert.ToDecimal(lotItem.editShares);
                        var signShares = Convert.ToDecimal(lotItem.SignShares);
                        var editLocalCost = Convert.ToDecimal(lotItem.editLocalCost);
                        var signCost = Convert.ToDecimal(lotItem.SignCost);
                        var localUnitCost = Convert.ToDecimal(lotItem.LocalUnitCost);
                        var taxLotMasterID = lotItem.TaxLotMasterID;

                        pars.Add("@TaxlotmasterID", Convert.ToInt32(taxLotMasterID));
                        pars.Add("@SpecLot_Assign_EditID", Convert.ToInt32(lotItem.SpecLot_Assign_EditID));

                        pars.Add("@AdjShares", lotItem.editShares);
                        pars.Add("@AdjLocalCost", lotItem.editLocalCost);
                        pars.Add("@AdjBaseCost", lotItem.editBaseCost);

                        if (lotItem.Stat != null)
                        {
                            pars.Add("@Stat", Convert.ToChar(lotItem.Stat));
                        }
                        else
                        {
                            pars.Add("@Stat", "A");
                        }

                        var data = "";// callProcedure("[adm].[usp_SpecLot_Assign_Insert]", pars);

                        if (itemCount == ops.lots.Count)
                        {
                            var d = dataClasses1DataContext.usp_SpecLot_Assign_Save(Convert.ToInt32(userId), Convert.ToInt32(lotItem.TransID), Convert.ToInt32(lotItem.NewEditTransID));
                        }


                    }
                    catch (Exception ex)
                    {
                        Helper.AddApiLogs(connectionString, new ApiLogObject
                        {
                            Method = MethodInfo.GetCurrentMethod().Name,
                            Exception = ex.Message,
                            Params = "web api",
                            UserID = userId
                        });
                        var exception = "Unknown Error Occurred";
                        return Ok(new { saved = false, error = true, message = exception });
                    }
                }

                return Ok(new { saved = true, error = false, message = "Lots were saved successfully." });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = userId
                });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("deleteLot")]
        public IActionResult DeleteSpecLots([FromQuery] string userId, [FromQuery] string transId, [FromQuery] string newEditTransId, string editClear, string assignClear, string specLotClear, string editUserID = "null")
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult)
            {
                return (IActionResult)handlerHelper.ValidateSource(Request);
            }

            if (handlerHelper.ValidateToken(Request, userId) != string.Empty)
            {
                return Unauthorized(handlerHelper.ValidateToken(Request, userId));
            }

            try
            {
                //var context = new DataClasses1DataContext(connectionString);
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();

                pars.Add("@UserID", userId);
                pars.Add("@TransID", transId);

                if (newEditTransId != "null")
                    pars.Add("@NewEditTransID", newEditTransId);

                pars.Add("@Edit_Clear", editClear);
                pars.Add("@Assign_Clear", assignClear);
                pars.Add("@SpecLot_Clear", specLotClear);

                if (editUserID != "null")
                {
                    pars.Add("@EditUserID", editUserID);
                }

                var data = "";// callProcedure("[adm].[usp_SpecLot_Assign_Delete]", pars);

                if (!string.IsNullOrEmpty(data) && !data.Equals("[]"))
                {

                    return Ok(new { userOverriden = false, deleted = false, error = true, message = data });
                }
                return Ok(new { userOverriden = userId != editUserID && editUserID != "null", deleted = true, error = false, message = "Lots were deleted successfully." });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = userId
                });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("saverpttmplate")]
        public IActionResult SaveRptTemplate([FromQuery] string UserID, [FromQuery] string rptTemplateName, [FromQuery] string rptTemplateNameDesc, [FromQuery] string showAddtionalFields,
                                                    [FromQuery] string selectedPwsRefid, [FromQuery] string selectedCurrencies, [FromQuery] string selectedUserGroup, [FromQuery] string selectedLowLevelPerf,
                                                    [FromQuery] string selectedModelId = null, [FromQuery] string selectedAssignPt = null, [FromQuery] string selectedAssignPtEnd = null,
                                                    [FromQuery] string selectedDrillDownInfo = null, [FromQuery] string selectedHeirRptInfo = null)
        {

            if (handlerHelper.ValidateSource(Request) is IActionResult)
            {
                return (IActionResult)handlerHelper.ValidateSource(Request);
            }

            if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
            {
                return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
            }

            try
            {
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@RptTemplateName", rptTemplateName);
                pars.Add("@RptTemplateNameDesc", rptTemplateNameDesc);
                pars.Add("@RptPortID", selectedPwsRefid);
                pars.Add("@CCYID", selectedCurrencies);
                pars.Add("@RptTemplateName", selectedUserGroup);
                pars.Add("@RptTemplateName", selectedLowLevelPerf);
                pars.Add("@ModelID", selectedModelId);
                pars.Add("@AssgnStartPt", selectedAssignPt);
                pars.Add("@AssgnEndPt", selectedAssignPtEnd);
                pars.Add("@DrillDown", selectedDrillDownInfo);
                pars.Add("@HierRpt", selectedHeirRptInfo);


                var data = "";// callProcedure("[adm].[usp_RptTemplateName_Insert]", pars);

                return Ok(new { saved = true, error = false, message = "Template has been saved successfully." });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("searchnewaccts")]
        public IActionResult SearchNewAccounts(string UserID, string acctName, string acctCode, string acctNum, string acctNickName,
                                                        string userGroup, string loadSource, string currency, string reliefMethod,
                                                        string entityCode, string portfolioCode, DateTime? loadStartDate, DateTime? LoadEndDate,
                                                        DateTime? updateStartDate, DateTime? updateEndDate)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult)
            {
                return (IActionResult)handlerHelper.ValidateSource(Request);
            }


            if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
            {
                return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
            }

            try
            {
                //var _contextPWS = new DataClasses1DataContext(connectionString);
                dataClasses1DataContext.Database.SetCommandTimeout(0);
                dataClasses1DataContext.Database.ExecuteSqlRaw("delete from [PWS].[dbo].[New_Accounts_tmp] where UserId=" + UserID);

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);

                if (!string.IsNullOrEmpty(acctName))
                {
                    pars.Add("@AcctName", acctName);
                }

                if (!string.IsNullOrEmpty(acctCode))
                {
                    pars.Add("@AcctCode", acctCode);
                }

                if (!string.IsNullOrEmpty(acctNum))
                {
                    pars.Add("@AcctNum", acctNum);
                }

                if (!string.IsNullOrEmpty(acctNickName))
                {
                    pars.Add("@AcctNickNamePrimary", acctNickName);
                }

                if (!string.IsNullOrEmpty(userGroup))
                {
                    pars.Add("@UserGroupID", userGroup);
                }

                if (!string.IsNullOrEmpty(loadSource))
                {
                    pars.Add("@LoadSourceID", loadSource);
                }

                if (!string.IsNullOrEmpty(currency))
                {
                    pars.Add("@BaseCurr", currency);
                }

                if (!string.IsNullOrEmpty(reliefMethod))
                {
                    pars.Add("@TaxLotReliefMethodID", reliefMethod);
                }

                if (!string.IsNullOrEmpty(entityCode))
                {
                    pars.Add("@EntityCode", entityCode);
                }

                if (!string.IsNullOrEmpty(portfolioCode))
                {
                    pars.Add("@PortfolioCode", portfolioCode);
                }

                if (loadStartDate != null)
                {
                    pars.Add("@LoadDateTimeStart", loadStartDate);
                }

                if (LoadEndDate != null)
                {
                    pars.Add("@LoadDateTimeEnd", LoadEndDate);
                }

                if (updateStartDate != null)
                {
                    pars.Add("@UpdateDateTimeStart", updateStartDate);
                }

                if (updateEndDate != null)
                {
                    pars.Add("@UpdateDateTimeEnd", updateEndDate);
                }
                var data = ""; //callProcedure("[adm].[usp_New_Accounts_Search]", pars);
                return Ok(new { data = data, success = true, error = false, message = "New Accounts Search is complete" });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("pwsrefids")]
        public IActionResult GetPwsRefIds([FromQuery] string UserID, [FromQuery] string RptStructureIDs, [FromQuery] string Edit = "0")
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult)
            {
                return (IActionResult)handlerHelper.ValidateSource(Request);
            }

            if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
            {
                return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
            }

            try
            {
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@RptStructureIDs", RptStructureIDs);
                pars.Add("@Edit", Edit);
                var data = "";// callProcedure("DD.usp_PWSRefID", pars);
                return Ok(new { data = data, error = false, success = true });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("usergroups")]
        public IActionResult UserGroups([FromQuery] string UserID, [FromQuery] string Edit = "0")
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult)
            {
                return (IActionResult)handlerHelper.ValidateSource(Request);
            }

            if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
            {
                return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
            }

            try
            {
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@Edit", Edit);
                var data = "";// callProcedure("DD.usp_UserGroupID", pars);
                return Ok(new { data = data, error = false, success = true });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]

        [Route("currency")]
        public IActionResult Currency([FromQuery] string UserID)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult)
            {
                return (IActionResult)handlerHelper.ValidateSource(Request);
            }

            if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
            {
                return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
            }

            try
            {
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();

                var data = "";// callProcedure("DD.usp_CurrencyID", pars);
                return Ok(new { data = data, error = false, success = true });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("loadsources")]
        public IActionResult LoadSources([FromQuery] string UserID)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult)
            {
                return (IActionResult)handlerHelper.ValidateSource(Request);
            }

            if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
            {
                return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
            }
            try
            {
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();

                var data = ""; //callProcedure("DD.usp_LoadSourceID", pars);
                return Ok(new { data = data, error = false, success = true });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("dataproviders")]
        public IActionResult DataProviders(string UserID, string Matching, string Match, int Top)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult)
            {
                return (IActionResult)handlerHelper.ValidateSource(Request);
            }

            if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
            {
                return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
            }
            try
            {
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();

                if (Matching != "0")
                {
                    pars.Add("@Match", Match);
                    pars.Add("@Top", Top);
                }


                var data = Matching == "0" ? callProcedure("DD.usp_DataProviderID", pars) : callProcedure("DD.usp_DataProviderID_Match", pars);
                return Ok(new { data = data, error = false, success = true });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });

                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("relifemethods")]
        public IActionResult ReliefMethods([FromQuery] string UserID)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult)
                {
                    return (IActionResult)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
                {
                    return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();

                var data = "";// callProcedure("DD.usp_Relief_Method", pars);
                return Ok(new { data = data, error = false, success = true });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });

                return Ok(new { error = true, success = false });
            }
        }

        [HttpGet]
        [Route("accounts")]
        public IActionResult Accounts([FromQuery] string UserID, [FromQuery] string Matching = null, [FromQuery] string Match = null, [FromQuery] string Top = null)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult)
            {
                return (IActionResult)handlerHelper.ValidateSource(Request);
            }

            if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
            {
                return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
            }

            try
            {


                Matching = Matching == "undefined" || Matching == null ? "0" : Matching;
                Match = Match == "undefined" || Match == null ? "0" : Match;
                Top = Top == "undefined" || Top == null ? "0" : Top;

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                if (Matching == "1")
                {
                    pars.Add("@Matching", Matching);
                    pars.Add("@Match", Match);
                    pars.Add("@Top", Top);
                }


                var data = Matching == "1" ? callProcedure("DD.[usp_AccountID_Match]", pars) : callProcedure("DD.[usp_AccountID]", pars);
                return Ok(new { data = data, error = false, success = true });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });

                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("modelids")]
        public IActionResult ModelIds([FromQuery] string UserID)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult)
            {
                return (IActionResult)handlerHelper.ValidateSource(Request);
            }

            if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
            {
                return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
            }

            try
            {

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                var data = "";// callProcedure("DD.usp_ModelID", pars);
                return Ok(new { data = data, error = false, success = true });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });

                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }


        [HttpGet]
        [Route("assignpt")]
        public IActionResult AssignPt([FromQuery] string UserID)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult)
            {
                return (IActionResult)handlerHelper.ValidateSource(Request);
            }

            if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
            {
                return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
            }

            try
            {
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();

                var data = "";// callProcedure("DD.usp_AssignPt", pars);
                return Ok(new { data = data, error = false, success = true });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });

                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }


        [HttpPost]
        [Route("trialrpttemplatenamesedit")]
        public IActionResult TrialRptTemplateNamesEdit(
            string UserID,
            string Trial_RptTemplateIDs,
            string value
            )
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult)
            {
                return (IActionResult)handlerHelper.ValidateSource(Request);
            }

            if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
            {
                return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
            }

            try
            {
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                JArray a = JArray.Parse(value);

                pars.Add("@UserID", UserID);
                pars.Add("@Trial_RptTemplateIDs", Trial_RptTemplateIDs);
                pars.Add($"@{a.First}", $"{a.Last}");


                var data = "";// callProcedure("adm.usp_Trial_RptTemplateNames_Edit", pars);

                return Ok(new { data = data, error = false, success = true });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });

                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }



        [HttpPost]
        [Route("trialrpttemplatenamessave")]
        public IActionResult TrialRptTemplateNamesSave(
            string UserID,
            string Trial_RptTemplateIDs
            )
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult)
            {
                return (IActionResult)handlerHelper.ValidateSource(Request);
            }

            if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
            {
                return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
            }

            try
            {
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@Trial_RptTemplateIDs", Trial_RptTemplateIDs);

                var data = "";// callProcedure("[adm].[usp_Trial_RptTemplateNames_Save]", pars);

                return Ok(new { data = data, error = false, success = true });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });

                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("searchsecuritymapping")]
        public IActionResult SearchSecurityMapping([FromQuery] string UserID, string loadSource, string security, string dpSecId = null)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult)
            {
                return (IActionResult)handlerHelper.ValidateSource(Request);
            }

            if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
            {
                return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
            }

            if (dpSecId == "undefined")
            {
                dpSecId = null;
            }

            try
            {

                string loadSourceId = loadSource.Contains("-") ? loadSource.Split('-')[0] : null;
                string secId = security.Contains("-") ? security.Split('-')[0] : null;

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();

                pars.Add("@LoadSourceID", loadSourceId);
                pars.Add("@SecID", secId);
                pars.Add("@DPSecurityID", dpSecId);
                var data = "";// callProcedure("adm.usp_MappingSecurity_Search", pars);

                return Ok(new { data = data, error = false, success = true });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });

                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("DeleteSecurityMapping")]
        public IActionResult DeleteSecurityMapping([FromQuery] string SecurityMappingID)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult)
                {
                    return (IActionResult)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return Unauthorized(handlerHelper.ValidateToken(Request));
                }

                //var _context = new DataClasses1DataContext(connectionString);
                dataClasses1DataContext.Database.SetCommandTimeout(3600);
                var id = Convert.ToInt32(SecurityMappingID);
                var selectedMapping = dataClasses1DataContext.MappingSecurities.Where(f => f.SecurityMappingID == id);

                if (selectedMapping.Any())
                {
                    dataClasses1DataContext.MappingSecurities.RemoveRange(selectedMapping);
                    dataClasses1DataContext.SaveChanges();

                }

                return Ok(new { error = false, success = true, message = "Mapping was deleted successfully", type = "success", color = "green" });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api"
                });

                var exception = "Unknown Error Occurred";
                return Ok(new { ex = ex, error = true, success = true, message = exception, type = "error", color = "red" });
            }
        }

        [HttpGet]
        [Route("acctmatchwithDPLoadSource")]
        public IActionResult GetAccountsMatchingWithDPAndLoadSource(string UserID, string Match)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult)
            {
                return (IActionResult)handlerHelper.ValidateSource(Request);
            }

            if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
            {
                return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
            }

            try
            {
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@Match", Match);

                var data = "";// callProcedure("[DD].[usp_MappingAccountID_Match]", pars);

                return Ok(new { data = data, error = false, success = true });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });

                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("insertsecuritymapping")]
        public IActionResult InserSecurityMapping(string DPID,
                                                        string LoadSourceID,
                                                        string SecID,
                                                        string DPSecurityID,
                                                        string DPSecurityName,
                                                        string MappedUsing,
                                                        string AcctID,
                                                        string BlockRecon,
                                                        string AdjustShares,
                                                        DateTime? StartDate = null,
                                                        DateTime? EndDate = null
                                                       )
        {

            if (handlerHelper.ValidateSource(Request) is IActionResult)
            {
                return (IActionResult)handlerHelper.ValidateSource(Request);
            }


            if (handlerHelper.ValidateToken(Request) != string.Empty)
            {
                return Unauthorized(handlerHelper.ValidateToken(Request));
            }

            if (AcctID == "null" || AcctID == "undefined")
            {
                AcctID = null;
            }

            try
            {
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@DPID", DPID.Split('-')[0]);
                pars.Add("@LoadSourceID", LoadSourceID.Split('-')[0]);
                pars.Add("@SecID", SecID.Split('-')[0]);
                pars.Add("@DPSecurityID", DPSecurityID);
                pars.Add("@DPSecurityName", DPSecurityName);
                pars.Add("@MappedUsing", MappedUsing);
                if (AcctID != null)
                {
                    pars.Add("@AcctID", AcctID.Split('-')[0]);
                }

                pars.Add("@BlockRecon", BlockRecon);
                pars.Add("@AdjustShares", AdjustShares);
                if (StartDate != null)
                {
                    pars.Add("@StartDate", StartDate);
                }

                if (EndDate != null)
                {
                    pars.Add("@EndDate", EndDate);
                }




                var data = "";// callProcedure("adm.usp_MappingSecurity_Insert", pars);

                return Ok(new { data = data, error = false, success = true });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api"
                });

                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }



        [HttpPost]
        [Route("transtrialtestsearch")]
        public IActionResult TransTrialTestSearch(string UserID = null,
                                             string AcctID_SubAcctID = null,
                                             string SecID = null,
                                             string StartDate = null,
                                             string EndDate = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult)
                {
                    return (IActionResult)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
                {
                    return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@AcctID_SubAcctID", AcctID_SubAcctID);
                pars.Add("@StartDate ", StartDate);
                pars.Add("@EndDate", EndDate);

                if (!Helper.IgnoreAsNullParameter(SecID))
                    pars.Add("@SecID", SecID);

                var data = "";// callProcedure("adm.usp_Trans_Trial_Test_Search", pars);

                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });

                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }

        }

        [HttpPost]
        [Route("deleteextrackedasset")]
        public IActionResult DeleteExTrackedAsset(string UserID = null, string ExTrackedAssetsID = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult)
                {
                    return (IActionResult)handlerHelper.ValidateSource(Request);
                }


                if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
                {
                    return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
                }
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@ExTrackedAssetsID", Convert.ToInt32(ExTrackedAssetsID));

                var data = callProcedure("adm.usp_ExTrackedAsset_Delete", pars);

                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });

                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }

        }

        [HttpPost]
        [Route("insertextrackedasset")]
        public IActionResult ExtrackedAssetInsert(string UserID = null,
                                             string CashAcctID_SubAcctID = null,
                                             string CashAcctSecID = null,
                                             string AltAcctID_SubAcctID = null,
                                             string AltSecID = null,
                                             string EOD_StartDate = null,
                                             string EOD_EndDate = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult)
                {
                    return (IActionResult)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
                {
                    return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
                }
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@CashAcctID_SubAcctID", CashAcctID_SubAcctID);
                pars.Add("@CashAcctSecID", CashAcctSecID);
                pars.Add("@AltAcctID_SubAcctID", AltAcctID_SubAcctID);
                pars.Add("@AltSecID", AltSecID);
                pars.Add("@EOD_StartDate", EOD_StartDate);
                pars.Add("@EOD_EndDate", EOD_EndDate);

                var data = ""; //callProcedure("adm.usp_ExTrackedAsset_Insert", pars);

                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });

                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }

        }

        [HttpPost]
        [Route("getfxrate")]
        public IActionResult GetFxRate(string PrincCCYID = null,
                                             string SettleCCYID = null,
                                             string Effectivedate = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult)
                {
                    return (IActionResult)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return Unauthorized(handlerHelper.ValidateToken(Request));
                }
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@PrincCCYID", PrincCCYID);
                pars.Add("@Effectivedate", Effectivedate);
                pars.Add("@SettleCCYID", SettleCCYID);

                var data = "";// callProcedure("adm.usp_GetFXRate", pars);

                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api"
                });

                var exception = "Unknown Error Occurred";
                return StatusCode(500, new { error = true, exception = exception });
            }

        }


        [HttpPost]
        [Route("extrackedassetsearch")]
        public IActionResult ExTrackedAssetSearch(string UserID = null,
                                             string CashAcctID = null,
                                             string CashSubAcctID = null,
                                             string CashAcctSecID = null,
                                             string AltAcctID = null,
                                             string AltSubAcctID = null,
                                             string AltSecID = null,
                                             string EOD_StartDateStart = null,
                                             string EOD_StartDateEnd = null,
                                             string EOD_EndDateStart = null,
                                             string EOD_EndDateEnd = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult)
                {
                    return (IActionResult)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
                {
                    return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
                }
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@EOD_StartDateStart", EOD_StartDateStart);
                pars.Add("@EOD_StartDateEnd", EOD_StartDateEnd);
                pars.Add("@EOD_EndDateStart", EOD_EndDateStart);
                pars.Add("@EOD_EndDateEnd", EOD_EndDateEnd);

                if (!Helper.IgnoreAsNullParameter(CashAcctID))
                    pars.Add("@CashAcctID", CashAcctID);

                if (!Helper.IgnoreAsNullParameter(CashAcctSecID))
                    pars.Add("@CashAcctSecID", CashAcctSecID);

                if (!Helper.IgnoreAsNullParameter(CashSubAcctID))
                    pars.Add("@CashSubAcctID", CashSubAcctID);

                if (!Helper.IgnoreAsNullParameter(AltAcctID))
                    pars.Add("@AltAcctID", AltAcctID);

                if (!Helper.IgnoreAsNullParameter(AltSecID))
                    pars.Add("@AltSecID", AltSecID);

                if (!Helper.IgnoreAsNullParameter(AltSubAcctID))
                    pars.Add("@AltSubAcctID", AltSubAcctID);


                var data = "";// callProcedure("adm.usp_ExTrackedAsset_Search", pars);

                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });

                var exception = "Unknown Error Occurred";
                return StatusCode(500, new { error = true, exception = exception });
            }

        }

        [HttpPost]
        [Route("transtrialtestinsert")]
        public IActionResult TransTrialTestInsert(string UserID = null,
                                             string LinkedAcctID_SubAcctID = null,
                                             string AcctID_SubAcctID = null,
                                             string SecID = null,
                                             string StartDate = null,
                                             string EndDate = null,
                                             string Effectivedate = null,
                                             string TcodeID = null,
                                             string Comment = null,
                                             string TradeDate = null,
                                             string SettleDate = null,
                                             string SecurityTradeCCYID = null,
                                             string BaseCCYID = null,
                                             string TradeFXBase = null,
                                             string AcctBaseAmt = null,
                                             string ReliefMethodID = null,
                                             string ProcessOrd = null,
                                             string Final = null,
                                             string SettleCCYID = null,
                                             string SettlementAmount = null,
                                             string NetLocal = null,
                                             string TradeFXSettle = null,
                                             string SecEnt = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult)
                {
                    return (IActionResult)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
                {
                    return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@AcctID_SubAcctID", AcctID_SubAcctID);
                pars.Add("@SecID", SecID);
                pars.Add("@StartDate ", StartDate);
                pars.Add("@EndDate", EndDate);
                pars.Add("@Effectivedate", Effectivedate);
                pars.Add("@TcodeID", TcodeID);
                pars.Add("@Comment ", Comment);
                pars.Add("@TradeDate", TradeDate);
                pars.Add("@SettleDate", SettleDate);
                pars.Add("@SecurityTradeCCYID ", SecurityTradeCCYID);
                pars.Add("@BaseCCYID", BaseCCYID);
                pars.Add("@TradeFXBase", TradeFXBase);
                pars.Add("@AcctBaseAmt", AcctBaseAmt);
                pars.Add("@ReliefMethodID ", ReliefMethodID);
                pars.Add("@ProcessOrd", ProcessOrd);
                pars.Add("@Final", Final);
                pars.Add("@SettleCCYID", SettleCCYID);
                // pars.Add("@SettlementAmount", SettlementAmount);
                pars.Add("@NetLocal", NetLocal);
                pars.Add("@TradeFXSettle", TradeFXSettle);

                if (!Helper.IgnoreAsNullParameter(SecEnt))
                    pars.Add("@SecEnt", SecEnt);

                if (!Helper.IgnoreAsNullParameter(LinkedAcctID_SubAcctID))
                    pars.Add("@LinkedAcctID_SubAcctID", LinkedAcctID_SubAcctID);


                var data = "";// callProcedure("adm.usp_Trans_Trial_Test_Insert", pars);

                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });

                var exception = "Unknown Error Occurred";
                return StatusCode(500, new { error = true, exception = exception });
            }

        }


        [HttpPost]
        [Route("testopsdate")]
        public IActionResult TransTrialTestInsert(string Date = null)
        {
            try
            {
                DateTime currentTime;
                var check = DateTime.TryParse(Date == null ? DateTime.Now.ToString() : Date, out currentTime);

                return Ok(new { error = false, data = currentTime, utcDate = currentTime.ToUniversalTime() });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                });

                var exception = "Unknown Error Occurred";
                return StatusCode(500, new { error = true, exception = exception });
            }

        }


        [HttpPost]
        [Route("transtrialtestedit")]
        public IActionResult TransTrialTestEdit(string UserID,
                                     string LinkedAcctID_SubAcctID,
                                     string Shrs = null,
                                     string Trans_TrialInsert_TestID = null,
                                     string StartDate = null,
                                     string EndDate = null,
                                     string Effectivedate = null,
                                     string TcodeID = null,
                                     string Comment = null,
                                     string TradeDate = null,
                                     string SettleDate = null,
                                     string SecurityTradeCCYID = null,
                                     string BaseCCYID = null,
                                     string TradeFXBase = null,
                                     string AcctBaseAmt = null,
                                     string ReliefMethodID = null,
                                     string ProcessOrd = null,
                                     string Final = null,
                                     string SettleCCYID = null,
                                     string SettlementAmount = null,
                                     string NetLocal = null,
                                     string TradeFXSettle = null,
                                     string SecEnt = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult)
                {
                    return (IActionResult)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
                {
                    return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@Trans_TrialInsert_TestID", Trans_TrialInsert_TestID);
                // pars.Add("@AcctID_SubAcctID", AcctID_SubAcctID);
                // pars.Add("@SecID", SecID);
                pars.Add("@StartDate ", StartDate);
                pars.Add("@EndDate", EndDate);
                pars.Add("@Effectivedate", Effectivedate);
                pars.Add("@TcodeID", TcodeID);
                pars.Add("@Comment ", Comment);
                pars.Add("@TradeDate", TradeDate);
                pars.Add("@SettleDate", SettleDate);
                pars.Add("@SecurityTradeCCYID ", SecurityTradeCCYID);
                pars.Add("@BaseCCYID", BaseCCYID);
                pars.Add("@TradeFXBase", TradeFXBase);
                pars.Add("@AcctBaseAmt", AcctBaseAmt);
                pars.Add("@ReliefMethodID ", ReliefMethodID);
                pars.Add("@ProcessOrd", ProcessOrd);
                pars.Add("@Final", Final);
                pars.Add("@SettleCCYID", SettleCCYID);
                // pars.Add("@SettlementAmount", SettlementAmount);
                pars.Add("@NetLocal", NetLocal);
                pars.Add("@TradeFXSettle", TradeFXSettle);

                if (!Helper.IgnoreAsNullParameter(LinkedAcctID_SubAcctID))
                    pars.Add("@LinkedAcctID_SubAcctID", LinkedAcctID_SubAcctID);

                if (!Helper.IgnoreAsNullParameter(SecEnt))
                    pars.Add("@SecEnt", SecEnt);

                if (!Helper.IgnoreAsNullParameter(Shrs))
                    pars.Add("@Shrs", Shrs);

                var data = "";// callProcedure("adm.usp_Trans_Trial_Test_Edit", pars);

                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });

                var exception = "Unknown Error Occurred";
                return StatusCode(500, new { error = true, exception = exception });
            }

        }


        [HttpPost]
        [Route("transtrialtestsave")]
        public IActionResult TransTrialTestSave(string UserID = null, string Process = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult)
                {
                    return (IActionResult)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
                {
                    return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
                }
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);

                if (!Helper.IgnoreAsNullParameter(Process))
                    pars.Add("@Process", Process);

                var data = ""; //callProcedure("adm.usp_Trans_Trial_Test_Save", pars);
                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });

                var exception = "Unknown Error Occurred";
                return StatusCode(500, new { error = true, exception = exception });
            }

        }

        [HttpPost]
        [Route("TransactionsEdit")]
        public IActionResult TransEdit(
            string UserID,
            string Trial_TransIDs,
            string value
        )
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult)
                {
                    return (IActionResult)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
                {
                    return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
                }
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                JArray a = JArray.Parse(value);
                pars.Add("@UserID", UserID);
                pars.Add("@Trial_TransIDs", Trial_TransIDs);

                if (!Helper.IgnoreAsNullParameter($"{a.Last}"))
                    pars.Add($"@{a.First}", $"{a.Last}");


                var data = "";// callProcedure("adm.usp_Trans_Edit", pars);
                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });

                var exception = "Unknown Error Occurred";
                return StatusCode(500, new { error = true, exception = exception });
            }

        }


        [HttpPost]
        [Route("transtrialtestdelete")]
        public IActionResult Trans_Trial_Test_Delete(string UserID = null,
                                                      string Trans_TrialInsert_TestID = null,
                                                      string AcctID_SubAcctID = null,
                                                      string SecID = null,
                                                      string StartDate = null,
                                                      string EndDate = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult)
                {
                    return (IActionResult)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
                {
                    return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
                }
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@Trans_TrialInsert_TestID", Trans_TrialInsert_TestID);
                pars.Add("@AcctID_SubAcctID", AcctID_SubAcctID);
                pars.Add("@SecID", SecID);
                pars.Add("@StartDate ", StartDate);
                pars.Add("@EndDate", EndDate);

                var data = ""; //callProcedure("adm.usp_Trans_Trial_Test_Delete", pars);
                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });

                var exception = "Unknown Error Occurred";
                return StatusCode(500, new { error = true, exception = exception });
            }

        }


        [HttpPost]
        [Route("transtrialtestperf")]
        public IActionResult Trans_Trial_Test_Perf(string UserID = null,
                                                      string AcctID_SubAcctID = null,
                                                      string SecID = null,
                                                      string StartDate = null,
                                                      string EndDate = null,
                                                      string CumReturn = null,
                                                      string Gross = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult)
                {
                    return (IActionResult)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
                {
                    return Unauthorized(handlerHelper.ValidateToken(Request, UserID));
                }
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@AcctID_SubAcctID", AcctID_SubAcctID);
                pars.Add("@SecID", SecID);
                pars.Add("@StartDate ", StartDate);
                pars.Add("@EndDate", EndDate);
                pars.Add("@CumReturn ", CumReturn);
                pars.Add("@Gross", Gross);


                var data = "";// callProcedure("adm.usp_Trans_Trial_Test_Perf", pars);
                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Params = "web api",
                    UserID = UserID
                });

                var exception = "Unknown Error Occurred";
                return StatusCode(500, new { error = true, exception = exception });
            }

        }



        public String callProcedure(String procedureName, Dictionary<string, dynamic> pars)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(procedureName, conn))
                    {
                        cmd.CommandTimeout = 5000;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        SqlParameter returnValue = new SqlParameter();
                        returnValue.Direction = ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(returnValue);

                        // Params
                        foreach (KeyValuePair<string, dynamic> kvp in pars)
                        {
                            cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                        }

                        conn.Open();
                        DataSet ds = new DataSet();

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(ds);
                        }
                        //SqlDataReader reader = cmd.ExecuteReader();
                        //while (reader.Read())
                        //{

                        //}

                        //System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                        //serializer.MaxJsonLength = Int32.MaxValue;


                        List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                        Dictionary<string, object> row;

                        foreach (DataTable table in ds.Tables)
                        {
                            foreach (DataRow dr in table.Rows)
                            {
                                row = new Dictionary<string, object>();
                                foreach (DataColumn col in table.Columns)
                                {
                                    row.Add(col.ColumnName, dr[col]);
                                    //if(col.DataType == typeof(DateTime))
                                    //row.Add(col.ColumnName, ((DateTime)dr[col]).ToLocalTime());
                                    //else
                                    // row.Add(col.ColumnName, (dr[col]));
                                }
                                rows.Add(row);
                            }
                        }
                        conn.Close();
                        // return serializer.Serialize(rows);
                        string procResultAsString = JsonConvert.SerializeObject(rows, Formatting.Indented, new JsonSerializerSettings
                        {
                            DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
                        });
                        return procResultAsString;
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject
                {
                    Method = MethodInfo.GetCurrentMethod().Name,
                    Exception = ex.Message,
                    Query = procedureName,
                    Params = "web api"
                });

                return string.Empty;
            }
        }
    }
}

