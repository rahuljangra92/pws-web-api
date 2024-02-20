using PWSWebApi.handlers;
using PWSWebApi.Models.PartnershipTracking;
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
    [Route("Security")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        private readonly string connectionString;
        private readonly Helper handlerHelper;

        public SecurityController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("PWSProd");
            handlerHelper = new Helper(configuration);
        }
      
        [HttpGet]
        [Route("Search")]
        public IActionResult SearchSecurities(string SettlementCCYID = null, string NotionalAmtMax = null, string SettlementAmtMax = null, string SettlementAmtMin = null, string NotionalCCYID = null, string NotionalAmtMin = null, string ContractTypeID = null, string StrikePriceMax = null, string StrikePriceMin = null, string ContractDateExpEnd = null, string ContractDateExpStart = null, string ContractDateStartEnd = null, string ContractDateStartStart = null, string BusDayConvID = null, string LastReg = null, string FirstReg = null, string AA = null, string USState = null, string AvgCostEligible = null, string CallSchd = null, string Comingled = null, string Cusip = null, string DatedDateEnd = null, string DatedDateStart = null, string DayCtBasisID = null, string DPID = null, string Exchg = null, string FactorSchd = null, string FirstPayDateEnd = null, string FirstPayDateStart = null, string FloatRate = null, string IncCCYID = null, string Income = null, string IntRate = null, string ISIN = null, string IssueDateEnd = null, string IssueDateStart = null, string LastPayDateEnd = null, string LastPayDateStart = null, string LoadSourceID = null, string MaturityDateEnd = null, string MaturityDateStart = null, string PeriodicityID = null, string PriceMult = null, string PrincCCYID = null, string RIC = null, string SecID = null, string SecName = null, string SecTypeID = null, string Sedol = null, string Stat = null, string StateTaxStat = null, string Symbol = null, string UnderlyingSecID = null, string Unitized = null, string UserGroupID = null, string UserID = null, string ValMethodID = null)
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
                if (!Helper.IgnoreAsNullParameter(AA))
                    pars.Add("@AA", AA);
                if (!Helper.IgnoreAsNullParameter(ContractTypeID))
                    pars.Add("@ContractTypeID", ContractTypeID);
                if (!Helper.IgnoreAsNullParameter(ContractDateStartStart))
                    pars.Add("@ContractDateStartStart", ContractDateStartStart);
                if (!Helper.IgnoreAsNullParameter(ContractDateStartEnd))
                    pars.Add("@ContractDateStartEnd", ContractDateStartEnd);
                if (!Helper.IgnoreAsNullParameter(ContractDateExpStart))
                    pars.Add("@ContractDateExpStart", ContractDateExpStart);
                if (!Helper.IgnoreAsNullParameter(ContractDateExpEnd))
                    pars.Add("@ContractDateExpEnd", ContractDateExpEnd);
                if (!Helper.IgnoreAsNullParameter(StrikePriceMin))
                    pars.Add("@StrikePriceMin", StrikePriceMin);
                if (!Helper.IgnoreAsNullParameter(StrikePriceMax))
                    pars.Add("@StrikePriceMax", StrikePriceMax);
                if (!Helper.IgnoreAsNullParameter(NotionalAmtMin))
                    pars.Add("@NotionalAmtMin", NotionalAmtMin);
                if (!Helper.IgnoreAsNullParameter(NotionalAmtMax))
                    pars.Add("@NotionalAmtMax", NotionalAmtMax);
                if (!Helper.IgnoreAsNullParameter(NotionalCCYID))
                    pars.Add("@NotionalCCYID", NotionalCCYID);
                if (!Helper.IgnoreAsNullParameter(SettlementAmtMin))
                    pars.Add("@SettlementAmtMin", SettlementAmtMin);
                if (!Helper.IgnoreAsNullParameter(SettlementAmtMax))
                    pars.Add("@SettlementAmtMax", SettlementAmtMax);
                if (!Helper.IgnoreAsNullParameter(SettlementCCYID))
                    pars.Add("@SettlementCCYID", SettlementCCYID);
                if (!Helper.IgnoreAsNullParameter(BusDayConvID))
                    pars.Add("@BusDayConvID", BusDayConvID);
                if (!Helper.IgnoreAsNullParameter(LastReg))
                    pars.Add("@LastReg", LastReg);
                if (!Helper.IgnoreAsNullParameter(FirstReg))
                    pars.Add("@FirstReg", FirstReg);
                if (!Helper.IgnoreAsNullParameter(USState))
                    pars.Add("@USState", USState);
                if (!Helper.IgnoreAsNullParameter(AvgCostEligible))
                    pars.Add("@AvgCostEligible", AvgCostEligible);
                if (!Helper.IgnoreAsNullParameter(CallSchd))
                    pars.Add("@CallSchd", CallSchd);
                if (!Helper.IgnoreAsNullParameter(Comingled))
                    pars.Add("@Comingled", Comingled);
                if (!Helper.IgnoreAsNullParameter(Cusip))
                    pars.Add("@Cusip", Cusip);
                if (!Helper.IgnoreAsNullParameter(DatedDateEnd))
                    pars.Add("@DatedDateEnd", DatedDateEnd);
                if (!Helper.IgnoreAsNullParameter(DatedDateStart))
                    pars.Add("@DatedDateStart", DatedDateStart);
                if (!Helper.IgnoreAsNullParameter(DayCtBasisID))
                    pars.Add("@DayCtBasisID", DayCtBasisID);
                if (!Helper.IgnoreAsNullParameter(DPID))
                    pars.Add("@DPID", DPID);
                if (!Helper.IgnoreAsNullParameter(Exchg))
                    pars.Add("@Exchg", Exchg);
                if (!Helper.IgnoreAsNullParameter(FactorSchd))
                    pars.Add("@FactorSchd", FactorSchd);
                if (!Helper.IgnoreAsNullParameter(FirstPayDateEnd))
                    pars.Add("@FirstPayDateEnd", FirstPayDateEnd);
                if (!Helper.IgnoreAsNullParameter(FirstPayDateStart))
                    pars.Add("@FirstPayDateStart", FirstPayDateStart);
                if (!Helper.IgnoreAsNullParameter(FloatRate))
                    pars.Add("@FloatRate", FloatRate);
                if (!Helper.IgnoreAsNullParameter(IncCCYID))
                    pars.Add("@IncCCYID", IncCCYID);
                if (!Helper.IgnoreAsNullParameter(Income))
                    pars.Add("@Income", Income);
                if (!Helper.IgnoreAsNullParameter(IntRate))
                    pars.Add("@IntRate", IntRate);
                if (!Helper.IgnoreAsNullParameter(ISIN))
                    pars.Add("@ISIN", ISIN);
                if (!Helper.IgnoreAsNullParameter(IssueDateEnd))
                    pars.Add("@IssueDateEnd", IssueDateEnd);
                if (!Helper.IgnoreAsNullParameter(IssueDateStart))
                    pars.Add("@IssueDateStart", IssueDateStart);
                if (!Helper.IgnoreAsNullParameter(LastPayDateEnd))
                    pars.Add("@LastPayDateEnd", LastPayDateEnd);
                if (!Helper.IgnoreAsNullParameter(LastPayDateStart))
                    pars.Add("@LastPayDateStart", LastPayDateStart);
                if (!Helper.IgnoreAsNullParameter(LoadSourceID))
                    pars.Add("@LoadSourceID", LoadSourceID);
                if (!Helper.IgnoreAsNullParameter(MaturityDateEnd))
                    pars.Add("@MaturityDateEnd", MaturityDateEnd);
                if (!Helper.IgnoreAsNullParameter(MaturityDateStart))
                    pars.Add("@MaturityDateStart", MaturityDateStart);
                if (!Helper.IgnoreAsNullParameter(PeriodicityID))
                    pars.Add("@PeriodicityID", PeriodicityID);
                if (!Helper.IgnoreAsNullParameter(PriceMult))
                    pars.Add("@PriceMult", PriceMult);
                if (!Helper.IgnoreAsNullParameter(PrincCCYID))
                    pars.Add("@PrincCCYID", PrincCCYID);
                if (!Helper.IgnoreAsNullParameter(RIC))
                    pars.Add("@RIC", RIC);
                if (!Helper.IgnoreAsNullParameter(SecID))
                    pars.Add("@SecID", SecID);
                if (!Helper.IgnoreAsNullParameter(SecName))
                    pars.Add("@SecName", SecName);
                if (!Helper.IgnoreAsNullParameter(SecTypeID))
                    pars.Add("@SecTypeID", SecTypeID);
                if (!Helper.IgnoreAsNullParameter(Sedol))
                    pars.Add("@Sedol", Sedol);
                if (!Helper.IgnoreAsNullParameter(Stat))
                    pars.Add("@Stat", Stat);
                if (!Helper.IgnoreAsNullParameter(StateTaxStat))
                    pars.Add("@StateTaxStat", StateTaxStat);
                if (!Helper.IgnoreAsNullParameter(Symbol))
                    pars.Add("@Symbol", Symbol);
                if (!Helper.IgnoreAsNullParameter(UnderlyingSecID))
                    pars.Add("@UnderlyingSecID", UnderlyingSecID);
                if (!Helper.IgnoreAsNullParameter(Unitized))
                    pars.Add("@Unitized", Unitized);
                if (!Helper.IgnoreAsNullParameter(UserGroupID))
                    pars.Add("@UserGroupID", UserGroupID);
                if (!Helper.IgnoreAsNullParameter(UserID))
                    pars.Add("@UserID", UserID);
                if (!Helper.IgnoreAsNullParameter(ValMethodID))
                    pars.Add("@ValMethodID", ValMethodID);
                var data = Helper.callProcedure("adm.usp_Sec_Search", pars);
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
        [Route("Insert")]
        public IActionResult InsertSecurities(string NextCallDate = null, string NextCallPrice = null, string BloombSymbol = null, string IssuerID = null, string DayofPmt = null, string OIPrice = null, string RedemptInfo = null, string Notional_Amt = null, string Settle_Amt = null, string SettleCCYID = null, string ContractStartDate = null, string ContractFirstPaymentDate = null, string ContractLastPaymentDate = null, string ContractExpiryDate = null, string OptionType = null, string OptionStyle = null, string CashSettle = null, string ETF = null, string NewSecID = null, string DailyAccrSec = null, string InactiveDate = null, string IntRate = null, string Hybrid = null, string SecuritizedID = null, string Par = null, string Valoren = null, string SettlementCCYID = null, string NotionalAmt = null, string SettlementAmt = null, string NotionalCCYID = null, string ContractTypeID = null, string StrikePrice = null, string ContractDateExp = null, string ContractDate = null, string ContractDateStart = null, string BusDayConvID = null, string LastReg = null, string FirstReg = null, string AA = null, string USState = null, string AvgCostEligible = null, string CallSchd = null, string Comingled = null, string Cusip = null, string DatedDate = null, string DayCtBasisID = null, string DPID = null, string Exchg = null, string FactorSchd = null, string FirstPayDate = null, string FloatRate = null, string IncCCYID = null, string Income = null, string IntRateHigh = null, string IntRateLow = null, string ISIN = null, string IssueDate = null, string LastPayDate = null, string LoadSourceID = null, string MaturityDate = null, string PeriodicityID = null, string PriceMult = null, string PrincCCYID = null, string RIC = null, string SecID = null, string SecName = null, string SecTypeID = null, string Sedol = null, string Stat = null, string StateTaxStat = null, string Symbol = null, string UnderlyingSecID = null, string Unitized = null, string UserGroupID = null, string UserID = null, string ValMethodID = null)
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
                if (!Helper.IgnoreAsNullParameter(SecuritizedID))
                    pars.Add("@SecuritizedID", SecuritizedID);
                if (!Helper.IgnoreAsNullParameter(Hybrid))
                    pars.Add("@Hybrid", Hybrid);
                if (!Helper.IgnoreAsNullParameter(IntRate))
                    pars.Add("@IntRate", IntRate);
                if (!Helper.IgnoreAsNullParameter(InactiveDate))
                    pars.Add("@InactiveDate", InactiveDate);
                if (!Helper.IgnoreAsNullParameter(DailyAccrSec))
                    pars.Add("@DailyAccrSec", DailyAccrSec);
                if (!Helper.IgnoreAsNullParameter(NewSecID))
                    pars.Add("@NewSecID", NewSecID);
                if (!Helper.IgnoreAsNullParameter(ETF))
                    pars.Add("@ETF", ETF);
                if (!Helper.IgnoreAsNullParameter(CashSettle))
                    pars.Add("@CashSettle", CashSettle);
                if (!Helper.IgnoreAsNullParameter(OptionStyle))
                    pars.Add("@OptionStyle", OptionStyle);
                if (!Helper.IgnoreAsNullParameter(OptionType))
                    pars.Add("@OptionType", OptionType);
                if (!Helper.IgnoreAsNullParameter(ContractExpiryDate))
                    pars.Add("@ContractExpiryDate", ContractExpiryDate);
                if (!Helper.IgnoreAsNullParameter(ContractLastPaymentDate))
                    pars.Add("@ContractLastPaymentDate", ContractLastPaymentDate);
                if (!Helper.IgnoreAsNullParameter(ContractFirstPaymentDate))
                    pars.Add("@ContractFirstPaymentDate", ContractFirstPaymentDate);
                if (!Helper.IgnoreAsNullParameter(ContractStartDate))
                    pars.Add("@ContractStartDate", ContractStartDate);
                if (!Helper.IgnoreAsNullParameter(SettleCCYID))
                    pars.Add("@SettleCCYID", SettleCCYID);
                if (!Helper.IgnoreAsNullParameter(Settle_Amt))
                    pars.Add("@Settle_Amt", Settle_Amt);
                if (!Helper.IgnoreAsNullParameter(Notional_Amt))
                    pars.Add("@Notional_Amt", Notional_Amt);
                if (!Helper.IgnoreAsNullParameter(RedemptInfo))
                    pars.Add("@RedemptInfo", RedemptInfo);
                if (!Helper.IgnoreAsNullParameter(OIPrice))
                    pars.Add("@OIPrice", OIPrice);
                if (!Helper.IgnoreAsNullParameter(DayofPmt))
                    pars.Add("@DayofPmt", DayofPmt);
                if (!Helper.IgnoreAsNullParameter(IssuerID))
                    pars.Add("@IssuerID", IssuerID);
                if (!Helper.IgnoreAsNullParameter(BloombSymbol))
                    pars.Add("@BloombSymbol", BloombSymbol);
                if (!Helper.IgnoreAsNullParameter(AA))
                    pars.Add("@AA", AA);
                if (!Helper.IgnoreAsNullParameter(Par))
                    pars.Add("@Par", Par);
                if (!Helper.IgnoreAsNullParameter(NextCallDate))
                    pars.Add("@CallDATE", NextCallDate);
                if (!Helper.IgnoreAsNullParameter(NextCallPrice))
                    pars.Add("@CallParValue", NextCallPrice);
                if (!Helper.IgnoreAsNullParameter(Valoren))
                    pars.Add("@Valoren", Valoren);
                if (!Helper.IgnoreAsNullParameter(ContractTypeID))
                    pars.Add("@ContractTypeID", ContractTypeID);
                if (!Helper.IgnoreAsNullParameter(ContractDateStart))
                    pars.Add("@ContractDateStart", ContractDateStart);
                if (!Helper.IgnoreAsNullParameter(ContractDateExp))
                    pars.Add("@ContractDateExp", ContractDateExp);
                if (!Helper.IgnoreAsNullParameter(StrikePrice))
                    pars.Add("@StrikePrice", StrikePrice);
                if (!Helper.IgnoreAsNullParameter(NotionalAmt))
                    pars.Add("@NotionalAmt", NotionalAmt);
                if (!Helper.IgnoreAsNullParameter(NotionalCCYID))
                    pars.Add("@NotionalCCYID", NotionalCCYID);
                if (!Helper.IgnoreAsNullParameter(SettlementAmt))
                    pars.Add("@SettlementAmt", SettlementAmt);
                if (!Helper.IgnoreAsNullParameter(SettlementCCYID))
                    pars.Add("@SettlementCCYID", SettlementCCYID);
                if (!Helper.IgnoreAsNullParameter(BusDayConvID))
                    pars.Add("@BusDayConvID", BusDayConvID);
                if (!Helper.IgnoreAsNullParameter(LastReg))
                    pars.Add("@LastReg", LastReg);
                if (!Helper.IgnoreAsNullParameter(FirstReg))
                    pars.Add("@FirstReg", FirstReg);
                if (!Helper.IgnoreAsNullParameter(USState))
                    pars.Add("@USState", USState);
                if (!Helper.IgnoreAsNullParameter(AvgCostEligible))
                    pars.Add("@AvgCostEligible", AvgCostEligible);
                if (!Helper.IgnoreAsNullParameter(CallSchd))
                    pars.Add("@CallSchd", CallSchd);
                if (!Helper.IgnoreAsNullParameter(Comingled))
                    pars.Add("@Comingled", Comingled);
                if (!Helper.IgnoreAsNullParameter(Cusip))
                    pars.Add("@Cusip", Cusip);
                if (!Helper.IgnoreAsNullParameter(DatedDate))
                    pars.Add("@DatedDate", DatedDate);
                if (!Helper.IgnoreAsNullParameter(DayCtBasisID))
                    pars.Add("@DayCtBasisID", DayCtBasisID);
                if (!Helper.IgnoreAsNullParameter(DPID))
                    pars.Add("@DPID", DPID);
                if (!Helper.IgnoreAsNullParameter(Exchg))
                    pars.Add("@Exchg", Exchg);
                if (!Helper.IgnoreAsNullParameter(FactorSchd))
                    pars.Add("@FactorSchd", FactorSchd);
                if (!Helper.IgnoreAsNullParameter(FirstPayDate))
                    pars.Add("@FirstPayDate", FirstPayDate);
                if (!Helper.IgnoreAsNullParameter(FloatRate))
                    pars.Add("@FloatRate", FloatRate);
                if (!Helper.IgnoreAsNullParameter(IncCCYID))
                    pars.Add("@IncCCYID", IncCCYID);
                if (!Helper.IgnoreAsNullParameter(Income))
                    pars.Add("@Income", Income);
                if (!Helper.IgnoreAsNullParameter(IntRateHigh))
                    pars.Add("@IntRateHigh", IntRateHigh);
                if (!Helper.IgnoreAsNullParameter(IntRateLow))
                    pars.Add("@IntRateLow", IntRateLow);
                if (!Helper.IgnoreAsNullParameter(ISIN))
                    pars.Add("@ISIN", ISIN);
                if (!Helper.IgnoreAsNullParameter(IssueDate))
                    pars.Add("@IssueDate", IssueDate);
                if (!Helper.IgnoreAsNullParameter(LastPayDate))
                    pars.Add("@LastPayDate", LastPayDate);
                if (!Helper.IgnoreAsNullParameter(LoadSourceID))
                    pars.Add("@LoadSourceID", LoadSourceID);
                if (!Helper.IgnoreAsNullParameter(MaturityDate))
                    pars.Add("@MaturityDate", MaturityDate);
                if (!Helper.IgnoreAsNullParameter(PeriodicityID))
                    pars.Add("@PeriodicityID", PeriodicityID);
                if (!Helper.IgnoreAsNullParameter(PriceMult))
                    pars.Add("@PriceMult", PriceMult);
                if (!Helper.IgnoreAsNullParameter(PrincCCYID))
                    pars.Add("@PrincCCYID", PrincCCYID);
                if (!Helper.IgnoreAsNullParameter(RIC))
                    pars.Add("@RIC", RIC);
                if (!Helper.IgnoreAsNullParameter(SecID))
                    pars.Add("@SecID", SecID);
                if (!Helper.IgnoreAsNullParameter(SecName))
                    pars.Add("@SecName", SecName);
                if (!Helper.IgnoreAsNullParameter(SecTypeID))
                    pars.Add("@SecTypeID", SecTypeID);
                if (!Helper.IgnoreAsNullParameter(Sedol))
                    pars.Add("@Sedol", Sedol);
                if (!Helper.IgnoreAsNullParameter(Stat))
                    pars.Add("@Stat", Stat);
                if (!Helper.IgnoreAsNullParameter(StateTaxStat))
                    pars.Add("@StateTaxStat", StateTaxStat);
                if (!Helper.IgnoreAsNullParameter(Symbol))
                    pars.Add("@Symbol", Symbol);
                if (!Helper.IgnoreAsNullParameter(UnderlyingSecID))
                    pars.Add("@UnderlyingSecID", UnderlyingSecID);
                if (!Helper.IgnoreAsNullParameter(Unitized))
                    pars.Add("@Unitized", Unitized);
                if (!Helper.IgnoreAsNullParameter(UserGroupID))
                    pars.Add("@UserGroupID", UserGroupID);
                if (!Helper.IgnoreAsNullParameter(UserID))
                    pars.Add("@UserID", UserID);
                if (!Helper.IgnoreAsNullParameter(ValMethodID))
                    pars.Add("@ValMethodID", ValMethodID);
                var data = Helper.callProcedure("adm.usp_Sec_Insert", pars);
                if (data == "[]")
                    return Ok( new { error = false, data = data });
                else
                    return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = "Database error" });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("Edit")]
        public IActionResult SecurityEdit(string DailyAccrSec = null, string InactiveDate = null, string Hybrid = null, string ETF = null, string BloombSymbol = null, string IssuerID = null, string DayofPmt = null, string OIPrice = null, string RedemptInfo = null, string Notional_Amt = null, string Settle_Amt = null, string SettleCCYID = null, string ContractStartDate = null, string ContractFirstPaymentDate = null, string ContractLastPaymentDate = null, string ContractExpiryDate = null, string OptionType = null, string OptionStyle = null, string CashSettle = null, string NewSecID = null, string IntRate = null, string SecuritizedID = null, string Par = null, string Valoren = null, string SettlementCCYID = null, string NotionalAmt = null, string SettlementAmt = null, string NotionalCCYID = null, string ContractTypeID = null, string StrikePrice = null, string ContractDateExp = null, string ContractDate = null, string ContractDateStart = null, string BusDayConvID = null, string LastReg = null, string FirstReg = null, string AA = null, string USState = null, string AvgCostEligible = null, string CallSchd = null, string Comingled = null, string Cusip = null, string DatedDate = null, string DayCtBasisID = null, string DPID = null, string Exchg = null, string FactorSchd = null, string FirstPayDate = null, string FloatRate = null, string IncCCYID = null, string Income = null, string IntRateHigh = null, string IntRateLow = null, string ISIN = null, string IssueDate = null, string LastPayDate = null, string LoadSourceID = null, string MaturityDate = null, string PeriodicityID = null, string PriceMult = null, string PrincCCYID = null, string RIC = null, string SecID = null, string SecName = null, string SecTypeID = null, string Sedol = null, string Stat = null, string StateTaxStat = null, string Symbol = null, string UnderlyingSecID = null, string Unitized = null, string UserGroupID = null, string UserID = null, string ValMethodID = null)
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
                pars.Add("@SecID", SecID);
                if (!Helper.IgnoreAsNullParameter(SecuritizedID))
                    pars.Add("@SecuritizedID", SecuritizedID);
                if (!Helper.IgnoreAsNullParameter(ETF))
                    pars.Add("@ETF", ETF);
                if (!Helper.IgnoreAsNullParameter(Hybrid))
                    pars.Add("@Hybrid", Hybrid);
                if (!Helper.IgnoreAsNullParameter(DailyAccrSec))
                    pars.Add("@DailyAccrSec", DailyAccrSec);
                if (!Helper.IgnoreAsNullParameter(InactiveDate))
                    pars.Add("@InactiveDate", InactiveDate);
                if (!Helper.IgnoreAsNullParameter(IntRate))
                    pars.Add("@IntRate", IntRate);
                if (!Helper.IgnoreAsNullParameter(NewSecID))
                    pars.Add("@NewSecID", NewSecID);
                if (!Helper.IgnoreAsNullParameter(CashSettle))
                    pars.Add("@CashSettle", CashSettle);
                if (!Helper.IgnoreAsNullParameter(OptionStyle))
                    pars.Add("@OptionStyle", OptionStyle);
                if (!Helper.IgnoreAsNullParameter(OptionType))
                    pars.Add("@OptionType", OptionType);
                if (!Helper.IgnoreAsNullParameter(ContractExpiryDate))
                    pars.Add("@ContractExpiryDate", ContractExpiryDate);
                if (!Helper.IgnoreAsNullParameter(ContractLastPaymentDate))
                    pars.Add("@ContractLastPaymentDate", ContractLastPaymentDate);
                if (!Helper.IgnoreAsNullParameter(ContractFirstPaymentDate))
                    pars.Add("@ContractFirstPaymentDate", ContractFirstPaymentDate);
                if (!Helper.IgnoreAsNullParameter(ContractStartDate))
                    pars.Add("@ContractStartDate", ContractStartDate);
                if (!Helper.IgnoreAsNullParameter(SettleCCYID))
                    pars.Add("@SettleCCYID", SettleCCYID);
                if (!Helper.IgnoreAsNullParameter(Settle_Amt))
                    pars.Add("@Settle_Amt", Settle_Amt);
                if (!Helper.IgnoreAsNullParameter(Notional_Amt))
                    pars.Add("@Notional_Amt", Notional_Amt);
                if (!Helper.IgnoreAsNullParameter(RedemptInfo))
                    pars.Add("@RedemptInfo", RedemptInfo);
                if (!Helper.IgnoreAsNullParameter(OIPrice))
                    pars.Add("@OIPrice", OIPrice);
                if (!Helper.IgnoreAsNullParameter(DayofPmt))
                    pars.Add("@DayofPmt", DayofPmt);
                if (!Helper.IgnoreAsNullParameter(IssuerID))
                    pars.Add("@IssuerID", IssuerID);
                if (!Helper.IgnoreAsNullParameter(BloombSymbol))
                    pars.Add("@BloombSymbol", BloombSymbol);
                if (!Helper.IgnoreAsNullParameter(AA))
                    pars.Add("@AA", AA);
                if (!Helper.IgnoreAsNullParameter(Par))
                    pars.Add("@Par", Par);
                if (!Helper.IgnoreAsNullParameter(Valoren))
                    pars.Add("@Valoren", Valoren);
                if (!Helper.IgnoreAsNullParameter(ContractTypeID))
                    pars.Add("@ContractTypeID", ContractTypeID);
                if (!Helper.IgnoreAsNullParameter(ContractDate))
                    pars.Add("@ContractDate", ContractDate);
                if (!Helper.IgnoreAsNullParameter(ContractDateExp))
                    pars.Add("@ContractDateExp", ContractDateExp);
                if (!Helper.IgnoreAsNullParameter(StrikePrice))
                    pars.Add("@StrikePrice", StrikePrice);
                if (!Helper.IgnoreAsNullParameter(NotionalAmt))
                    pars.Add("@NotionalAmt", NotionalAmt);
                if (!Helper.IgnoreAsNullParameter(NotionalCCYID))
                    pars.Add("@NotionalCCYID", NotionalCCYID);
                if (!Helper.IgnoreAsNullParameter(SettlementAmt))
                    pars.Add("@SettlementAmt", SettlementAmt);
                if (!Helper.IgnoreAsNullParameter(SettlementCCYID))
                    pars.Add("@SettlementCCYID", SettlementCCYID);
                if (!Helper.IgnoreAsNullParameter(BusDayConvID))
                    pars.Add("@BusDayConvID", BusDayConvID);
                if (!Helper.IgnoreAsNullParameter(LastReg))
                    pars.Add("@LastReg", LastReg);
                if (!Helper.IgnoreAsNullParameter(FirstReg))
                    pars.Add("@FirstReg", FirstReg);
                if (!Helper.IgnoreAsNullParameter(USState))
                    pars.Add("@USState", USState);
                if (!Helper.IgnoreAsNullParameter(AvgCostEligible))
                    pars.Add("@AvgCostEligible", AvgCostEligible);
                if (!Helper.IgnoreAsNullParameter(CallSchd))
                    pars.Add("@CallSchd", CallSchd);
                if (!Helper.IgnoreAsNullParameter(Cusip))
                    pars.Add("@Cusip", Cusip);
                if (!Helper.IgnoreAsNullParameter(DatedDate))
                    pars.Add("@DatedDate", DatedDate);
                if (!Helper.IgnoreAsNullParameter(DayCtBasisID))
                    pars.Add("@DayCtBasisID", DayCtBasisID);
                if (!Helper.IgnoreAsNullParameter(DPID))
                    pars.Add("@DPID", DPID);
                if (!Helper.IgnoreAsNullParameter(Exchg))
                    pars.Add("@Exchg", Exchg);
                if (!Helper.IgnoreAsNullParameter(FactorSchd))
                    pars.Add("@FactorSchd", FactorSchd);
                if (!Helper.IgnoreAsNullParameter(FirstPayDate))
                    pars.Add("@FirstPayDate", FirstPayDate);
                if (!Helper.IgnoreAsNullParameter(FloatRate))
                    pars.Add("@FloatRate", FloatRate);
                if (!Helper.IgnoreAsNullParameter(IncCCYID))
                    pars.Add("@IncCCYID", IncCCYID);
                if (!Helper.IgnoreAsNullParameter(Income))
                    pars.Add("@Income", Income);
                if (!Helper.IgnoreAsNullParameter(ISIN))
                    pars.Add("@ISIN", ISIN);
                if (!Helper.IgnoreAsNullParameter(IssueDate))
                    pars.Add("@IssueDate", IssueDate);
                if (!Helper.IgnoreAsNullParameter(LastPayDate))
                    pars.Add("@LastPayDate", LastPayDate);
                if (!Helper.IgnoreAsNullParameter(LoadSourceID))
                    pars.Add("@LoadSourceID", LoadSourceID);
                if (!Helper.IgnoreAsNullParameter(MaturityDate))
                    pars.Add("@MaturityDate", MaturityDate);
                if (!Helper.IgnoreAsNullParameter(PeriodicityID))
                    pars.Add("@PeriodicityID", PeriodicityID);
                if (!Helper.IgnoreAsNullParameter(PriceMult))
                    pars.Add("@PriceMult", PriceMult);
                if (!Helper.IgnoreAsNullParameter(PrincCCYID))
                    pars.Add("@PrincCCYID", PrincCCYID);
                if (!Helper.IgnoreAsNullParameter(RIC))
                    pars.Add("@RIC", RIC);
                if (!Helper.IgnoreAsNullParameter(SecName))
                    pars.Add("@SecName", SecName);
                if (!Helper.IgnoreAsNullParameter(SecTypeID))
                    pars.Add("@SecTypeID", SecTypeID);
                if (!Helper.IgnoreAsNullParameter(Sedol))
                    pars.Add("@Sedol", Sedol);
                if (!Helper.IgnoreAsNullParameter(Stat))
                    pars.Add("@Stat", Stat);
                if (!Helper.IgnoreAsNullParameter(StateTaxStat))
                    pars.Add("@StateTaxStat", StateTaxStat);
                if (!Helper.IgnoreAsNullParameter(Symbol))
                    pars.Add("@Symbol", Symbol);
                if (!Helper.IgnoreAsNullParameter(UnderlyingSecID))
                    pars.Add("@UnderlyingSecID", UnderlyingSecID);
                if (!Helper.IgnoreAsNullParameter(UserGroupID))
                    pars.Add("@UserGroupID", UserGroupID);
                if (!Helper.IgnoreAsNullParameter(UserID))
                    pars.Add("@UserID", UserID);
                if (!Helper.IgnoreAsNullParameter(ValMethodID))
                    pars.Add("@ValMethodID", ValMethodID);
                var data = Helper.callProcedure("adm.usp_Sec_Edit", pars);
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
        [Route("Delete")]
        public IActionResult SecurityDelete(string UserID, string SecID)
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
                if (!Helper.IgnoreAsNullParameter(UserID))
                    pars.Add("@UserID", UserID);
                if (!Helper.IgnoreAsNullParameter(SecID))
                    pars.Add("@SecID", SecID);
                var data = Helper.callProcedure("adm.usp_Sec_Delete", pars);
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
        [Route("securitygetprinceccyid")]
        public IActionResult SecurityGetPrinceCCYID(string UserID)
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
                var data = Helper.callProcedure("usp_Userpreferences", pars);
                return Ok(new { error = false, data = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }
    }
}