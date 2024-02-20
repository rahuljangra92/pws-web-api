using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PWSWebApi.Domains;
using PWSWebApi.Domains.DBContext;
using PWSWebApi.handlers;
using PWSWebApi.ViewModel.Models.Reports;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace PWSWebApi.Controllers
{
    public class Reports
    {
        public string UserID { get; set; }

        public string UserGroupID { get; set; }

        public string json { get; set; }

        public string Action { get; set; }
    }

    [Route("reports")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly string connectionString;
        private readonly string pwsRectConnectionString;
        public Helper handlerHelper;


        public ReportsController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("PWSProd");
            pwsRectConnectionString = configuration.GetConnectionString("PWSRec");
            handlerHelper = new Helper(configuration);
        }

        [HttpGet]
        [Route("ReportParamMonitorPDF")]
        public HttpResponseMessage ReportParamMonitorPDF(int userId)
        {
            HttpResponseMessage response;
            object responseData;
            string jsonResponse;
            try
            {
                if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
                {
                    return (HttpResponseMessage)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent(handlerHelper.ValidateToken(Request))
                    };
                }

                var options = new DbContextOptionsBuilder<DataClasses1DataContext>().UseSqlServer(connectionString).Options;
                var pwsContext = new DataClasses1DataContext(options);
                var reportMonitorPdf = pwsContext.ReportParamMonitorPDFs; //.Where(f => f.UserID == userId);
                var reportBook = pwsContext.ReportBooks; //.Where(f => f.UserID == userId);
                responseData = new { success = true, reportMonitorPdf = reportMonitorPdf, reportBook = reportBook };
                jsonResponse = JsonConvert.SerializeObject(responseData);

                response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse)
                };
                return response;

            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api" });
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = ex.Message, success = false }))
                };
            }
        }

        [HttpGet]
        [Route("userpreferences2")]
        public HttpResponseMessage GetUserPrefences2(int userId, string userPrefTypeName = null, string rptTemplateNameID = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
                {
                    return (HttpResponseMessage)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, userId.ToString()) != string.Empty)
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent(handlerHelper.ValidateToken(Request, userId.ToString()))
                    };
                }

                var options = new DbContextOptionsBuilder<DataClasses1DataContext>().UseSqlServer(connectionString).Options;
                var pwsContext = new DataClasses1DataContext(options);


                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", userId);
                pars.Add("@RptTemplateNameID", rptTemplateNameID);
                pars.Add("@UserPrefTypeName", userPrefTypeName);
                //pars.Add("@DrillDown", selectedDrillDownInfo == "-1" ? "1" : selectedDrillDownInfo.ToString());
                var data = callProcedure("dbo.usp_UserPreferences_2", pars);
                var responseData = new { success = true, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse)
                };
                return response;
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = userId.ToString() });

                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = ex.Message, success = false }))
                };
            }
        }

        [HttpPost]
        [Route("getmodelsearch")]
        public HttpResponseMessage GetModelSearch(string UserID, string ModelID = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
                {
                    return (HttpResponseMessage)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent(handlerHelper.ValidateToken(Request, UserID))
                    };
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@ModelID ", ModelID);
                var data = Helper.callProcedure("adm.usp_Model_Search", pars);
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = false, data = data }))
                };
                return response;
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception, success = false }))
                };
            }
        }

        [HttpPost]
        [Route("setattribtypeinsert")]
        public HttpResponseMessage SetAttribTypeInsert(string UserID, string AttribTypeName = null, string UserGroupID = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
                {
                    return (HttpResponseMessage)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent(handlerHelper.ValidateToken(Request, UserID))
                    };
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@AttribTypeName ", AttribTypeName);
                pars.Add("@UserGroupID ", UserGroupID);
                var data = Helper.callProcedure("adm.usp_AttribType_Insert", pars);
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = false, data = data }))
                };
                return response;
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception, success = false }))
                };
            }
        }

        [HttpPost]
        [Route("rptmodeldesccheck")]
        public HttpResponseMessage RptModelDescCheck(string UserGroupID, string RptModelDesc)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
                {
                    return (HttpResponseMessage)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent(handlerHelper.ValidateToken(Request))
                    };
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserGroupID", UserGroupID);
                pars.Add("@RptModelDesc ", RptModelDesc);
                var data = Helper.callProcedure("adm.usp_RptModelDesc_Check", pars);
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = false, data = data }))
                };
                return response;
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api" });
                var exception = "Unknown Error Occurred";
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception, success = false }))
                };
            }
        }

        [HttpPost]
        [Route("rptmodelview")]
        public HttpResponseMessage RptModelView(string UserID, string ModelID)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
                {
                    return (HttpResponseMessage)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent(handlerHelper.ValidateToken(Request, UserID))
                    };
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@ModelID ", ModelID);
                var data = Helper.callProcedure("[dbo].[usp_RptModel_View]", pars);
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = false, data = data }))
                };
                return response;
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception, success = false }))
                };
            }
        }

        [HttpPost]
        [Route("setattribnameinsert")]
        public HttpResponseMessage SetAttribNameInsert(string UserID, string AttribName = null, string UserGroupID = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
                {
                    return (HttpResponseMessage)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent(handlerHelper.ValidateToken(Request, UserID))
                    };
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@AttribName ", AttribName);
                pars.Add("@UserGroupID ", UserGroupID);
                var data = Helper.callProcedure("adm.usp_AttribName_Insert", pars);
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = false, data = data }))
                };
                return response;
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception, success = false }))
                };
            }
        }

        [HttpPost]
        [Route("rptmodelinsert")]
        public HttpResponseMessage CreateRptModelInsert([FromBody] Reports body)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
                {
                    return (HttpResponseMessage)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, body.UserID) != string.Empty)
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent(handlerHelper.ValidateToken(Request, body.UserID))
                    };
                }

                var options = new DbContextOptionsBuilder<DataClasses1DataContext>().UseSqlServer(connectionString).Options;
                var pwsContext = new DataClasses1DataContext(options);
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", body.UserID);
                pars.Add("@UserGroupID", body.UserGroupID);
                pars.Add("@json", body.json);
                pars.Add("@Action", body.Action);
                var data = callProcedure("adm.usp_RptModel_Insert", pars);
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { success = true, error = false, data = data }))
                };
                return response;
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = body.UserID });
                var exception = "Unknown Error Occurred";
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception, success = false }))
                };
            }
        }

        [HttpPost]
        [Route("savereportschedule")]
        public HttpResponseMessage SaveReportSchedule([FromBody] ReportSchedule reportSchedule)
        {
            if (handlerHelper.ValidateToken(Request, reportSchedule.UserID.ToString()) != string.Empty)
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent(handlerHelper.ValidateToken(Request, reportSchedule.UserID.ToString()))
                };
            }

            try
            {
                var options = new DbContextOptionsBuilder<DataClasses1DataContext>().UseSqlServer(connectionString).Options;
                var pwsContext = new DataClasses1DataContext(options);
                var reportBook = new ReportBook();
                //first insert or update in report book
                if (reportSchedule.BookID != "0") // update
                {
                    reportBook = pwsContext.ReportBooks.Where(f => f.ReportBookID == Convert.ToUInt32(reportSchedule.BookID)).FirstOrDefault();
                    reportBook.UpdateDateTime = DateTime.Now;
                }
                else // new entry
                {
                    reportBook.CreateDateTime = DateTime.Now;
                    reportBook.BusinessDaysAfter = Convert.ToInt32(reportSchedule.WaitForBusinessDays);
                    reportBook.NextEndDate = Convert.ToDateTime(reportSchedule.BookEndDate);
                    reportBook.PeriodicityID = Convert.ToInt32(reportSchedule.periodicityID);
                    reportBook.ReportBookName1 = reportSchedule.BookName;
                    reportBook.Title1 = reportSchedule.BookTitle;
                    reportBook.UserID = reportSchedule.UserID;
                    reportBook.UpdateDateTime = DateTime.Now;
                    reportBook.LogoID = Convert.ToInt32(reportSchedule.LogoID);
                    pwsContext.ReportBooks.Add(reportBook);
                    pwsContext.SaveChanges();
                }

                if (reportSchedule.BookID == "0")
                {
                    reportBook = pwsContext.ReportBooks.Where(f => f.UserID == reportSchedule.UserID).OrderByDescending(f => f.UpdateDateTime).FirstOrDefault();
                }

                var newRecord = new ReportParamMonitorPDF();
                newRecord.UserID = Convert.ToInt32(reportBook.UserID);
                newRecord.ReportBookID = reportBook.ReportBookID;
                newRecord.LogoID = Convert.ToString(reportBook.LogoID);
                newRecord.UserPrefTypeID = reportSchedule.UserPrefTypeID;
                newRecord.DelimParams = reportSchedule.DelimParams;
                newRecord.RptTemplateNameID = Convert.ToInt32(reportSchedule.selectedGroupingStyle);
                newRecord.RptTemplateName = reportSchedule.selectedGroupingStyleName;
                newRecord.StartDate = reportSchedule.StartDate;
                newRecord.EndDate = reportSchedule.EndDate;
                newRecord.ReportName = reportSchedule.UserPrefTypeName;
                newRecord.CreateDateTime = DateTime.Now;
                pwsContext.ReportParamMonitorPDFs.Add(newRecord);
                pwsContext.SaveChanges();
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { success = true }))
                };
                return response;
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = reportSchedule.UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception, success = false }))
                };

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
                                }

                                rows.Add(row);
                            }
                        }

                        conn.Close();
                        string procResultAsString = JsonConvert.SerializeObject(rows, Formatting.Indented, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
                        return procResultAsString;
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Query = procedureName, Params = "web api" });
                return string.Empty;
            }
        }
    }

}