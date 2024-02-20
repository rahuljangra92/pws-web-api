using PWSWebApi.handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace PWSWebApi.Controllers
{
    [Route("process")]
    [ApiController]
    public class ProcessController : ControllerBase
    {
        private readonly string connectionString;
        private readonly string pwsRecConnectionString;
        private readonly Helper handlerHelper;

        public ProcessController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("PWSProd");
            pwsRecConnectionString = configuration.GetConnectionString("PWSRec");
            handlerHelper = new Helper(configuration);
        }

        [HttpPost]
        [Route("getprocessresults")]
        public IActionResult AttribAssignSearch(string UserID, string AcctID_SubAcctID, string AllUnprocessed, string UnprocessedTransOnly, string UserGroupIDs, string DPIDs, string LoadSourceIDs, string EffectiveDateStart, string EffectiveDateEnd, string SecIDs, string Reprocess, int NumRows = 30)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                if (!Helper.IgnoreAsNullParameter(AcctID_SubAcctID))
                    pars.Add("@AcctID_SubAcctIDs", AcctID_SubAcctID);
                if (!Helper.IgnoreAsNullParameter(AllUnprocessed))
                    pars.Add("@AllUnprocessed", AllUnprocessed);
                if (!Helper.IgnoreAsNullParameter(UnprocessedTransOnly))
                    pars.Add("@UnprocessedTransOnly", UnprocessedTransOnly);
                if (!Helper.IgnoreAsNullParameter(UserGroupIDs))
                    pars.Add("@UserGroupIDs", UserGroupIDs);
                if (!Helper.IgnoreAsNullParameter(DPIDs))
                    pars.Add("@DPIDs", DPIDs);
                if (!Helper.IgnoreAsNullParameter(LoadSourceIDs))
                    pars.Add("@LoadSourceIDs", LoadSourceIDs);
                if (!Helper.IgnoreAsNullParameter(EffectiveDateStart))
                    pars.Add("@EffectiveDateStart", EffectiveDateStart);
                if (!Helper.IgnoreAsNullParameter(EffectiveDateEnd))
                    pars.Add("@EffectiveDateEnd", EffectiveDateEnd);
                if (!Helper.IgnoreAsNullParameter(SecIDs))
                    pars.Add("@SecIDs", SecIDs);
                if (!Helper.IgnoreAsNullParameter(Reprocess))
                    pars.Add("@Reprocess", Reprocess);
                pars.Add("@NumRows", NumRows);
                var data = Helper.callProcedure("pws_rec.[trnproc].[usp_TLP_List_Create_All]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("processclear")]
        public IActionResult ProcessClear(string UserID)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                var data = Helper.callProcedure("pws_rec.[trnproc].[usp_TLP_Clear]", pars);
                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("rpttemplatenamesinsert")]
        public IActionResult RptTemplateNamesInsert(string UserID, string Trial_RptTemplateIDs)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@Trial_RptTemplateIDs", Trial_RptTemplateIDs);
                var data = Helper.callProcedure("[adm].[usp_RptTemplateNames_Insert]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("trialrpttemplatenameinsertclear")]
        public IActionResult TrialRptTemplateNamesInsert(string UserID)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                var data = Helper.callProcedure("adm.usp_Trial_RptTemplateName_Clear", pars);
                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("trialrpttemplatenameinsert")]
        public IActionResult TrialRptTemplateNamesInsert(string UserID, string RptTemplateName, string RptPortID, string CCYID, string ModelID, string HierRpt, string AssgnStartPt, string AssgnEndPt, string AttribUserGroupID)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@RptTemplateName", RptTemplateName);
                pars.Add("@ModelID", ModelID);
                pars.Add("@RptPortID", RptPortID);
                pars.Add("@HierRpt", HierRpt);
                pars.Add("@AttribUserGroupID", AttribUserGroupID);
                if (!Helper.IgnoreAsNullParameter(CCYID))
                    pars.Add("@CCYID", CCYID);
                // if (!Helper.IgnoreAsNullParameter(AttribUserGroupID))
                // if (!Helper.IgnoreAsNullParameter(HierRpt))
                if (!Helper.IgnoreAsNullParameter(AssgnStartPt))
                    pars.Add("@AssgnStartPt", AssgnStartPt);
                if (!Helper.IgnoreAsNullParameter(AssgnEndPt))
                    pars.Add("@AssgnEndPt", AssgnEndPt);
                var data = Helper.callProcedure("[adm].[usp_Trial_RptTemplateName_Insert]", pars);
                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("processproduction")]
        public IActionResult ProcessToProduction(string UserID)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                var data = Helper.callProcedure("Pws_Rec.[trnproc].[usp_PopulateProcessing3]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("processtoreview")]
        public IActionResult ProcessToReview(string UserID)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                var data = Helper.callProcedure("Pws_Rec.[trnproc].[usp_PopulateProcessing]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("sendtoproduction")]
        public IActionResult SendToProduction(string UserID, string AcctsToGo)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@AcctsToGo", AcctsToGo);
                var data = Helper.callProcedure("Pws_Rec.[trnproc].[usp_PopulateProcessing2]", pars);
                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("deleteresults")]
        public IActionResult DeleteResults(string UserID, string AcctSubSecListIDs, string ExecutionLogID)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@AcctSubSecListIDs", AcctSubSecListIDs);
                if (!Helper.IgnoreAsNullParameter(ExecutionLogID))
                    pars.Add("@ExecutionLogID", ExecutionLogID);
                var data = Helper.callProcedure("pws_rec.[trnproc].[usp_TLP_List_Delete]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("showstaginglist")]
        public IActionResult ShowStagingList(string UserID)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                var data = Helper.callProcedure("Pws_Rec.[trnproc].[usp_TLP_List_ShowStaging]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("editresults")]
        public IActionResult EditResults(string UserID, string AcctSubSecListIDs, string MinEffectiveDateLess1, string ReconDate, string ExecutionLogID)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@AcctSubSecListIDs", AcctSubSecListIDs);
                pars.Add("@MinEffectiveDateLess1", MinEffectiveDateLess1);
                pars.Add("@ReconDate", ReconDate);
                if (!Helper.IgnoreAsNullParameter(ExecutionLogID))
                    pars.Add("@ExecutionLogID", ExecutionLogID);
                var data = Helper.callProcedure("pws_rec.[trnproc].[usp_TLP_List_Edit]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("recontrans")]
        public IActionResult ReconTrans(string UserID, string Sec, string SecID, string UserGroupID = null, string TransactionCodes = null, string AcctID = null, string StartDate = null, string EndDate = null, int TrialBalance = 1, int ForInsert = 1)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@TrialBalance", TrialBalance);
                pars.Add("@ForInsert", ForInsert);
                pars.Add("@Sec", Sec);
                pars.Add("@SecID", SecID);
                if (!Helper.IgnoreAsNullParameter(AcctID))
                    pars.Add("@AcctID", AcctID);
                if (!Helper.IgnoreAsNullParameter(StartDate))
                    pars.Add("@StartDate", StartDate);
                if (!Helper.IgnoreAsNullParameter(EndDate))
                    pars.Add("@EndDate", EndDate);
                if (!Helper.IgnoreAsNullParameter(UserGroupID))
                    pars.Add("@UserGroupID", UserGroupID);
                if (!Helper.IgnoreAsNullParameter(TransactionCodes))
                    pars.Add("@TransactionCodes ", TransactionCodes);
                var data = Helper.callProcedure("pws.[adm].[usp_ReconTrans]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("reconpositionsedit")]
        public IActionResult ReconPositionsEdit(string UserID, string Recon_PositionCompareIDs = null, string FreeReceipt = null, string FreeDeliver = null, string CashFlow = null, string AdjustFractionalShares = null, string InsertPrice = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@Recon_PositionCompareIDs", Recon_PositionCompareIDs);
                if (!Helper.IgnoreAsNullParameter(FreeReceipt))
                    pars.Add("@FreeReceipt ", FreeReceipt);
                if (!Helper.IgnoreAsNullParameter(FreeDeliver))
                    pars.Add("@FreeDeliver ", FreeDeliver);
                if (!Helper.IgnoreAsNullParameter(CashFlow))
                    pars.Add("@CashFlow ", CashFlow);
                if (!Helper.IgnoreAsNullParameter(AdjustFractionalShares))
                    pars.Add("@AdjustFractionalShares ", AdjustFractionalShares);
                if (!Helper.IgnoreAsNullParameter(InsertPrice))
                    pars.Add("@InsertPrice ", InsertPrice);
                var data = Helper.callProcedure("pws.[adm].[usp_ReconPositions_Edit]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("reconpositionsinsert")]
        public IActionResult ReconPositionsInsert(string UserID, string PositionCompareIDs, string TcodeID, string DPPrice)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@PositionCompareIDs", PositionCompareIDs);
                if (!Helper.IgnoreAsNullParameter(DPPrice))
                    pars.Add("@DPPrice", DPPrice);
                if (!Helper.IgnoreAsNullParameter(TcodeID))
                    pars.Add("@TcodeID", TcodeID);
                var data = Helper.callProcedure("[adm].[usp_ReconPositions_Insert]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("reconpositions")]
        public IActionResult ReconPositions(string UserID, string Sec, string SecID, string AcctID = null, string StartDate = null, string EndDate = null, string PWS = null, string DP = null, string Compare = "1")
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@Sec", Sec);
                pars.Add("@SecID", SecID);
                pars.Add("@Compare", Compare);
                if (!Helper.IgnoreAsNullParameter(AcctID))
                    pars.Add("@AcctID", AcctID);
                if (!Helper.IgnoreAsNullParameter(StartDate))
                    pars.Add("@StartDate", StartDate);
                if (!Helper.IgnoreAsNullParameter(EndDate))
                    pars.Add("@EndDate", EndDate);
                if (!Helper.IgnoreAsNullParameter(PWS))
                    pars.Add("@PWS", PWS);
                if (!Helper.IgnoreAsNullParameter(DP))
                    pars.Add("@DP", DP);
                var data = Helper.callProcedure("[adm].[usp_ReconPositions]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [Route("rpttemplatenamesearch")]
        public IActionResult RptTemplateNameSearch(string UserID, string RptPortIDs, string RptTemplateNameIDs, string ModelIDs)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                if (!Helper.IgnoreAsNullParameter(RptPortIDs))
                    pars.Add("@RptPortIDs", RptPortIDs);
                if (!Helper.IgnoreAsNullParameter(RptTemplateNameIDs))
                    pars.Add("@RptTemplateNameIDs", RptTemplateNameIDs);
                if (!Helper.IgnoreAsNullParameter(ModelIDs))
                    pars.Add("@ModelIDs", ModelIDs);
                var data = Helper.callProcedure("[adm].[usp_RptTemplateName_Search]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [Route("rptbuildinsert")]
        public IActionResult RptBuildInsert(string UserID, string StartDate, string RptTemplateNameIDs)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult)
                {
                    return (IActionResult)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                if (!Helper.IgnoreAsNullParameter(RptTemplateNameIDs))
                    pars.Add("@RptTemplateNameIDs", RptTemplateNameIDs);
                if (!Helper.IgnoreAsNullParameter(StartDate))
                    pars.Add("@StartDate", StartDate);
                var data = Helper.callProcedure("[adm].[usp_RptBuild_Insert]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [Route("processingcheck")]
        public IActionResult ProcessingCheck(string UserID, int Angular = 1)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@Angular", Angular);
                var data = Helper.callProcedure("pws_rec.[trnproc].[usp_Processing_Check]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("processingviewoutages")]
        public IActionResult ProcessingViewOutages(string UserID, string AcctID_SubAcctIDs, string LoadSourceIDs, string SecIDs, string UserGroupIDs)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                if (!Helper.IgnoreAsNullParameter(AcctID_SubAcctIDs))
                    pars.Add("@AcctID_SubAcctIDs", AcctID_SubAcctIDs);
                if (!Helper.IgnoreAsNullParameter(LoadSourceIDs))
                    pars.Add("@LoadSourceIDs", LoadSourceIDs);
                if (!Helper.IgnoreAsNullParameter(SecIDs))
                    pars.Add("@SecIDs", SecIDs);
                if (!Helper.IgnoreAsNullParameter(UserGroupIDs))
                    pars.Add("@UserGroupIDs", UserGroupIDs);
                var data = Helper.callProcedure("[trnproc].[usp_Processing_ViewOutages]", pars, ConfigurationManager.Configuration.GetSection("ConnectionStrings")["PWSRec"]);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("recontransinsert")]
        public IActionResult ReconTransInsert(string UserID, string Recon_TrialBalance_TransIDs = "")
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@Recon_TrialBalance_TransIDs", Recon_TrialBalance_TransIDs);
                var data = Helper.callProcedure("[adm].[usp_ReconTrans_Insert]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("mappingsecuritydelete")]
        public IActionResult MappingSecurityDelete(string UserID, string TransID_NewEditTransIDs = "")
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@SecurityMappingIDs", TransID_NewEditTransIDs);
                var data = Helper.callProcedure("[adm].[usp_MappingSecurity_Delete]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("recontransdelete")]
        public IActionResult ReconTransDelete(string UserID, string TransID_NewEditTransIDs = "")
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@TransID_NewEditTransIDs", TransID_NewEditTransIDs);
                var data = Helper.callProcedure("[adm].[usp_ReconTrans_Delete]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("seclevelmassInsert")]
        public IActionResult SecLevelMassInsert(string UserID, string Recon_ErrorsPerfID, string AssetOnly, string Explain, string Variance)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                if (!Helper.IgnoreAsNullParameter(Recon_ErrorsPerfID))
                    pars.Add("@Recon_ErrorsPerfID", Recon_ErrorsPerfID);
                if (!Helper.IgnoreAsNullParameter(AssetOnly))
                    pars.Add("@AssetOnly", AssetOnly);
                if (!Helper.IgnoreAsNullParameter(Explain))
                    pars.Add("@Explain", Explain);
                if (!Helper.IgnoreAsNullParameter(Variance))
                    pars.Add("@Variance", Variance);
                var data = Helper.callProcedure("[adm].[usp_SecLevel_PerfExceptions_MassInsert]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [Route("seclevelinsert")]
        public IActionResult SecLevelInsert(string UserID, string SecID, string AcctID, string SubAcctID, string EffectiveDate, string LowBpts, string HighBpts, string Explain)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                if (!Helper.IgnoreAsNullParameter(SecID))
                    pars.Add("@SecID", SecID);
                if (!Helper.IgnoreAsNullParameter(AcctID))
                    pars.Add("@AcctID", AcctID);
                if (!Helper.IgnoreAsNullParameter(SubAcctID))
                    pars.Add("@SubAcctID", SubAcctID);
                if (!Helper.IgnoreAsNullParameter(EffectiveDate))
                    pars.Add("@EffectiveDate", EffectiveDate);
                if (!Helper.IgnoreAsNullParameter(LowBpts))
                    pars.Add("@LowBpts", LowBpts);
                if (!Helper.IgnoreAsNullParameter(HighBpts))
                    pars.Add("@HighBpts", HighBpts);
                if (!Helper.IgnoreAsNullParameter(Explain))
                    pars.Add("@Explain", Explain);
                var data = Helper.callProcedure("[adm].[usp_SecLevel_PerfExceptions_Insert]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("mappingsecurityedit")]
        public IActionResult MappingSecurityEdit(string UserID, string LoadSourceID, string DPID, string SecID, string DPSecurityID, string DPSecurityName, string MappedUsing, string AcctID, string BlockRecon, string AdjustShares, string StartDate, string EndDate, string SecurityMappingIDs)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@SecurityMappingIDs", SecurityMappingIDs);
                if (!Helper.IgnoreAsNullParameter(LoadSourceID))
                    pars.Add("@LoadSourceID", LoadSourceID);
                if (!Helper.IgnoreAsNullParameter(DPID))
                    pars.Add("@DPID", DPID);
                if (!Helper.IgnoreAsNullParameter(SecID))
                    pars.Add("@SecID", SecID);
                if (!Helper.IgnoreAsNullParameter(DPSecurityID))
                    pars.Add("@DPSecurityID", DPSecurityID);
                if (!Helper.IgnoreAsNullParameter(DPSecurityName))
                    pars.Add("@DPSecurityName", DPSecurityName);
                if (!Helper.IgnoreAsNullParameter(MappedUsing))
                    pars.Add("@MappedUsing", MappedUsing);
                if (!Helper.IgnoreAsNullParameter(AcctID))
                    pars.Add("@AcctID", AcctID);
                if (!Helper.IgnoreAsNullParameter(BlockRecon))
                    pars.Add("@BlockRecon", BlockRecon);
                if (!Helper.IgnoreAsNullParameter(AdjustShares))
                    pars.Add("@AdjustShares", AdjustShares);
                if (!Helper.IgnoreAsNullParameter(StartDate))
                    pars.Add("@StartDate", StartDate);
                if (!Helper.IgnoreAsNullParameter(EndDate))
                    pars.Add("@EndDate", EndDate);
                var data = Helper.callProcedure("[adm].[usp_MappingSecurity_Edit]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("mappingsecurityinsert")]
        public IActionResult MappingSecurityInsert(// string UserID,
        string LoadSourceID, string DPID, string SecID, string DPSecurityID, string DPSecurityName, string MappedUsing, string AcctID, string BlockRecon, string AdjustShares, string StartDate, string EndDate)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                // pars.Add("@UserID", UserID);
                if (!Helper.IgnoreAsNullParameter(LoadSourceID))
                    pars.Add("@LoadSourceID", LoadSourceID);
                if (!Helper.IgnoreAsNullParameter(DPID))
                    pars.Add("@DPID", DPID);
                if (!Helper.IgnoreAsNullParameter(SecID))
                    pars.Add("@SecID", SecID);
                if (!Helper.IgnoreAsNullParameter(DPSecurityID))
                    pars.Add("@DPSecurityID", DPSecurityID);
                if (!Helper.IgnoreAsNullParameter(DPSecurityName))
                    pars.Add("@DPSecurityName", DPSecurityName);
                if (!Helper.IgnoreAsNullParameter(MappedUsing))
                    pars.Add("@MappedUsing", MappedUsing);
                if (!Helper.IgnoreAsNullParameter(AcctID))
                    pars.Add("@AcctID", AcctID);
                if (!Helper.IgnoreAsNullParameter(BlockRecon))
                    pars.Add("@BlockRecon", BlockRecon);
                if (!Helper.IgnoreAsNullParameter(AdjustShares))
                    pars.Add("@AdjustShares", AdjustShares);
                if (!Helper.IgnoreAsNullParameter(StartDate))
                    pars.Add("@StartDate", StartDate);
                if (!Helper.IgnoreAsNullParameter(EndDate))
                    pars.Add("@EndDate", EndDate);
                var data = Helper.callProcedure("[adm].[usp_MappingSecurity_Insert]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api" });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("dailyoutage")]
        public IActionResult DailyOutage(string UserID, string AcctID, string DPSecurityID, string SecID, string StartDate, string EndDate)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                if (!Helper.IgnoreAsNullParameter(AcctID))
                    pars.Add("@AcctID", AcctID);
                if (!Helper.IgnoreAsNullParameter(DPSecurityID))
                    pars.Add("@DPSecurityID", DPSecurityID);
                if (!Helper.IgnoreAsNullParameter(SecID))
                    pars.Add("@SecID", SecID);
                if (!Helper.IgnoreAsNullParameter(StartDate))
                    pars.Add("@StartDate", StartDate);
                if (!Helper.IgnoreAsNullParameter(EndDate))
                    pars.Add("@EndDate", EndDate);
                var data = Helper.callProcedure("[adm].[usp_ReconDailyOutages_Research]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("performanceoutage")]
        public IActionResult PerformanceOutage(string UserID, string AcctIDs, string UserGroupIDs, string SecIDs, string UserIDs, string DateRangeStart, string DateRangeEnd)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                if (!Helper.IgnoreAsNullParameter(AcctIDs))
                    pars.Add("@AcctIDs", AcctIDs);
                if (!Helper.IgnoreAsNullParameter(SecIDs))
                    pars.Add("@SecIDs", SecIDs);
                if (!Helper.IgnoreAsNullParameter(UserGroupIDs))
                    pars.Add("@UserGroupIDs", UserGroupIDs);
                if (!Helper.IgnoreAsNullParameter(UserIDs))
                    pars.Add("@UserIDs", UserIDs);
                if (!Helper.IgnoreAsNullParameter(DateRangeStart))
                    pars.Add("@DateRangeStart", DateRangeStart);
                if (!Helper.IgnoreAsNullParameter(DateRangeEnd))
                    pars.Add("@DateRangeEnd", DateRangeEnd);
                var data = Helper.callProcedure("[adm].[usp_Recon_PerfError_Research]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("mappingsecurity")]
        public IActionResult MappingSecurity(string UserID, string LoadSourceID, string SecID, string DPSecurityID)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                if (!Helper.IgnoreAsNullParameter(LoadSourceID))
                    pars.Add("@LoadSourceID", LoadSourceID);
                if (!Helper.IgnoreAsNullParameter(SecID))
                    pars.Add("@SecID", SecID);
                if (!Helper.IgnoreAsNullParameter(DPSecurityID))
                    pars.Add("@DPSecurityID", DPSecurityID);
                var data = Helper.callProcedure("[adm].[usp_MappingSecurity_Search]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }
    }
}