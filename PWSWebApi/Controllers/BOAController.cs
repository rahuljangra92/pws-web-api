using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PWSWebApi.Domains;
using PWSWebApi.Domains.DBContext;
using PWSWebApi.handlers;
using PWSWebApi.ViewModel.BOA;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace PWSWebApi.Controllers
{
    [Route("boa")]
    public class BOAController : ControllerBase
    {

        private readonly PWSRecDataContext _pWSRecDataContext;
        private readonly string pwsRectConnectionString;

        public BOAController(IConfiguration configuration, PWSRecDataContext pWSRecDataContext)
        {
            _pWSRecDataContext = pWSRecDataContext;
            pwsRectConnectionString = configuration.GetConnectionString("PWSRec");
        }
        private Tuple<string, string,string> GetCredentialInfo(string mode)
        {
            var isProduction = mode.Equals("prod", StringComparison.OrdinalIgnoreCase);

            var uri = !isProduction ? "https://api-sb.bofa.com/" : "https://api.bofa.com/";
            var providerID = !isProduction ? 
                                "MmPAwC6acyEEiK2dkWMTBR5T6z2kyP+IXouCSHfObAiROaiLnuDkRJdBgt1SX+ZTH81vquU12cs5BboCtjY514/qPDxDyreCDFH0ezv2xDz0c3cHhkb6YigklgFsBHLUlR/s5aDfX6clk6SBLL57Iw==" : 
                                "MmPAwC6acyEEiK2dkWMTBVaP1ujivleARLQoGBUKm6OOKbYxCoULKvhArP97fXbkHa7fLIxIJGvTbR3JlTc84/oHQr2hYDvB70ZCMTmdt6WwBVyVHTNmifFfYwf0Rp96hX10r9bL31EfnMug3j8kMg==";

            var payload = !isProduction ?
                                "{\"applicationID\": \"app_PrivateWealth_Reporting_SB\",\"authn\": { \"client_id\": \"CASHPRO_TONYROBB_EF15BCFA_SB\",  \"client_secret\": \"RR9YyXiBtSr4nyeeRSR50LZqfCzkwPkcFVmtGohXdmO06FgxJEsEyIRAAIm5dOSH\" }\r\n}\r\n" :
                                "{\"applicationID\": \"app_PrivateWealth_Reporting_PD\",\"authn\": { \"client_id\": \"CASHPRO_TONYROBB_07EC866C_PD\",  \"client_secret\": \"hYRPm1skZlqgettXFDaPTnYPC2aOhUVL1hGJWhvuVCdfumWnTzAMcsqsp8XKnWhS\" }\r\n}\r\n";


            return new Tuple<string, string, string>(uri, providerID, payload);
        }


        private void WritePtocessHistory(int processId, DateTime runDateTime, Exception ex = null)
        {
            var newRow = new CashProProcessHistory();
            newRow.Process_ID = processId;
            newRow.RunDateTime = runDateTime;
            newRow.UpdateDateTime = DateTime.Now;

            if (ex != null)
                newRow.Error = ex.Message;

            _pWSRecDataContext.CashProProcessHistory.Add(newRow);
            _pWSRecDataContext.SaveChanges();
        }


        private async Task<TokenObject> GetToken(string mode)
        {
            var client = new RestClient(GetCredentialInfo(mode).Item1 + "authn/v1/client-authentication");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/json");
            request.AddHeader("provider_id", GetCredentialInfo(mode).Item2);
            request.AddParameter("application/json", GetCredentialInfo(mode).Item3, ParameterType.RequestBody);
            IRestResponse response = await client.ExecuteAsync(request);
            var content = response.Content;

            return JsonConvert.DeserializeObject<TokenObject>(content);
        }


        #region  transaction

        [HttpPost]
        [Route("transaction")]
        public async Task<IActionResult> GetTransactionInquiries([FromQuery] string type, [FromQuery] string mode)
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var tokenObject = await GetToken(mode);

                var transactionInquiries = new List<VMTransactionInquiry>();
                if (tokenObject.AccessToken != null)
                {
                    var inputRecords = GetTransactionInputData();

                    foreach (var record in inputRecords)
                    {
                        record.Accounts = new List<InputAccount>();
                        record.Accounts.Add(new InputAccount { AccountNumber = record.AccountNumber, BankID = record.BankID });

                        var outputObj = await GetTransactions(tokenObject.AccessToken, record,type,mode);

                        transactionInquiries.Add(outputObj);
                    }
                }
                else if (!string.IsNullOrEmpty(tokenObject.Status) && tokenObject.Status.ToLower() == "failure")
                {
                    Exception ex = new Exception(JsonConvert.SerializeObject(tokenObject));
                    WritePtocessHistory(2, DateTime.Now, ex);
                    return Ok(tokenObject);

                }
                WritePtocessHistory(2, DateTime.Now);
                return Ok("Process Completed");

            }
            catch (WebException ex)
            {
                WritePtocessHistory(2, DateTime.Now, ex);
                return StatusCode((int)HttpStatusCode.InternalServerError, "Process Failed with Internal PWS error");
            }

        }

        private List<VMTransactionInputObject> GetTransactionInputData()
        {

            Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
            var inputRecords = JsonConvert.DeserializeObject<List<VMTransactionInputObject>>(Helper.callProcedure("[cashpro].[usp_Transaction_Inquiry_SEL]", pars, pwsRectConnectionString));

            return inputRecords;

        }


        private async Task<VMTransactionInquiry> GetTransactions(string token, VMTransactionInputObject inputData, string type, string mode)
        {
            type = type.ToLower().Trim() == "currentday" ? "current-day" : "previous-day";
            var client = new RestClient(GetCredentialInfo(mode).Item1 + "cashpro/reporting/v1/transaction-inquiries/" + type);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("authorization", "Bearer " + token);

            var paramObj = new VMTransInputObjectForBOAParams();
            paramObj.FromDate = inputData.FromDate;
            paramObj.ToDate = inputData.ToDate;
            paramObj.Accounts = new List<InputAccount>();
            paramObj.Accounts.Add(new InputAccount { AccountNumber = inputData.AccountNumber, BankID = inputData.BankID });
            var inputParams = JsonConvert.SerializeObject(paramObj);
            request.AddParameter("application/json", inputParams, ParameterType.RequestBody);
            IRestResponse response = await client.ExecuteAsync(request);

            var outputData = JsonConvert.DeserializeObject<VMTransactionInquiry>(response.Content);
            if (outputData.Errors != null && outputData.Errors.Count > 0)
            {
                // save in the error table for logging any of their errors
                var inputRecordAsString = JsonConvert.SerializeObject(inputData);
                var outputAsString = response.Content;

                _pWSRecDataContext.ErrorLogs.Add(new ErrorLog
                {
                    errorJSON = outputAsString,
                    inputJSON = inputRecordAsString,
                    Load_DateTime = DateTime.Now
                });

                _pWSRecDataContext.SaveChanges();
            }
            else
            {
                SaveTransactions(outputData);
            }

            return outputData;

        }


        private void SaveTransactions(VMTransactionInquiry outputData)
        {
            if (outputData.AccountTransactions != null)
            {
                foreach (var row in outputData.AccountTransactions)
                {
                    foreach (var transaction in row.Transactions)
                    {
                        Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                        pars.Add("@accountNumber", row.AccountNumber);
                        pars.Add("@bankId", row.BankID);
                        pars.Add("@currency", row.Currency);
                        pars.Add("@asOfDate", transaction.AsOfDate);
                        pars.Add("@transactionCode", transaction.TransactionCode);
                        pars.Add("@transactionDescription", transaction.TransactionDescription);
                        pars.Add("@transactionType", transaction.TransactionType);
                        pars.Add("@amount", transaction.Amount);
                        pars.Add("@creditDebitIndicator", transaction.CreditDebitIndicator);
                        pars.Add("@availabilityImmediateAmount", transaction.AvailabilityImmediateAmount);
                        pars.Add("@availability1DayAmount", transaction.Availability1DayAmount);
                        pars.Add("@availability2PlusDayAmount", transaction.Availability2PlusDayAmount);
                        pars.Add("@itemCount", transaction.ItemCount);
                        pars.Add("@valueDate", transaction.ValueDate);
                        pars.Add("@customerReference", transaction.CustomerReference);
                        pars.Add("@bankReference", transaction.BankReference);
                        pars.Add("@detailText", transaction.DetailText);

                        var data = JsonConvert.SerializeObject(Helper.callProcedure("[cashpro].[usp_Load_Transaction_INS]", pars, pwsRectConnectionString));

                    }
                }
            }
        }


        #endregion transaction




        #region balance

        [HttpPost]
        [Route("balance")]
        public async Task<IActionResult> GetBalanceInquiries([FromQuery] string type, [FromQuery]string mode)
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var tokenObject = await GetToken(mode);

                var balanceInquries = new List<VMBalanceInquiry>();
                if (tokenObject.AccessToken != null)
                {
                    var inputRecords = GetBalanceInputData();

                    foreach (var record in inputRecords)
                    {
                        record.Accounts = new List<InputAccount>();
                        record.Accounts.Add(new InputAccount { AccountNumber = record.AccountNumber, BankID = record.BankID });
                        var outputObj = await GetBalances(tokenObject.AccessToken, record, type, mode);

                        balanceInquries.Add(outputObj);

                    }
                }
                else if (!string.IsNullOrEmpty(tokenObject.Status) && tokenObject.Status.ToLower() == "failure")
                {
                    Exception ex = new Exception(JsonConvert.SerializeObject(tokenObject));
                    WritePtocessHistory(1, DateTime.Now, ex);
                    return Ok(tokenObject);

                }
                WritePtocessHistory(1, DateTime.Now);
                return Ok("Process Completed");

            }
            catch (WebException ex)
            {
                WritePtocessHistory(1, DateTime.Now, ex);
                return StatusCode((int)HttpStatusCode.InternalServerError, "Process Failed with Internal PWS error");
            }

        }

        

        private List<VMBalanceInputObject> GetBalanceInputData()
        {
            VMBalanceInputObject inputData = new VMBalanceInputObject();
            Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
            var inputRecords = JsonConvert.DeserializeObject<List<VMBalanceInputObject>>(Helper.callProcedure("[cashpro].[usp_Balance_Inquiry_SEL]", pars, pwsRectConnectionString));

            return inputRecords;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="inputData"></param>
        /// <param name="type">current-day or previous-day</param>
        /// <returns></returns>
        private async Task<VMBalanceInquiry> GetBalances(string token, VMBalanceInputObject inputData, string type, string mode)
        {
            //var pwsRec = new PWSRecDataContext(pwsRectConnectionString);
            type = type.ToLower().Trim() == "currentday" ? "current-day" : "previous-day";
            var client = new RestClient(GetCredentialInfo(mode).Item1 + "cashpro/reporting/v1/balance-inquiries/" + type);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("authorization", "Bearer " + token);
            var paramObj = new VMBalanceInputObjectForBOAParams();
            paramObj.BalanceAsOfDate = inputData.BalanceAsOfDate;
            paramObj.Accounts = new List<InputAccount>();
            paramObj.Accounts.Add(new InputAccount { AccountNumber = inputData.AccountNumber, BankID = inputData.BankID });
            var inputParams = JsonConvert.SerializeObject(paramObj);
            request.AddParameter("application/json", inputParams, ParameterType.RequestBody);
            IRestResponse response = await client.ExecuteAsync(request);

            var outputData = JsonConvert.DeserializeObject<VMBalanceInquiry>(response.Content);
            if (outputData.Errors != null && outputData.Errors.Count > 0)
            {
                // save in the error table for logging any of their errors
                var inputRecordAsString = JsonConvert.SerializeObject(inputData);
                var outputAsString = response.Content;

                _pWSRecDataContext.ErrorLogs.Add(new ErrorLog
                {
                    errorJSON = outputAsString,
                    inputJSON = inputRecordAsString,
                    Load_DateTime = DateTime.Now
                });

                _pWSRecDataContext.SaveChanges();
            }
            else
            {
                SaveBalanceInquiries(outputData);
            }
            
            return outputData;

        }


        private void SaveBalanceInquiries(VMBalanceInquiry outputData)
        {
            if (outputData.AccountBalances != null)
            {
                foreach (var acctBalance in outputData.AccountBalances)
                {
                    
                    foreach (var balance in acctBalance.balances)
                    {
                        Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                        pars.Add("@accountNumber", acctBalance.AccountNumber);
                        pars.Add("@bankId", acctBalance.BankID);
                        pars.Add("@transactionDescription", balance.TransactionDescription);
                        pars.Add("@transactionCode", balance.TransactionCode);
                        pars.Add("@currency", acctBalance.Currency);
                        pars.Add("@amount", balance.Amount);
                        pars.Add("@lastUpdatedDate", DateTime.Now);

                        var data = JsonConvert.SerializeObject(Helper.callProcedure("[cashpro].[usp_Load_Balance_INS]", pars, pwsRectConnectionString));
                    }

                }
            }
        }

        #endregion  balance

    }
}
