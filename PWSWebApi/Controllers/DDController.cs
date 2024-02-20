using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PWSWebApi.Domains.DBContext;
using PWSWebApi.handlers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace PWSWebApi.ControllershandlerHelper
{
    [Route("dd")]
    [ApiController]
    public class DDController : ControllerBase
    {
        private readonly string connectionString;
        private readonly Helper handlerHelper;

        public DDController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("PWSProd");
            handlerHelper = new Helper(configuration);
        }
        [HttpGet]
        [Route("getTemplates")]
        public IActionResult GetTemplates(int UserID)
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
                var data = Helper.callProcedure("usp_RptTemplate_Lkup", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getDayCtBasis")]
        public IActionResult GetDayCtBasis(string UserID = null)
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
                var data = Helper.callProcedure("[DD].[usp_DayCtBasis]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getentityidmatch")]
        public IActionResult GetEntityIdMatch(int UserID, string Match, int Top = 20, int Edit = 0, int SecuritizedID = 1)
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
                pars.Add("@Match", Match.Trim());
                pars.Add("@Top", Top);
                pars.Add("@Edit", Edit);
                pars.Add("@SecuritizedID", SecuritizedID);
                var data = Helper.callProcedure("DD.usp_EntityID_Match", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Erorr Occcured";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getacctidsubacctismatch")]
        public IActionResult GetAcctIDSubAcctIDMatch(int UserID, string Match = null, string EntityIDs = "", int Top = 30, int PartnersOnly = 1)
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
                pars.Add("@Top", Top);
                if (!Helper.IgnoreAsNullParameter(EntityIDs))
                    pars.Add("@EntityIDs", EntityIDs);
                pars.Add("@PartnersOnly ", PartnersOnly);
                if (!Helper.IgnoreAsNullParameter(Match))
                    pars.Add("@Match", Match.Trim());
                var data = Helper.callProcedure("DD.usp_AcctID_SubAcctID_Match", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getClientID")]
        public IActionResult GetExTrackedAssets(string UserID, string Match, string CSD_UserIDs)
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
                if (!Helper.IgnoreAsNullParameter(Match))
                    pars.Add("@Match", Match);
                if (!Helper.IgnoreAsNullParameter(CSD_UserIDs))
                    pars.Add("@CSD_UserIDs", CSD_UserIDs);
                var data = Helper.callProcedure("DD.usp_PWSClientID_Match", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getextrackedassets")]
        public IActionResult GetExTrackedAssets(int UserID, string TransID = null, string AcctID = null)
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
                pars.Add("@TransID", TransID);
                pars.Add("@AcctID", AcctID);
                var data = Helper.callProcedure("DD.usp_ExTrackedAssets", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("gettcodeidextracked")]
        public IActionResult GetTCodeIDExtracked(int UserID, string CashAcctTcodeID)
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
                pars.Add("@CashAcctTcodeID", CashAcctTcodeID);
                var data = Helper.callProcedure("DD.usp_TCodeID_ExTracked", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        //string UserID,string Matching, string Match, string Top
        [HttpGet]
        [Route("getsecidmatch")]
        public IActionResult GetSecIDMatch(string UserID, string Match = "", int Top = 20, string EntityID = "0", int AcctOnlyID = 0)
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
                if (!Helper.IgnoreAsNullParameter(Match))
                    pars.Add("@Match", Match.Trim());
                if (!Helper.IgnoreAsNullParameter(EntityID))
                    pars.Add("@EntityID", EntityID);
                pars.Add("@Top", Top);
                pars.Add("@AcctOnlyID", AcctOnlyID);
                var data = Helper.callProcedure("DD.usp_SecID_Match", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("reconpositioncomparetrans")]
        public IActionResult ReconPositionCompareTrans()
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                var data = Helper.callProcedure("[DD].[usp_Recon_PositionCompare_Trans]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("pwsSecID")]
        public IActionResult PWSSecID(string UserID, string Match = "", int Top = 30, int Bmk = 0, string LoadSourceIDs = "")
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
                pars.Add("@Bmk", Bmk);
                pars.Add("@Top", Top);
                if (!Helper.IgnoreAsNullParameter(Match))
                    pars.Add("@Match", Match.Trim());
                if (!Helper.IgnoreAsNullParameter(LoadSourceIDs))
                    pars.Add("@LoadSourceIDs", LoadSourceIDs);
                var data = Helper.callProcedure("[DD].[usp_SecID_Match]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("mappingsecurityidmatch")]
        public IActionResult MappingSecurityIDMatch(string UserID, string Match = "", string LoadSourceIDs = "", int Top = 10)
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
                if (!Helper.IgnoreAsNullParameter(Match))
                    pars.Add("@Match", Match.Trim());
                pars.Add("@LoadSourceIDs", LoadSourceIDs);
                pars.Add("@Top", Top);
                var data = Helper.callProcedure("[DD].[usp_MappingSecurityID_Match]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("mappedusing")]
        public IActionResult MappedUsing()
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                var data = Helper.callProcedure("[DD].[usp_MappedUsing]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getheirids")]
        public IActionResult GetHeirIDs(string UserID = null, string Rpting = null, string DefaultChoiceEntity = null, string UserGroupID = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                if (!Helper.IgnoreAsNullParameter(UserID))
                    pars.Add("@UserID", UserID);
                if (!Helper.IgnoreAsNullParameter(Rpting))
                    pars.Add("@Rpting", Rpting);
                if (!Helper.IgnoreAsNullParameter(DefaultChoiceEntity))
                    pars.Add("@DefaultChoiceEntity", DefaultChoiceEntity);
                if (!Helper.IgnoreAsNullParameter(UserGroupID))
                    pars.Add("@UserGroupID", UserGroupID);
                var data = Helper.callProcedure("[DD].[usp_HierID]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getHierAssignLookup")]
        public IActionResult getHierAssignLookup(string UserID, string HierAssgnID, string HideDupTitles)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@HierAssgnID", HierAssgnID);
                pars.Add("@HideDupTitles", HideDupTitles);
                var data = Helper.callProcedure("[DD].[usp_HierAssgnID_Lkup]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getTCodes")]
        public IActionResult GetTCodes(string AcctID = null, string SubAcctID = null, string SecID = null, string PartnershipRA = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                if (!Helper.IgnoreAsNullParameter(AcctID))
                    pars.Add("@AcctID", AcctID);
                if (!Helper.IgnoreAsNullParameter(SubAcctID))
                    pars.Add("@SubAcctID", SubAcctID);
                if (!Helper.IgnoreAsNullParameter(SecID))
                    pars.Add("@SecID", SecID);
                if (!Helper.IgnoreAsNullParameter(PartnershipRA))
                    pars.Add("@PartnershipRA", PartnershipRA);
                var data = Helper.callProcedure("[DD].[usp_TCode]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getTCodesMatch")]
        public IActionResult GetTCodesMatch(string UserID, string AcctID = null, string SubAcctID = null, string SecID = null, string Match = null, string PartnershipRA = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                if (!Helper.IgnoreAsNullParameter(AcctID))
                    pars.Add("@AcctID", AcctID);
                if (!Helper.IgnoreAsNullParameter(SubAcctID))
                    pars.Add("@SubAcctID", SubAcctID);
                if (!Helper.IgnoreAsNullParameter(SecID))
                    pars.Add("@SecID", SecID);
                if (!Helper.IgnoreAsNullParameter(Match))
                    pars.Add("@Match", Match);
                if (!Helper.IgnoreAsNullParameter(PartnershipRA))
                    pars.Add("@PartnershipRA", PartnershipRA);
                var data = Helper.callProcedure("[DD].[usp_TCode_Match]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getAttribID")]
        public IActionResult getAttribID(string UserID, string Match, string Top = "30")
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@Top", Top);
                if (!Helper.IgnoreAsNullParameter(Match))
                    pars.Add("@Match", Match);
                var data = Helper.callProcedure("[DD].[usp_AttribComboID_Match]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getCountryID")]
        public IActionResult getCountryID()
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                var data = Helper.callProcedure("[DD].[usp_CountryID]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getIndexID")]
        public IActionResult GetIndexID(string UserID, string Match = "")
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                if (!Helper.IgnoreAsNullParameter(Match))
                    pars.Add("@Match", Match);
                var data = Helper.callProcedure("[DD].[usp_IndexID]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("rptstructureid")]
        public IActionResult GetRptStructureID(string UserID)
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
                var data = Helper.callProcedure("[DD].[usp_RptStructureID]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getpartnershipfeestructureids")]
        public IActionResult GetPartnershipFeeStructureIDs(string UserID)
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
                var data = Helper.callProcedure("[DD].[usp_PartnershipFeeStructureID]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getcurrencies")]
        public IActionResult GetCurrencies(string UserID = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                var data = Helper.callProcedure("[DD].[usp_CurrencyID]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("pwsrefidmatch")]
        public IActionResult PWSRefIDMatch(string UserID = null, string Match = "", string RptStructureIDs = "", int Edit = 0, int AttribAssign = 1, int Top = 20)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@Match", Match);
                if (!Helper.IgnoreAsNullParameter(RptStructureIDs))
                    pars.Add("@RptStructureIDs", RptStructureIDs);
                pars.Add("@Edit", Edit);
                pars.Add("@AttribAssign", AttribAssign);
                pars.Add("@Top", Top);
                var data = Helper.callProcedure("[DD].[usp_PWSRefID_Match]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getsecmatch")]
        public IActionResult SecIDMatch(string UserID = null, string Match = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@Match", Match);
                pars.Add("@UserID", UserID);
                var data = Helper.callProcedure("[DD].[usp_SecID_Match]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("assetidmatch")]
        public IActionResult AssetIDMatch(string UserID = null, string Match = null, int Top = 30)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                pars.Add("@Match", Match);
                pars.Add("@Top", Top);
                var data = Helper.callProcedure("[DD].[usp_AssetID_Match]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getportfolios")]
        public IActionResult RptPortIDMatch(string UserID = null, string Match = null, string PWSRefIDs = null, string PWSClientIDs = null, string UserGroupIDs = "", int Top = 30, int Edit = 1)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                string blankMatch = Match ?? "";
                pars.Add("@UserID", UserID);
                pars.Add("@Match", blankMatch);
                pars.Add("@UserGroupIDs", UserGroupIDs);
                pars.Add("@Top", Top);
                pars.Add("@Edit", Edit);
                if (!Helper.IgnoreAsNullParameter(PWSRefIDs))
                    pars.Add("@PWSRefIDs", PWSRefIDs);
                if (!Helper.IgnoreAsNullParameter(PWSClientIDs))
                    pars.Add("@PWSClientIDs", PWSClientIDs);
                var data = Helper.callProcedure("[DD].[usp_RptPortID_Match]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("loadsourceid")]
        public IActionResult LoadSourceID()
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                var data = Helper.callProcedure("[DD].[usp_LoadSourceID]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("loadsourceidmatch")]
        public IActionResult LoadSourceIDMatch(string UserID = null, string Match = null, int AcctID = 0)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID ", UserID);
                pars.Add("@AcctID ", AcctID);
                if (!Helper.IgnoreAsNullParameter(Match))
                    pars.Add("@Match ", Match);
                var data = Helper.callProcedure("[DD].[usp_LoadSourceID_Match]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("refidmatch")]
        public IActionResult RefIDMatch(string UserID = null, string Match = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@Match", Match);
                pars.Add("@UserID", UserID);
                var data = Helper.callProcedure("[DD].[usp_PWSRefID_Match]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("dpid")]
        public IActionResult DPID(string Match = null, int Top = 20, string LoadsourceID = null, string UserID = null)
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
                pars.Add("@Match", Match);
                pars.Add("@Top", Top);
                if (!Helper.IgnoreAsNullParameter(LoadsourceID))
                    pars.Add("@LoadsourceID", LoadsourceID);
                var data = Helper.callProcedure("[DD].[usp_DataProviderID_Match]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("fncttypeid")]
        public IActionResult FnctTypeID(string UserID)
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
                var data = Helper.callProcedure("[DD].[usp_FnctTypeID]", pars);
                return Ok( new { error = false, data = data, r = handlerHelper.RefreshToken(Request, UserID) });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getusergroups")]
        public IActionResult GetUserGroupID(string UserID, string Match = null, string RptPortID = null, string PWSClientID = null, string DefaultChoiceEntity = null, int Edit = 1)
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
                pars.Add("@Edit", Edit);
                if (!Helper.IgnoreAsNullParameter(Match))
                    pars.Add("@Match ", Match);
                if (!Helper.IgnoreAsNullParameter(DefaultChoiceEntity))
                    pars.Add("@DefaultChoiceEntity", DefaultChoiceEntity);
                if (!Helper.IgnoreAsNullParameter(RptPortID))
                    pars.Add("@RptPortID", RptPortID);
                if (!Helper.IgnoreAsNullParameter(PWSClientID))
                    pars.Add("@PWSClientID", PWSClientID);
                var data = Helper.callProcedure("[DD].[usp_UsergroupID]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getfocusrpt")]
        public IActionResult GetFocusRpt(string UserID, string RptTemplateNameID, string GroupingPWSRefString, string Expand, string PerfSearch)
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
                if (!Helper.IgnoreAsNullParameter(RptTemplateNameID))
                    pars.Add("@RptTemplateNameID", RptTemplateNameID);
                if (!Helper.IgnoreAsNullParameter(GroupingPWSRefString))
                    pars.Add("@GroupingPWSRefString", GroupingPWSRefString);
                if (!Helper.IgnoreAsNullParameter(Expand))
                    pars.Add("@Expand", Expand);
                if (!Helper.IgnoreAsNullParameter(PerfSearch))
                    pars.Add("@PerfSearch", PerfSearch);
                var data = Helper.callProcedure("[DD].[usp_FocusRptTempOrd]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getcomparator")]
        public IActionResult GetComparator(string UserID)
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
                var data = Helper.callProcedure("[DD].[usp_Comparator]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getattribtypeids")]
        public IActionResult GetAttribTypeIDs(string UserID, string Match = null)
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
                if (!Helper.IgnoreAsNullParameter(Match))
                    pars.Add("@Match ", Match);
                var data = Helper.callProcedure("[DD].[usp_AttribTypeID]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getattribtypenames")]
        public IActionResult GetAttribTypeNames(string UserID, string Match = null)
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
                if (!Helper.IgnoreAsNullParameter(Match))
                    pars.Add("@Match ", Match);
                var data = Helper.callProcedure("[DD].[usp_AttribNameID]", pars);
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
        [Route("getmodelsearch")]
        public IActionResult GetModelSearch(string UserID, string ModelID = null)
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
                pars.Add("@ModelID ", ModelID);
                var data = Helper.callProcedure("[DD].[usp_Model_Search]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("rpttemplatename")]
        public IActionResult RptTemplateNameID(string UserID, string Match = null, string RptPortIDs = null, string UserGroupIDs = null)
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
                if (!Helper.IgnoreAsNullParameter(Match))
                    pars.Add("@Match", Match);
                if (!Helper.IgnoreAsNullParameter(RptPortIDs))
                    pars.Add("@RptPortIDs", RptPortIDs);
                if (!Helper.IgnoreAsNullParameter(UserGroupIDs))
                    pars.Add("@UserGroupIDs", UserGroupIDs);
                var data = Helper.callProcedure("[DD].[usp_RptTemplateNameID_Match]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getrptmodelid")]
        public IActionResult GetRptModelID(string UserID, string RptTemplateNameIDs = null, string AttribTypeIDs = null, string AttribNameIDs = null, string UserGroupIDs = null)
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
                if (!Helper.IgnoreAsNullParameter(RptTemplateNameIDs))
                    pars.Add("@RptTemplateNameIDs ", RptTemplateNameIDs);
                if (!Helper.IgnoreAsNullParameter(AttribTypeIDs))
                    pars.Add("@AttribTypeIDs ", AttribTypeIDs);
                if (!Helper.IgnoreAsNullParameter(AttribNameIDs))
                    pars.Add("@AttribNameIDs ", AttribNameIDs);
                if (!Helper.IgnoreAsNullParameter(UserGroupIDs))
                    pars.Add("@UserGroupIDs ", UserGroupIDs);
                var data = Helper.callProcedure("[DD].[usp_RptModelID]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("accounts")]
        public IActionResult Accounts([FromQuery] string UserID, [FromQuery] string Matching = null, [FromQuery] string Match = null, [FromQuery] string Top = null, [FromQuery] string LoadSource = null, [FromQuery] string DataProviders = null, [FromQuery] string entityIDs = null)
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

                Matching = Matching == "undefined" || Matching == null ? "0" : Matching;
                Match = Match == "undefined" || Match == null ? "0" : Match;
                Top = Top == "undefined" || Top == null ? "0" : Top;
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                if (Matching == "1")
                {
                    //pars.Add("@Matching", Matching);
                    pars.Add("@Match", Match);
                    pars.Add("@Top", Top);
                }

                if (!Helper.IgnoreAsNullParameter(LoadSource))
                    pars.Add("@LoadsourceIDs", LoadSource);
                if (!Helper.IgnoreAsNullParameter(DataProviders))
                    pars.Add("@DPIDs", DataProviders);
                if (!Helper.IgnoreAsNullParameter(entityIDs))
                    pars.Add("@entityIDs", entityIDs);
                var data = Matching == "1" ? Helper.callProcedure("DD.[usp_AccountID_Match]", pars) : Helper.callProcedure("DD.[usp_AccountID]", pars);
                return Ok( new { data = data, error = false, success = true });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getsubaccountid")]
        public IActionResult GetSubAccountID([FromQuery] string UserID, [FromQuery] string AcctID)
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
                if (!Helper.IgnoreAsNullParameter(AcctID))
                    pars.Add("@AcctID ", Convert.ToDecimal(AcctID));
                var data = Helper.callProcedure("[DD].[usp_SubAccountID]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getperiodicityid")]
        public IActionResult GetPeriodicityId([FromQuery] string Evergreen)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                if (!Helper.IgnoreAsNullParameter(Evergreen))
                    pars.Add("@Evergreen", Evergreen);
                var data = Helper.callProcedure("[DD].[usp_PeriodicityID]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("issuer")]
        public IActionResult IssuerID(string UserID = null)
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
                var data = Helper.callProcedure("[DD].[usp_IssuerID]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getstatesmatch")]
        public IActionResult GetStatesMatch([FromQuery] string UserID, [FromQuery] string Match)
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
                if (!Helper.IgnoreAsNullParameter(Match))
                    pars.Add("@Match ", Match);
                var data = Helper.callProcedure("[DD].[usp_StatesID_Match]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getusers")]
        public IActionResult GetUserID(string UserID)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("UserID", UserID);
                var data = Helper.callProcedure("DD.usp_UserID", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api" });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getusersmatch")]
        public IActionResult GetUserIDMatch(string UserID, string IncludePWSTeam, string Match, string CSD)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                if (!Helper.IgnoreAsNullParameter(IncludePWSTeam))
                    pars.Add("@IncludePWSTeam", IncludePWSTeam);
                if (!Helper.IgnoreAsNullParameter(Match))
                    pars.Add("@Match", Match);
                if (!Helper.IgnoreAsNullParameter(CSD))
                    pars.Add("@CSD", CSD);
                var data = Helper.callProcedure("DD.usp_UserID_Match", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api" });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("gettcodeidnonunit")]
        public IActionResult GetTCodeIDNonUnit([FromQuery] string UserID, string Match, string Cash, string MV, string Commit, string Inc, string Cost, string Flow, string Perf)
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
                pars.Add("@Match", Match);
                if (!Helper.IgnoreAsNullParameter(Cash))
                    pars.Add("@Cash ", Cash);
                if (!Helper.IgnoreAsNullParameter(MV))
                    pars.Add("@MV ", MV);
                if (!Helper.IgnoreAsNullParameter(Commit))
                    pars.Add("@Commit", Commit);
                if (!Helper.IgnoreAsNullParameter(Inc))
                    pars.Add("@Inc ", Inc);
                if (!Helper.IgnoreAsNullParameter(Cost))
                    pars.Add("@Cost ", Cost);
                if (!Helper.IgnoreAsNullParameter(Flow))
                    pars.Add("@Flow ", Flow);
                if (!Helper.IgnoreAsNullParameter(Perf))
                    pars.Add("@Perf ", Perf);
                var data = Helper.callProcedure("[DD].[usp_TCodeID_NonUnit]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getreliefmethod")]
        public IActionResult GetReliefMethod(string UserID, string Match)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result) // usp_ReliefMethodID
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID ", UserID);
                if (!Helper.IgnoreAsNullParameter(Match))
                    pars.Add("@Match ", Match);
                var data = Helper.callProcedure("[DD].[usp_Relief_Method]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api" });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("optiontype")]
        public IActionResult GetOptionType()
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                var data = Helper.callProcedure("[DD].[usp_OptionTypeID]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api" });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("optionstyle")]
        public IActionResult GetOptionStyle()
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                var data = Helper.callProcedure("[DD].[usp_OptionStyleID]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api" });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getsimple")]
        public IActionResult GetSimple()
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                var data = Helper.callProcedure("[DD].[usp_Simple]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api" });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("exchangeid")]
        public IActionResult ExchangeID(string UserID)
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
                var data = Helper.callProcedure("DD.usp_ExchgID", pars);
                return Ok( new { data = data, error = false, success = true });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        //usp_PartnershipAccountID_Match
        [HttpGet]
        [Route("partnershipaccountidmatch")]
        public IActionResult PartnershipAccountIDMatch([FromQuery] string UserID, [FromQuery] string Match, [FromQuery] string Top, [FromQuery] string GPAcct, [FromQuery] string CashAcct, [FromQuery] string PayRecAcct)
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
                pars.Add("@Match", Match);
                pars.Add("@Top", Top);
                pars.Add("@GPAcct", GPAcct);
                pars.Add("@CashAcct", CashAcct);
                pars.Add("@PayRecAcct", PayRecAcct);
                var data = Helper.callProcedure("[DD].[usp_PartnershipAccountID_Match]", pars);
                return Ok( new { data = data, error = false, success = true });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("lookup")]
        public IActionResult GetLookUpByID([FromQuery] string userId, [FromQuery] string lookupID)
        {
            if (handlerHelper.ValidateSource(Request) is IActionResult result)
            {
                return result;
            }

            if (handlerHelper.ValidateToken(Request, userId) != string.Empty)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, userId));
            }

            try
            {
                var context = new DataClasses1DataContext(connectionString);
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", userId);
                pars.Add("@LookupID", lookupID);
                //var data = context.usp_LookUp_Values(Convert.ToInt32(userId),Convert.ToInt32(id)).AsQueryable();
                var data = Helper.callProcedure("[DD].[usp_LookUp_Values]", pars);
                return Ok( new { data = data, error = false, success = true });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = userId });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getPriceType")]
        public IActionResult GetPricingData()
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                var data = Helper.callProcedure("dd.usp_PriceTypeID", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api" });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("getCorpsActsTypeIDs")]
        public IActionResult GetCorpActionsTypeID()
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                var data = Helper.callProcedure("[DD].[usp_CorpActsTypeID]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api" });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }
    }
}