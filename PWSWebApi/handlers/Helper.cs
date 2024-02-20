using JWT;
using JWT.Algorithms;
using JWT.Exceptions;
using Newtonsoft.Json;
using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Reflection;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.IO;
using System.Web.Http;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using PWSWebApi.Domains;
using Microsoft.Extensions.Configuration;
using PWSWebApi.Domains.DBContext;

namespace PWSWebApi.handlers
{
    public class Helper
    {        
        static string connectionString;
        static string apiHost;

        public Helper(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("PWSProd");
            apiHost = configuration.GetSection("appSettings")["ApiHost"];
        }

        internal object ValidateSource(HttpRequest request)
        {
            var isAuthorized = true;
            var context = request.HttpContext;

            // Check if the property exists in HttpContext
            if (context.Items.TryGetValue("isvalid", out var isValidObj))
            {
                if (isValidObj is bool isValid)
                {
                    isAuthorized = isValid;
                }
            }

            if (!isAuthorized)
            {
                // Return an unauthorized response
                return new ObjectResult(request)
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            else
            {
                // You may want to return null or some other meaningful result
                return null;
            }
        }

        internal string ReturnToken(HttpRequest request)
        {
            return request.Headers["accessToken"].FirstOrDefault() ?? string.Empty;
        }

        public string RefreshToken(HttpRequest request, string userID)
        {
            var jwt = request.Headers["accessToken"].FirstOrDefault() ?? string.Empty;
            jwt = string.IsNullOrEmpty(jwt) && request.Headers.ContainsKey("Authorization") ? request.Headers["Authorization"] : jwt;
            if (!string.IsNullOrEmpty(jwt) && jwt != "null")
            {
                jwt = jwt.Replace("\"", string.Empty);
            }

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt).Payload;
            //check for token expiration
            var expiration = GetExpiryTimestamp(token["exp"].ToString());
            var timeUtc = DateTime.UtcNow;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);
            if (expiration > easternTimeNow)
            {
                HttpClient client = new HttpClient();
                //specify to use TLS 1.3 as default connection
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                HttpResponseMessage response = client.GetAsync(apiHost + "authenticate/" + userID + "/refresh-tokens").Result;
                var result = response.Content.ReadAsStringAsync().Result;
                var responseMetaData = JsonConvert.DeserializeObject<ResponseMetaData<List<TokenMeta>>>(result);
                if (responseMetaData.result.Any())
                {
                    return responseMetaData.result.FirstOrDefault().accessCode;
                }
            }
            return null;
        }

        internal string ValidateToken(HttpRequest request, string userID = "")
        {
            try
            {
                var jwt = request.Headers["accessToken"].FirstOrDefault() ?? string.Empty;
                jwt = string.IsNullOrEmpty(jwt) && request.Headers.ContainsKey("Authorization") ? request.Headers["Authorization"] : jwt;

                if (string.IsNullOrEmpty(jwt) || jwt == "null")
                    return "Token missing";

                jwt = jwt.Replace("\"", string.Empty);

                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt);

                // Check for token expiration
                var expiration = token.ValidTo;
                var timeUtc = DateTime.UtcNow;
                TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                DateTime easternTimeNow = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);

                if (expiration < easternTimeNow && !IncreaseReactSession(jwt))
                    return "Token has expired";

                // Check for user match
                if (!string.IsNullOrEmpty(userID))
                {
                    var userIDFromToken = token.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
                    if (!string.Equals(userID.Trim(), userIDFromToken, StringComparison.OrdinalIgnoreCase))
                        return "Access denied, invalid User ID detected";
                }

                // Set session token cookie
                var response = request.HttpContext.Response;
                response.Cookies.Append("session-token", jwt, new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddDays(2),
                    Domain = request.Host.Host,
                    Path = "/"
                });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api" });
                return "An error occurred while validating token";
            }

            return string.Empty;
        }

        public bool IncreaseReactSession(string accessCode)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessCode);
                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36");
                HttpResponseMessage response = client.GetAsync(apiHost + "general/session").Result;
                var result = response.Content.ReadAsStringAsync().Result;
                var responseMetaData = JsonConvert.DeserializeObject<ResponseMetaData<bool>>(result);
                return responseMetaData.status == 200 && responseMetaData.result;
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api" });
                return false;
            }
        }

        public DateTime GetExpiryTimestamp(string expiration)
        {
            try
            {
                System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddSeconds(Convert.ToDouble(expiration)).ToLocalTime();
                return dtDateTime;
            }
            catch (TokenExpiredException)
            {
                return DateTime.MinValue;
            }
            catch (SignatureVerificationException)
            {
                return DateTime.MinValue;
            }
            catch (Exception ex)
            {
                // ... remember to handle the generic exception ...
                return DateTime.MinValue;
            }
        }

        public static void AddApiLogs(string connectionString, ApiLogObject apiLog)
        {
            try
            {
                DataClasses1DataContext _context = new DataClasses1DataContext(connectionString);
                var logTableRow = new ApiLog
                {
                    Exception = string.IsNullOrEmpty(apiLog.Exception) ? string.Empty : apiLog.Exception,
                    Method = string.IsNullOrEmpty(apiLog.Method) ? string.Empty : apiLog.Method,
                    Query = string.IsNullOrEmpty(apiLog.Query) ? string.Empty : apiLog.Query,
                    UserID = string.IsNullOrEmpty(apiLog.UserID) ? string.Empty : apiLog.UserID,
                    Params = string.IsNullOrEmpty(apiLog.Params) ? string.Empty : apiLog.Params,
                    UpdatedOn = DateTime.Now
                };
                _context.ApiLogs.Add(logTableRow);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
            }
        }

        public static String callProcedure(String procedureName, Dictionary<string, dynamic> pars, string connString = "")
        {
            var userID = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(connString))
                {
                    connString = connectionString;
                }

                using (SqlConnection conn = new SqlConnection(connString))
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
                            if (kvp.Key.ToLower().Replace("@", string.Empty) == "userid")
                            {
                                userID = Convert.ToString(kvp.Value);
                            }
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
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Query = procedureName, Params = "web api", UserID = userID, UpdatedOn = DateTime.Now });
                return string.Empty;
            }
        }

        public static bool IgnoreAsNullParameter(string paramValue)
        {
            if (string.IsNullOrEmpty(paramValue) || string.IsNullOrWhiteSpace(paramValue))
            {
                return true;
            }

            return paramValue.Trim().Equals("null", StringComparison.OrdinalIgnoreCase) || paramValue.Trim().Equals("undefined", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsDate1OnOrBeforeDate2(string dateText1, string dateText2, bool compareDateOnly = false)
        {
            var date1 = Convert.ToDateTime(dateText1);
            var date2 = Convert.ToDateTime(dateText2);
            return compareDateOnly ? date1.Date <= date2.Date : date1 <= date2;
        }

        public static bool IsOneNullFromPair(string text1, string text2) => (!IgnoreAsNullParameter(text1) && IgnoreAsNullParameter(text2)) || (IgnoreAsNullParameter(text1) && !IgnoreAsNullParameter(text2));
        public static bool IsBothNonNull(string text1, string text2) => !IgnoreAsNullParameter(text1) && !IgnoreAsNullParameter(text2);
        //public static byte[] CreateExcelFile(string fileName)
        //{
        //    var newFile = new FileInfo(fileName);
        //    byte[] bytes;
        //    ExcelPackage.LicenseContext = LicenseContext.Commercial;
        //    using (ExcelPackage xlPackage = new ExcelPackage())
        //    {
        //        // do work here
        //        ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets.Add("test");
        //        worksheet.Cells["A1"].Value = "test";
        //        xlPackage.Save();
        //        bytes = xlPackage.GetAsByteArray();
        //    }

        //    return bytes;
        //}

        public static bool IsValidDate(DateTime? dateTime)
        {
            if (dateTime == null)
                return false;
            DateTime parsedDate;
            return DateTime.TryParse(dateTime.ToString(), out parsedDate);
        }

        public static string GetValidationForProp(string propName, string propDisplayName = null, string customMessageTemplate = null)
        {
            return $"{propDisplayName ?? propName} {customMessageTemplate}";
        }

        public static bool IsInteger(string value)
        {
            if (IgnoreAsNullParameter(value))
            {
                return false;
            }

            int parsedInt;
            return int.TryParse(value, out parsedInt);
        }

        public static bool IsNumber(string value)
        {
            if (IgnoreAsNullParameter(value))
            {
                return false;
            }

            decimal parsedNumber;
            return decimal.TryParse(value, out parsedNumber);
        }

        public static bool IsEqualOrSmallerThan999(string value)
        {
            if (!IsNumber(value))
            {
                return false;
            }

            decimal parsedNumber = Convert.ToDecimal(value);
            return parsedNumber <= 999;
        }
    }

    public class JwtToken
    {
        public long exp { get; set; }
    }
}