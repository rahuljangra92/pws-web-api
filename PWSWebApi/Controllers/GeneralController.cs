using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PWSWebApi.Domains;
using PWSWebApi.Domains.DBContext;
using PWSWebApi.handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

namespace PWSWebApi.Controllers
{
    [Route("general")]
    [ApiController]
    public class GeneralController : ControllerBase
    {
        string connectionString;
        public Helper handlerHelper;
        public GeneralController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("PWSProd");
            handlerHelper = new Helper(configuration);
        }
        [HttpGet]
        [Route("currentUserInfo")]
        public HttpResponseMessage GetUserInfo([FromQuery] int UserID)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
                {
                    return (HttpResponseMessage)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent(handlerHelper.ValidateToken(Request, UserID.ToString()))
                    };
                }
                var options = new DbContextOptionsBuilder<DataClasses1DataContext>().UseSqlServer(connectionString).Options;
                var _context = new DataClasses1DataContext(options);
                //_context.CommandTimeout = 3600;
                var users = _context.Users.Where(f => f.UserID == UserID && f.RetiredDateTime == null);
                users.ToList().ForEach(user =>
                {
                    user.UserPassword = null; // ensure user password is not returned as part of the response
                });
               
                var jsonResponse = JsonConvert.SerializeObject(users);
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;

                // return Request.CreateResponse(HttpStatusCode.OK, users);
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = ex }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost]
        [Route("signout")]
        public HttpResponseMessage SignOut([FromQuery] int UserID, [FromQuery] string session)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
                {
                    return (HttpResponseMessage)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent(handlerHelper.ValidateToken(Request, UserID.ToString()))
                    };
                }

                var options = new DbContextOptionsBuilder<DataClasses1DataContext>().UseSqlServer(connectionString).Options;
                var _context = new DataClasses1DataContext(options);
                //_context.CommandTimeout = 3600;
                var reactSessionToLogOut = _context.SiteMonitors.Where(f => f.Session == session && f.UserID == UserID);
                var recordUpatedCount = 0;
                if (reactSessionToLogOut.Any())
                {
                    var logOutNumbeAssignment = 1;
                    var logOutMuberExistForSession = true;
                    while (logOutMuberExistForSession)
                    {
                        if (_context.SiteMonitors.Where(f => f.UserID == UserID && f.Session == session + "-" + logOutNumbeAssignment.ToString()).Any())
                        {
                            logOutNumbeAssignment++;
                        }
                        else
                        {
                            logOutMuberExistForSession = false;
                        }
                    }

                   // _context.usp_Invalidate_Session(UserID, session, session + "-" + logOutNumbeAssignment);
                }

               // return Request.CreateResponse(HttpStatusCode.OK, new { success = true });
                //isActive = true;
                var jsonResponse = JsonConvert.SerializeObject(true);
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                };
                return response;

            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = ex }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpGet]
        [Route("useractive")]
        public HttpResponseMessage DetectActiveUser([FromQuery] int UserID, [FromQuery] string session)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
                {
                    return (HttpResponseMessage)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent(handlerHelper.ValidateToken(Request, UserID.ToString()))
                    };
                }

                var isActive = true;
                var options = new DbContextOptionsBuilder<DataClasses1DataContext>().UseSqlServer(connectionString).Options;
                var _context = new DataClasses1DataContext(options);
                //_context.CommandTimeout = 3600;
                var entriedForThisuser = _context.Users.Where(f => f.UserID == UserID).ToList();
                isActive = entriedForThisuser.Any();
                if (isActive)
                {
                    isActive = entriedForThisuser.Where(f => f.RetiredDateTime == null).Any();
                }

                if (isActive)
                {
                    var monitor = _context.SiteMonitors.Where(f => f.UserID == UserID && f.Session == session).OrderByDescending(f => f.LoadTimeStamp).ToList();
                    isActive = monitor.Any();
                    if (isActive)
                    {
                        //find the most recent Angular seesion for this user and see if thats not matching
                        var mostRecentAngSessionIDForThisUser = _context.SiteMonitors.Where(f => f.Site == "Angular" && f.UserID == UserID && f.HREF.ToLower().Contains("/pws/") && f.AutomatedPing == false).OrderByDescending(f => f.LoadTimeStamp).Take(1);
                        isActive = mostRecentAngSessionIDForThisUser.Any();
                        if (isActive)
                        {
                            // if the latest Angular session does not match, current react session wont work
                            isActive = mostRecentAngSessionIDForThisUser.FirstOrDefault().Session == session;
                        }
                    }

                    if (isActive)
                    {
                        //lets check if enough time has elapsed so the session should expire
                        var mostRecentSelectedAngSessionIDForThisUser = _context.SiteMonitors.Where(f => f.Site == "Angular" && f.HREF.ToLower().Contains("/pws/") && f.UserID == UserID && f.Session == session && f.AutomatedPing == false).OrderByDescending(f => f.LoadTimeStamp).Take(1);
                        var timeUtc = DateTime.UtcNow;
                        TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                        DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);
                        var timeThresholdForBrowsing = entriedForThisuser.FirstOrDefault().BrowserTimeoutThreshold;
                        if (timeThresholdForBrowsing == null)
                        {
                            PwsConfigs value = _context.PwsConfigs.FirstOrDefault(f => f.Name == "ReactTimeOutThreshold");
                            timeThresholdForBrowsing = Convert.ToInt32(value);
                        }

                        if (mostRecentSelectedAngSessionIDForThisUser.Any())
                        {
                            var timeDIfference = (easternTime - mostRecentSelectedAngSessionIDForThisUser.FirstOrDefault()?.LoadTimeStamp)?.TotalMinutes;
                            isActive = timeDIfference < timeThresholdForBrowsing;
                        }
                    }
                }

                isActive = true; 
                var jsonResponse = JsonConvert.SerializeObject(isActive);
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse,Encoding.UTF8, "application/json")
                };
                return response;
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = ex }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpGet]
        [Route("userprefdates")]
        public HttpResponseMessage GetUserPrefDates(int UserID)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
                {
                    return (HttpResponseMessage)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent(handlerHelper.ValidateToken(Request, UserID.ToString()))
                    };
                }
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                var data = Helper.callProcedure("usp_UserPref_Dates", pars);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ObjectContent<object>(
                      new { results = data, deleted = true, error = false },
                      new System.Net.Http.Formatting.JsonMediaTypeFormatter()
                  )
                };
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = ex }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }
    }
}