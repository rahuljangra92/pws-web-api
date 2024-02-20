using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PWSWebApi.Domains.DBContext;
using PWSWebApi.handlers;
using PWSWebApi.Models.PartnershipTracking;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace PWSWebApi.Controllers
{
    [Route("partnershiptracking")]
    [ApiController]
    public class PartnershipTrackingController : ControllerBase
    {
        private readonly Helper handlerHelper;
        private readonly string connectionString;

        public PartnershipTrackingController(IConfiguration configuration, DataClasses1DataContext dataClasses1DataContext)
        {
            connectionString = dataClasses1DataContext.Database.GetConnectionString();
            handlerHelper = new Helper(configuration);
        }


        [HttpPost]
        [Route("searchpartnershiptracking")]
        public IActionResult SearchPartnershipTracking([FromBody] ObjectPartnershipTracking requestBody)
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
                pars.Add("@PartnersAcctIDs", requestBody.PartnersAcctIDs);
                pars.Add("@EntityID", requestBody.EntityID);
                pars.Add("@StartDate", requestBody.StartDate);
                pars.Add("@EndDate", requestBody.EndDate);
                pars.Add("@ByPartner", requestBody.ByPartner);
                pars.Add("@ByPartnership", requestBody.ByPartnership);
                pars.Add("@PeriodicityID", requestBody.PeriodicityID);
                pars.Add("@TrialBalance", requestBody.TrialBalance);
                var data = Helper.callProcedure("[adm].[usp_PartnershipTracking_Search]", pars);
                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = requestBody.UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("searchpartners")]
        public IActionResult SearchPartners([FromBody] ObjectPartnershipTracking requestBody)
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
                pars.Add("@AcctID_SubAcctIDs", requestBody.AcctID_SubAcctIDs);
                pars.Add("@EntityID", requestBody.EntityID);
                pars.Add("@EOD_StartDateStart ", requestBody.EOD_StartDateStart);
                pars.Add("@EOD_StartDateEnd ", requestBody.EOD_StartDateEnd);
                var data = Helper.callProcedure("[adm].[usp_Partners_Search]", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = requestBody.UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("createpartnershiprafeetrans")]
        public IActionResult CreatePartnershipRAFeeTrans([FromBody] ObjectPartnershipTracking requestBody)
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
                pars.Add("@Effectivedate", requestBody.Effectivedate);
                pars.Add("@EntityID", requestBody.EntityID);
                pars.Add("@Test", requestBody.Test);
                pars.Add("@TransSets", requestBody.TransSets);
                var data = Helper.callProcedure("adm.usp_Partnership_RA_Fee_Trans_Create", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = requestBody.UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("searchpartnershipramatch")]
        public IActionResult SearchPartnershipRAMatch([FromBody] ObjectPartnershipTracking requestBody)
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
                pars.Add("@EntityID", requestBody.EntityID);
                pars.Add("@Unmatched ", requestBody.Unmatched);
                pars.Add("@Matched ", requestBody.Matched);
                pars.Add("@EffectiveStartDate", requestBody.EffectiveStartDate);
                pars.Add("@EffectiveEndDate", requestBody.EffectiveEndDate);
                var data = Helper.callProcedure("adm.usp_Partnership_RA_Match_Search", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = requestBody.UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("updatepartnershipramatch")]
        public IActionResult UpdatePartnershipRAMatch([FromBody] ObjectPartnershipTracking requestBody)
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
                pars.Add("@Unmatch", requestBody.Unmatch);
                pars.Add("@PartnershipFlow", requestBody.PartnershipFlow);
                pars.Add("@PartnershipTransID", requestBody.PartnershipTransID);
                pars.Add("@PartnerTransID", requestBody.PartnerTransID);
                var data = Helper.callProcedure("adm.usp_Partnership_RA_Match_Update", pars);
                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = requestBody.UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("searchpartnershiptransaction")]
        public IActionResult SearchPartnershipTransaction([FromBody] ObjectPartnershipTracking requestBody)
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
                pars.Add("@PartnershipTransIDs", requestBody.PartnershipTransIDs);
                pars.Add("@PartnerTransIDs", requestBody.PartnerTransIDs);
                var data = Helper.callProcedure("adm.usp_Partnership_Transaction_Search", pars);
                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = requestBody.UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("createpartnershipramatch")]
        public IActionResult CreatePartnershipRAMatch([FromBody] ObjectPartnershipTracking requestBody)
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
                pars.Add("@PartnershipTransID", requestBody.PartnershipTransID);
                pars.Add("@MatchingTransID", requestBody.MatchingTransID);
                pars.Add("@CreateMatch", requestBody.CreateMatch);
                pars.Add("@NonRAFlow", requestBody.NonRAFlow);
                var data = Helper.callProcedure("adm.usp_Partnership_RA_Match_Create", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = requestBody.UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("createpartnershipextrackedtrans")]
        public IActionResult CreatePartnershipExTrackedTrans([FromBody] ObjectPartnershipTracking requestBody)
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
                pars.Add("@PartnershipTransID", requestBody.PartnershipTransID);
                pars.Add("@PartnershipAcctID", requestBody.PartnershipAcctID);
                pars.Add("@ExTrackedSecID", requestBody.ExTrackedSecID);
                pars.Add("@NewTcodeID", requestBody.NewTcodeID);
                pars.Add("@Test", requestBody.Test);
                var data = Helper.callProcedure("adm.usp_Partnership_ExTrackedTrans_Create", pars);
                return Ok( new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = requestBody.UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("createpartnershiplinkedtrans")]
        public IActionResult CreatePartnershipLinkedTrans([FromBody] ObjectPartnershipTracking requestBody)
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
                pars.Add("@PartnershipTransID", requestBody.PartnershipTransID);
                pars.Add("@AcctID", requestBody.AcctID);
                pars.Add("@SubAcctID ", requestBody.SubAcctID);
                pars.Add("@SecID", requestBody.SecID);
                pars.Add("@TcodeID", requestBody.TcodeID);
                pars.Add("@Final", requestBody.Final);
                pars.Add("@SettleCCYID", requestBody.SettleCCYID);
                pars.Add("@Test", requestBody.Test);
                var data = Helper.callProcedure("adm.usp_Partnership_LinkedTrans_Create", pars);
                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = requestBody.UserID });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpGet]
        [Route("searchparthership")]
        public IActionResult SearchParthership(string UserID = null, string EntityIDs = null, string PartnershipFeeStructureID = null, string GPAcctIDs = null, string CashAcctIDs = null)
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
                if (!Helper.IgnoreAsNullParameter(EntityIDs))
                    pars.Add("@EntityIDs", EntityIDs);
                if (!Helper.IgnoreAsNullParameter(PartnershipFeeStructureID))
                    pars.Add("@PartnershipFeeStructureID", PartnershipFeeStructureID);
                if (!Helper.IgnoreAsNullParameter(GPAcctIDs))
                    pars.Add("@GPAcctIDs ", GPAcctIDs);
                if (!Helper.IgnoreAsNullParameter(CashAcctIDs))
                    pars.Add("@CashAcctIDs", CashAcctIDs);
                var data = Helper.callProcedure("[adm].[usp_Partnership_Search]", pars);
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
        [Route("partnershippartnerscompare")]
        public IActionResult PartnershipPartnersCompare(string UserID = null, string PartnershipID = null, string EffectiveDateStart = null, string EffectivedateEnd = null, string DifferencesOnly = null, string DifferenceTolerance = null)
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
                if (!Helper.IgnoreAsNullParameter(PartnershipID))
                    pars.Add("@PartnershipID", PartnershipID);
                if (!Helper.IgnoreAsNullParameter(EffectiveDateStart))
                    pars.Add("@EffectiveDateStart", EffectiveDateStart);
                if (!Helper.IgnoreAsNullParameter(EffectivedateEnd))
                    pars.Add("@EffectivedateEnd ", EffectivedateEnd);
                if (!Helper.IgnoreAsNullParameter(DifferencesOnly))
                    pars.Add("@DifferencesOnly", DifferencesOnly);
                if (!Helper.IgnoreAsNullParameter(DifferenceTolerance))
                    pars.Add("@DifferenceTolerance", DifferenceTolerance);
                var data = Helper.callProcedure("[adm].[usp_Partnership_Partners_Compare]", pars);
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
        [Route("insertparthership")]
        public IActionResult InsertParthership(string UserID = null, string EntityID = null, string PartnershipFeeStructureID = null, string CCYID = null, string MinRAFlow = null, string FeesPaidbyGP = null, string GPAcctID = null, string CashAcctID = null, string PayRecAcctID = null)
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
                if (!Helper.IgnoreAsNullParameter(EntityID))
                    pars.Add("@EntityID", EntityID);
                if (!Helper.IgnoreAsNullParameter(PartnershipFeeStructureID))
                    pars.Add("@PartnershipFeeStructureID", PartnershipFeeStructureID);
                if (!Helper.IgnoreAsNullParameter(CCYID))
                    pars.Add("@CCYID ", CCYID);
                if (!Helper.IgnoreAsNullParameter(MinRAFlow))
                    pars.Add("@MinRAFlow", MinRAFlow);
                if (!Helper.IgnoreAsNullParameter(FeesPaidbyGP))
                    pars.Add("@FeesPaidbyGP", FeesPaidbyGP);
                if (!Helper.IgnoreAsNullParameter(GPAcctID))
                    pars.Add("@GPAcctID", GPAcctID);
                if (!Helper.IgnoreAsNullParameter(CashAcctID))
                    pars.Add("@CashAcctID", CashAcctID);
                if (!Helper.IgnoreAsNullParameter(PayRecAcctID))
                    pars.Add("@PayRecAcctID", PayRecAcctID);
                var data = Helper.callProcedure("[adm].[usp_Partnership_Insert]", pars);
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
        [Route("partnershipEditDelete")]
        public IActionResult PartnershipEditDelete(string UserID = null, string PartnershipID = null, string Delete = null)
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
                if (!Helper.IgnoreAsNullParameter(PartnershipID))
                    pars.Add("@PartnershipID", PartnershipID);
                if (!Helper.IgnoreAsNullParameter(Delete))
                    pars.Add("@Delete", Delete);
                var data = Helper.callProcedure("[adm].[usp_Partnership_Edit_Delete]", pars);
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
        [Route("insertpartners")]
        public IActionResult InsertPartners(string UserID = null, string EntityID = null, string AcctID_SubAcctIDs = null, string DayCountBasisID = null, string FirstTier1 = null, string FirstTierComparison1 = null, string FirstTier2 = null, string FirstTierComparison2 = null, string SecondTier = null, string SecondTierComparison = null, string EOD_StartDate = null, string GPFeeMult = null)
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
                if (!Helper.IgnoreAsNullParameter(EntityID))
                    pars.Add("@EntityID", EntityID);
                if (!Helper.IgnoreAsNullParameter(AcctID_SubAcctIDs))
                    pars.Add("@AcctID_SubAcctIDs", AcctID_SubAcctIDs);
                if (!Helper.IgnoreAsNullParameter(DayCountBasisID))
                    pars.Add("@DayCountBasisID ", DayCountBasisID);
                if (!Helper.IgnoreAsNullParameter(FirstTier1))
                    pars.Add("@FirstTier1", FirstTier1);
                if (!Helper.IgnoreAsNullParameter(FirstTierComparison1))
                    pars.Add("@FirstTierComparison1", FirstTierComparison1);
                if (!Helper.IgnoreAsNullParameter(FirstTier2))
                    pars.Add("@FirstTier2", FirstTier2);
                if (!Helper.IgnoreAsNullParameter(FirstTierComparison2))
                    pars.Add("@FirstTierComparison2", FirstTierComparison2);
                if (!Helper.IgnoreAsNullParameter(SecondTier))
                    pars.Add("@SecondTier", SecondTier);
                if (!Helper.IgnoreAsNullParameter(SecondTierComparison))
                    pars.Add("@SecondTierComparison", SecondTierComparison);
                if (!Helper.IgnoreAsNullParameter(EOD_StartDate))
                    pars.Add("@EOD_StartDate", EOD_StartDate);
                if (!Helper.IgnoreAsNullParameter(GPFeeMult))
                    pars.Add("@GPFeeMult", GPFeeMult);
                var data = Helper.callProcedure("[adm].[usp_Partners_Insert]", pars);
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
        [Route("partnersEditDelete")]
        public IActionResult PartnersEditDelete(string UserID = null, string PartnersID = null, string Delete = null)
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
                if (!Helper.IgnoreAsNullParameter(PartnersID))
                    pars.Add("@PartnersID", PartnersID);
                if (!Helper.IgnoreAsNullParameter(Delete))
                    pars.Add("@Delete", Delete);
                var data = Helper.callProcedure("[adm].[usp_Partners_Edit_Delete]", pars);
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
        [Route("createracontdist")]
        public IActionResult CreateRAContDist(string UserID, string EntityID, string AcctIDs, string TcodeID, string Amount, string SettleCCYID, string EffectiveDate, string PartnerTrans = null, string CashOutCash = null, string Test = null, string PartnerToCash = null, string PayRec = null)
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
                pars.Add("@EntityID", EntityID);
                pars.Add("@AcctIDs", AcctIDs);
                pars.Add("@TcodeID", TcodeID);
                pars.Add("@Amount", Amount);
                pars.Add("@SettleCCYID", SettleCCYID);
                pars.Add("@EffectiveDate", EffectiveDate);
                if (!Helper.IgnoreAsNullParameter(PartnerTrans))
                    pars.Add("@PartnerTrans", PartnerTrans);
                if (!Helper.IgnoreAsNullParameter(PayRec))
                    pars.Add("@PayRec", PayRec);
                if (!Helper.IgnoreAsNullParameter(CashOutCash))
                    pars.Add("@CashOutCash", CashOutCash);
                if (!Helper.IgnoreAsNullParameter(Test))
                    pars.Add("@Test", Test);
                if (!Helper.IgnoreAsNullParameter(PartnerToCash))
                    pars.Add("@PartnerToCash", PartnerToCash);
                var data = Helper.callProcedure("adm.usp_Partnership_RA_ContDist_Tran_Create", pars);
                return Ok(new { error = false, data = data });
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