using Datastream.DswsApi;
using Newtonsoft.Json;
using PWSWebApi.ThompsonReuters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Cors;
using DataScope.Select.Api.Content;
using DataScope.Select.Api.Extractions;
using DataScope.Select.Api.Extractions.ExtractionRequests;
using DataScope.Select.Api.Extractions.ReportTemplates;
using DataScope.Select.Api.Extractions.SubjectLists;
using DataScope.Select.Core.RestApi;
using PWSWebApi.handlers;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

namespace PWSWebApi.Controllers
{
    public class TRSearchTermnsAndCondJson
    {
        public string Security { get; set; }

        public string RIC { get; set; }

        public string CurrCode { get; set; }

        public string ExchangeCode { get; set; }

        public string ExchangeName { get; set; }

        public string DPID { get; set; }

        public string Identifier { get; set; }

        public string IdentifierType { get; set; }

        public string LoadSourceID { get; set; }

        public string Source { get; set; }
    }

    public class IndexCodeGetIdentifierObject
    {
        public string DS_Index_Code { get; set; }

        public string Preferred_DataType { get; set; }

        public string SecID { get; set; }
    }

    //[EnableCors(origins: "http://localhost/", headers: "*", methods: "*")]
    /* Added by CTA: RoutePrefix attribute is no longer supported */
    [RoutePrefix("TR")]
    [ApiController]
    public class TRController : ControllerBase
    {
        static string connectionString = ConfigurationManager.Configuration.GetSection("ConnectionStrings")["PWSProd"].ToString();
        static string pwsRectConnectionString = ConfigurationManager.Configuration.GetSection("ConnectionStrings")["PWSRec"].ToString();
        PWSRecDataContext pwsrec = new PWSRecDataContext(pwsRectConnectionString);
        DataClasses1DataContext pws = new DataClasses1DataContext(connectionString);
        Helper handlerHelper = new Helper();
        bool devMode = Convert.ToBoolean(Convert.ToString(ConfigurationManager.Configuration.GetSection("appSettings")["devMode"]));
        public TRController() : base()
        {
            pwsrec = new PWSRecDataContext(pwsRectConnectionString);
            pwsrec.CommandTimeout = 60000;
            pws = new DataClasses1DataContext(connectionString);
            pws.CommandTimeout = 60000;
        }

        protected ExtractionsContext ExtractionsContext { get; set; }

        private void InitDssWs()
        {
            DSClient.Options.UserName = "ZPWS001";
            DSClient.Options.Password = "WATER602";
            DSClient.Init();
        }

        private void WritePtocessHistory(int processId, DateTime runDateTime, Exception ex = null)
        {
            var newRow = new ProcessHistory();
            newRow.Process_ID = processId;
            newRow.RunDateTime = runDateTime;
            newRow.UpdateDateTime = DateTime.Now;
            if (ex != null)
                newRow.Error = ex.Message;
            pwsrec.ProcessHistories.InsertOnSubmit(newRow);
            pwsrec.SubmitChanges();
        }

        [HttpPost]
        [Route("Index")]
        public HttpResponseMessage GetSampleDSSWS(string DSIndexCode, string PreferredDataType, string SecID, DateTime? startDate = null, DateTime? endDate = null, string RepChar = null, string IndexChar = null)
        {
            try
            {
                //if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
                //{
                //    return (HttpResponseMessage)handlerHelper.ValidateSource(Request);
                //}
                //if (handlerHelper.ValidateToken(Request) != string.Empty)
                //{
                //    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
                //}
                InitDssWs();
                if (!string.IsNullOrEmpty(IndexChar) && !string.IsNullOrEmpty(RepChar))
                {
                    var charsToInsert = RepChar.Split(',');
                    var indicesToReplaceIn = IndexChar.Split(',');
                    StringBuilder sb = new StringBuilder(DSIndexCode);
                    for (int i = charsToInsert.Length - 1; i >= 0; i--)
                    {
                        sb = sb.Insert(Convert.ToInt32(indicesToReplaceIn[i]), Util.GetConvertedCharacter(charsToInsert[i]));
                    }

                    DSIndexCode = sb.ToString();
                }

                pwsrec = new PWSRecDataContext();
                pwsrec.CommandTimeout = 0;
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                //var prodDataString = callProcedure("[tr].usp_Index_Code_GetIdentifiers", pars);
                var identifierData = new List<IndexCodeGetIdentifierObject>(); //JsonConvert.DeserializeObject<List<IndexCodeGetIdentifierObject>>(prodDataString);
                identifierData.Add(new IndexCodeGetIdentifierObject { DS_Index_Code = DSIndexCode, Preferred_DataType = PreferredDataType, SecID = SecID });
                endDate = endDate == null ? DateTime.Now : endDate;
                startDate = startDate == null ? endDate : startDate;
                foreach (var d in identifierData)
                {
                    var dataTypeConstructions = "DPL#(X(" + d.Preferred_DataType + "),8)";
                    if (d.Preferred_DataType.Contains("X(IN)+100") || d.Preferred_DataType.Contains("X(IN) 100"))
                    {
                        dataTypeConstructions = "DPL#(X(IN)+100,8)";
                    }

                    var request = //d.DS_List_Index_Code.ToLower() == "none" || d.DS_List_Index_Code.ToLower() == "n/a" ?
 new DSDataRequest()
                    {
                        Instrument = new DSInstrument(d.DS_Index_Code),
                        DataTypes = new DSDataTypes(dataTypeConstructions), //new DSDataTypes("DPL#(X(" + d.Preferred_DataType + "),8)"), //"DY", "MV", "PH", "PL", "PO"),
                        Date = new DSTimeSeriesDate(DSDateType.Absolute(Convert.ToDateTime(startDate)), DSDateType.Absolute(Convert.ToDateTime(endDate)), DSDateFrequency.Daily),
                        Properties = new DSDataRequestProperties(DSDataRequestPropertyTypes.ReturnDataTypeExpandedName | DSDataRequestPropertyTypes.ReturnInstrumentExpandedName)
                    };
                    try
                    {
                        // Execute the request 
                        var response = DSClient.DataService.GetData(request);
                        // Get the response value 
                        //double[] dt1Values = response["MSRI"]["MSWRLD$"].GetValue<double[]>();
                        //double[] dt2Values = response["MSNR"]["MSWRLD$"].GetValue<double[]>();
                        if (response.Dates == null)
                        {
                            continue;
                        }

                        var recordCount = response.Dates.Length;
                        for (int i = 0; i < recordCount; i++)
                        {
                            var newRow = new Load_Index();
                            if (response.SymbolNames.Length > 0)
                            {
                                newRow.Symbol = response.SymbolNames[0].Key;
                                newRow.SymbolName = response.SymbolNames[0].Value;
                            }

                            newRow.IndexDate = response.Dates[i].ToString();
                            newRow.DataType = response.DataTypeNames[0].Key;
                            newRow.DataTypeName = response.DataTypeNames[0].Value;
                            newRow.IndexValue = Convert.ToString(response.DataTypeValues[0].SymbolValues[0].GetValue<double[]>()[i]);
                            newRow.SecID = Convert.ToInt32(d.SecID);
                            pwsrec.Load_Indexes.InsertOnSubmit(newRow);
                            pwsrec.SubmitChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = "index code: " + d.DS_Index_Code + " , datatype: " + d.Preferred_DataType + " , Secid: " + d.SecID + " - " + ex.Message;
                        WritePtocessHistory(4, DateTime.Now, ex);
                    }
                }

                // pwsrec.SubmitChanges();
                DSClient.ShutDown();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, data = "ok" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false });
            }
        }

        public InstrumentIdentifier getIdentifierTypeEnum(Intermediate_GetIdentifier identifier)
        {
            if (identifier.IDType.ToLower() == "cusip")
            {
                return new InstrumentIdentifier
                {
                    UserDefinedIdentifier = Convert.ToString(identifier.UserIdentifier),
                    IdentifierType = IdentifierType.Cusip,
                    Identifier = identifier.Identifier
                };
            }

            if (identifier.IDType.ToLower() == "sedol")
            {
                return new InstrumentIdentifier
                {
                    UserDefinedIdentifier = Convert.ToString(identifier.UserIdentifier),
                    IdentifierType = IdentifierType.Sedol,
                    Identifier = identifier.Identifier
                };
            }

            if (identifier.IDType.ToLower() == "isin")
            {
                return new InstrumentIdentifier
                {
                    UserDefinedIdentifier = Convert.ToString(identifier.UserIdentifier),
                    IdentifierType = IdentifierType.Isin,
                    Identifier = identifier.Identifier
                };
            }

            if (identifier.IDType.ToLower() == "ric")
            {
                return new InstrumentIdentifier
                {
                    UserDefinedIdentifier = Convert.ToString(identifier.UserIdentifier),
                    IdentifierType = IdentifierType.Ric,
                    Identifier = identifier.Identifier
                };
            }

            if (identifier.IDType.ToLower() == "symbol")
            {
                return new InstrumentIdentifier
                {
                    UserDefinedIdentifier = Convert.ToString(identifier.UserIdentifier),
                    IdentifierType = IdentifierType.Sym,
                    Identifier = identifier.Identifier
                };
            }

            return new InstrumentIdentifier
            {
                UserDefinedIdentifier = Convert.ToString(identifier.UserIdentifier),
                IdentifierType = IdentifierType.Sym,
                Identifier = identifier.Identifier
            };
        }

        [HttpPost]
        [Route("TRResults")]
        public string TRResults(string UserID, string PrincCCYID, string SecTypeID, string CUSIP = null, string ISIN = null, string Symbol = null, string Ric = null, string SEDOL = null)
        {
            try
            {
                //[adm].[usp_ThomsonReutersResults_SEL]
                CUSIP = CUSIP == "null" || CUSIP == "undefined" ? null : CUSIP;
                ISIN = ISIN == "null" || ISIN == "undefined" ? null : ISIN;
                Symbol = Symbol == "null" || Symbol == "undefined" ? null : Symbol;
                Ric = Ric == "null" || Ric == "undefined" ? null : Ric;
                SEDOL = SEDOL == "null" || SEDOL == "undefined" ? null : SEDOL;
                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@UserID", UserID);
                if (Ric != null)
                    pars.Add("@Ric", Ric);
                else if (CUSIP != null)
                    pars.Add("@CUSIP", CUSIP);
                else if (ISIN != null)
                    pars.Add("@ISIN", ISIN);
                else if (Symbol != null)
                    pars.Add("@Symbol", Symbol);
                else if (SEDOL != null)
                    pars.Add("@SEDOL", SEDOL);
                pars.Add("@PrincCCYID", PrincCCYID);
                pars.Add("@SecTypeID", SecTypeID);
                var results = callProcedure("[adm].[usp_ThomsonReutersResults_SEL]", pars, "PWSProd");
                return results;
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api" });
                var exception = "Unknown Error Occurred";
                return exception;
            }
        }

        [HttpPost]
        [Route("SearchTermsAndCond")]
        public HttpResponseMessage SearchTermsAndCond(string identifier, string identifierType, string PrincCCYID, string UserID = "1", string LoadSourceID = "0", string DPID = "0", string UserIdentifier = null)
        {
            try
            {
                var results = SearchTermsAndConditions(identifier, identifierType, PrincCCYID, UserID, LoadSourceID, DPID, UserIdentifier);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, data = results });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        public Tuple<bool, List<TRSearchTermnsAndCondJson>, List<Load_TermsAndCondition>> SearchTermsAndConditions(string identifier, string identifierType, string PrincCCYID, string UserID = "1", string LoadSourceID = "0", string DPID = "0", string UserIdentifier = null)
        {
            try
            {
                List<TRSearchTermnsAndCondJson> searchResultsDeserialized = new List<TRSearchTermnsAndCondJson>();
                var extractionsContext = ContextHelper.CreateExtractionsContext();
                var IdentType = IdentifierType.NONE;
                if (identifierType.ToLower() == "ric")
                {
                    IdentType = IdentifierType.Ric;
                }
                else if (identifierType.ToLower() == "cusip")
                {
                    IdentType = IdentifierType.Cusip;
                }
                else if (identifierType.ToLower() == "isin")
                {
                    IdentType = IdentifierType.Isin;
                }
                else if (identifierType.ToLower() == "symbol")
                {
                    IdentType = IdentifierType.Sym;
                }
                else if (identifierType.ToLower() == "sedol")
                {
                    IdentType = IdentifierType.Sedol;
                }

                var results = extractionsContext.InstrumentSearch(IdentType, identifier, //"74726M505"
 new[] { InstrumentTypeGroup.CollatetizedMortgageObligations, InstrumentTypeGroup.Commodities, InstrumentTypeGroup.Equities, InstrumentTypeGroup.FuturesAndOptions, InstrumentTypeGroup.GovCorp, InstrumentTypeGroup.MortgageBackedSecurities, InstrumentTypeGroup.Money, InstrumentTypeGroup.Municipals, InstrumentTypeGroup.Funds }, IdentifierType.Ric, //Preferred identifier
 100).ToList(); //Max results
                if (UserID != "1")
                    pwsrec.ExecuteCommand("delete from tr.SecuritySearchResults where UserID = " + UserID);
                foreach (var r in results)
                {
                    Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                    if (UserID == "null" || UserID == "undefined")
                    {
                        UserID = null;
                    }

                    if (LoadSourceID == "null" || LoadSourceID == "undefined")
                    {
                        LoadSourceID = null;
                    }

                    if (DPID == "null" || DPID == "undefined")
                    {
                        DPID = null;
                    }

                    var InstrumentTypeName = Enum.GetName(typeof(InstrumentType), r.IdentifierType);
                    pars.Add("@UserID", UserID);
                    pars.Add("@DPID", DPID);
                    pars.Add("@LoadSourceID", LoadSourceID);
                    pars.Add("@Description", r.Description);
                    pars.Add("@Identifier", identifier);
                    pars.Add("@IdentifierType", identifierType);
                    pars.Add("@InstrumentType", InstrumentTypeName);
                    pars.Add("@Key", r.Key);
                    pars.Add("@Source", r.Source);
                    pars.Add("@Status", r.Status);
                    pars.Add("@PrincCCYID", PrincCCYID);
                    pars.Add("@RIC", r.Identifier);
                    pars.Add("@SecuritySearchResultsIdentifierType", Convert.ToString(r.IdentifierType));
                    //pars.Add("@UserIdentifier", identifier);
                    var data = callProcedure("[tr].usp_SecuritySearchResults_INS", pars);
                }

                Dictionary<string, dynamic> pars2 = new Dictionary<string, dynamic>();
                pars2.Add("@UserID", UserID);
                pars2.Add("@Identifier", identifier);
                pars2.Add("@IdentifierType", identifierType);
                pars2.Add("@PrincCCYID", PrincCCYID);
                var d = callProcedure("[tr].[usp_SecuritySearchResults_SEL_UI]", pars2); // getting the unique data out by removing duplicates
                var dataConverted = JsonConvert.DeserializeObject<List<TRSearchTermnsAndCondJson>>(d);
                searchResultsDeserialized.AddRange(dataConverted);
                return new Tuple<bool, List<TRSearchTermnsAndCondJson>, List<Load_TermsAndCondition>>(true, searchResultsDeserialized, new List<Load_TermsAndCondition>());
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                return new Tuple<bool, List<TRSearchTermnsAndCondJson>, List<Load_TermsAndCondition>>(false, new List<TRSearchTermnsAndCondJson>(), new List<Load_TermsAndCondition>());
            }
        }

        [HttpPost]
        [Route("AutoSetup")]
        public HttpResponseMessage AutoSetup(string UserID = null,string CUSIP = null, string ISIN = null, string Sedol = null, string Symbol = null, string PrincCCYID = null, string SecTypeID = null, string SSISIdentifierType = null, int? loadSourceID = 0,int? DPID = 0,string ric = null, string source = null, string identifier = null,string identifierType = null, string userIdentifier = null, bool termsAndConditionsOnly = false, string valoren = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
                {
                    return (HttpResponseMessage)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, UserID) != string.Empty)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID));
                }

                bool ssisProcess = false;
                ///used for SSIS
                if (identifierType != null && identifier != null)
                {
                    ssisProcess = true;
                    CUSIP = identifierType.ToLower() == "cusip" ? identifier : CUSIP;
                    ISIN = identifierType.ToLower() == "isin" ? identifier : ISIN;
                    Symbol = identifierType.ToLower() == "symbol" ? identifier : Symbol;
                    ric = identifierType.ToLower() == "ric" ? identifier : ric;
                    Sedol = identifierType.ToLower() == "sedol" ? identifier : Sedol;
                    valoren = identifierType.ToLower() == "valoren" ? identifier : valoren;
                }

                var data = TermsAndConditions(Sedol, UserID, loadSourceID, DPID, CUSIP, ISIN, Symbol, SSISIdentifierType, Convert.ToInt32(PrincCCYID), Convert.ToInt32(SecTypeID), ric, source, ssisProcess, userIdentifier, termsAndConditionsOnly, valoren);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = data });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                var exception = "Unknown Error Occurred";
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, ex = exception });
            }
        }

        public Tuple<bool, List<TRSearchTermnsAndCondJson>, List<Load_TermsAndCondition>> TermsAndConditions(string sedol = null, string UserID = null, int? loadSourceID = 0, int? DPID = 0, string cusip = null, string isin = null, string symbol = null, string SSISIdentifierType = null, int? PrincCCYID = null, int? SecTypeID = null, string RIC = null, string source = null, bool ssisProcess = false, string userIdentifider = null, bool termsAndConditionsOnly = false, string valoren = null)
        {
            try
            {
                if (UserID == "null" || UserID == "undefined")
                {
                    UserID = null;
                }

                //if (LoadSourceID == "null" || LoadSourceID == "undefined" || LoadSourceID == "0")
                //{
                //    LoadSourceID = null;
                //}
                //if (DPID == "null" || DPID == "undefined" || DPID == "0")
                //{
                //    DPID = null;
                //}
                List<Load_TermsAndCondition> loadTermsAndCondsRows = new List<Load_TermsAndCondition>();
                var ExtractionsContext = ContextHelper.CreateExtractionsContext();
                //var availableFields = ExtractionsContext.GetValidContentFieldTypes(ReportTemplateTypes.TermsAndConditions);
                DssCollection<InstrumentIdentifier> instrumentIdentifiers = new DssCollection<InstrumentIdentifier>();
                List<Intermediate_GetIdentifier> data = new List<Intermediate_GetIdentifier>();
                //if (!string.IsNullOrEmpty(SSISIdentifierType))
                //{
                //    data = pwsrec.usp_SecuritySetup_GetIdentifiers(SSISIdentifierType, Convert.ToInt32(UserID), loadSourceID, DPID).ToList();
                //    data.ForEach(item =>
                //    {
                //        var identiferInfo = new InstrumentIdentifier();
                //        identiferInfo.Identifier = item.Cusip;
                //        identiferInfo.IdentifierType = IdentifierType.Cusip;
                //        identiferInfo.UserDefinedIdentifier = item.Cusip;
                //        instrumentIdentifiers.Add(identiferInfo);
                //    });
                //}
                var insIdentifer = new InstrumentIdentifier();
                if (!string.IsNullOrEmpty(RIC))
                {
                    insIdentifer.Identifier = RIC;
                    insIdentifer.IdentifierType = IdentifierType.Ric;
                //insIdentifer.Source = "*";
                }
                else if (!string.IsNullOrEmpty(cusip))
                {
                    insIdentifer.Identifier = cusip;
                    insIdentifer.IdentifierType = IdentifierType.Cusip;
                    insIdentifer.Source = source;
                }
                else if (!string.IsNullOrEmpty(isin))
                {
                    insIdentifer.Identifier = isin;
                    insIdentifer.IdentifierType = IdentifierType.Isin;
                    insIdentifer.Source = source;
                }
                else if (!string.IsNullOrEmpty(symbol))
                {
                    insIdentifer.Identifier = symbol;
                    insIdentifer.IdentifierType = IdentifierType.Sym;
                    insIdentifer.Source = source;
                }
                else if (!string.IsNullOrEmpty(sedol))
                {
                    insIdentifer.Identifier = sedol;
                    insIdentifer.IdentifierType = IdentifierType.Sedol;
                    insIdentifer.Source = source;
                }
                else if (!string.IsNullOrEmpty(valoren))
                {
                    insIdentifer.Identifier = symbol;
                    insIdentifer.IdentifierType = IdentifierType.Valoren;
                    insIdentifer.Source = source;
                }

                instrumentIdentifiers.Add(insIdentifer);
                Tuple<bool, List<TRSearchTermnsAndCondJson>, List<Load_TermsAndCondition>> tupleResults;
                if (!data.Any() && insIdentifer.IdentifierType == IdentifierType.NONE)
                {
                    return new Tuple<bool, List<TRSearchTermnsAndCondJson>, List<Load_TermsAndCondition>>(false, new List<TRSearchTermnsAndCondJson>(), new List<Load_TermsAndCondition>());
                }
                else if (!data.Any() && insIdentifer.IdentifierType != IdentifierType.NONE)
                {
                    if (!ssisProcess)
                    {
                        if (termsAndConditionsOnly)
                        {
                            goto TermsAndConditionsExtraction;
                        }

                        tupleResults = SearchTermsAndConditions(insIdentifer.Identifier, insIdentifer.IdentifierType.ToString(), Convert.ToString(PrincCCYID), UserID, Convert.ToString(loadSourceID), Convert.ToString(DPID));
                        return tupleResults;
                        if (tupleResults.Item2.Count > 0)
                        {
                            instrumentIdentifiers.Clear();
                            tupleResults.Item2.ForEach(trItem =>
                            {
                                var identTypeFromTupleResult = tupleResults.Item2.FirstOrDefault().IdentifierType;
                                var insIdentiferNew = new InstrumentIdentifier();
                                if (identTypeFromTupleResult.ToLower().Trim() == "ric")
                                {
                                    insIdentiferNew.IdentifierType = IdentifierType.Ric;
                                    insIdentiferNew.Identifier = trItem.Identifier;
                                    insIdentiferNew.Source = trItem.Source;
                                }
                                else if (identTypeFromTupleResult.ToLower().Trim() == "cusip")
                                {
                                    insIdentiferNew.IdentifierType = IdentifierType.Cusip;
                                    insIdentiferNew.Identifier = insIdentifer.Identifier;
                                    insIdentiferNew.Source = tupleResults.Item2.FirstOrDefault().Source;
                                }
                                else if (identTypeFromTupleResult.ToLower().Trim() == "isin")
                                {
                                    insIdentiferNew.IdentifierType = IdentifierType.Isin;
                                    insIdentiferNew.Identifier = insIdentifer.Identifier;
                                    insIdentiferNew.Source = tupleResults.Item2.FirstOrDefault().Source;
                                }
                                else if (identTypeFromTupleResult.ToLower().Trim() == "symbol")
                                {
                                    insIdentiferNew.IdentifierType = IdentifierType.Sym;
                                    insIdentiferNew.Identifier = insIdentifer.Identifier;
                                    insIdentiferNew.Source = tupleResults.Item2.FirstOrDefault().Source;
                                }
                                else if (identTypeFromTupleResult.ToLower().Trim() == "sedol")
                                {
                                    insIdentiferNew.IdentifierType = IdentifierType.Sedol;
                                    insIdentiferNew.Identifier = insIdentifer.Identifier;
                                    insIdentiferNew.Source = tupleResults.Item2.FirstOrDefault().Source;
                                }
                                else if (identTypeFromTupleResult.ToLower().Trim() == "valoren")
                                {
                                    insIdentiferNew.IdentifierType = IdentifierType.Valoren;
                                    insIdentiferNew.Identifier = insIdentifer.Identifier;
                                    insIdentiferNew.Source = tupleResults.Item2.FirstOrDefault().Source;
                                }

                                instrumentIdentifiers.Add(insIdentiferNew);
                            });
                        }

                        if (instrumentIdentifiers.Count == 0)
                        {
                            instrumentIdentifiers.Add(insIdentifer);
                        }
                    }

                    TermsAndConditionsExtraction:
                        //"037833100"
                        var extractionRequest = new TermsAndConditionsExtractionRequest
                        {
                            IdentifierList = InstrumentIdentifierList.Create(// string.IsNullOrEmpty(SSISIdentifierType) ? new[] { insIdentifer } : instrumentIdentifiers
                            instrumentIdentifiers, null, false),
                            ContentFieldNames = new[]
                            {
                                "Asset Type",
                                "Asset Type Description",
                                "Asset SubType",
                                "Asset SubType Description",
                                "Day Count Code Description",
                                "Last Coupon Date",
                                "First Coupon Date",
                                "Coupon Rate",
                                "Coupon Type",
                                "Coupon Type Description",
                                "Coupon Frequency",
                                "Coupon Frequency Description",
                                //  "Current Coupon Class Code",
                                //  "Current Coupon Class Description",
                                "Coupon Currency",
                                "Next Rate Reset Date",
                                "Last Rate Reset Date",
                                "Next Call Date",
                                "Next Call Price",
                                "ISO Country Code",
                                "State Code",
                                "Moodys Rating",
                                //"Fitch Rating",
                                //"S&P Rating",
                                "Industry Description",
                                "Industry Sector Description",
                                //"TRBC Economic Sector Code",
                                //"TRBC Economic Sector Code Description",
                                //"TRBC Business Sector Code",
                                //"TRBC Business Sector Code Description",
                                //"GICS Sector Code",
                                //"GICS Sector Code Description",
                                //"GICS Industry Code",
                                //"GICS Industry Code Description",
                                "Hybrid Flag",
                                "Refinitiv Classification Scheme",
                                "Refinitiv Classification Scheme Description",
                                "Par Value",
                                //"Country of Issuance",
                                //"Country of Issuance Description",
                                "Security Description",
                                "Currency Code",
                                "Currency Code Description",
                                "Dividend Currency",
                                "Dividend Currency Description",
                                "Investment Type",
                                "GICS Industry Group Code",
                                "GICS Industry Group Code Description",
                                "Exchange Code",
                                "Exchange Description",
                                "Underlying ISIN",
                                "Underlying RIC",
                                "RIC",
                                "Ticker",
                                "SEDOL",
                                "CUSIP",
                                "ISIN",
                                "Trading Symbol",
                                "Issue Date",
                                "Accrual Date",
                                "Maturity Date",
                                //"Latest Payment Date",
                                //"Periodicity",
                                "Day Count Code",
                                "TRBC Industry Code",
                                "TRBC Industry Code Description",
                                //"TRBC Industry Group Code",
                                //"TRBC Industry Group Code Description",
                                //"TRBC Business Sector Code",
                                //"TRBC Business Sector Code Description",
                                "TRBC Economic Sector Code",
                                "TRBC Economic Sector Code Description",
                                //"TRBC Activity Code",
                                //"TRBC Activity Code Description",
                                "Original Issue Discount Flag",
                                "Denomination Increment",
                                "Issue Price",
                                "Domicile",
                                // "Issuer ID",
                                "Issuer Name",
                                "Issuer OrgID",
                                // "Bond Grade",
                                "Convertible Flag",
                                //  "Covered Bond Flag",
                                "ETF Type",
                                "Float Index Type",
                                //  "Floater Flag",
                                //   "Initial Interest-Only Flag",
                                //   "Is Step Up Flag",
                                //  "Seniority Code",
                                //  "Step Up Date",
                                // "Underlying Asset",
                                "Coupon Reset Frequency",
                                "Coupon Reset Frequency Description",
                                "Coupon Reset Rule Code",
                                "Coupon Reset Rule Code Description",
                                //"Float Reset Frequency Code",
                                //"Float Reset Frequency Code Description",
                                //"Initial Reset Date",
                                //"Last Rate Reset Date",
                                //"Next Payment Reset Date",
                                //"Next Rate Reset Date",
                                "Shares Amount",
                                "Shares Amount Type",
                                //"Original Issue Discount Flag",
                                "Previous Coupon Date",
                                //  "Previous Coupon Rate",
                                // "Coupon Type",
                                // "Next Call Date",
                                // "Next Call Price",
                                "Call Type",
                                "Call Type Description",
                                "Geographical Focus",
                                "Lipper Global Classification",
                                "Fund Manager Benchmark"
                            },
                            Condition = new TermsAndConditionsCondition
                            {
                                IssuerAssetClassType = IssuerAssetClassType.AllSupportedAssets,
                                ExcludeWarrants = false,
                            //DaysAgo = 3, //Use either DaysAgo or StartDate
                            //StartDate = new DateTimeOffset(DateTime.Now.Date.AddDays(-5000)),
                            //FixedIncomeRatingSources = FixedIncomeRatingSource.,
                            }
                        };
                    //Extract - NOTE: If the extraction request takes more than 30 seconds the async mechansim will be used.  See Key Mechanisms 
                    var extractionResult = ExtractionsContext.ExtractWithNotes(extractionRequest);
                    var extractedRows = extractionResult.Contents;
                    //Output
                    foreach (var row in extractedRows)
                    {
                        var newRow = new Load_TermsAndCondition();
                        //var testrow = new Load_TermsAndCondTest();
                        newRow.Identifier = row.Identifier;
                        newRow.IdentifierType = row.IdentifierType.ToString();
                        newRow.Valoren = string.IsNullOrEmpty(valoren) ? null : valoren;
                        if (!string.IsNullOrEmpty(UserID))
                            newRow.UserID = Convert.ToInt32(UserID);
                        cusip = string.IsNullOrEmpty(cusip) ? Convert.ToString(row.DynamicProperties.Where(f => f.Key == "CUSIP").FirstOrDefault().Value) : cusip;
                        if (!string.IsNullOrEmpty(cusip))
                        {
                            newRow.CUSIP = Convert.ToString(cusip);
                        ////testrow.CUSIP = Convert.ToString(cusip);
                        }

                        isin = string.IsNullOrEmpty(isin) ? Convert.ToString(row.DynamicProperties.Where(f => f.Key == "ISIN").FirstOrDefault().Value) : isin;
                        if (!string.IsNullOrEmpty(isin))
                        {
                            newRow.ISIN = Convert.ToString(isin);
                        ////testrow.ISIN = Convert.ToString(isin);
                        }

                        var ric = row.DynamicProperties.Where(f => f.Key == "RIC").FirstOrDefault().Value;
                        if (ric != null)
                        {
                            newRow.RIC = Convert.ToString(ric);
                        ////testrow.RIC = Convert.ToString(ric);
                        }

                        var ticker = row.DynamicProperties.Where(f => f.Key == "Ticker").FirstOrDefault().Value;
                        if (ticker != null)
                        {
                            newRow.Ticker = Convert.ToString(ticker);
                        ////testrow.Ticker = Convert.ToString(ticker);
                        }

                        var assetType = row.DynamicProperties.Where(f => f.Key == "Asset Type").FirstOrDefault().Value;
                        if (assetType != null)
                        {
                            newRow.AssetType = assetType.ToString();
                        ////testrow.AssetType = assetType.ToString();
                        }

                        var assetTypeDesc = row.DynamicProperties.Where(f => f.Key == "Asset Type Description").FirstOrDefault().Value;
                        if (assetTypeDesc != null)
                        {
                            newRow.AssetTypeDescription = assetTypeDesc.ToString();
                        ////testrow.AssetTypeDescription = assetTypeDesc.ToString();
                        }

                        var assetSubType = row.DynamicProperties.Where(f => f.Key == "Asset SubType").FirstOrDefault().Value;
                        if (assetSubType != null)
                        {
                            newRow.AssetSubType = assetSubType.ToString();
                        ////testrow.AssetSubType = assetSubType.ToString();
                        }

                        var assetTypeDescription = row.DynamicProperties.Where(f => f.Key == "Asset SubType Description").FirstOrDefault().Value;
                        if (assetTypeDescription != null)
                        {
                            newRow.AssetSubTypeDescription = assetTypeDescription.ToString();
                        //testrow.AssetSubTypeDescription = assetTypeDescription.ToString();
                        }

                        var dayCountCodeDescription = row.DynamicProperties.Where(f => f.Key == "Day Count Code Description").FirstOrDefault().Value;
                        if (dayCountCodeDescription != null)
                        {
                            newRow.DayCountCodeDescription = dayCountCodeDescription.ToString();
                        //testrow.DayCountCodeDescription = dayCountCodeDescription.ToString();
                        }

                        var lastCouponDate = row.DynamicProperties.Where(f => f.Key == "Last Coupon Date").FirstOrDefault().Value;
                        if (lastCouponDate != null)
                        {
                            newRow.LastCouponDate = Convert.ToString(lastCouponDate);
                        //testrow.LastCouponDate = Convert.ToString(lastCouponDate);
                        }

                        var firstCouponDate = row.DynamicProperties.Where(f => f.Key == "First Coupon Date").FirstOrDefault().Value;
                        if (firstCouponDate != null)
                        {
                            newRow.FirstCouponDate = Convert.ToString(firstCouponDate);
                        //testrow.FirstCouponDate = Convert.ToString(firstCouponDate);
                        }

                        var couponRate = row.DynamicProperties.Where(f => f.Key == "Coupon Rate").FirstOrDefault().Value;
                        if (couponRate != null)
                        {
                            newRow.CouponRate = Convert.ToString(couponRate);
                        //testrow.CouponRate = Convert.ToString(couponRate);
                        }

                        var couponType = row.DynamicProperties.Where(f => f.Key == "Coupon Type").FirstOrDefault().Value;
                        if (couponType != null)
                        {
                            newRow.CouponType = couponType.ToString();
                        //testrow.CouponType = couponType.ToString();
                        }

                        var couponTypeDescription = row.DynamicProperties.Where(f => f.Key == "Coupon Type Description").FirstOrDefault().Value;
                        if (couponTypeDescription != null)
                        {
                            newRow.CouponTypeDescription = couponTypeDescription.ToString();
                        //testrow.CouponTypeDescription = couponTypeDescription.ToString();
                        }

                        var couponFrequency = row.DynamicProperties.Where(f => f.Key == "Coupon Frequency").FirstOrDefault().Value;
                        if (couponFrequency != null)
                        {
                            newRow.CouponFrequency = couponFrequency.ToString();
                        //testrow.CouponFrequency = couponFrequency.ToString();
                        }

                        var couponFrequencyDescription = row.DynamicProperties.Where(f => f.Key == "Coupon Frequency Description").FirstOrDefault().Value;
                        if (couponFrequencyDescription != null)
                        {
                            newRow.CouponFrequencyDescription = couponFrequencyDescription.ToString();
                        //testrow.CouponFrequencyDescription = couponFrequencyDescription.ToString();
                        }

                        //var currentCouponClassCode = row.DynamicProperties.Where(f => f.Key == "Current Coupon Class Code").FirstOrDefault().Value;
                        //if (currentCouponClassCode != null)
                        //{
                        //    newRow.CurrentCouponClassCode = currentCouponClassCode.ToString();
                        //    //testrow.CurrentCouponClassCode = currentCouponClassCode.ToString();
                        //}
                        //var currentCouponClassDescription = row.DynamicProperties.Where(f => f.Key == "Current Coupon Class Description").FirstOrDefault().Value;
                        //if (currentCouponClassDescription != null)
                        //{
                        //    newRow.CurrentCouponClassDescription = currentCouponClassDescription.ToString();
                        //    //testrow.CurrentCouponClassDescription = currentCouponClassDescription.ToString();
                        //}
                        var couponCurrency = row.DynamicProperties.Where(f => f.Key == "Coupon Currency").FirstOrDefault().Value;
                        if (couponCurrency != null)
                        {
                            newRow.CouponCurrency = couponCurrency.ToString();
                        //testrow.CouponCurrency = couponCurrency.ToString();
                        }

                        //var lastRateResetDate = row.DynamicProperties.Where(f => f.Key == "Next Rate Reset Date").FirstOrDefault().Value;
                        //if (lastRateResetDate != null)
                        //{
                        //    newRow.LastRateResetDate = Convert.ToString(lastRateResetDate);
                        //    //testrow.LastRateResetDate = Convert.ToString(lastRateResetDate);
                        //}
                        var nextCallDate = row.DynamicProperties.Where(f => f.Key == "Next Call Date").FirstOrDefault().Value;
                        if (nextCallDate != null)
                        {
                            newRow.NextCallDate = Convert.ToString(nextCallDate);
                        //testrow.NextCallDate = Convert.ToString(nextCallDate);
                        }

                        var nextCallPrice = row.DynamicProperties.Where(f => f.Key == "Next Call Price").FirstOrDefault().Value;
                        if (nextCallPrice != null)
                        {
                            newRow.NextCallPrice = Convert.ToString(nextCallPrice);
                        //testrow.NextCallPrice = Convert.ToString(nextCallPrice);
                        }

                        var iSOCountryCode = row.DynamicProperties.Where(f => f.Key == "ISO Country Code").FirstOrDefault().Value;
                        if (iSOCountryCode != null)
                        {
                            newRow.ISOCountryCode = iSOCountryCode.ToString();
                        //testrow.ISOCountryCode = Convert.ToString(iSOCountryCode);
                        }

                        var stateCode = row.DynamicProperties.Where(f => f.Key == "State Code").FirstOrDefault().Value;
                        if (stateCode != null)
                        {
                            newRow.StateCode = stateCode.ToString();
                        //testrow.StateCode = stateCode.ToString();
                        }

                        var moodysRating = row.DynamicProperties.Where(f => f.Key == "Moodys Rating").FirstOrDefault().Value;
                        if (moodysRating != null)
                        {
                            newRow.MoodysRating = moodysRating.ToString();
                        //testrow.MoodysRating = moodysRating.ToString();
                        }

                        //var fitchRating = row.DynamicProperties.Where(f => f.Key == "Fitch Rating").FirstOrDefault().Value;
                        //if (fitchRating != null)
                        //{
                        //    newRow.FitchRating = fitchRating.ToString();
                        //    //testrow.FitchRating = fitchRating.ToString();
                        //}
                        //var sAndPRating = row.DynamicProperties.Where(f => f.Key == "S&P Rating").FirstOrDefault().Value;
                        //if (sAndPRating != null)
                        //{
                        //    newRow.SAndPRating = sAndPRating.ToString();
                        //    //testrow.SAndPRating = sAndPRating.ToString();
                        //}
                        var industryDescription = row.DynamicProperties.Where(f => f.Key == "Industry Description").FirstOrDefault().Value;
                        if (industryDescription != null)
                        {
                            newRow.IndustryDescription = industryDescription.ToString();
                        //testrow.IndustryDescription = industryDescription.ToString();
                        }

                        var industrySectorDescription = row.DynamicProperties.Where(f => f.Key == "Industry Sector Description").FirstOrDefault().Value;
                        if (industrySectorDescription != null)
                        {
                            newRow.IndustrySectorDescription = industrySectorDescription.ToString();
                        //testrow.IndustrySectorDescription = industrySectorDescription.ToString();
                        }

                        //var tRBCEconomicSectorCode = row.DynamicProperties.Where(f => f.Key == "TRBC Economic Sector Code").FirstOrDefault().Value;
                        //if (tRBCEconomicSectorCode != null)
                        //{
                        //    newRow.TRBCEconomicSectorCode = tRBCEconomicSectorCode.ToString();
                        //}
                        //var tRBCEconomicSectorCodeDescription = row.DynamicProperties.Where(f => f.Key == "TRBC Economic Sector Code Description").FirstOrDefault().Value;
                        //if (tRBCEconomicSectorCodeDescription != null)
                        //{
                        //    newRow.TRBCEconomicSectorCodeDescription = tRBCEconomicSectorCodeDescription.ToString();
                        //}
                        //var tRBCBusinessSectorCode = row.DynamicProperties.Where(f => f.Key == "TRBC Business Sector Code").FirstOrDefault().Value;
                        //if (tRBCBusinessSectorCode != null)
                        //{
                        //    newRow.TRBCBusinessSectorCode = tRBCBusinessSectorCode.ToString();
                        //}
                        //var tRBCBusinessSectorCodeDescription = row.DynamicProperties.Where(f => f.Key == "TRBC Business Sector Code Description").FirstOrDefault().Value;
                        //if (tRBCBusinessSectorCodeDescription != null)
                        //{
                        //    newRow.TRBCBusinessSectorCodeDescription = tRBCBusinessSectorCodeDescription.ToString();
                        //}
                        //var gICSSectorCode = row.DynamicProperties.Where(f => f.Key == "GICS Sector Code").FirstOrDefault().Value;
                        //if (gICSSectorCode != null)
                        //{
                        //    newRow.GICSSectorCode = gICSSectorCode.ToString();
                        //}
                        //var gICSSectorCodeDescription = row.DynamicProperties.Where(f => f.Key == "GICS Sector Code Description").FirstOrDefault().Value;
                        //if (gICSSectorCodeDescription != null)
                        //{
                        //    newRow.GICSSectorCodeDescription = gICSSectorCodeDescription.ToString();
                        //}
                        //var gICSIndustryCode = row.DynamicProperties.Where(f => f.Key == "GICS Industry Code").FirstOrDefault().Value;
                        //if (gICSIndustryCode != null)
                        //{
                        //    newRow.GICSIndustryCode = gICSIndustryCode.ToString();
                        //}
                        //var gICSIndustryCodeDescription = row.DynamicProperties.Where(f => f.Key == "GICS Industry Code Description").FirstOrDefault().Value;
                        //if (gICSIndustryCodeDescription != null)
                        //{
                        //    newRow.GICSIndustryCodeDescription = gICSIndustryCodeDescription.ToString();
                        //}
                        //var gICSIndustryGroupCode = row.DynamicProperties.Where(f => f.Key == "GICS Industry Group Code").FirstOrDefault().Value;
                        //if (gICSIndustryGroupCode != null)
                        //{
                        //    newRow.GICSIndustryGroupCode = gICSIndustryGroupCode.ToString();
                        //}
                        //var gICSIndustryGroupCodeDescription = row.DynamicProperties.Where(f => f.Key == "GICS Industry Group Code Descriptionn").FirstOrDefault().Value;
                        //if (gICSIndustryGroupCodeDescription != null)
                        //{
                        //    newRow.GICSIndustryGroupCodeDescription = gICSIndustryGroupCodeDescription.ToString();
                        //}
                        var hybridFlag = row.DynamicProperties.Where(f => f.Key == "Hybrid Flag").FirstOrDefault().Value;
                        if (hybridFlag != null)
                        {
                            newRow.HybridFlag = hybridFlag.ToString();
                        //testrow.HybridFlag = hybridFlag.ToString();
                        }

                        var thomsonReutersClassificationScheme = row.DynamicProperties.Where(f => f.Key == "Refinitiv Classification Scheme").FirstOrDefault().Value;
                        if (thomsonReutersClassificationScheme != null)
                        {
                            newRow.RefinitivClassificationScheme = thomsonReutersClassificationScheme.ToString();
                        //testrow.RefinitivClassificationScheme = thomsonReutersClassificationScheme.ToString();
                        }

                        var thomsonReutersClassificationSchemeDescription = row.DynamicProperties.Where(f => f.Key == "Refinitiv Classification Scheme Description").FirstOrDefault().Value;
                        if (thomsonReutersClassificationSchemeDescription != null)
                        {
                            newRow.RefinitivClassificationSchemeDescription = thomsonReutersClassificationScheme.ToString();
                        //testrow.RefinitivClassificationSchemeDescription = thomsonReutersClassificationScheme.ToString();
                        }

                        var parValue = row.DynamicProperties.Where(f => f.Key == "Par Value").FirstOrDefault().Value;
                        if (parValue != null)
                        {
                            newRow.ParValue = parValue.ToString();
                        //testrow.ParValue = parValue.ToString();
                        }

                        //var countryofIssuance = row.DynamicProperties.Where(f => f.Key == "Country of Issuance").FirstOrDefault().Value;
                        //if (countryofIssuance != null)
                        //{
                        //    newRow.CountryofIssuance = countryofIssuance.ToString();
                        //    //testrow.CountryofIssuance = countryofIssuance.ToString();
                        //}
                        //var countryofIssuanceDescription = row.DynamicProperties.Where(f => f.Key == "Country of Issuance Description").FirstOrDefault().Value;
                        //if (countryofIssuanceDescription != null)
                        //{
                        //    newRow.CountryofIssuanceDescription = countryofIssuanceDescription.ToString();
                        //    //testrow.CountryofIssuanceDescription = countryofIssuanceDescription.ToString();
                        //}
                        var securityDescription = row.DynamicProperties.Where(f => f.Key == "Security Description").FirstOrDefault().Value;
                        if (securityDescription != null)
                        {
                            newRow.SecurityDescription = securityDescription.ToString();
                        //testrow.SecurityDescription = securityDescription.ToString();
                        }

                        var currencyCode = row.DynamicProperties.Where(f => f.Key == "Currency Code").FirstOrDefault().Value;
                        if (currencyCode != null)
                        {
                            newRow.CurrencyCode = currencyCode.ToString();
                        //testrow.CurrencyCode = currencyCode.ToString();
                        }

                        var currencyCodeDescription = row.DynamicProperties.Where(f => f.Key == "Currency Code Description").FirstOrDefault().Value;
                        if (currencyCodeDescription != null)
                        {
                            newRow.CurrencyCodeDescription = currencyCodeDescription.ToString();
                        //testrow.CurrencyCodeDescription = currencyCodeDescription.ToString();
                        }

                        var dividendCurrency = row.DynamicProperties.Where(f => f.Key == "Dividend Currency").FirstOrDefault().Value;
                        if (dividendCurrency != null)
                        {
                            newRow.DividendCurrency = dividendCurrency.ToString();
                        //testrow.DividendCurrency = dividendCurrency.ToString();
                        }

                        var dividendCurrencyDescription = row.DynamicProperties.Where(f => f.Key == "Dividend Currency Description").FirstOrDefault().Value;
                        if (dividendCurrencyDescription != null)
                        {
                            newRow.DividendCurrencyDescription = dividendCurrencyDescription.ToString();
                        //testrow.DividendCurrencyDescription = dividendCurrencyDescription.ToString();
                        }

                        var investmentType = row.DynamicProperties.Where(f => f.Key == "Investment Type").FirstOrDefault().Value;
                        if (investmentType != null)
                        {
                            newRow.InvestmentType = investmentType.ToString();
                        //testrow.InvestmentType = investmentType.ToString();
                        }

                        var exchangeCode = row.DynamicProperties.Where(f => f.Key == "Exchange Code").FirstOrDefault().Value;
                        if (exchangeCode != null)
                        {
                            newRow.ExchangeCode = exchangeCode.ToString();
                        //testrow.ExchangeCode = exchangeCode.ToString();
                        }

                        var exchangeDescription = row.DynamicProperties.Where(f => f.Key == "Exchange Description").FirstOrDefault().Value;
                        if (exchangeDescription != null)
                        {
                            newRow.ExchangeDescription = exchangeDescription.ToString();
                        //testrow.ExchangeDescription = exchangeDescription.ToString();
                        }

                        var accrualDate = row.DynamicProperties.Where(f => f.Key == "Accrual Date").FirstOrDefault().Value;
                        if (accrualDate != null)
                        {
                            newRow.AccrualDate = accrualDate.ToString();
                        //testrow.AccrualDate = accrualDate.ToString();
                        }

                        var maturityDate = row.DynamicProperties.Where(f => f.Key == "Maturity Date").FirstOrDefault().Value;
                        if (maturityDate != null)
                        {
                            newRow.MaturityDate = maturityDate.ToString();
                        //testrow.MaturityDate = maturityDate.ToString();
                        }

                        //var latestPaymentDate = row.DynamicProperties.Where(f => f.Key == "Latest Payment Date").FirstOrDefault().Value;
                        //if (latestPaymentDate != null)
                        //{
                        //    newRow.LastPaymentDate = latestPaymentDate.ToString();
                        //    //testrow.LastPaymentDate = latestPaymentDate.ToString();
                        //}
                        //var periodicity = row.DynamicProperties.Where(f => f.Key == "Periodicity").FirstOrDefault().Value;
                        //if (periodicity != null)
                        //{
                        //    newRow.Periodicity = periodicity.ToString();
                        //    //testrow.Periodicity = periodicity.ToString();
                        //}
                        var dayCountCode = row.DynamicProperties.Where(f => f.Key == "Day Count Code").FirstOrDefault().Value;
                        if (dayCountCode != null)
                        {
                            newRow.DayCountCode = dayCountCode.ToString();
                        //testrow.DayCountCode = dayCountCode.ToString();
                        }

                        var issueDate = row.DynamicProperties.Where(f => f.Key == "Issue Date").FirstOrDefault().Value;
                        if (issueDate != null)
                        {
                            newRow.IssueDate = issueDate.ToString();
                        //testrow.IssueDate = issueDate.ToString();
                        }

                        var TRBCIndustryCode = row.DynamicProperties.Where(f => f.Key == "TRBC Industry Code").FirstOrDefault().Value;
                        if (TRBCIndustryCode != null)
                        {
                            newRow.TRBCIndustryCode = TRBCIndustryCode.ToString();
                        //testrow.TRBCIndustryCode = TRBCIndustryCode.ToString();
                        }

                        var TRBCIndustryCodeDescription = row.DynamicProperties.Where(f => f.Key == "TRBC Industry Code Description").FirstOrDefault().Value;
                        if (TRBCIndustryCodeDescription != null)
                        {
                            newRow.TRBCIndustryCodeDescription = TRBCIndustryCodeDescription.ToString();
                        //testrow.TRBCIndustryCodeDescription = TRBCIndustryCodeDescription.ToString();
                        }

                        //var TRBCIndustryGroupCode = row.DynamicProperties.Where(f => f.Key == "TRBC Industry Group Code").FirstOrDefault().Value;
                        //if (TRBCIndustryGroupCode != null)
                        //{
                        //    newRow.TRBCIndustryGroupCode = TRBCIndustryGroupCode.ToString();
                        //    //testrow.TRBCIndustryGroupCode = TRBCIndustryGroupCode.ToString();
                        //}
                        //var TRBCIndustryGroupCodeDescription = row.DynamicProperties.Where(f => f.Key == "TRBC Industry Group Code Description").FirstOrDefault().Value;
                        //if (TRBCIndustryGroupCodeDescription != null)
                        //{
                        //    newRow.TRBCIndustryGroupCodeDescription = TRBCIndustryGroupCodeDescription.ToString();
                        //    //testrow.TRBCIndustryGroupCodeDescription = TRBCIndustryGroupCodeDescription.ToString();
                        //}
                        //var TRBCBusinessSectorCode = row.DynamicProperties.Where(f => f.Key == "TRBC Business Sector Code").FirstOrDefault().Value;
                        //if (TRBCBusinessSectorCode != null)
                        //{
                        //    newRow.TRBCBusinessSectorCode = TRBCBusinessSectorCode.ToString();
                        //    //testrow.TRBCBusinessSectorCode = TRBCBusinessSectorCode.ToString();
                        //}
                        //var TRBCBusinessSectorCodeDescription = row.DynamicProperties.Where(f => f.Key == "TRBC Business Sector Code Description").FirstOrDefault().Value;
                        //if (TRBCBusinessSectorCodeDescription != null)
                        //{
                        //    newRow.TRBCBusinessSectorCodeDescription = TRBCBusinessSectorCodeDescription.ToString();
                        //    //testrow.TRBCBusinessSectorCodeDescription = TRBCBusinessSectorCodeDescription.ToString();
                        //}
                        var TRBCEconomicSectorCode = row.DynamicProperties.Where(f => f.Key == "TRBC Economic Sector Code").FirstOrDefault().Value;
                        if (TRBCEconomicSectorCode != null)
                        {
                            newRow.TRBCEconomicSectorCode = TRBCEconomicSectorCode.ToString();
                        //testrow.TRBCEconomicSectorCode = TRBCEconomicSectorCode.ToString();
                        }

                        var TRBCEconomicSectorCodeDescription = row.DynamicProperties.Where(f => f.Key == "TRBC Economic Sector Code Description").FirstOrDefault().Value;
                        if (TRBCEconomicSectorCodeDescription != null)
                        {
                            newRow.TRBCEconomicSectorCodeDescription = TRBCEconomicSectorCodeDescription.ToString();
                        //testrow.TRBCEconomicSectorCodeDescription = TRBCEconomicSectorCodeDescription.ToString();
                        }

                        //var TRBCActivityCode = row.DynamicProperties.Where(f => f.Key == "TRBC Activity Code").FirstOrDefault().Value;
                        //if (TRBCActivityCode != null)
                        //{
                        //    newRow.TRBCActivityCode = TRBCActivityCode.ToString();
                        //    //testrow.TRBCActivityCode = TRBCActivityCode.ToString();
                        //}
                        //var TRBCActivityCodeDescription = row.DynamicProperties.Where(f => f.Key == "TRBC Activity Code Description").FirstOrDefault().Value;
                        //if (TRBCActivityCodeDescription != null)
                        //{
                        //    newRow.TRBCActivityCodeDescription = TRBCActivityCodeDescription.ToString();
                        //    //testrow.TRBCActivityCodeDescription = TRBCActivityCodeDescription.ToString();
                        //}
                        var OriginalIssueDiscountFlag = row.DynamicProperties.Where(f => f.Key == "Original Issue Discount Flag").FirstOrDefault().Value;
                        if (OriginalIssueDiscountFlag != null)
                        {
                            newRow.OriginalIssueDiscountFlag = OriginalIssueDiscountFlag.ToString();
                        //testrow.OriginalIssueDiscountFlag = OriginalIssueDiscountFlag.ToString();
                        }

                        var DenominationIncrement = row.DynamicProperties.Where(f => f.Key == "Denomination Increment").FirstOrDefault().Value;
                        if (DenominationIncrement != null)
                        {
                            newRow.DenominationIncrement = DenominationIncrement.ToString();
                        //testrow.DenominationIncrement = DenominationIncrement.ToString();
                        }

                        var IssuePrice = row.DynamicProperties.Where(f => f.Key == "Issue Price").FirstOrDefault().Value;
                        if (IssuePrice != null)
                        {
                            newRow.IssuePrice = IssuePrice.ToString();
                        // testrow.IssuePrice = IssuePrice.ToString();
                        }

                        var domicile = row.DynamicProperties.Where(f => f.Key == "Domicile").FirstOrDefault().Value;
                        if (domicile != null)
                        {
                            newRow.Domicile = domicile.ToString();
                        //testrow.Domicile = domicile.ToString();
                        }

                        var sedolInfo = row.DynamicProperties.Where(f => f.Key == "SEDOL").FirstOrDefault().Value;
                        if (sedol != null)
                        {
                            newRow.SEDOL = sedol.ToString();
                        //testrow.SEDOL = sedol.ToString();
                        }

                        newRow.UserDefinedIdentifier = userIdentifider;
                        //testrow.UserDefinedIdentifier = userIdentifider;
                        //var IssuerID = row.DynamicProperties.Where(f => f.Key == "Issuer ID").FirstOrDefault().Value;
                        //if (IssuerID != null)
                        //{
                        //    newRow.IssuerID = IssuerID.ToString();
                        //    //testrow.IssuerID = IssuerID.ToString();
                        //}
                        var IssuerName = row.DynamicProperties.Where(f => f.Key == "Issuer Name").FirstOrDefault().Value;
                        if (IssuerName != null)
                        {
                            newRow.IssuerName = IssuerName.ToString();
                        //testrow.IssuerName = IssuerName.ToString();
                        }

                        var IssuerOrgID = row.DynamicProperties.Where(f => f.Key == "Issuer OrgID").FirstOrDefault().Value;
                        if (IssuerOrgID != null)
                        {
                            newRow.IssuerOrgID = IssuerOrgID.ToString();
                        //testrow.IssuerOrgID = IssuerOrgID.ToString();
                        }

                        //testrow.Identifier = newRow.Identifier;
                        //testrow.IdentifierType = newRow.IdentifierType;
                        //testrow.RIC = newRow.RIC;
                        //testrow.CUSIP = newRow.CUSIP;
                        //testrow.ISIN = newRow.ISIN;
                        //testrow.SEDOL = newRow.SEDOL;
                        ////testrow.LastRateResetDate = newRow.NextRateResetDate;
                        ////testrow.NextRateResetDate = newRow.NextRateResetDate;
                        //testrow.OriginalIssueDiscountFlag = newRow.OriginalIssueDiscountFlag;
                        //testrow.CouponType = newRow.CouponType;
                        //testrow.NextCallDate = newRow.NextCallDate;
                        //testrow.NextCallPrice = newRow.NextCallPrice;
                        ////testrow.ThomsonReutersClassificationScheme = newRow.ThomsonReutersClassificationScheme;
                        ////testrow.ThomsonReutersClassificationSchemeDescription = newRow.ThomsonReutersClassificationSchemeDescription;
                        var BondGrade = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Bond Grade ").FirstOrDefault().Value);
                        //testrow.BondGrade = BondGrade;
                        var ConvertibleFlag = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Convertible Flag").FirstOrDefault().Value);
                        //testrow.ConvertibleFlag = ConvertibleFlag;
                        newRow.ConvertibleFlag = ConvertibleFlag;
                        var ETFType = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "ETF Type").FirstOrDefault().Value);
                        //testrow.ETFType = ETFType;
                        newRow.ETFType = ETFType;
                        var FloatIndexType = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Float Index Type").FirstOrDefault().Value);
                        //testrow.FloatIndexType = FloatIndexType;
                        newRow.FloatIndexType = FloatIndexType;
                        var FloaterFlag = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Floater Flag").FirstOrDefault().Value);
                        //testrow.FloaterFlag = FloaterFlag;
                        var InitialInterestOnlyFlag = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Initial Interest-Only Flag").FirstOrDefault().Value);
                        //testrow.InitialInterestOnlyFlag = InitialInterestOnlyFlag;
                        var IsStepUpFlag = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Is Step Up Flag").FirstOrDefault().Value);
                        //testrow.IsStepUpFlag = IsStepUpFlag;
                        var SeniorityCode = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Seniority Code").FirstOrDefault().Value);
                        //testrow.SeniorityCode = SeniorityCode;
                        var StepUpDate = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Step Up Date").FirstOrDefault().Value);
                        //testrow.StepUpDate = StepUpDate;
                        var UnderlyingAsset = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Underlying Asset").FirstOrDefault().Value);
                        //testrow.UnderlyingAsset = UnderlyingAsset;
                        var CouponResetFrequency = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Coupon Reset Frequency").FirstOrDefault().Value);
                        newRow.CouponResetFrequency = CouponResetFrequency;
                        var CouponResetFrequencyDescription = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Coupon Reset Frequency Description").FirstOrDefault().Value);
                        newRow.CouponResetFrequencyDescription = CouponResetFrequencyDescription;
                        var CouponResetRuleCode = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Coupon Reset Rule Code").FirstOrDefault().Value);
                        newRow.CouponResetRuleCode = CouponResetRuleCode;
                        var CouponResetRuleCodeDescription = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Coupon Reset Rule Code Description").FirstOrDefault().Value);
                        newRow.CouponResetRuleCodeDescription = CouponResetRuleCodeDescription;
                        var FloatResetFrequencyCode = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Float Reset Frequency Code").FirstOrDefault().Value);
                        //testrow.FloatResetFrequencyCode = FloatResetFrequencyCode;
                        var FloatResetFrequencyCodeDescription = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Float Reset Frequency Code Description").FirstOrDefault().Value);
                        //testrow.FloatResetFrequencyCodeDescription = FloatResetFrequencyCodeDescription;
                        var InitialResetDate = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Initial Reset Date").FirstOrDefault().Value);
                        //testrow.InitialResetDate = InitialResetDate;
                        var NextPaymentResetDate = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Next Payment Reset Date").FirstOrDefault().Value);
                        //testrow.NextPaymentResetDate = NextPaymentResetDate;
                        var SharesAmount = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Shares Amount").FirstOrDefault().Value);
                        newRow.SharesAmount = SharesAmount;
                        var SharesAmountType = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Shares Amount Type").FirstOrDefault().Value);
                        newRow.SharesAmountType = SharesAmountType;
                        var PreviousCouponDate = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Previous Coupon Date").FirstOrDefault().Value);
                        newRow.PreviousCouponDate = PreviousCouponDate;
                        var PreviousCouponRate = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Previous Coupon Rate").FirstOrDefault().Value);
                        //testrow.PreviousCouponRate = PreviousCouponRate;
                        var CallType = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Call Type").FirstOrDefault().Value);
                        //testrow.CallType = CallType;
                        var CallTypeDescription = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Call Type Description").FirstOrDefault().Value);
                        //testrow.CallTypeDescription = CallTypeDescription;
                        //testrows.Add(//testrow);
                        //continue;
                        var GeographicalFocus = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Geographical Focus").FirstOrDefault().Value);
                        newRow.GeographicalFocus = GeographicalFocus;
                        var LipperGlobalClassification = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Lipper Global Classification").FirstOrDefault().Value);
                        newRow.LipperGlobalClassification = LipperGlobalClassification;
                        var FundManagerBenchmark = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Fund Manager Benchmark").FirstOrDefault().Value);
                        newRow.FundManagerBenchmark = FundManagerBenchmark;
                        // pwsrec.Load_TermsAndConditions.InsertOnSubmit(newRow);
                        pwsrec.usp_Load_TermsAndConditions_INS(identifier: newRow.Identifier, identifierType: newRow.IdentifierType, loadSourceID: Convert.ToString(loadSourceID), dPID: Convert.ToString(DPID), cUSIP: cusip, iSIN: isin == "" ? null : isin, princCCYID: Convert.ToString(PrincCCYID), userID: UserID, ticker: newRow.Ticker, rIC: newRow.RIC, assetType: newRow.AssetType, assetTypeDescription: newRow.AssetTypeDescription, assetSubTypeDescription: newRow.AssetSubTypeDescription, assetSubType: newRow.AssetSubType, lastCouponDate: newRow.LastCouponDate, firstCouponDate: newRow.FirstCouponDate, couponRate: newRow.CouponRate, couponType: newRow.CouponType, couponTypeDescription: newRow.CouponTypeDescription, couponFrequency: newRow.CouponFrequency, couponFrequencyDescription: newRow.CouponFrequencyDescription// , currentCouponClassCode: newRow.CurrentCouponClassCode
                        //  , currentCouponClassDescription: newRow.CurrentCouponClassDescription
                        , couponCurrency: newRow.CouponCurrency//  , nextRateResetDate: newRow.NextRateResetDate
                        //  , lastRateResetDate: newRow.LastRateResetDate
                        , nextCallDate: newRow.NextCallDate, nextCallPrice: newRow.NextCallPrice, iSOCountryCode: newRow.ISOCountryCode, stateCode: newRow.StateCode, moodysRating: newRow.MoodysRating//, fitchRating: newRow.FitchRating
                        //, sAndPRating: newRow.SAndPRating
                        , industryDescription: newRow.IndustryDescription, industrySectorDescription: newRow.IndustrySectorDescription//, tRBCEconomicSectorCode: newRow.TRBCEconomicSectorCode
                        //, tRBCEconomicSectorCodeDescription: newRow.TRBCEconomicSectorCodeDescription
                        //, tRBCBusinessSectorCode: newRow.TRBCBusinessSectorCode
                        //, tRBCBusinessSectorCodeDescription: newRow.TRBCBusinessSectorCodeDescription
                        //, gICSIndustryCode: newRow.GICSIndustryCode
                        // , gICSIndustryCodeDescription: newRow.GICSIndustryCodeDescription
                        , hybridFlag: newRow.HybridFlag// , gICSSectorCodeDescription: newRow.GICSSectorCodeDescription
                        , dayCountCodeDescription: newRow.DayCountCodeDescription//  , gICSSectorCode: newRow.GICSSectorCode
                        , refinitivClassificationScheme: newRow.RefinitivClassificationScheme, refinitivClassificationSchemeDescription: newRow.RefinitivClassificationSchemeDescription, parValue: newRow.ParValue//, countryofIssuance: newRow.CountryofIssuance
                        //, countryofIssuanceDescription: newRow.CountryofIssuanceDescription
                        , securityDescription: newRow.SecurityDescription, currencyCode: newRow.CurrencyCode, currencyCodeDescription: newRow.CurrencyCodeDescription, dividendCurrency: newRow.DividendCurrency, dividendCurrencyDescription: newRow.DividendCurrencyDescription, investmentType: newRow.InvestmentType//  , gICSIndustryGroupCode: newRow.GICSIndustryGroupCode
                        //   , gICSIndustryGroupCodeDescription: newRow.GICSIndustryCodeDescription
                        , exchangeCode: newRow.ExchangeCode, exchangeDescription: newRow.ExchangeDescription, userDefinedIdentifier: newRow.UserDefinedIdentifier, accrualDate: newRow.AccrualDate, maturityDate: newRow.MaturityDate//, lastPaymentDate: newRow.LastPaymentDate
                        , dayCountCode: newRow.DayCountCode//, periodicity: newRow.Periodicity
                        , issueDate: newRow.IssueDate, issuePrice: newRow.IssuePrice, tRBCIndustryCode: newRow.TRBCIndustryCode, tRBCIndustryCodeDescription: newRow.TRBCIndustryCodeDescription//, tRBCIndustryGroupCode: newRow.TRBCIndustryGroupCode
                        //, tRBCIndustryGroupCodeDescription: newRow.TRBCIndustryGroupCodeDescription
                        //, tRBCBusinessSectorCode: newRow.TRBCBusinessSectorCode
                        //, tRBCBusinessSectorCodeDescription: newRow.TRBCBusinessSectorCodeDescription
                        , tRBCEconomicSectorCode: newRow.TRBCEconomicSectorCode, tRBCEconomicSectorCodeDescription: newRow.TRBCEconomicSectorCodeDescription//, tRBCActivityCode: newRow.TRBCActivityCode
                        //   , tRBCActivityCodeDescription: newRow.TRBCActivityCodeDescription
                        , denominationIncrement: newRow.DenominationIncrement, originalIssueDiscountFlag: newRow.OriginalIssueDiscountFlag, domicile: newRow.Domicile, sEDOL: newRow.SEDOL// , issuerID: newRow.IssuerID
                        , issuerName: newRow.IssuerName, issuerOrgID: newRow.IssuerOrgID, eTFType: newRow.ETFType, convertibleFlag: newRow.ConvertibleFlag, floatIndexType: newRow.FloatIndexType, couponResetFrequency: newRow.CouponResetFrequency, couponResetFrequencyDescription: newRow.CouponResetFrequencyDescription, couponResetRuleCode: newRow.CouponResetRuleCode, couponResetRuleCodeDescription: newRow.CouponResetRuleCodeDescription, previousCouponDate: newRow.PreviousCouponDate, sharesAmount: newRow.SharesAmount, sharesAmountType: newRow.SharesAmountType, valoren: newRow.Valoren, geographicalFocus: newRow.GeographicalFocus, lipperGlobalClassification: newRow.LipperGlobalClassification, fundManagerBenchmark: newRow.FundManagerBenchmark);
                    }

                    //pwsrec.ExecuteCommand("truncate table tr.Load_TermsAndConditions");
                    //pwsrec.SubmitChanges();
                    WritePtocessHistory(3, DateTime.Now);
                }

                return new Tuple<bool, List<TRSearchTermnsAndCondJson>, List<Load_TermsAndCondition>>(true, new List<TRSearchTermnsAndCondJson>(), loadTermsAndCondsRows);
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID });
                WritePtocessHistory(3, DateTime.Now, ex);
                return new Tuple<bool, List<TRSearchTermnsAndCondJson>, List<Load_TermsAndCondition>>(false, new List<TRSearchTermnsAndCondJson>(), new List<Load_TermsAndCondition>());
            }
        }

        [HttpPost]
        [Route("EODPricing")]
        public HttpResponseMessage EODPricing(DateTime? runDateTime = null, bool SSISIdentifierType = false)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
                {
                    return (HttpResponseMessage)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
                }

                var success = EndOfDay(runDateTime);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = success });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api" });
                var exception = "Unknown Error Occurred";
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        public bool EndOfDay(DateTime? runDateTime = null, bool SSISIdentifierType = false)
        {
            if (runDateTime == null)
            {
                runDateTime = DateTime.Now;
            }

            try
            {
                ExtractionsContext ExtractionsContext = ContextHelper.CreateExtractionsContext();
                //var availableFields = ExtractionsContext.GetValidContentFieldTypes(ReportTemplateTypes.EndOfDayPricing);
                var data = pwsrec.usp_Price_GetIdentifiers().ToList();
                //DssCollection<InstrumentIdentifier> instrumentIdentifiers = new DssCollection<InstrumentIdentifier>();
                //data.ForEach(item =>
                //{
                //    instrumentIdentifiers.Add(getIdentifierTypeEnum(item));
                //});
                int start = 0, end = 0, bandwidth = 800;
                for (int i = start; i < data.Count; i++)
                {
                    if (end >= data.Count)
                    {
                        break;
                    }

                    DssCollection<InstrumentIdentifier> instrumentIdentifiers = new DssCollection<InstrumentIdentifier>();
                    start = end;
                    end += bandwidth;
                    var currentRows = data.Skip(start).Take(bandwidth).ToList();
                    currentRows.ForEach(item =>
                    {
                        instrumentIdentifiers.Add(getIdentifierTypeEnum(item));
                    });
                    try
                    {
                        //Create the request
                        var extractionRequest = new EndOfDayPricingExtractionRequest
                        {
                            IdentifierList = InstrumentIdentifierList.Create(//new[]
                            //{
                            //new InstrumentIdentifier { Identifier = "191216100", IdentifierType = IdentifierType.Cusip },
                            //new InstrumentIdentifier { Identifier = "2005973", IdentifierType = IdentifierType.Sedol },
                            //new InstrumentIdentifier { Identifier = "AAPL.OQ", IdentifierType = IdentifierType.Ric }
                            //}
                            instrumentIdentifiers, null, false),
                            ContentFieldNames = new[]
                            {
                                "User Defined Identifier",
                                "User Defined Identifier2",
                                "User Defined Identifier3",
                                "User Defined Identifier4",
                                "User Defined Identifier5",
                                "User Defined Identifier6",
                                "Underlying Identifier",
                                "Security Identifier",
                                "Asset Status",
                                "Asset Type",
                                "Bid Price",
                                "Currency Code",
                                "CUSIP",
                                "ISIN",
                                "File Code",
                                "Ask Price",
                                "High Price",
                                "Low Price",
                                "Mid Price",
                                "Volume",
                                "Net Asset Value",
                                "Offer Price",
                                "Close Price",
                                "Open Price",
                                "Previous Close Price",
                                "RIC",
                                "Security Description",
                                "SEDOL",
                                "Ticker",
                                "Trade Date",
                                "Maturity Date",
                                "Universal Close Price"
                            }
                        };
                        //Extract - NOTE: If the extraction request takes more than 30 seconds the async mechansim will be used.  See Key Mechanisms 
                        var extractionResult = ExtractionsContext.ExtractWithNotes(extractionRequest);
                        var extractedRows = extractionResult.Contents;
                        // var loadForDay = pwsrec.Load_EODs.Where(g => g.LoadDateTime == DateTime.Today.Date);
                        //pwsrec.Load_EODs.DeleteAllOnSubmit(loadForDay);
                        //pwsrec.CommandTimeout = 10000000;
                        var count = 0;
                        extractedRows.ToList().ForEach(item =>
                        {
                            count++;
                            var newRow = new Load_EOD();
                            newRow.Identifier = item.Identifier;
                            newRow.IdentifierType = item.IdentifierType.ToString();
                            //newRow.ISIN = Convert.ToString(item.DynamicProperties.Where(f => f.Key == "ISIN").FirstOrDefault().Value);
                            //newRow.SEDOL = Convert.ToString(item.DynamicProperties.Where(f => f.Key == "SEDOL").FirstOrDefault().Value);
                            newRow.Ticker = Convert.ToString(item.DynamicProperties.Where(f => f.Key == "Ticker").FirstOrDefault().Value);
                            //newRow.CUSIP = Convert.ToString(item.DynamicProperties.Where(f => f.Key == "CUSIP").FirstOrDefault().Value);
                            newRow.RIC = Convert.ToString(item.DynamicProperties.Where(f => f.Key == "RIC").FirstOrDefault().Value);
                            newRow.CurrencyCode = Convert.ToString(item.DynamicProperties.Where(f => f.Key == "Currency Code").FirstOrDefault().Value);
                            newRow.BidPrice = Convert.ToString(item.DynamicProperties.Where(f => f.Key == "Bid Price").FirstOrDefault().Value);
                            newRow.AskPrice = Convert.ToString(item.DynamicProperties.Where(f => f.Key == "Ask Price").FirstOrDefault().Value);
                            newRow.MidPrice = Convert.ToString(item.DynamicProperties.Where(f => f.Key == "Mid Price").FirstOrDefault().Value);
                            newRow.ClosePrice = Convert.ToString(item.DynamicProperties.Where(f => f.Key == "Close Price").FirstOrDefault().Value);
                            newRow.UniversalClosePrice = Convert.ToString(item.DynamicProperties.Where(f => f.Key == "Universal Close Price").FirstOrDefault().Value);
                            var tradeDate = Convert.ToDateTime(item.DynamicProperties.Where(f => f.Key == "Trade Date").FirstOrDefault().Value);
                            if (tradeDate >= (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue)
                            {
                                newRow.TradeDate = Convert.ToString(tradeDate);
                            }

                            var maturityDate = Convert.ToDateTime(item.DynamicProperties.Where(f => f.Key == "Maturity Date").FirstOrDefault().Value);
                            if (maturityDate >= (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue)
                            {
                                newRow.MaturityDate = Convert.ToString(maturityDate);
                            }

                            newRow.PutCallIndicator = Convert.ToString(item.DynamicProperties.Where(f => f.Key == "Put Call Indicator").FirstOrDefault().Value);
                            newRow.UserDefinedIdentifier = Convert.ToString(item.UserDefinedIdentifier);
                            pwsrec.Load_EODs.InsertOnSubmit(newRow);
                        });
                        pwsrec.SubmitChanges();
                    }
                    catch (Exception ex2)
                    {
                        Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex2.Message, Params = "web api" });
                        WritePtocessHistory(1, Convert.ToDateTime(runDateTime), ex2);
                    }
                //pwsrec.ExecuteCommand("truncate table tr.Load_EOD");
                }

                WritePtocessHistory(1, Convert.ToDateTime(runDateTime));
                return true;
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api" });
                WritePtocessHistory(1, Convert.ToDateTime(runDateTime), ex);
                return false;
            }
        }

        [HttpPost]
        [Route("CorpActionStandard")]
        public HttpResponseMessage CorpActionStandard(DateTime? runDateTime = null)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
                {
                    return (HttpResponseMessage)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request) != string.Empty)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request));
                }

                var success = CorporateActionsStandard(runDateTime);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = success });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api" });
                var exception = "Unknown Error Occurred";
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        public bool CorporateActionsStandard(DateTime? runDateTime = null)
        {
            if (runDateTime == null)
            {
                runDateTime = DateTime.Now;
            }

            try
            {
                ExtractionsContext ExtractionsContext = ContextHelper.CreateExtractionsContext();
                //var availableFields = ExtractionsContext.GetValidContentFieldTypes(ReportTemplateTypes.CorporateActions);
                //Status.Notify(ExtractionsContext, null, "GetValidContentFieldTypesForTemplateCode", MethodType.Operation, Publish.Primary);
                //Retrieve the list of available fields.  You can skip this step if you already know the list of fields to extract.
                //The list of fields contains the code, name, format.  Either the code or name can be used as the fieldName when
                //adding content fields
                //var availableFieldsForTempl = ExtractionsContext.GetValidContentFieldTypesForTemplateCode("COR");
                // Status.Notify(ExtractionsContext, null, "GetValidExtractionFieldNames", MethodType.Operation, Publish.Primary);
                //Retrieve the list of available fields.  You can skip this step if you already know the list of fields to extract.
                //The list of fields contains the code, name, format.  Either the code or name can be used as the fieldName when
                //adding content fields
                var fieldNames = ExtractionsContext.GetValidExtractionFieldNames(ReportTemplateTypes.CorporateActions);
                foreach (var field in fieldNames)
                {
                // Status.WriteLine("{0})", field);
                }

                // Status.EndNotify(ExtractionsContext);
                //Status.Notify(ExtractionsContext, null, "GetValidExtractionFieldNamesForTemplateCode", MethodType.Operation, Publish.Primary);
                //Retrieve the list of available fields.  You can skip this step if you already know the list of fields to extract.
                //The list of fields contains the code, name, format.  Either the code or name can be used as the fieldName when
                //adding content fields
                var fieldNmTemplCode = ExtractionsContext.GetValidExtractionFieldNamesForTemplateCode("COR");
                foreach (var field in fieldNmTemplCode)
                {
                //Status.WriteLine("{0})", field);
                }

                // Status.EndNotify(ExtractionsContext);
                DssCollection<InstrumentIdentifier> instrumentIdentifiers = new DssCollection<InstrumentIdentifier>();
                var data = pwsrec.usp_CorpAction_GetIdentifiers().ToList();
                data.ForEach(item =>
                {
                    instrumentIdentifiers.Add(getIdentifierTypeEnum(item));
                });
                //Status.Notify(ExtractionsContext, null, "ExtractWithNotes", MethodType.Operation, Publish.Secondary);
                //Create the new report template
                var extractionRequest = new CorporateActionsStandardExtractionRequest
                {
                    IdentifierList = InstrumentIdentifierList.Create(//new[]
                    //{
                    //    new InstrumentIdentifier { Identifier = "191216100", IdentifierType = IdentifierType.Cusip },
                    //    new InstrumentIdentifier { Identifier = "2005973", IdentifierType = IdentifierType.Sedol },
                    //}
                    instrumentIdentifiers, null, false),
                    ContentFieldNames = new[]
                    {
                        //   "Capital Change Announcement Date", "Capital Change Ex Date", "Effective Date", "Record Date",
                        //   "Capital Change Event Type", "Old Shares Terms", "New Shares Terms", "New Shares Currency" 
                        "Dividend Rate",
                        "Dividend Ex Date",
                        "Dividend Pay Date",
                        "Record Date",
                        "Corporate Actions Type",
                        "CUSIP",
                        "ISIN",
                        "SEDOL",
                        "RIC",
                        "Ticker",
                        "Currency Code",
                        "Corporate Actions ID",
                        "Dividend Currency",
                        "Dividend Currency Description",
                        "Dividend Frequency",
                        "Dividend Frequency Description",
                        "Dividend Tax Marker",
                        "Dividend Tax Marker Description",
                        "Dividend Type Marker",
                        "Dividend Type Marker Description"
                    },
                    Condition = new CorporateActionsStandardCondition
                    {
                        PreviousDays = null,
                        ReportDateRangeType = ReportDateRangeType.Range,
                        CorporateActionsCapitalChangeType = CorporateActionsCapitalChangeType.CapitalChangeExDate,
                        CorporateActionsDividendsType = CorporateActionsDividendsType.DividendPayDate,
                        CorporateActionsEarningsType = CorporateActionsEarningsType.PeriodEndDate,
                        CorporateActionsEquityOfferingsType = CorporateActionsEquityOfferingsType.AllPendingDeals,
                        CorporateActionsMergersAcquisitionsType = CorporateActionsMergersAcquisitionsType.DealAnnouncementDate,
                        CorporateActionsNominalValueType = CorporateActionsNominalValueType.NominalValueDate,
                        CorporateActionsSharesType = CorporateActionsSharesType.SharesAmountDate,
                        CorporateActionsStandardEventsType = CorporateActionsStandardEventsType.None,
                        CorporateActionsVotingRightsType = CorporateActionsVotingRightsType.VotingRightsDate,
                        QueryStartDate = new DateTimeOffset(Convert.ToDateTime(runDateTime)),
                        NextDays = null,
                        QueryEndDate = new DateTimeOffset(Convert.ToDateTime(runDateTime).AddYears(60)),
                        PendingEventsHours = null,
                        PendingEventsMinutes = null,
                        IncludeInstrumentsWithNoEvents = null,
                        IncludeNullDates = null,
                        ExcludeDeletedEvents = true,
                        IncludeCapitalChangeEvents = false,
                        IncludeDividendEvents = true,
                        IncludeEarningsEvents = false,
                        IncludeMergersAndAcquisitionsEvents = true,
                        IncludeNominalValueEvents = false,
                        IncludePublicEquityOfferingsEvents = false,
                        IncludeSharesOutstandingEvents = true,
                        IncludeVotingRightsEvents = false,
                        ShareAmountTypes = new DssCollection<ShareAmountType>
                        {
                            ShareAmountType.Outstanding
                        }
                    },
                };
                //Extract - NOTE: If the extraction request takes more than 30 seconds the async mechansim will be used.  See Key Mechanisms 
                var extractionResult = ExtractionsContext.ExtractWithNotes(extractionRequest);
                var extractedRows = extractionResult.Contents;
                //Output
                foreach (var row in extractedRows)
                {
                    var newRow = new Load_CorpAction();
                    newRow.Identifier = row.Identifier;
                    newRow.IdentifierType = row.IdentifierType.ToString();
                    newRow.CUSIP = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "CUSIP").FirstOrDefault().Value);
                    //newRow.SEDOL = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "SEDOL").FirstOrDefault().Value);
                    newRow.RIC = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "RIC").FirstOrDefault().Value);
                    newRow.Ticker = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Ticker").FirstOrDefault().Value);
                    newRow.ISIN = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "ISIN").FirstOrDefault().Value);
                    newRow.CorporateActionsType = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Corporate Actions Type").FirstOrDefault().Value);
                    newRow.CorporateActionsID = Convert.ToString(row.DynamicProperties.Where(f => f.Key == "Corporate Actions ID").FirstOrDefault().Value);
                    var currencyCode = row.DynamicProperties.Where(f => f.Key == "Currency Code").FirstOrDefault().Value;
                    newRow.CurrencyCode = Convert.ToString(currencyCode);
                    var dividendRate = row.DynamicProperties.Where(f => f.Key == "Dividend Rate").FirstOrDefault().Value;
                    if (dividendRate != null)
                    {
                        newRow.DividendRate = Convert.ToString(dividendRate);
                    }

                    var dividendExDate = row.DynamicProperties.Where(f => f.Key == "Dividend Ex Date").FirstOrDefault().Value;
                    if (dividendExDate != null)
                    {
                        newRow.DividendExDate = Convert.ToString(dividendExDate);
                    }

                    var dividendPayDate = row.DynamicProperties.Where(f => f.Key == "Dividend Pay Date").FirstOrDefault().Value;
                    if (dividendPayDate != null)
                    {
                        newRow.DividendPayDate = Convert.ToString(dividendPayDate);
                    }

                    var recordDate = row.DynamicProperties.Where(f => f.Key == "Record Date").FirstOrDefault().Value;
                    if (recordDate != null)
                    {
                        newRow.RecordDate = Convert.ToString(recordDate);
                    }

                    var dividendCurrency = row.DynamicProperties.Where(f => f.Key == "Dividend Currency").FirstOrDefault().Value;
                    newRow.DividendCurrency = Convert.ToString(dividendCurrency);
                    var dividendCurrencyDescription = row.DynamicProperties.Where(f => f.Key == "Dividend Currency Description").FirstOrDefault().Value;
                    newRow.DividendCurrencyDescription = Convert.ToString(dividendCurrencyDescription);
                    var dividendFrequency = row.DynamicProperties.Where(f => f.Key == "Dividend Frequency").FirstOrDefault().Value;
                    newRow.DividendFrequency = Convert.ToString(dividendFrequency);
                    var dividendFrequencyDescription = row.DynamicProperties.Where(f => f.Key == "Dividend Frequency Description").FirstOrDefault().Value;
                    newRow.DividendFrequencyDescription = Convert.ToString(dividendFrequencyDescription);
                    var dividendTaxMarker = row.DynamicProperties.Where(f => f.Key == "Dividend Tax Marker").FirstOrDefault().Value;
                    newRow.DividendTaxMarker = Convert.ToString(dividendTaxMarker);
                    var dividendTaxMarkerDescription = row.DynamicProperties.Where(f => f.Key == "Dividend Tax Marker Description").FirstOrDefault().Value;
                    newRow.DividendTaxMarkerDescription = Convert.ToString(dividendTaxMarkerDescription);
                    var dividendTypeMarker = row.DynamicProperties.Where(f => f.Key == "Dividend Type Marker").FirstOrDefault().Value;
                    newRow.DividendTypeMarker = Convert.ToString(dividendTypeMarker);
                    var dividendTypeMarkerDescription = row.DynamicProperties.Where(f => f.Key == "Dividend Type Marker Description").FirstOrDefault().Value;
                    newRow.DividendTypeMarkerDescription = Convert.ToString(dividendTypeMarkerDescription);
                    newRow.UserDefinedIdentifier = row.UserDefinedIdentifier;
                    pwsrec.Load_CorpActions.InsertOnSubmit(newRow);
                }

                if (extractedRows.Count > 0)
                {
                    //pwsrec.ExecuteCommand("truncate table tr.Load_CorpActions");
                    pwsrec.SubmitChanges();
                }

                //foreach (var note in extractionResult.Notes)
                //{
                //}
                WritePtocessHistory(2, Convert.ToDateTime(runDateTime));
                return true;
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api" });
                WritePtocessHistory(2, Convert.ToDateTime(runDateTime), ex);
                return false;
            }
        }

        public String callProcedure(String procedureName, Dictionary<string, dynamic> pars, string db = "PWSRec")
        {
            try
            {
                GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
                };
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.Configuration.GetSection("ConnectionStrings")[db]))
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
                        System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                        serializer.MaxJsonLength = Int32.MaxValue;
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