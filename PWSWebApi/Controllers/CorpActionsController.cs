using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OfficeOpenXml;
using PWSWebApi.handlers;
using PWSWebApi.Models;
using PWSWebApi.Models.Imports2;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace PWSWebApi.Controllers
{
    [Route("corpactions")]
    [ApiController]
    public class CorpActionsController : ControllerBase
    {
        //static string pwsRectConnectionString = System.Configuration.ConfigurationManager.AppSettings["PWSRec"].ToString();
        static string connectionString;// = System.Configuration.ConfigurationManager.AppSettings["PWSProd"].ToString();
        private readonly Helper handlerHelper;

        public CorpActionsController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("PWSProd");
            handlerHelper = new Helper(configuration);
        }

        private bool NoChildrenSent(CorpActionSearchRequest request)
        {
            return request.Child1 == null && request.Child2 == null && request.Child3 == null && request.Child4 == null && request.Child5 == null && request.Child6 == null && request.Child7 == null;
        }

        [HttpPost]
        [Route("PostFile")]

        public HttpResponseMessage SubmitFile([FromBody] FileObject fileObject)
        {
            //var s = HttpContext.Current.Request.Files;
            using (MemoryStream memStream = new MemoryStream(fileObject.FileAsBytes))
            {
                //ExcelPackage package = new ExcelPackage();
                //package.Load(memStream);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (ExcelPackage package = new ExcelPackage(memStream))
                {
                    //get the first worksheet in the workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int colCount = worksheet.Dimension.End.Column; //get Column Count
                    int rowCount = worksheet.Dimension.End.Row; //get row count
                    for (int row = 1; row <= rowCount; row++)
                    {
                        for (int col = 1; col <= colCount; col++)
                        {
                            // Console.WriteLine(" Row:" + row + " column:" + col + " Value:" + worksheet.Cells[row, col].Value?.ToString().Trim());
                        }
                    }

                    // download xlsx
                    UTF8Encoding utf8 = new UTF8Encoding();
                    byte[] data = fileObject.FileAsBytes;
                    ByteArrayContent byteContent = new ByteArrayContent(data);
                    HttpResponseMessage response = new(HttpStatusCode.OK)
                    {
                        Content = byteContent
                    };
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = "Sample File.xlsx"
                    };
                    //System.Web.Http.HttpConfiguration config = System.Web.Http.GlobalConfiguration.Configuration;
                    return response;
                }
            }
        }

        [HttpPost("RemoveRow")]
        public HttpResponseMessage DeleteCorpActionRow([FromBody] CorpActionSearchRequest request, int clearResults = 0)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
                {
                    return (HttpResponseMessage)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, request.UserID) != string.Empty)
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent(handlerHelper.ValidateToken(Request, request.UserID))
                    };
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                var responseFromProc = "[]";
                var parsedData = new List<CorpActionSearchRequest>();
                pars.Add("@UserID", request.UserID);
                pars.Add("@ClearResults", clearResults);
                if (clearResults == 0)
                {
                    if (request.IsParent)
                    {
                        pars.Add("@Trial_CorpActionsIDs", request.Trial_CorpActionsID);
                        pars.Add("@CorpActsIDs", request.CorpActsID);
                    }
                    else
                    {
                        pars.Add("@Trial_CorpActionsChildIDs", request.Trial_CorpActionsChildID);
                        pars.Add("@CorpActsChildIDs", request.CorpActsChildID);
                    }
                }

                responseFromProc = Helper.callProcedure("[adm].[usp_CorpActions_Delete]", pars);
                if (string.IsNullOrEmpty(responseFromProc))
                {
                    //return Request.CreateResponse(HttpStatusCode.OK, new { error = false, data = new { originalRequest = request, removalSuccessful = false, removeStatusMessageObject = new ToastObject { Description = "Removal failed due to a database error" } } });
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(new { originalRequest = request, removalSuccessful = false, removeStatusMessageObject = new ToastObject { Description = "Removal failed due to a database error" } }))
                    };
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    return response;
                }

                request.IsDeleted = true;

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ObjectContent<object>(
                                new
                                {
                                    error = false,
                                    data = new
                                    {
                                        originalRequest = request,
                                        removalSuccessful = true,
                                        clearTrialTable = clearResults == 1,
                                        removeStatusMessageObject = new ToastObject
                                        {
                                            Type = "success",
                                            Message = "Deletion Complete",
                                            Description = "Object was removed successfully. It will not return in the next search.",
                                            Duration = 10
                                        }
                                    }
                                }, new System.Net.Http.Formatting.JsonMediaTypeFormatter())
                };
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = request.UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost("Edit")]
        public HttpResponseMessage Update([FromBody] CorpActionSearchRequest request)
        {
            var validationErrorMessages = new List<ToastObject>();
            try
            {
                if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
                {
                    return (HttpResponseMessage)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, request.UserID) != string.Empty)
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent(handlerHelper.ValidateToken(Request, request.UserID))
                    };
                }

                var isParent = request.IsParent;
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                var responseFromProc = "[]";
                var parsedData = new List<CorpActionSearchRequest>();
                var corpActionType = request.CorpActTypes.FirstOrDefault(f => f.CorpActsTypeID.ToString() == request.CorpActsTypeID);
                if (isParent)
                    validationErrorMessages.AddRange(ValidateIncomingNewParentRecord(request));
                pars.Add("@UserID", request.UserID);
                pars.Add("@CorpActsID", request.CorpActsID);
                pars.Add("@CorpActsTypeID", request.CorpActsTypeID);
                if (isParent)
                {
                    if (!string.IsNullOrEmpty(request.Optional) && (request.Optional == "1" || request.Optional.Equals("yes", StringComparison.OrdinalIgnoreCase)))
                    {
                        request.Optional = "1";
                    }
                    else
                        request.Optional = "0";
                    pars.Add("@Optional", request.Optional);
                    if (corpActionType.ExDate > -1)
                        pars.Add("@ExDate", request.ExDate);
                    if (corpActionType.RecordDate > -1)
                        pars.Add("@RecordDate", request.RecordDate);
                    if (corpActionType.PayDate > -1)
                        pars.Add("@PayDate", request.PayDate);
                    pars.Add("@ParentSecID", request.ParentSecID);
                    pars.Add("@ProcessOrder", request.ProcessOrder);
                    if (corpActionType.ParentCashRate > -1)
                        pars.Add("@CashRate", request.CashRate);
                    if (corpActionType.ParentShrRate > -1)
                        pars.Add("@ShrRate", request.ShrRate);
                    if (corpActionType.ParentCostRate > -1)
                        pars.Add("@CostRate", request.CostRate);
                    if (corpActionType.ParentMovementPrice > -1)
                        pars.Add("@MovementPrice", request.MovementPrice);
                    if (!Helper.IgnoreAsNullParameter(request.Periodicity))
                        pars.Add("@PeriodsPerYear", request.Periodicity);
                    pars.Add("@Trial_CorpActionsID", request.Trial_CorpActionsID);
                }
                else
                {
                    // validation for the child record to determine if we can move forward with calling the edit proc later
                    validationErrorMessages.AddRange(ValidateChildRecordWhileEditing(request));
                    // this condition indicates we are adding a brand new child to an existing group
                    if (Helper.IgnoreAsNullParameter(request.CorpActsChildID))
                    {
                        pars.Add("@Trial_CorpActionsID", request.Trial_CorpActionsID);
                    }

                    if (!Helper.IgnoreAsNullParameter(request.CorpActsChildID))
                        pars.Add("@CorpActsChildID", request.CorpActsChildID);
                    if (!Helper.IgnoreAsNullParameter(request.Trial_CorpActionsChildID))
                        pars.Add("@Trial_CorpActionsChildID", request.Trial_CorpActionsChildID);
                    pars.Add("@ChildSecID", request.ChildSecID ?? request.SecID);
                    var targetedCashRate = !request.IsChildRecord ? request.ParentCashRate : request.ChildCashRate;
                    var cashRateValidationMessage = ShouldCheckForValidData(corpActionType?.ParentCashRate, targetedCashRate) && !Helper.IsNumber(targetedCashRate) ? Helper.GetValidationForProp(nameof(request.CashRate), "Cash Rate", "is required and needs to be a number") : string.Empty;
                    var targetedCostRate = !request.IsChildRecord ? request.ParentCostRate : request.ChildCostRate;
                    var costRateValidationMessage = ShouldCheckForValidData(corpActionType?.ParentCostRate, targetedCostRate) && !Helper.IsNumber(targetedCostRate) ? Helper.GetValidationForProp(nameof(request.CostRate), "Cost Rate", "is required and needs to be a number") : string.Empty;
                    var targetedMovementPrice = !request.IsChildRecord ? request.ParentMovementPrice : request.ChildMovementPrice;
                    if (!Helper.IgnoreAsNullParameter(request.ChildCashRate ?? request.CashRate))
                    {
                        var targetedShareRate = !request.IsChildRecord ? request.ParentShrRate : request.ChildShrRate;
                        pars.Add("@ChildCashRate", request.ChildCashRate ?? request.CashRate);
                    }

                    pars.Add("@ChildShrRate", request.ChildShrRate ?? request.ShrRate);
                    pars.Add("@ChildCostRate", request.ChildCostRate ?? request.CostRate);
                    pars.Add("@ChildMovementPrice", request.ChildMovementPrice ?? request.MovementPrice);
                    pars.Add("@ChildProcessOrder", request.ChildProcessOrder ?? request.ProcessOrder);
                }

                if (!validationErrorMessages.Any())
                    responseFromProc = Helper.callProcedure("[adm].[usp_CorpActions_Edit]", pars);
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ObjectContent<object>(
                            new
                            {
                                error = false,
                                data = new
                                {
                                    originalRequest = request,
                                    validationErrorMessages = validationErrorMessages,
                                    updateSuccessful = false,
                                    gridData = new List<CorpActionSearchRequest>(),
                                    editValidationErrorsOccurred = true
                                }
                            },
                            new System.Net.Http.Formatting.JsonMediaTypeFormatter()
                        )
                    };
                }

                if (string.IsNullOrEmpty(responseFromProc))
                {
                    validationErrorMessages.Clear();
                    validationErrorMessages.Add(new ToastObject { Message = "Server Error", Description = "Update failed due to an error" });

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ObjectContent<object>(
                            new
                            {
                                error = false,
                                data = new
                                {
                                    originalRequest = request,
                                    validationErrorMessages = validationErrorMessages,
                                    updateSuccessful = !validationErrorMessages.Any(),
                                    gridData = new List<CorpActionSearchRequest>(),
                                    updateStatusMessageObject = new ToastObject { Description = "Update failed due to an error" }
                                }
                            },
                            new System.Net.Http.Formatting.JsonMediaTypeFormatter()
                        )
                    };
                }

                parsedData = ParseSearchActionItemResponse(responseFromProc);
                var gridData = GetGridData(parsedData, request.PwsIdentification);
                var rowWithActionStatus = gridData.Where(f => !string.IsNullOrEmpty(f.ActionStatus)).FirstOrDefault();
                if (rowWithActionStatus == null)
                {
                    //do nothing
                }
                else if (gridData.Any() && gridData.Any(f => !string.IsNullOrEmpty(f.ActionStatus) && f.ActionStatus.Equals("No records found for search criteria.", StringComparison.OrdinalIgnoreCase)))
                {
                    validationErrorMessages.Add(new ToastObject { Description = gridData.First().ActionStatus });
                    gridData.Clear();
                }

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ObjectContent<object>(
                        new
                        {
                            error = false,
                            data = new
                            {
                                originalRequest = request,
                                validationErrorMessages = validationErrorMessages,
                                updateSuccessful = !validationErrorMessages.Any(),
                                gridData = gridData,
                                updateSuccessfulMessage = "Update Successful"
                            }
                        },
                        new System.Net.Http.Formatting.JsonMediaTypeFormatter()
                    )
                };
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = request.UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        [HttpPost()]
        [Route("Create/Parent")]
        public HttpResponseMessage CreateParent([FromBody] CorpActionSearchRequest request)
        {
            var validationErrorMessages = new List<ToastObject>();
            try
            {
                if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
                {
                    return (HttpResponseMessage)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, request.UserID) != string.Empty)
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent(handlerHelper.ValidateToken(Request, request.UserID))
                    };
                }

                if (!string.IsNullOrEmpty(request.Optional) && (request.Optional == "1" || request.Optional.Equals("yes", StringComparison.OrdinalIgnoreCase)))
                {
                    request.Optional = "1";
                }
                else
                    request.Optional = "0";
                validationErrorMessages.AddRange(ValidateIncomingNewParentRecord(request));
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                var responseFromProc = "[]";
                var parsedData = new List<CorpActionSearchRequest>();

                // lets deal with some of the child records
                validationErrorMessages.AddRange(ValidateAndPopulateChildRecordParams(request, pars, request.Child1, nameof(request.Child1)));
                validationErrorMessages.AddRange(ValidateAndPopulateChildRecordParams(request, pars, request.Child2, nameof(request.Child2)));
                validationErrorMessages.AddRange(ValidateAndPopulateChildRecordParams(request, pars, request.Child3, nameof(request.Child3)));
                validationErrorMessages.AddRange(ValidateAndPopulateChildRecordParams(request, pars, request.Child4, nameof(request.Child4)));
                validationErrorMessages.AddRange(ValidateAndPopulateChildRecordParams(request, pars, request.Child5, nameof(request.Child5)));
                validationErrorMessages.AddRange(ValidateAndPopulateChildRecordParams(request, pars, request.Child6, nameof(request.Child6)));
                validationErrorMessages.AddRange(ValidateAndPopulateChildRecordParams(request, pars, request.Child7, nameof(request.Child7)));

                if (!validationErrorMessages.Any())
                {
                    var corpActionType = request.CorpActTypes.FirstOrDefault(f => f.CorpActsTypeID.ToString() == request.CorpActsTypeID);
                    pars.Add("@UserID", request.UserID);
                    pars.Add("@CorpActsTypeID", request.CorpActsTypeID);
                    if (corpActionType.ExDate > -1)
                        pars.Add("@ExDate", request.ExDate?.ToString("yyyy-MM-dd"));
                    if (corpActionType.PayDate > -1)
                        pars.Add("@PayDate", request.PayDate?.ToString("yyyy-MM-dd"));
                    if (corpActionType.RecordDate > -1)
                        pars.Add("@RecordDate", request.RecordDate?.ToString("yyyy-MM-dd"));
                    if (corpActionType.ParentCashRate > -1)
                        pars.Add("@CashRate", request.CashRate);
                    if (corpActionType.ParentShrRate > -1)
                        pars.Add("@ShrRate", request.ShrRate);
                    if (corpActionType.ParentCostRate > -1)
                        pars.Add("@CostRate", request.CostRate);
                    if (corpActionType.ParentMovementPrice > -1)
                        pars.Add("@MovementPrice", request.MovementPrice);
                    pars.Add("@Optional", request.Optional);
                    pars.Add("@ParentSecID", request.SecID);
                    if (!Helper.IgnoreAsNullParameter(request.Periodicity) && corpActionType.PeriodsPerYear != -1)
                        pars.Add("@PeriodsPerYear", request.Periodicity);
                    var data = Helper.callProcedure("[adm].[usp_CorpActions_Insert]", pars);
                    responseFromProc = data;
                    parsedData = ParseSearchActionItemResponse(data);
                }

                var groupedData = !validationErrorMessages.Any() ? ParseByCorpActionType(parsedData) : new Dictionary<long, IEnumerable<CorpActionSearchRequest>>();
                var gridData = !validationErrorMessages.Any() ? GetGridData(parsedData) : new List<CorpActionSearchRequest>();
                if (gridData.Any() && !validationErrorMessages.Any() && NoChildrenSent(request))
                {
                    var rowWithActionStatus = gridData.Where(f => !string.IsNullOrEmpty(f.ActionStatus)).FirstOrDefault();
                    if (rowWithActionStatus == null)
                    {
                        //do nothing
                    }
                    else if (rowWithActionStatus.ActionStatus.Equals("Already Exists"))
                    {
                        validationErrorMessages.Add(new ToastObject { Description = "Please note: The parent record already exists" });
                        gridData.Clear();
                    }
                    else if (gridData.Any() && gridData.Any(f => !string.IsNullOrEmpty(f.ActionStatus) && f.ActionStatus.Equals("No records found for search criteria.", StringComparison.OrdinalIgnoreCase)))
                    {
                        validationErrorMessages.Add(new ToastObject { Description = gridData.First().ActionStatus });
                        gridData.Clear();
                    }
                }

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ObjectContent<object>(
                        new
                        {
                            error = false,
                            data = new
                            {
                                parsedData = JsonConvert.SerializeObject(parsedData),
                                groupedData = JsonConvert.SerializeObject(groupedData),
                                gridData = JsonConvert.SerializeObject(gridData),
                                validationErrorMessages = validationErrorMessages,
                                insertionSuccessful = !validationErrorMessages.Any(),
                                insertionSuccessMessage = "Record Insertion Successful"
                            }
                        },
                        new System.Net.Http.Formatting.JsonMediaTypeFormatter()
                    )
                };
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = request.UserID });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        private List<ToastObject> ValidateAndPopulateChildRecordParams(CorpActionSearchRequest request, Dictionary<string, dynamic> pars, ChildForPosting child, string instanceName)
        {
            var validationErrorMessages = new List<ToastObject>();
            if (child != null)
            {
                var corpActionType = request.CorpActTypes.FirstOrDefault(f => f.CorpActsTypeID.ToString() == request.CorpActsTypeID);
                if (ShouldCheckForValidData(corpActionType?.ChildSecID, child.ChildSecID))
                {
                    if (Helper.IsNumber(child.ChildSecID))
                        pars.Add($"@{instanceName}SecID", child.ChildSecID);
                    else
                        validationErrorMessages.Add(new ToastObject { Description = Helper.GetValidationForProp(nameof(child.ChildSecID), "Child Sec ID", "is a required number") });
                }

                if (ShouldCheckForValidData(corpActionType?.ChildCashRate, child.ChildCashRate))
                {
                    if (Helper.IsNumber(child.ChildCashRate))
                        pars.Add($"@{instanceName}CashRate", child.ChildCashRate);
                    else
                        validationErrorMessages.Add(new ToastObject { Description = Helper.GetValidationForProp(nameof(child.ChildCashRate), "Child Cash Rate", "is a required number") });
                }

                if (ShouldCheckForValidData(corpActionType?.ChildShrRate, child.ChildShrRate))
                {
                    if (Helper.IsNumber(child.ChildShrRate))
                        pars.Add($"@{instanceName}ShrRate", child.ChildShrRate);
                    else
                        validationErrorMessages.Add(new ToastObject { Description = Helper.GetValidationForProp(nameof(child.ChildShrRate), "Child Share Rate", "is a required number") });
                }

                if (ShouldCheckForValidData(corpActionType?.ChildCostRate, child.ChildCostRate))
                {
                    if (Helper.IsNumber(child.ChildCostRate))
                        pars.Add($"@{instanceName}CostRate", child.ChildCostRate);
                    else
                        validationErrorMessages.Add(new ToastObject { Description = Helper.GetValidationForProp(nameof(child.ChildCostRate), "Child Cost Rate", "is a required number") });
                }

                if (ShouldCheckForValidData(corpActionType?.ChildMovementPrice, child.ChildMovementPrice))
                {
                    if (Helper.IsNumber(child.ChildMovementPrice))
                        pars.Add($"@{instanceName}MovementPrice", child.ChildMovementPrice);
                    else
                        validationErrorMessages.Add(new ToastObject { Description = Helper.GetValidationForProp(nameof(child.ChildMovementPrice), "Child Movement Price", "is a required number") });
                }

                validationErrorMessages = validationErrorMessages.Where(f => !Helper.IgnoreAsNullParameter(f.Description)).ToList();
            }

            return validationErrorMessages;
        }

        private bool ShouldCheckForValidData(int? requiredIndicator, string data)
        {
            if (data != null)
                data = data.Trim();
            if (requiredIndicator == -1)
                return false; //field is forbidden
            if (requiredIndicator == 1)
                return true; //field is required
            if (requiredIndicator == 0 && !string.IsNullOrEmpty(data))
                return true; // optional but non empty data was sent so check for valid data
            return false;
        }

        private List<ToastObject> ValidateIncomingNewParentRecord(CorpActionSearchRequest request)
        {
            var validationErrorMessages = new List<ToastObject>();
            var corpActionType = request.CorpActTypes.FirstOrDefault(f => f.CorpActsTypeID.ToString() == request.CorpActsTypeID);
            var corpActsTypeIdValidationMessage = Helper.IgnoreAsNullParameter(request.CorpActsTypeID) ? Helper.GetValidationForProp(nameof(request.CorpActsTypeID), "Corporate Action Type", "is a required field") : string.Empty;
            validationErrorMessages.Add(new ToastObject { Description = corpActsTypeIdValidationMessage });
            var exDateValidationMessage = ShouldCheckForValidData(corpActionType?.ExDate, Convert.ToString(request.ExDate)) && !Helper.IsValidDate(request.ExDate) ? Helper.GetValidationForProp(nameof(request.ExDate), "Ex Date", "is a required field") : string.Empty;
            validationErrorMessages.Add(new ToastObject { Description = exDateValidationMessage });
            var recordDateValidationMessage = ShouldCheckForValidData(corpActionType?.RecordDate, Convert.ToString(request.RecordDate)) && !Helper.IsValidDate(request.RecordDate) ? Helper.GetValidationForProp(nameof(request.RecordDate), "Record Date", "is a required field") : string.Empty;
            validationErrorMessages.Add(new ToastObject { Description = recordDateValidationMessage });
            var payDateValidationMessage = ShouldCheckForValidData(corpActionType?.PayDate, Convert.ToString(request.PayDate)) && !Helper.IsValidDate(request.PayDate) ? Helper.GetValidationForProp(nameof(request.PayDate), "Pay Date", "is a required field") : string.Empty;
            validationErrorMessages.Add(new ToastObject { Description = payDateValidationMessage });
            var processOrderValidationMessage = !Helper.IsInteger(request.ProcessOrder) ? Helper.GetValidationForProp(nameof(request.ProcessOrder), "Process Order", "is a required integer") : string.Empty;
            validationErrorMessages.Add(new ToastObject { Description = processOrderValidationMessage });
            var securityValidationMessage = !Helper.IsInteger(request.SecID) ? Helper.GetValidationForProp(nameof(request.SecID), null, "Security is required") : string.Empty;
            validationErrorMessages.Add(new ToastObject { Description = securityValidationMessage });
            var targetedShareRate = !request.IsChildRecord ? request.ParentShrRate : request.ShrRate;
            var shareRateValidationMessage = ShouldCheckForValidData(corpActionType?.ParentShrRate, targetedShareRate) && !Helper.IsEqualOrSmallerThan999(targetedShareRate) ? Helper.GetValidationForProp(nameof(request.ShrRate), "Share Rate", "is required and needs to be a number less than or equal to 999") : string.Empty;
            validationErrorMessages.Add(new ToastObject { Description = shareRateValidationMessage });
            var targetedCashRate = !request.IsChildRecord ? request.ParentCashRate : request.CashRate;
            var cashRateValidationMessage = ShouldCheckForValidData(corpActionType?.ParentCashRate, targetedCashRate) && !Helper.IsNumber(targetedCashRate) ? Helper.GetValidationForProp(nameof(request.CashRate), "Cash Rate", "is required and needs to be a number") : string.Empty;
            validationErrorMessages.Add(new ToastObject { Description = cashRateValidationMessage });
            var targetedCostRate = !request.IsChildRecord ? request.ParentCostRate : request.CostRate;
            var costRateValidationMessage = ShouldCheckForValidData(corpActionType?.ParentCostRate, targetedCostRate) && !Helper.IsEqualOrSmallerThan999(targetedCostRate) ? Helper.GetValidationForProp(nameof(request.CostRate), "Cost Rate", "is required and needs to be a number less than or equal to 999") : string.Empty;
            validationErrorMessages.Add(new ToastObject { Description = costRateValidationMessage });
            var targetedMovementPrice = !request.IsChildRecord ? request.ParentMovementPrice : request.MovementPrice;
            var movementPriceValidationMessage = ShouldCheckForValidData(corpActionType?.ParentMovementPrice, targetedMovementPrice) && !Helper.IsNumber(targetedMovementPrice) ? Helper.GetValidationForProp(nameof(request.MovementPrice), "Movement Price", "is required and needs to be a number") : string.Empty;
            validationErrorMessages.Add(new ToastObject { Description = movementPriceValidationMessage });
            var periodicityMessage = request.ShouldShowPeriodicityDropdown && (Helper.IgnoreAsNullParameter(request.Periodicity) || !Helper.IsNumber(request.Periodicity)) ? Helper.GetValidationForProp(nameof(request.Periodicity), null, "is a required field") : string.Empty;
            validationErrorMessages.Add(new ToastObject { Description = periodicityMessage });
            validationErrorMessages = validationErrorMessages.Where(f => !Helper.IgnoreAsNullParameter(f.Description)).ToList();
            return validationErrorMessages;
        }

        [HttpGet]
        [Route("Search")]
        public HttpResponseMessage SearchCorpActions(int UserID, bool pwsIdentification, string selectedSecIds = null, string selectedCorpActIds = null, string ExDateStart = null, string ExDateEnd = null, string PayDateStart = null, string PayDateEnd = null, string RecDateStart = null, string RecDateEnd = null)
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

                //validate input starting
                var validationErrorMessages = new List<ToastObject>();
                var haveSecIDs = !Helper.IgnoreAsNullParameter(selectedSecIds);
                var haveCorpActsIDs = !Helper.IgnoreAsNullParameter(selectedCorpActIds);
                //must have selected Security or corp action id selected at least
                if (!haveSecIDs && !haveCorpActsIDs)
                {
                    validationErrorMessages.Add(new ToastObject { Description = "Must select at least one Security or Corp Action TypeID" });
                    goto callProc;
                }

                // just secID selected but not corp action selected so valid input
                if (haveSecIDs && !haveCorpActsIDs)
                    goto validateDates;
                // if corp action Id is selected but no secid is selected, then there needs to be at least one pair of dates selected
                else if (haveCorpActsIDs && !haveSecIDs)
                {
                    if (Helper.IgnoreAsNullParameter(ExDateStart) && Helper.IgnoreAsNullParameter(ExDateEnd) && Helper.IgnoreAsNullParameter(RecDateStart) && Helper.IgnoreAsNullParameter(RecDateEnd) && Helper.IgnoreAsNullParameter(PayDateStart) && Helper.IgnoreAsNullParameter(PayDateEnd))
                    {
                        validationErrorMessages.Add(new ToastObject { Description = "Must select at least one pair of date range with corp action Type" });
                        goto callProc;
                    }
                }

            validateDates:
                // whenever a date is selected, it must have the whole pair selected as a range and the start date must NOT be after end date
                if (Helper.IsOneNullFromPair(ExDateStart, ExDateEnd))
                    validationErrorMessages.Add(new ToastObject { Description = "Must select both start and end for Ex Date" });
                else if (Helper.IsBothNonNull(ExDateEnd, ExDateStart) && !Helper.IsDate1OnOrBeforeDate2(ExDateStart, ExDateEnd, true))
                    validationErrorMessages.Add(new ToastObject { Description = "Ex date start must be equal or earlier than Ex date End" });
                if (Helper.IsOneNullFromPair(PayDateStart, PayDateEnd))
                    validationErrorMessages.Add(new ToastObject { Description = "Must select both min and max for Pay Date" });
                else if (Helper.IsBothNonNull(PayDateStart, PayDateEnd) && !Helper.IsDate1OnOrBeforeDate2(PayDateStart, PayDateEnd, true))
                    validationErrorMessages.Add(new ToastObject { Description = "Pay date start must be equal or earlier than Ex date End" });
                if (Helper.IsOneNullFromPair(RecDateStart, RecDateEnd))
                    validationErrorMessages.Add(new ToastObject { Description = "Must select both min and max for Record Date" });
                else if (Helper.IsBothNonNull(RecDateStart, RecDateEnd) && !Helper.IsDate1OnOrBeforeDate2(RecDateStart, RecDateEnd, true))
                    validationErrorMessages.Add(new ToastObject { Description = "Record date start must be equal or earlier than Record date End" });
                // ending input validation
                callProc:
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                if (!Helper.IgnoreAsNullParameter(selectedCorpActIds))
                {
                    var collection = selectedCorpActIds.Split(',');
                    var corpActsIds = collection.Select(f => f.Split('-')[0].ToString()).ToList();
                    var corpActsIdsCommadelimited = string.Join(",", corpActsIds);
                    pars.Add("@CorpActsTypeIDs", corpActsIdsCommadelimited);
                }

                pars.Add("@SecIDs", selectedSecIds);
                if (!Helper.IgnoreAsNullParameter(ExDateStart))
                    pars.Add("@ExDateStart", ExDateStart);
                if (!Helper.IgnoreAsNullParameter(ExDateEnd))
                    pars.Add("@ExDateEnd", ExDateEnd);
                if (!Helper.IgnoreAsNullParameter(PayDateStart))
                    pars.Add("@PayDateStart", PayDateStart);
                if (!Helper.IgnoreAsNullParameter(PayDateEnd))
                    pars.Add("@PayDateEnd", PayDateEnd);
                if (!Helper.IgnoreAsNullParameter(RecDateStart))
                    pars.Add("@RecordDateStart", RecDateStart);
                if (!Helper.IgnoreAsNullParameter(RecDateEnd))
                    pars.Add("@RecordDateEnd", RecDateEnd);
                var data = !validationErrorMessages.Any() ? Helper.callProcedure("[adm].[usp_CorpActions_Search]", pars) : string.Empty;
                var parsedData = !validationErrorMessages.Any() ? ParseSearchActionItemResponse(data) : new List<CorpActionSearchRequest>();
                var groupedData = !validationErrorMessages.Any() ? ParseByCorpActionType(parsedData) : new Dictionary<long, IEnumerable<CorpActionSearchRequest>>();
                // var gridData = !validationErrorMessages.Any() ? GetGridData(groupedData) : new List<CorpActionSearchRequest>();
                var gridData = !validationErrorMessages.Any() ? GetGridData(parsedData, pwsIdentification) : new List<CorpActionSearchRequest>();
                if (gridData.Any() && gridData.Any(f => !string.IsNullOrEmpty(f.ActionStatus) && f.ActionStatus.Equals("No records found for search criteria.", StringComparison.OrdinalIgnoreCase)))
                {
                    validationErrorMessages.Add(new ToastObject { Message = "Search Results", Description = gridData.First().ActionStatus, Type = "info" });
                    gridData.Clear();
                }

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ObjectContent<object>(
                        new
                        {
                            error = false,
                            data = new
                            {
                                parsedData = JsonConvert.SerializeObject(parsedData),
                                groupedData = JsonConvert.SerializeObject(groupedData),
                                gridData = JsonConvert.SerializeObject(gridData),
                                validationErrorMessages = validationErrorMessages,
                                searchSuccessful = gridData.Any(),
                                searchResponseText = "Search Complete"
                            }
                        },
                        new System.Net.Http.Formatting.JsonMediaTypeFormatter()
                    )
                };
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodBase.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { error = true, exception = exception }))
                };
                errorResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return errorResponse;
            }
        }

        private List<CorpActionSearchRequest> ParseSearchActionItemResponse(string data)
        {
            return JsonConvert.DeserializeObject<List<CorpActionSearchRequest>>(data);
        }

        private Dictionary<Int64, IEnumerable<CorpActionSearchRequest>> ParseByCorpActionType(List<CorpActionSearchRequest> data)
        {
            Dictionary<Int64, IEnumerable<CorpActionSearchRequest>> keyValuePairs = new Dictionary<Int64, IEnumerable<CorpActionSearchRequest>>();
            // get the records where child sec is null. those are individual items with no child that will be displayed.
            var individualChildlessParents = data.Where(f => f.ChildSecID == null);
            if (individualChildlessParents.Any())
            {
                keyValuePairs[0] = individualChildlessParents;
            }

            var remainingItems = data.Except(individualChildlessParents); // the ones with children
            //get all distinct corpActsIDs
            var groupsByCorpActsIds = remainingItems.GroupBy(f => f.CorpActsID);
            foreach (var item in groupsByCorpActsIds)
            {
                keyValuePairs[Convert.ToInt64(item.Key)] = item.Select(g => g);
            }

            return keyValuePairs;
        }

        public List<CorpActionSearchRequest> GetGridData(List<CorpActionSearchRequest> inputData, bool pwsIdentification = false)
        {
            inputData = inputData.OrderBy(f => f.Trial_CorpActionsIDAsNum).ThenBy(f => f.CorpActsID).ThenBy(f => f.CorpActsChildID).ToList();
            //lets set the key values 
            for (int i = 0; i < inputData.Count; i++)
            {
                inputData[i].key = Convert.ToString(i + 1); // needed for antd table component                                            
                inputData[i].PwsIdentification = pwsIdentification; // not really useful but its assigned.because th proc deals with everything internally
                if (inputData[i].IsParent)
                {
                    inputData[i].ParentSecID = inputData[i].SecID;
                    inputData[i].ParentProcessOrder = inputData[i].ProcessOrder;
                    inputData[i].ParentCashRate = inputData[i].CashRate;
                    inputData[i].ParentCostRate = inputData[i].CostRate;
                    inputData[i].ParentShrRate = inputData[i].ShrRate;
                    inputData[i].ParentMovementPrice = inputData[i].MovementPrice;
                    var possibleChildList = inputData.Where(f => f.CorpActsID == inputData[i].CorpActsID && !string.IsNullOrEmpty(f.CorpActsChildID));
                    inputData[i].IsParentWithChildren = possibleChildList.Any();
                    if (!inputData[i].IsParentWithChildren)
                    {
                        inputData[i].LastItemInGroup = true;
                    }
                    else // lets find the bottommost child that should be the LastItem
                    {
                        var bottomMostCorpChildActsID = possibleChildList.Max(f => f.CorpActsChildIDNum);
                        var indexOfBottomMostChild = inputData.FindIndex(f => f.CorpActsChildIDNum == bottomMostCorpChildActsID);
                        inputData[indexOfBottomMostChild].LastItemInGroup = true;
                    }
                }
                else //is child
                {
                    //find parent
                    var parentRecord = inputData.Find(f => f.CorpActsID == inputData[i].CorpActsID && f.IsParent);
                    inputData[i].ParentSecID = parentRecord.SecID; // set the parentSecId of the child
                    inputData[i].ParentProcessOrder = parentRecord.ProcessOrder; // set the parentprocessorder to processorder
                    inputData[i].ParentCashRate = parentRecord.CashRate;
                    inputData[i].ParentCostRate = parentRecord.CostRate;
                    inputData[i].ParentShrRate = parentRecord.ShrRate;
                    inputData[i].ParentMovementPrice = parentRecord.MovementPrice;
                }
            }

            return inputData;
        }

        private List<ToastObject> ValidateChildRecordWhileEditing(CorpActionSearchRequest request)
        {
            var validationErrorMessages = new List<ToastObject>();
            var child = request;
            if (child != null)
            {
                var corpActionType = request.CorpActTypes.FirstOrDefault(f => f.CorpActsTypeID.ToString() == request.CorpActsTypeID);
                var targerSecID = Helper.IgnoreAsNullParameter(request.CorpActsChildID) ? request.ChildSecID : request.SecID;
                if (ShouldCheckForValidData(corpActionType?.ChildSecID, targerSecID))
                {
                    if (!Helper.IsNumber(targerSecID))
                        validationErrorMessages.Add(new ToastObject { Description = Helper.GetValidationForProp("Sec ID", "Child Sec ID", "is a required number") });
                }

                var targetCashRate = request.ChildCashRate ?? request.CashRate;
                if (ShouldCheckForValidData(corpActionType?.ChildCashRate, targetCashRate))
                {
                    if (!Helper.IsNumber(targetCashRate))
                        validationErrorMessages.Add(new ToastObject { Description = Helper.GetValidationForProp("Cash Rate", "Child Cash Rate", "is a required number") });
                }

                var targetShareRate = child.ChildShrRate ?? child.ShrRate;
                if (ShouldCheckForValidData(corpActionType?.ChildShrRate, targetShareRate))
                {
                    if (!Helper.IsNumber(targetShareRate))
                        validationErrorMessages.Add(new ToastObject { Description = Helper.GetValidationForProp("Share Rate", "Child Share Rate", "is a required number") });
                }

                var targetCostRate = child.ChildCostRate ?? child.CostRate;
                if (ShouldCheckForValidData(corpActionType?.ChildCostRate, targetCostRate))
                {
                    if (!Helper.IsNumber(targetCostRate))
                        validationErrorMessages.Add(new ToastObject { Description = Helper.GetValidationForProp("Cost Rate", "Child Cost Rate", "is a required number") });
                }

                var targetMovementPrice = child.ChildMovementPrice ?? child.MovementPrice;
                if (ShouldCheckForValidData(corpActionType?.ChildMovementPrice, targetMovementPrice))
                {
                    if (!Helper.IsNumber(targetMovementPrice))
                        validationErrorMessages.Add(new ToastObject { Description = Helper.GetValidationForProp("Movement Price", "Child Movement Price", "is a required number") });
                }

                validationErrorMessages = validationErrorMessages.Where(f => !Helper.IgnoreAsNullParameter(f.Description)).ToList();
            }

            return validationErrorMessages;
        }
    }
}