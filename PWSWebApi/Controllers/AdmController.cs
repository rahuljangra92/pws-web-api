using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PWSWebApi.Domains;
using PWSWebApi.Domains.DBContext;
using PWSWebApi.handlers;
using PWSWebApi.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

namespace PWSWebApi.Controllers
{
    [Route("adm")]
    [ApiController]
    public class AdmController : ControllerBase
    {
        private readonly string connectionString;
        private readonly Helper handlerHelper;

        public AdmController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("PWSProd");
            handlerHelper = new Helper(configuration);
        }

        [HttpPost("saverpttmplate")]
        //[Route("saverpttmplate")]
        public HttpResponseMessage SaveRptTemplate([FromQuery] string UserID, [FromQuery] string rptTemplateName, [FromQuery] string rptTemplateNameDesc, [FromQuery] bool showAddtionalFields, [FromQuery] string selectedPwsRefid, [FromQuery] string selectedCurrencies, [FromQuery] string selectedUserGroup, [FromQuery] string selectedLowLevelPerf, [FromQuery] string selectedModelId = null, [FromQuery] string selectedAssignPt = null, [FromQuery] string selectedAssignPtEnd = null, [FromQuery] string selectedDrillDownInfo = null, [FromQuery] string selectedHeirRptInfo = null)
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
                pars.Add("@RptTemplateName", rptTemplateName);
                pars.Add("@RptTemplateNameDesc", rptTemplateNameDesc);
                pars.Add("@RptPortID", selectedPwsRefid);
                pars.Add("@CCYID", selectedCurrencies);
                pars.Add("@UserGroupID", selectedUserGroup);
                pars.Add("@HierRpt", selectedHeirRptInfo);
                pars.Add("@LowlevelPerf", selectedLowLevelPerf);
                pars.Add("@AssgnStartPt", selectedAssignPt);
                pars.Add("@ModelID", selectedModelId);
                var data = Helper.callProcedure("[adm].[usp_RptTemplateName_Insert]", pars);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { saved = true, error = false, message = "Template has been saved successfully." }))
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return response;
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("insertdptrans")]
        public HttpResponseMessage InsertDPTranTrialBalance([FromQuery] string userId, [FromQuery] string Recon_TrialBalance_TransIDs)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
                {
                    return (HttpResponseMessage)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, userId) != string.Empty)
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent(handlerHelper.ValidateToken(Request, userId))
                    };
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", userId);
                pars.Add("@Recon_TrialBalance_TransIDs", Recon_TrialBalance_TransIDs);
                var results = Helper.callProcedure("adm.usp_ReconTrans_Insert", pars);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ObjectContent<object>(
                        new { results = results, inserted = true, error = false },
                        new System.Net.Http.Formatting.JsonMediaTypeFormatter()
                    )
                };
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = userId });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("deletetranpws")]
        public HttpResponseMessage DeletePWSTranFromTrailBalance([FromQuery] string userId, [FromQuery] string TransID_NewEditTransIDs)
        {
            //ReconTrans_Delete
            try
            {
                if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
                {
                    return (HttpResponseMessage)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, userId) != string.Empty)
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent(handlerHelper.ValidateToken(Request, userId))
                    };
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", userId);
                if (TransID_NewEditTransIDs.Length > 1)
                {
                    TransID_NewEditTransIDs = new StringBuilder(TransID_NewEditTransIDs.Substring(0, TransID_NewEditTransIDs.Length - 1)).ToString();
                }

                pars.Add("@TransID_NewEditTransIDs", TransID_NewEditTransIDs);
                var results = Helper.callProcedure("[adm].[usp_ReconTrans_Delete]", pars);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ObjectContent<object>(
                        new { results = results, deleted = true, error = false },
                        new System.Net.Http.Formatting.JsonMediaTypeFormatter()
                    )
                };
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = userId });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpGet]
        [Route("trialBalance")]
        public HttpResponseMessage SearchTrialBalanace([FromQuery] string userId, [FromQuery] string searchSection, [FromQuery] string accountsCommaDelimited, [FromQuery] string securitiesCommaDelimited = null, [FromQuery] string dpSecIdCommaDelimited = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] DateTime? fileStartDate = null, [FromQuery] DateTime? fileEndDate = null, [FromQuery] string accountsCommaDelimitedPos = null, [FromQuery] string securitiesCommaDelimitedPos = null, [FromQuery] string dpSecIdCommaDelimitedPos = null, [FromQuery] DateTime? startDatePos = null, [FromQuery] DateTime? endDatePos = null)
        {
            if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
            {
                return (HttpResponseMessage)handlerHelper.ValidateSource(Request);
            }

            if (handlerHelper.ValidateToken(Request, userId) != string.Empty)
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent(handlerHelper.ValidateToken(Request, userId))
                };
            }
            object responseData;
            string jsonResponse;
            HttpResponseMessage response;
            string outages = "[]", posNorm = "[]", posPWS = "[]", tranPWS = "[]", tranNorm = "[]";
            if (searchSection == "trans")
            {
                try
                {
                    Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                    pars.Add("@UserID", userId);
                    pars.Add("@AcctID", accountsCommaDelimited);
                    pars.Add("@StartDate", startDate);
                    pars.Add("@Enddate", endDate);
                    outages = (string)callProcedure("[adm].[usp_ReconDailyOutages]", pars);
                    if (dpSecIdCommaDelimited != "null" && dpSecIdCommaDelimited != "undefined")
                        pars.Add("@Sec", dpSecIdCommaDelimited);
                    if (securitiesCommaDelimited != "null" && securitiesCommaDelimited != "undefined")
                        pars.Add("@SecID", securitiesCommaDelimited);
                    pars.Add("@StartDateFile", fileStartDate);
                    pars.Add("@EndDateFile", fileEndDate);
                    posNorm = (string)callProcedure("[adm].[usp_ReconTran_Norm]", pars);
                    posPWS = (string)callProcedure("[adm].[usp_ReconTrans_PWS]", pars);

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ObjectContent<object>(
                            new { trans = true, error = false, outages = outages, norm = posNorm, pws = posPWS },
                            new System.Net.Http.Formatting.JsonMediaTypeFormatter()
                        )
                    };
                }
                catch (Exception ex)
                {
                    Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = userId });

                    responseData = new { trans = true, error = true, outages = outages, norm = posNorm, pws = posPWS };
                    jsonResponse = JsonConvert.SerializeObject(responseData);

                    response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                    };

                    return response;
                }
            }
            else if (searchSection == "positions")
            {
                try
                {
                    Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                    pars.Add("@UserID", userId);
                    pars.Add("@AcctID", accountsCommaDelimitedPos);
                    pars.Add("@StartDate", startDatePos);
                    pars.Add("@Enddate", endDatePos);
                    outages = (string)callProcedure("[adm].[usp_ReconDailyOutages]", pars);
                    if (dpSecIdCommaDelimitedPos != "null" && dpSecIdCommaDelimitedPos != "undefined")
                        pars.Add("@Sec", dpSecIdCommaDelimitedPos);
                    if (securitiesCommaDelimitedPos != "null" && securitiesCommaDelimitedPos != "undefined")
                        pars.Add("@SecID", securitiesCommaDelimitedPos);
                    tranPWS = (string)callProcedure("[adm].[usp_ReconPositions_PWS]", pars);
                    tranNorm = (string)callProcedure("[adm].[usp_ReconPositions_Norm]", pars);

                    responseData = new { positions = true, error = false, outages = outages, norm = tranNorm, pws = tranPWS };
                    jsonResponse = JsonConvert.SerializeObject(responseData);

                    response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                    };

                    return response;
                }
                catch (Exception ex)
                {
                    Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = userId });
                    responseData = new { trans = true, error = true, outages = outages, norm = posNorm, pws = posPWS };
                    jsonResponse = JsonConvert.SerializeObject(responseData);

                    response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                    };

                    return response;
                }
            }

            responseData = new { none = true, error = false };
            jsonResponse = JsonConvert.SerializeObject(responseData);

            response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            };

            return response;
        }

        [HttpPost]
        [Route("uspPriceSearch")]
        public HttpResponseMessage SearchPrices(string UserID, string SecIDs, string CCYIDs, string FromDate, string ToDate, string PriceType, string UserGroupIDs, string DPIDs, string LoadSourceIDs, string Status)
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
                pars.Add("@SecIDs", SecIDs);
                pars.Add("@UserID", UserID);
                if (!Helper.IgnoreAsNullParameter(CCYIDs))
                    pars.Add("@CCYIDs", CCYIDs);
                if (!Helper.IgnoreAsNullParameter(FromDate))
                    pars.Add("@FromDate", FromDate);
                if (!Helper.IgnoreAsNullParameter(ToDate))
                    pars.Add("@ToDate", ToDate);
                if (!Helper.IgnoreAsNullParameter(PriceType))
                    pars.Add("@PriceType", PriceType);
                if (!Helper.IgnoreAsNullParameter(UserGroupIDs))
                    pars.Add("@UserGroupIDs", UserGroupIDs);
                if (!Helper.IgnoreAsNullParameter(DPIDs))
                    pars.Add("@DPIDs", DPIDs);
                if (!Helper.IgnoreAsNullParameter(LoadSourceIDs))
                    pars.Add("@LoadSourceIDs", LoadSourceIDs);
                if (!Helper.IgnoreAsNullParameter(Status))
                    pars.Add("@Status", Status);
                var data = Helper.callProcedure("[adm].[usp_Price_Search]", pars);
                var responseData = new { error = false, data = data };

                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("uspPriceDelete")]
        public HttpResponseMessage DeletePrice(string UserID, string PriceID)
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
                pars.Add("@PriceID", PriceID);
                var data = Helper.callProcedure("[adm].[usp_Price_Delete]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };

                return response;
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("uspPriceEdit")]
        public HttpResponseMessage InserPrice(string UserID, string Trial_PriceID, string SecID, string PriceDate, string Price, string PeriodicityID, string PriceType, string LoadsourceID, string DPID, string UserGroupID)
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

                // all params are required
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@Trial_PriceID", Trial_PriceID);
                pars.Add("@SecID", SecID);
                pars.Add("@PriceDate", PriceDate);
                pars.Add("@Price", Price);
                pars.Add("@PeriodicityID", PeriodicityID);
                pars.Add("@PriceType", PriceType);
                pars.Add("@LoadsourceID", LoadsourceID);
                pars.Add("@DPID", DPID);
                pars.Add("@UserGroupID", UserGroupID);
                var data = Helper.callProcedure("[adm].[usp_Price_Edit]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("uspPriceInsert")]
        public HttpResponseMessage InserPrice(string UserID, string SecID, string PriceDate, string Price, string PeriodicityID, string PriceType, string LoadsourceID, string DPID, string UserGroupID)
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

                // all params are required
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@SecID", SecID);
                pars.Add("@PriceDate", PriceDate);
                pars.Add("@Price", Price);
                pars.Add("@PeriodicityID", PeriodicityID);
                pars.Add("@PriceType", PriceType);
                pars.Add("@LoadsourceID", LoadsourceID);
                pars.Add("@DPID", DPID);
                pars.Add("@UserGroupID", UserGroupID);
                var data = Helper.callProcedure("[adm].[usp_Price_Insert]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("uspPricePageClear")]
        public HttpResponseMessage ClearPricingPage(string UserID, string Trial_PriceIDs)
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
                if (!Helper.IgnoreAsNullParameter(Trial_PriceIDs))
                    pars.Add("@Trial_PriceIDs", Trial_PriceIDs);
                var data = Helper.callProcedure("[adm].[usp_Price_ClearTrial]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("uspFXRateSearch")]
        public HttpResponseMessage SearchFXRates(string UserID, string FromCCYID, string FromDate, string ToDate, string UserGroupID, string DPID, string LoadSourceID, string Status)
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
                pars.Add("@FromCCYID", FromCCYID);
                pars.Add("@UserID", UserID);
                pars.Add("@FromDate", FromDate);
                pars.Add("@ToDate", ToDate);
                if (!Helper.IgnoreAsNullParameter(UserGroupID))
                    pars.Add("@UserGroupID", UserGroupID);
                if (!Helper.IgnoreAsNullParameter(LoadSourceID))
                    pars.Add("@LoadSourceID", LoadSourceID);
                if (!Helper.IgnoreAsNullParameter(DPID))
                    pars.Add("@DPID", DPID);
                if (!Helper.IgnoreAsNullParameter(Status))
                    pars.Add("@Status", Status);
                var data = Helper.callProcedure("[adm].[usp_FXRate_Search]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("uspFXRateDelete")]
        public HttpResponseMessage DeleteFXRate(string UserID, string FXRateID)
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
                pars.Add("@FXRateID", FXRateID);
                var data = Helper.callProcedure("[adm].[usp_FXRate_Delete]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost("uspFXRateEdit")]
        //[Route("uspFXRateEdit")]
        public HttpResponseMessage FXRateEdit(string UserID, string FXRateID, string FromCCYID, string AsOfDate, string Rate, string UserGroupID, string DPID, string LoadSourceID)
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

                // all params are required
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@FXRateID", FXRateID);
                pars.Add("@FromCCYID", FromCCYID);
                pars.Add("@AsOfDate", AsOfDate);
                pars.Add("@Rate", Rate);
                pars.Add("@LoadSourceID", LoadSourceID);
                pars.Add("@DPID", DPID);
                pars.Add("@UserGroupID", UserGroupID);
                var data = Helper.callProcedure("[adm].[usp_FXRate_Edit]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("uspFXRateInsert")]
        public HttpResponseMessage FXRateInsert(string UserID, string FromCCYID, string AsOfDate, string Rate, string UserGroupID, string DPID, string LoadSourceID)
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

                // all params are required
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@FromCCYID", FromCCYID);
                pars.Add("@AsOfDate", AsOfDate);
                pars.Add("@Rate", Rate);
                pars.Add("@LoadSourceID", LoadSourceID);
                pars.Add("@DPID", DPID);
                pars.Add("@UserGroupID", UserGroupID);
                var data = Helper.callProcedure("[adm].[usp_FXRate_Insert]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("CallScheduleSearch")]
        public HttpResponseMessage CallScheduleSearch(string UserID, string CallDateStart, string CallDateEnd, string AcctID_SubAcctID, string Final, string SecID, string UserGroupID)
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
                pars.Add("@SecID", SecID);
                if (!Helper.IgnoreAsNullParameter(AcctID_SubAcctID))
                    pars.Add("@AcctID_SubAcctID", AcctID_SubAcctID);
                if (!Helper.IgnoreAsNullParameter(CallDateStart))
                    pars.Add("@CallDateStart", CallDateStart);
                if (!Helper.IgnoreAsNullParameter(CallDateEnd))
                    pars.Add("@CallDateEnd", CallDateEnd);
                if (!Helper.IgnoreAsNullParameter(UserGroupID))
                    pars.Add("@UserGroupID", UserGroupID);
                if (!Helper.IgnoreAsNullParameter(Final))
                    pars.Add("@Final", Final);
                var data = Helper.callProcedure("[adm].[usp_CallSchd_Search]", pars);
                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("CallScheduleEdit")]
        public HttpResponseMessage CallScheduleEdit(string UserID, string ScheduleID, string SecID, string CallDate, string CallPct, string CallParValue, string CallCapitalAmount, string CallCapitalCCYID, string CallCapitalAcctID, string DPID, string LoadsourceID, string UserGroupID, string PeriodicityID, string NumPeriods, string IncExp, string Final, string Notes)
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
                pars.Add("@ScheduleID", ScheduleID);
                pars.Add("@CallDate", CallDate);
                pars.Add("@CallCapitalCCYID", CallCapitalCCYID);
                pars.Add("@DPID", DPID);
                pars.Add("@LoadsourceID", LoadsourceID);
                pars.Add("@UserGroupID", UserGroupID);
                if (!Helper.IgnoreAsNullParameter(PeriodicityID))
                    pars.Add("@PeriodicityID", PeriodicityID);
                if (!Helper.IgnoreAsNullParameter(NumPeriods))
                    pars.Add("@NumPeriods", NumPeriods);
                if (!Helper.IgnoreAsNullParameter(IncExp))
                    pars.Add("@IncExp", IncExp);
                if (!Helper.IgnoreAsNullParameter(Final))
                    pars.Add("@Final", Final);
                if (!Helper.IgnoreAsNullParameter(Notes))
                    pars.Add("@Notes", Notes);
                if (!Helper.IgnoreAsNullParameter(SecID))
                    pars.Add("@SecID", SecID);
                if (!Helper.IgnoreAsNullParameter(CallPct))
                    pars.Add("@CallPct", CallPct);
                if (!Helper.IgnoreAsNullParameter(CallCapitalAmount))
                    pars.Add("@CallCapitalAmount", CallCapitalAmount);
                if (!Helper.IgnoreAsNullParameter(CallCapitalAcctID))
                    pars.Add("@CallCapitalAcctID", CallCapitalAcctID);
                if (!Helper.IgnoreAsNullParameter(CallParValue))
                    pars.Add("@CallParValue", CallParValue);
                var data = Helper.callProcedure("[adm].[usp_CallSchd_Edit]", pars);
                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("CallScheduleInsert")]
        public HttpResponseMessage CallScheduleInsert(string UserID, string SecID, string CallDate, string CallPct, string CallParValue, string CallCapitalAmount, string CallCapitalCCYID, string CallCapitalAcctID, string DPID, string LoadsourceID, string UserGroupID, string PeriodicityID, string NumPeriods, string IncExp, string Final, string Notes)
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
                pars.Add("@SecID", SecID);
                pars.Add("@CallDate", CallDate);
                pars.Add("@CallCapitalCCYID", CallCapitalCCYID);
                pars.Add("@DPID", DPID);
                pars.Add("@LoadsourceID", LoadsourceID);
                pars.Add("@UserGroupID", UserGroupID);
                if (!Helper.IgnoreAsNullParameter(PeriodicityID))
                    pars.Add("@PeriodicityID", PeriodicityID);
                if (!Helper.IgnoreAsNullParameter(NumPeriods))
                    pars.Add("@NumPeriods", NumPeriods);
                if (!Helper.IgnoreAsNullParameter(IncExp))
                    pars.Add("@IncExp", IncExp);
                if (!Helper.IgnoreAsNullParameter(Final))
                    pars.Add("@Final", Final);
                if (!Helper.IgnoreAsNullParameter(Notes))
                    pars.Add("@Notes", Notes);
                if (!Helper.IgnoreAsNullParameter(CallPct))
                    pars.Add("@CallPct", CallPct);
                if (!Helper.IgnoreAsNullParameter(CallCapitalAmount))
                    pars.Add("@CallCapitalAmount", CallCapitalAmount);
                if (!Helper.IgnoreAsNullParameter(CallCapitalAcctID))
                    pars.Add("@CallCapitalAcctID", CallCapitalAcctID);
                if (!Helper.IgnoreAsNullParameter(CallParValue))
                    pars.Add("@CallParValue", CallParValue);
                var data = Helper.callProcedure("[adm].[usp_CallSchd_Insert]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("CallScheduleDelete")]
        public HttpResponseMessage CallScheduleDelete(string UserID, string ScheduleID)
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
                pars.Add("@ScheduleID", ScheduleID);
                var data = Helper.callProcedure("[adm].[usp_CallSchd_Delete]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("FloatingRatesSearch")]
        public HttpResponseMessage FloatingRatesSearch(string UserID, string SecID, string ResetDateStartStart, string ResetDateStartEnd, string ResetDateEndStart, string ResetDateEndEnd, string DependentSecID, string HierAssgnID, string UserGroupID, string LoadsourceID, string DPID)
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
                pars.Add("@SecID", SecID);
                if (!Helper.IgnoreAsNullParameter(ResetDateStartStart))
                    pars.Add("@ResetDateStartStart", ResetDateStartStart);
                if (!Helper.IgnoreAsNullParameter(ResetDateStartEnd))
                    pars.Add("@ResetDateStartEnd", ResetDateStartEnd);
                if (!Helper.IgnoreAsNullParameter(ResetDateEndStart))
                    pars.Add("@ResetDateEndStart", ResetDateEndStart);
                if (!Helper.IgnoreAsNullParameter(ResetDateEndEnd))
                    pars.Add("@ResetDateEndEnd", ResetDateEndEnd);
                if (!Helper.IgnoreAsNullParameter(DependentSecID))
                    pars.Add("@DependentSecID", DependentSecID);
                if (!Helper.IgnoreAsNullParameter(HierAssgnID))
                    pars.Add("@HierAssgnID", HierAssgnID);
                if (!Helper.IgnoreAsNullParameter(UserGroupID))
                    pars.Add("@UserGroupID", UserGroupID);
                if (!Helper.IgnoreAsNullParameter(LoadsourceID))
                    pars.Add("@LoadsourceID", LoadsourceID);
                if (!Helper.IgnoreAsNullParameter(DPID))
                    pars.Add("@DPID", DPID);
                var data = Helper.callProcedure("[adm].[usp_FloatRate_Search]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("FloatingRatesInsert")]
        public HttpResponseMessage FloatingRatesInsert(string UserID, string SecID, string Rate, string ResetDateStart, string ResetDateEnd, string DependentSecID, string DependentSecPeriodicityID, string RateMult, string DependentSecMult, string UserGroupID, string HierAssgnID, string LoadSourceID, string DPID)
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
                pars.Add("@SecID", SecID);
                pars.Add("@Rate", Rate);
                pars.Add("@ResetDateStart", ResetDateStart);
                pars.Add("@ResetDateEnd", ResetDateEnd);
                pars.Add("@RateMult", RateMult);
                pars.Add("@UserGroupID", UserGroupID);
                pars.Add("@HierAssgnID", HierAssgnID);
                pars.Add("@LoadSourceID", LoadSourceID);
                pars.Add("@DPID", DPID);
                if (!Helper.IgnoreAsNullParameter(DependentSecID))
                    pars.Add("@DependentSecID", DependentSecID);
                if (!Helper.IgnoreAsNullParameter(DependentSecPeriodicityID))
                    pars.Add("@DependentSecPeriodicityID", DependentSecPeriodicityID);
                if (!Helper.IgnoreAsNullParameter(DependentSecMult))
                    pars.Add("@DependentSecMult", DependentSecMult);
                var data = Helper.callProcedure("[adm].[usp_FloatRate_Insert]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("FloatingRatesEdit")]
        public HttpResponseMessage FloatingRatesEdit(string UserID, string FloatRateID, string SecID, string Rate, string ResetDateStart, string ResetDateEnd, string DependentSecID, string DependentSecPeriodicityID, string RateMult, string DependentSecMult, string UserGroupID, string HierAssgnID, string LoadSourceID, string DPID)
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
                pars.Add("@FloatRateID", FloatRateID);
                pars.Add("@SecID", SecID);
                pars.Add("@Rate", Rate);
                pars.Add("@ResetDateStart", ResetDateStart);
                pars.Add("@ResetDateEnd", ResetDateEnd);
                pars.Add("@RateMult", RateMult);
                pars.Add("@UserGroupID", UserGroupID);
                pars.Add("@HierAssgnID", HierAssgnID);
                pars.Add("@LoadSourceID", LoadSourceID);
                pars.Add("@DPID", DPID);
                if (!Helper.IgnoreAsNullParameter(DependentSecID))
                    pars.Add("@DependentSecID", DependentSecID);
                if (!Helper.IgnoreAsNullParameter(DependentSecPeriodicityID))
                    pars.Add("@DependentSecPeriodicityID", DependentSecPeriodicityID);
                if (!Helper.IgnoreAsNullParameter(DependentSecMult))
                    pars.Add("@DependentSecMult", DependentSecMult);
                var data = Helper.callProcedure("[adm].[usp_FloatRate_Edit]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("FloatingRatesDelete")]
        public HttpResponseMessage FloatingRatesDelete(string UserID, string FloatRateID)
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
                pars.Add("@FloatRateID", FloatRateID);
                var data = Helper.callProcedure("[adm].[usp_FloatRate_Delete]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("FactorsSearch")]
        public HttpResponseMessage FactorsSearch(string UserID, string SecID, string FactorDateStart, string FactorDateEnd, string UserGroupID, string LoadsourceID, string DPID)
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
                pars.Add("@SecID", SecID);
                if (!Helper.IgnoreAsNullParameter(FactorDateStart))
                    pars.Add("@FactorDateStart", FactorDateStart);
                if (!Helper.IgnoreAsNullParameter(FactorDateEnd))
                    pars.Add("@FactorDateEnd", FactorDateEnd);
                if (!Helper.IgnoreAsNullParameter(UserGroupID))
                    pars.Add("@UserGroupID", UserGroupID);
                if (!Helper.IgnoreAsNullParameter(LoadsourceID))
                    pars.Add("@LoadsourceID", LoadsourceID);
                if (!Helper.IgnoreAsNullParameter(DPID))
                    pars.Add("@DPID", DPID);
                var data = Helper.callProcedure("[adm].[usp_Factors_Search]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("FactorsInsert")]
        public HttpResponseMessage FactorsInsert(string UserID, string SecID, string Factor, string FactorDate, string FactorChgDate, string LoadSourceID, string DPID)
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
                pars.Add("@SecID", SecID);
                pars.Add("@Factor", Factor);
                pars.Add("@FactorDate", FactorDate);
                pars.Add("@LoadSourceID", LoadSourceID);
                pars.Add("@DPID", DPID);
                if (!Helper.IgnoreAsNullParameter(FactorChgDate))
                    pars.Add("@FactorChgDate", FactorChgDate);
                var data = Helper.callProcedure("[adm].[usp_Factors_Insert]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("FactorsEdit")]
        public HttpResponseMessage FactorsEdit(string UserID, string FactorID, string SecID, string Factor, string FactorDate, string FactorChgDate, string LoadSourceID, string DPID)
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
                pars.Add("@SecID", SecID);
                pars.Add("@FactorID", FactorID);
                pars.Add("@Factor", Factor);
                pars.Add("@FactorDate", FactorDate);
                pars.Add("@LoadSourceID", LoadSourceID);
                pars.Add("@DPID", DPID);
                if (!Helper.IgnoreAsNullParameter(FactorChgDate))
                    pars.Add("@FactorChgDate", FactorChgDate);
                var data = Helper.callProcedure("[adm].[usp_Factors_Edit]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("FactorsDelete")]
        public HttpResponseMessage FactorsDelete(string UserID, string FactorID)
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
                pars.Add("@FactorID", FactorID);
                var data = Helper.callProcedure("[adm].[usp_Factors_Delete]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("IndicesSearch")]
        public HttpResponseMessage IndicesSearch(string UserID, string SecID, string FromDate, string ToDate, string CurrCode, string UserGroupID, string LoadsourceID, string DPID, string Status)
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
                pars.Add("@SecID", SecID);
                pars.Add("@Status", Status);
                if (!Helper.IgnoreAsNullParameter(FromDate))
                    pars.Add("@FromDate", FromDate);
                if (!Helper.IgnoreAsNullParameter(ToDate))
                    pars.Add("@ToDate", ToDate);
                if (!Helper.IgnoreAsNullParameter(CurrCode))
                    pars.Add("@CurrCode", CurrCode);
                if (!Helper.IgnoreAsNullParameter(UserGroupID))
                    pars.Add("@UserGroupID", UserGroupID);
                if (!Helper.IgnoreAsNullParameter(LoadsourceID))
                    pars.Add("@LoadsourceID", LoadsourceID);
                if (!Helper.IgnoreAsNullParameter(DPID))
                    pars.Add("@DPID", DPID);
                var data = Helper.callProcedure("[adm].[usp_Index_Search]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("IndicesInsert")]
        public HttpResponseMessage IndicesInsert(string UserID, string UserGroupID, string DPID, string LoadSourceID, string SecID, string IndexLevel, string LevelDate, string PeriodicityID)
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
                pars.Add("@UserGroupID", UserGroupID);
                pars.Add("@DPID", DPID);
                pars.Add("@LoadSourceID", LoadSourceID);
                pars.Add("@SecID", SecID);
                pars.Add("@LevelDate", LevelDate);
                pars.Add("@PeriodicityID", PeriodicityID);
                pars.Add("@IndexLevel", IndexLevel);
                var data = Helper.callProcedure("[adm].[usp_Index_Insert]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("IndicesEdit")]
        public HttpResponseMessage IndicesEdit(string UserID, string IndicesID, string UserGroupID, string DPID, string LoadSourceID, string IndexLevel, string LevelDate, string PeriodicityID)
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
                pars.Add("@IndicesID", IndicesID);
                pars.Add("@UserGroupID", UserGroupID);
                pars.Add("@LevelDate", LevelDate);
                pars.Add("@LoadSourceID", LoadSourceID);
                pars.Add("@DPID", DPID);
                pars.Add("@PeriodicityID", PeriodicityID);
                pars.Add("@IndexLevel", IndexLevel);
                var data = Helper.callProcedure("[adm].[usp_Index_Edit]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("IndicesDelete")]
        public HttpResponseMessage IndicesDelete(string UserID, string IndicesID)
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
                pars.Add("@IndicesID", IndicesID);
                var data = Helper.callProcedure("[adm].[usp_Index_Delete]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("AltTagsSearch")]
        public HttpResponseMessage AltTagsSearch(string UserID, string TitleSearch, string UsergroupIDs, string EntityNameMatch, string SearchSecurity, string CheckMVDate, string StatementReceiptDate, string StatementDate, string Flow, string Valuation)
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
                pars.Add("@TitleSearch", TitleSearch);
                pars.Add("@SearchSecurity", SearchSecurity);
                pars.Add("@StatementReceiptDate", StatementReceiptDate);
                pars.Add("@StatementDate", StatementDate);
                pars.Add("@Flow", Flow);
                pars.Add("@Valuation", Valuation);
                if (!Helper.IgnoreAsNullParameter(EntityNameMatch))
                    pars.Add("@EntityNameMatch", EntityNameMatch);
                if (!Helper.IgnoreAsNullParameter(UsergroupIDs))
                    pars.Add("@UsergroupIDs", UsergroupIDs);
                if (!Helper.IgnoreAsNullParameter(CheckMVDate))
                    pars.Add("@CheckMVDate", CheckMVDate);
                var data = Helper.callProcedure("[adm].[usp_AltsWorkFlow_Tags_Search]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("AltTagsInsert")]
        public HttpResponseMessage AltTagsInsert(string UserID, string AcctID, string SecID, string StatementReceiptDate, string StatementDate, string Flow, string Valuation, string AccountTag, string CashAccountSec, string GroupTag, string CSD, string NotDuplicate)
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
                pars.Add("@AcctID", AcctID);
                pars.Add("@SecID", SecID);
                pars.Add("@StatementReceiptDate", StatementReceiptDate);
                pars.Add("@StatementDate", StatementDate);
                pars.Add("@Flow", Flow);
                pars.Add("@Valuation", Valuation);
                pars.Add("@AccountTag", AccountTag);
                pars.Add("@CashAccountSec", CashAccountSec);
                pars.Add("@GroupTag", GroupTag);
                pars.Add("@CSD", CSD);
                pars.Add("@NotDuplicate", NotDuplicate);
                var data = Helper.callProcedure("[adm].[usp_Alts_Tags_Insert]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("HoldingsSearch")]
        public HttpResponseMessage HoldingsSearch(string UserID, string AcctID_SubAcctIDs, string UserGroupIDs, string LoadSourceIDs, string RollUp, string SecIDs, string IssuerIDs, string CountryID, string SecCurrency, string EffectiveDateStart, string EffectiveDateEnd, string UnrealizedGLLocalStart, string UnrealizedGLLocalEnd, string UnrealizedGLBaseStart, string UnrealizedGLBaseEnd, string PerfLocalStart, string PerfLocalEnd, string PerfBaseStart, string PerfBaseEnd, string AttribCodeIDs, string MonthEndOnly, string ChangesOnly, string Pending, string Index, string NumRows)
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
                if (!Helper.IgnoreAsNullParameter(AcctID_SubAcctIDs))
                    pars.Add("@AcctID_SubAcctIDs", AcctID_SubAcctIDs);
                if (!Helper.IgnoreAsNullParameter(UserGroupIDs))
                    pars.Add("@UserGroupIDs", UserGroupIDs);
                if (!Helper.IgnoreAsNullParameter(LoadSourceIDs))
                    pars.Add("@LoadSourceIDs", LoadSourceIDs);
                if (!Helper.IgnoreAsNullParameter(RollUp))
                    pars.Add("@RollUp", RollUp);
                if (!Helper.IgnoreAsNullParameter(SecIDs))
                    pars.Add("@SecIDs", SecIDs);
                if (!Helper.IgnoreAsNullParameter(IssuerIDs))
                    pars.Add("@IssuerIDs", IssuerIDs);
                if (!Helper.IgnoreAsNullParameter(CountryID))
                    pars.Add("@CountryID", CountryID);
                if (!Helper.IgnoreAsNullParameter(SecCurrency))
                    pars.Add("@SecCurrency", SecCurrency);
                if (!Helper.IgnoreAsNullParameter(EffectiveDateStart))
                    pars.Add("@EffectiveDateStart", EffectiveDateStart);
                if (!Helper.IgnoreAsNullParameter(EffectiveDateEnd))
                    pars.Add("@EffectiveDateEnd", EffectiveDateEnd);
                if (!Helper.IgnoreAsNullParameter(UnrealizedGLLocalStart))
                    pars.Add("@UnrealizedGLLocalStart", UnrealizedGLLocalStart);
                if (!Helper.IgnoreAsNullParameter(UnrealizedGLLocalEnd))
                    pars.Add("@UnrealizedGLLocalEnd", UnrealizedGLLocalEnd);
                if (!Helper.IgnoreAsNullParameter(UnrealizedGLBaseStart))
                    pars.Add("@UnrealizedGLBaseStart", UnrealizedGLBaseStart);
                if (!Helper.IgnoreAsNullParameter(UnrealizedGLBaseEnd))
                    pars.Add("@UnrealizedGLBaseEnd", UnrealizedGLBaseEnd);
                if (!Helper.IgnoreAsNullParameter(PerfLocalStart))
                    pars.Add("@PerfLocalStart", PerfLocalStart);
                if (!Helper.IgnoreAsNullParameter(PerfLocalEnd))
                    pars.Add("@PerfLocalEnd", PerfLocalEnd);
                if (!Helper.IgnoreAsNullParameter(PerfBaseStart))
                    pars.Add("@PerfBaseStart", PerfBaseStart);
                if (!Helper.IgnoreAsNullParameter(PerfBaseEnd))
                    pars.Add("@PerfBaseEnd", PerfBaseEnd);
                if (!Helper.IgnoreAsNullParameter(AttribCodeIDs))
                    pars.Add("@AttribCodeIDs", AttribCodeIDs);
                if (!Helper.IgnoreAsNullParameter(MonthEndOnly))
                    pars.Add("@MonthEndOnly", MonthEndOnly);
                if (!Helper.IgnoreAsNullParameter(ChangesOnly))
                    pars.Add("@ChangesOnly", ChangesOnly);
                if (!Helper.IgnoreAsNullParameter(Pending))
                    pars.Add("@Pending", Pending);
                if (!Helper.IgnoreAsNullParameter(Index))
                    pars.Add("@Index", Index);
                if (!Helper.IgnoreAsNullParameter(NumRows))
                    pars.Add("@NumRows", NumRows);
                var data = Helper.callProcedure("[adm].[usp_Holdings_Search]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("getAvailableDatesSearch")]
        public HttpResponseMessage getAvailableDatesSearch(string UserID, string RptPortIDs, string UserGroupIDs, string PWSClientIDs, string CheckDate)
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
                if (!Helper.IgnoreAsNullParameter(RptPortIDs))
                    pars.Add("@RptPortIDs", RptPortIDs);
                if (!Helper.IgnoreAsNullParameter(UserGroupIDs))
                    pars.Add("@UserGroupIDs", UserGroupIDs);
                if (!Helper.IgnoreAsNullParameter(PWSClientIDs))
                    pars.Add("@PWSClientIDs", PWSClientIDs);
                if (!Helper.IgnoreAsNullParameter(CheckDate))
                    pars.Add("@CheckDate", CheckDate);
                var data = Helper.callProcedure("[adm].[usp_Portfolio_AvailableDate]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("getAccountInfoSearch")]
        public HttpResponseMessage getAccountInfoSearch(string UserID, string RptPortIDs, string CheckDate)
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
                if (!Helper.IgnoreAsNullParameter(RptPortIDs))
                    pars.Add("@RptPortIDs", RptPortIDs);
                if (!Helper.IgnoreAsNullParameter(CheckDate))
                    pars.Add("@CheckDate", CheckDate);
                var data = Helper.callProcedure("[adm].[usp_Portfolio_Account_AvailableDate]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("getUnclassifiedSearch")]
        public HttpResponseMessage getUnclassifiedSearch(string UserID, string RptPortIDs, string UserGroupIDs, string RptTemplateNameIDs)
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
                if (!Helper.IgnoreAsNullParameter(RptPortIDs))
                    pars.Add("@RptPortIDs", RptPortIDs);
                if (!Helper.IgnoreAsNullParameter(UserGroupIDs))
                    pars.Add("@UserGroupIDs", UserGroupIDs);
                if (!Helper.IgnoreAsNullParameter(RptTemplateNameIDs))
                    pars.Add("@RptTemplateNameIDs", RptTemplateNameIDs);
                var data = Helper.callProcedure("[adm].[usp_Unclassified_Search]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("getUnclassifiedOptions")]
        public HttpResponseMessage getUnclassifiedOptions(string UserID, string Trial_UnclassifiedID)
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
                pars.Add("@Trial_UnclassifiedID", Trial_UnclassifiedID);
                var data = Helper.callProcedure("[adm].[usp_Unclassified_Options]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("UnclassifiedInsert")]
        public HttpResponseMessage UnclassifiedInsert(string UserID, string AttribTypeID, string AttribNameID, string UserGroupID, string DPID, string LoadSourceID, string PWSRefID, string FnctTypeID, string AttribTypeValue, string AttribMinValue, string AttribMaxValue, string AttribStartDate, string AttribEndDate, string SortOrder, string Angular, string Trial_UnclassifiedID)
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
                pars.Add("@AttribTypeID", AttribTypeID);
                pars.Add("@AttribNameID", AttribNameID);
                pars.Add("@UserGroupID", UserGroupID);
                if (!Helper.IgnoreAsNullParameter(DPID))
                    pars.Add("@DPID", DPID);
                if (!Helper.IgnoreAsNullParameter(LoadSourceID))
                    pars.Add("@LoadSourceID", LoadSourceID);
                pars.Add("@PWSRefID", PWSRefID);
                if (!Helper.IgnoreAsNullParameter(FnctTypeID))
                    pars.Add("@FnctTypeID", FnctTypeID);
                if (!Helper.IgnoreAsNullParameter(AttribTypeValue))
                    pars.Add("@AttribTypeValue", AttribTypeValue);
                if (!Helper.IgnoreAsNullParameter(AttribMinValue))
                    pars.Add("@AttribMinValue", AttribMinValue);
                if (!Helper.IgnoreAsNullParameter(AttribMaxValue))
                    pars.Add("@AttribMaxValue", AttribMaxValue);
                if (!Helper.IgnoreAsNullParameter(AttribStartDate))
                    pars.Add("@AttribStartDate", AttribStartDate);
                if (!Helper.IgnoreAsNullParameter(AttribEndDate))
                    pars.Add("@AttribEndDate", AttribEndDate);
                if (!Helper.IgnoreAsNullParameter(SortOrder))
                    pars.Add("@SortOrder", SortOrder);
                if (!Helper.IgnoreAsNullParameter(Angular))
                    pars.Add("@Angular", Angular);
                if (!Helper.IgnoreAsNullParameter(Trial_UnclassifiedID))
                    pars.Add("@Trial_UnclassifiedID", Trial_UnclassifiedID);
                var data = Helper.callProcedure("[adm].[usp_AttribAssign_Insert]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("getPerformanceSearch")]
        public HttpResponseMessage getPerformanceSearch(string UserID, string RptTemplateNameID, string GroupingPWSRefString, string Comparator, string StartDate, string EndDate, string NumRows)
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
                if (!Helper.IgnoreAsNullParameter(RptTemplateNameID))
                    pars.Add("@RptTemplateNameID", RptTemplateNameID);
                if (!Helper.IgnoreAsNullParameter(GroupingPWSRefString))
                    pars.Add("@GroupingPWSRefString", GroupingPWSRefString);
                if (!Helper.IgnoreAsNullParameter(Comparator))
                    pars.Add("@Comparator", Comparator);
                if (!Helper.IgnoreAsNullParameter(StartDate))
                    pars.Add("@StartDate", StartDate);
                if (!Helper.IgnoreAsNullParameter(EndDate))
                    pars.Add("@EndDate", EndDate);
                if (!Helper.IgnoreAsNullParameter(NumRows))
                    pars.Add("@NumRows", NumRows);
                var data = Helper.callProcedure("[adm].[usp_Performance_Search]", pars);

                var responseData = new { error = false, data = data };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("saverptmodel")]
        public HttpResponseMessage SaveReportModels([FromBody] VmAdministration vm, [FromQuery] string userID)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
                {
                    return (HttpResponseMessage)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, userID) != string.Empty)
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent(handlerHelper.ValidateToken(Request, userID))
                    };
                }
                var options = new DbContextOptionsBuilder<DataClasses1DataContext>().UseSqlServer(connectionString).Options;
                var _contextPWS = new DataClasses1DataContext(options);
                //_contextPWS.CommandTimeout = 0;
                //_contextPWS.ExecuteCommand("delete from [PWS].[dbo].[RptModel_tmp] where UserId=" + userID);

               var res =  _contextPWS.RptModel_tmps.Where(x => x.UserID == Convert.ToInt32(userID)).ToList();
                if (res != null)
                {
                    _contextPWS.Remove(res);
                }



                List<PWSWebApi.ViewModel.Models.Administration.ReportModel> models = new List<PWSWebApi.ViewModel.Models.Administration.ReportModel>();
                vm.backUpReportModels.ForEach(item =>
                {
                    item.RowOrderNum = Convert.ToInt32(item.RowOrder);
                    item.LevelsNum = Convert.ToInt32(item.Levels);
                });
                vm.reportModels.ForEach(item =>
                {
                    item.RowOrderNum = Convert.ToInt32(item.RowOrder);
                });
                if (vm.newRow != null)
                {
                    vm.newRow.LevelsNum = Convert.ToInt32(vm.newRow.Levels);
                    vm.newRow.RowOrderNum = Convert.ToInt32(vm.newRow.RowOrder);
                }

                if (vm.Action == "addTemplate")
                {
                    vm.newRow.RowOrderNum = Convert.ToInt32(vm.newRow.RowOrder);
                    vm.reportModels.Add(vm.newRow);
                }
                else if (vm.Action == "addSubClass")
                {
                    //find how many recods with the parent and its siblings it has to pass
                    var totalSiblings = vm.reportModels.Where(f => f.ParentUniqueID == vm.newRow.ParentUniqueID && f.RowOrderNum < vm.newRow.RowOrderNum).ToList();
                    if (totalSiblings.Any())
                    {
                        var lastSibling = totalSiblings.LastOrDefault();
                        var nextHigherRankedItems = vm.reportModels.Where(g => g.LevelsNum < lastSibling.LevelsNum && g.RowOrderNum > lastSibling.RowOrderNum);
                        if (nextHigherRankedItems.Any())
                        {
                            var indexOfHigherLevelItem = vm.reportModels.FindIndex(f => f.RowOrder == nextHigherRankedItems.FirstOrDefault().RowOrder);
                            vm.reportModels.Insert(indexOfHigherLevelItem, vm.newRow);
                        }
                        else
                        {
                            vm.reportModels.Add(vm.newRow);
                        }
                    }
                    else
                    {
                        var parent = vm.reportModels.Where(f => f.RowOrder == vm.newRow.ParentRowOrder).FirstOrDefault();
                        var countForChildInThisParent = vm.reportModels.Where(f => f.ParentRowOrder == parent.RowOrder).Count();
                        var indexOfParent = vm.reportModels.FindIndex(f => f.RowOrder == parent.RowOrder);
                        vm.reportModels.Insert(indexOfParent + countForChildInThisParent + 1, vm.newRow);
                    }

                    //adjust the row orders and the parentroworders in the array
                    for (int i = 0; i < vm.reportModels.Count; i++)
                    {
                        if (i == 0)
                        {
                            continue;
                        }

                        // set the row order
                        vm.reportModels[i].RowOrder = (Convert.ToInt32(vm.reportModels[i - 1].RowOrder) + 1).ToString();
                        vm.reportModels[i].RowOrderNum = Convert.ToInt32(vm.reportModels[i].RowOrder);
                        //set the parentRowOrder
                        if (vm.reportModels[i].Levels != "1")
                        {
                            //find the roworder the of parent now and set it to the current element's parent row order
                            var rowOrderOfParent = vm.reportModels.Where(f => f.UniqueRowID == vm.reportModels[i].ParentUniqueID).FirstOrDefault().RowOrder;
                            vm.reportModels[i].ParentRowOrder = rowOrderOfParent;
                        }
                        else
                        {
                            vm.reportModels[i].ParentRowOrder = null;
                        }
                    }
                }
                else if (vm.Action == "remove")
                {
                    var firstItemToRemove = vm.reportModels.Where(f => f.UniqueRowID == vm.newRow.UniqueRowID).ToList();
                    var additionalItemtoRemove = new List<PWSWebApi.ViewModel.Models.Administration.ReportModel>();
                    foreach (var item in vm.reportModels)
                    {
                        if (item.RowOrderNum > firstItemToRemove.FirstOrDefault().RowOrderNum)
                        {
                            if (item.LevelsNum <= firstItemToRemove.FirstOrDefault().LevelsNum)
                            {
                                break;
                            }

                            additionalItemtoRemove.Add(item);
                        }
                    }

                    firstItemToRemove.AddRange(additionalItemtoRemove);
                    //var itemsToRemove = vm.reportModels.Where(f => f.UniqueRowID == vm.newRow.UniqueRowID || f.ParentUniqueID == vm.newRow.UniqueRowID);
                    vm.reportModels = vm.reportModels.Except(firstItemToRemove).ToList();
                    //adjust the row orders and the parentroworders in the array
                    for (int i = 0; i < vm.reportModels.Count; i++)
                    {
                        if (i == 0 && vm.reportModels[i].RowOrder == "1")
                        {
                            continue;
                        }

                        // set the row order
                        vm.reportModels[i].RowOrder = i > 0 ? (Convert.ToInt32(vm.reportModels[i - 1].RowOrder) + 1).ToString() : "1";
                        vm.reportModels[i].RowOrderNum = Convert.ToInt32(vm.reportModels[i].RowOrder);
                        //set the parentRowOrder
                        if (vm.reportModels[i].Levels != "1")
                        {
                            //find the roworder the of parent now and set it to the current element's parent row order
                            var rowOrderOfParent = vm.reportModels.Where(f => f.UniqueRowID == vm.reportModels[i].ParentUniqueID).FirstOrDefault().RowOrder;
                            vm.reportModels[i].ParentRowOrder = rowOrderOfParent;
                        }
                    }
                }
                else if (vm.Action == "moveUp")
                {
                    var rowSetToMoveUpWards = vm.reportModels.Where(f => f.RowOrder == vm.newRow.RowOrder).ToList(); // row that got clicked
                    var nextRowWithHigherRank = vm.reportModels.Where(f => f.LevelsNum <= vm.newRow.LevelsNum && f.RowOrderNum > vm.newRow.RowOrderNum);
                    if (nextRowWithHigherRank.Any())
                    {
                        var additionalRowsTiShiftUp = vm.reportModels.Where(f => f.RowOrderNum > rowSetToMoveUpWards.First().RowOrderNum && f.RowOrderNum < nextRowWithHigherRank.First().RowOrderNum).ToList();
                        rowSetToMoveUpWards.AddRange(additionalRowsTiShiftUp);
                    }
                    else
                    {
                        var additionalRowsTiShiftUp = vm.reportModels.Where(f => f.RowOrderNum > rowSetToMoveUpWards.First().RowOrderNum).ToList();
                        rowSetToMoveUpWards.AddRange(additionalRowsTiShiftUp);
                    }

                    var previousSibling = vm.backUpReportModels.Where(f => f.RowOrderNum < vm.newRow.RowOrderNum && f.ParentRowOrder == vm.newRow.ParentRowOrder).LastOrDefault();
                    var rowSetToMoveDownWards = vm.reportModels.Where(f => f.RowOrderNum == previousSibling.RowOrderNum).ToList(); // just the first row in the group thats going down
                    var additionalRowsTiShiftDown = vm.reportModels.Where(f => f.RowOrderNum > rowSetToMoveDownWards.First().RowOrderNum && f.RowOrderNum < rowSetToMoveUpWards.First().RowOrderNum).ToList();
                    rowSetToMoveDownWards.AddRange(additionalRowsTiShiftDown);
                    var fullLength = vm.reportModels.Count;
                    vm.reportModels = vm.reportModels.Except(rowSetToMoveDownWards).Except(rowSetToMoveUpWards).ToList();
                    var insertIndexEnd = -1;
                    for (int i = 0; i < vm.backUpReportModels.Count; i++)
                    {
                        var rowOrderFromBackUp = vm.backUpReportModels[i].RowOrder;
                        var correspondingRowInRealArray = vm.reportModels.Where(f => f.RowOrder == rowOrderFromBackUp);
                        if (!correspondingRowInRealArray.Any())
                        {
                            vm.reportModels.InsertRange(i, rowSetToMoveUpWards);
                            insertIndexEnd = i + rowSetToMoveUpWards.Count;
                            break;
                        }
                    }

                    //adjust the row orders and the parentroworders in the array
                    for (int i = 0; i < vm.reportModels.Count; i++)
                    {
                        if ((i == 0 && vm.reportModels[i].RowOrder == "1") || i >= insertIndexEnd)
                        {
                            continue;
                        }

                        // set the row order
                        vm.reportModels[i].RowOrder = i > 0 ? (Convert.ToInt32(vm.reportModels[i - 1].RowOrder) + 1).ToString() : "1";
                        vm.reportModels[i].RowOrderNum = Convert.ToInt32(vm.reportModels[i].RowOrder);
                        //set the parentRowOrder
                        if (vm.reportModels[i].Levels != "1")
                        {
                            //find the roworder the of parent now and set it to the current element's parent row order
                            var rowOrderOfParent = vm.reportModels.Where(f => f.UniqueRowID == vm.reportModels[i].ParentUniqueID).FirstOrDefault().RowOrder;
                            vm.reportModels[i].ParentRowOrder = rowOrderOfParent;
                        }
                    }

                    for (int i = 0; i < vm.backUpReportModels.Count; i++)
                    {
                        var rowOrderFromBackUp = vm.backUpReportModels[i].RowOrder;
                        var correspondingRowInRealArray = vm.reportModels.Where(f => f.RowOrder == rowOrderFromBackUp);
                        if (!correspondingRowInRealArray.Any())
                        {
                            vm.reportModels.InsertRange(i, rowSetToMoveDownWards);
                            break;
                        }
                    }

                    //adjust the row orders and the parentroworders in the array
                    for (int i = 0; i < vm.reportModels.Count; i++)
                    {
                        if (i == 0 && vm.reportModels[i].RowOrder == "1")
                        {
                            continue;
                        }

                        // set the row order
                        vm.reportModels[i].RowOrder = (Convert.ToInt32(vm.reportModels[i - 1].RowOrder) + 1).ToString();
                        vm.reportModels[i].RowOrderNum = Convert.ToInt32(vm.reportModels[i].RowOrder);
                        //set the parentRowOrder
                        if (vm.reportModels[i].Levels != "1")
                        {
                            //find the roworder the of parent now and set it to the current element's parent row order
                            var rowOrderOfParent = vm.reportModels.Where(f => f.UniqueRowID == vm.reportModels[i].ParentUniqueID).FirstOrDefault().RowOrder;
                            vm.reportModels[i].ParentRowOrder = rowOrderOfParent;
                        }
                    }
                }
                else if (vm.Action == "moveDown")
                {
                    //if(vm.newRow.Levels == "1")
                    //{
                    //    var 
                    //}
                    var rowSetToMoveDown = vm.reportModels.Where(f => f.RowOrder == vm.newRow.RowOrder).ToList();
                    var nextSibling = vm.reportModels.Where(f => f.ParentRowOrder == vm.newRow.ParentRowOrder && f.RowOrderNum > vm.newRow.RowOrderNum).FirstOrDefault();
                    var addtionalRowsToShiftDown = new List<PWSWebApi.ViewModel.Models.Administration.ReportModel>();
                    addtionalRowsToShiftDown = vm.reportModels.Where(f => f.RowOrderNum > rowSetToMoveDown.FirstOrDefault().RowOrderNum && f.RowOrderNum < nextSibling.RowOrderNum).ToList();
                    rowSetToMoveDown.AddRange(addtionalRowsToShiftDown);
                    var rowSetToMoveUp = vm.reportModels.Where(f => f.RowOrder == nextSibling.RowOrder).ToList();
                    var remainingItems = vm.reportModels.Where(f => f.RowOrderNum > rowSetToMoveUp.FirstOrDefault().RowOrderNum);
                    var nextRowWithHigherRank = vm.reportModels.Where(f => f.LevelsNum <= vm.newRow.LevelsNum && f.RowOrderNum > vm.newRow.RowOrderNum).ToList();
                    var additionalRowsTiShiftUp = new List<PWSWebApi.ViewModel.Models.Administration.ReportModel>();
                    var secondSiblingFound = false;
                    foreach (var item in remainingItems)
                    {
                        if (item.LevelsNum <= rowSetToMoveUp.FirstOrDefault().LevelsNum)
                        {
                            break;
                        }

                        additionalRowsTiShiftUp.Add(item);
                    }

                    rowSetToMoveUp.AddRange(additionalRowsTiShiftUp);

                    vm.reportModels = vm.reportModels.Except(rowSetToMoveDown).Except(rowSetToMoveUp).ToList();
                    var insertIndexEnd = -1;
                    for (int i = 0; i < vm.backUpReportModels.Count; i++)
                    {
                        var rowOrderFromBackUp = vm.backUpReportModels[i].RowOrder;
                        var correspondingRowInRealArray = vm.reportModels.Where(f => f.RowOrder == rowOrderFromBackUp);
                        if (!correspondingRowInRealArray.Any())
                        {
                            vm.reportModels.InsertRange(i, rowSetToMoveUp);
                            insertIndexEnd = i + rowSetToMoveUp.Count;
                            break;
                        }
                    }

                    //adjust the row orders and the parentroworders in the array
                    for (int i = 0; i < vm.reportModels.Count; i++)
                    {
                        if ((i == 0 && vm.reportModels[i].RowOrder == "1") || i >= insertIndexEnd)
                        {
                            continue;
                        }

                        // set the row order
                        vm.reportModels[i].RowOrder = i > 0 ? (Convert.ToInt32(vm.reportModels[i - 1].RowOrder) + 1).ToString() : "1";
                        vm.reportModels[i].RowOrderNum = Convert.ToInt32(vm.reportModels[i].RowOrder);
                        //set the parentRowOrder
                        if (vm.reportModels[i].Levels != "1")
                        {
                            //find the roworder the of parent now and set it to the current element's parent row order
                            var rowOrderOfParent = vm.reportModels.Where(f => f.UniqueRowID == vm.reportModels[i].ParentUniqueID).FirstOrDefault().RowOrder;
                            vm.reportModels[i].ParentRowOrder = rowOrderOfParent;
                        }
                    }

                    for (int i = 0; i < vm.backUpReportModels.Count; i++)
                    {
                        var rowOrderFromBackUp = vm.backUpReportModels[i].RowOrder;
                        var correspondingRowInRealArray = vm.reportModels.Where(f => f.RowOrder == rowOrderFromBackUp);
                        if (!correspondingRowInRealArray.Any())
                        {
                            vm.reportModels.InsertRange(i, rowSetToMoveDown);
                            break;
                        }
                    }

                    //adjust the row orders and the parentroworders in the array
                    for (int i = 0; i < vm.reportModels.Count; i++)
                    {
                        if (i == 0 && vm.reportModels[i].RowOrder == "1")
                        {
                            continue;
                        }

                        // set the row order
                        vm.reportModels[i].RowOrder = (Convert.ToInt32(vm.reportModels[i - 1].RowOrder) + 1).ToString();
                        vm.reportModels[i].RowOrderNum = Convert.ToInt32(vm.reportModels[i].RowOrder);
                        //set the parentRowOrder
                        if (vm.reportModels[i].Levels != "1")
                        {
                            //find the roworder the of parent now and set it to the current element's parent row order
                            var rowOrderOfParent = vm.reportModels.Where(f => f.UniqueRowID == vm.reportModels[i].ParentUniqueID).FirstOrDefault().RowOrder;
                            vm.reportModels[i].ParentRowOrder = rowOrderOfParent;
                        }
                    }
                }

                if (vm.Action == "save")
                {
                    List<RptModel_tmp> rptModelTmps = new List<RptModel_tmp>();
                    foreach (var row in vm.reportModels)
                    {
                        var newRow = new RptModel_tmp();
                        newRow.RowOrder = Convert.ToInt32(row.RowOrder);
                        newRow.ParentRowOrder = Convert.ToInt32(row.ParentRowOrder);
                        newRow.Levels = Convert.ToInt32(row.Levels);
                        newRow.Title = row.ModelTitle;
                        newRow.Description = row.Description;
                        newRow.AttribTypeID = Convert.ToInt32(row.AttribTypeID);
                        newRow.AttribNameID_ = Convert.ToInt32(row.AttribNameID);
                        newRow.ModelTitle = row.Title;
                        newRow.UserGroupID = Convert.ToInt32(row.UserGroupID);
                        if (newRow.UserGroupID == 0)
                        {
                            newRow.UserGroupID = Convert.ToInt32(vm.reportModels.First().UserGroupID);
                        }

                        newRow.UserID = Convert.ToInt32(userID);
                        newRow.AttribOrder = Convert.ToInt32(row.AttribOrder);
                        rptModelTmps.Add(newRow);
                    }

                    //_contextPWS.RptModel_tmps.InsertAllOnSubmit(rptModelTmps); // saving to tmp table
                    //_contextPWS.SubmitChanges();
                    // now save in the perm table
                    //_contextPWS.usp_RptModel_Insert(Convert.ToInt32(userID), vm.reportModels[0].Title);

                    var responseDatanew = new { error = false, saved = true };
                    var jsonResponsenew = JsonConvert.SerializeObject(responseDatanew);

                    var responsenew = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(jsonResponsenew, Encoding.UTF8, "application/json")
                    };
                    return responsenew;
                }

                //iterate again to fix the attribute order and set flag for upward and/or downward arrow signs
                for (int i = 0; i < vm.reportModels.Count; i++)
                {
                    var attrbOrderMaxForLevel = vm.reportModels.Where(f => f.Levels == vm.reportModels[i].Levels && vm.reportModels[i].RowOrderNum >= f.RowOrderNum).Count();
                    vm.reportModels[i].AttribOrder = Convert.ToString(attrbOrderMaxForLevel * 10);
                    if (vm.reportModels[i].Levels == "1")
                    {
                        var siblingsAbove = vm.reportModels.Where(f => f.ParentRowOrder == null && f.RowOrderNum < vm.reportModels[i].RowOrderNum).ToList();
                        vm.reportModels[i].CanMoveUp = siblingsAbove.Any();
                        var siblingsBelow = vm.reportModels.Where(f => f.ParentRowOrder == null && f.RowOrderNum > vm.reportModels[i].RowOrderNum).ToList();
                        vm.reportModels[i].CanMoveDown = siblingsBelow.Any();
                    }
                    else
                    {
                        var siblingsAbove = vm.reportModels.Where(f => f.ParentRowOrder != null && f.ParentRowOrder == vm.reportModels[i].ParentRowOrder && f.RowOrderNum < vm.reportModels[i].RowOrderNum).ToList();
                        vm.reportModels[i].CanMoveUp = siblingsAbove.Any();
                        var siblingsBelow = vm.reportModels.Where(f => f.ParentRowOrder != null && f.ParentRowOrder == vm.reportModels[i].ParentRowOrder && f.RowOrderNum > vm.reportModels[i].RowOrderNum).ToList();
                        vm.reportModels[i].CanMoveDown = siblingsBelow.Any();
                    }
                }

                var currentSuperParentRow = "";
                vm.reportModels.ForEach(modelRow =>
                {
                    if (modelRow.Levels == "1")
                    {
                        currentSuperParentRow = modelRow.RowOrder;
                    }

                    modelRow.SuperParentRowOrder = currentSuperParentRow;
                });


                var responseData = new { error = false, data = vm.reportModels };
                var jsonResponse = JsonConvert.SerializeObject(responseData);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;               
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api" });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        public string callProcedure(String procedureName, Dictionary<string, dynamic> pars)
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
                Console.WriteLine(ex.ToString());
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api" });
                return ex.Message;
            }
        }
    }
}