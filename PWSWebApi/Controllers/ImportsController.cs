using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PWSWebApi.Domains.DBContext;
using PWSWebApi.handlers;
using PWSWebApi.Models.Imports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace PWSWebApi.Controllers
{
    [Route("imports")]
    [ApiController]
    public class ImportsController : ControllerBase
    {

        private readonly string connectionString;
        private readonly string pwsRecConnectionString;
        private readonly Helper handlerHelper;

        public ImportsController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("PWSProd");
            pwsRecConnectionString = configuration.GetConnectionString("PWSRec");
            handlerHelper = new Helper(configuration);
        }
       
        private void DeleteDataFromStagingTable(string userID, string fileName, string targetTable)
        {
            PWSRecDataContext pwsRectContext = new PWSRecDataContext(pwsRecConnectionString);
        // pwsRectContext.ExecuteCommand("delete from import.Load_" + targetTable + " where User_ID={0}",userID);
        }

        [HttpPost]
        [Route("accountinsert")]
        public IActionResult AccountInsert([FromBody] List<AccountList> requestBodyArray)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is HttpResponseMessage)
                {
                    return (IActionResult)handlerHelper.ValidateSource(Request);
                }

                if (handlerHelper.ValidateToken(Request, requestBodyArray[0].User_ID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, requestBodyArray[0].User_ID.ToString()));
                }

                if (requestBodyArray.Count > 0)
                {
                    DeleteDataFromStagingTable(requestBodyArray[0].User_ID, requestBodyArray[0].FIle_Name, "Account");
                }

                foreach (var requestBody in requestBodyArray)
                {
                    Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                    pars.Add("@ExcelRowNumber", requestBody.ExcelRowNumber);
                    pars.Add("@Account_ID", requestBody.Account_ID);
                    pars.Add("@Account_Name", requestBody.Account_Name);
                    pars.Add("@Account_Code", requestBody.Account_Code);
                    pars.Add("@Base_Currency", requestBody.Base_Currency);
                    pars.Add("@Account_Number", requestBody.Account_Number);
                    pars.Add("@Account_Nick_Name_Primary", requestBody.Account_Nick_Name_Primary);
                    pars.Add("@Data_Source", requestBody.Data_Source);
                    pars.Add("@Load_Source", requestBody.Load_Source);
                    pars.Add("@User_Group_Code", requestBody.User_Group_Code);
                    pars.Add("@Tax_Lot_Relief_Method", requestBody.Tax_Lot_Relief_Method);
                    pars.Add("@Benchmark", requestBody.Benchmark);
                    if (requestBody.Performance_Start_Date != null && requestBody.Performance_Start_Date.Length < 6)
                        pars.Add("@Performance_Start_Date", DateTime.FromOADate(Convert.ToDouble(requestBody.Performance_Start_Date)).ToShortDateString());
                    else if (requestBody.Performance_Start_Date != null)
                        pars.Add("@Performance_Start_Date", requestBody.Performance_Start_Date);
                    pars.Add("@Manager", requestBody.Manager);
                    pars.Add("@User_ID", requestBodyArray[0].User_ID);
                    pars.Add("@FIle_Name", requestBody.FIle_Name);
                    var data = Helper.callProcedure("import.usp_Load_Account_INS", pars, pwsRecConnectionString);
                }

                return requestBodyArray.Any() ? ValidateImport(requestBodyArray[0].User_ID, "Account") : Ok(new { error = false, data = "[]" });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = requestBodyArray[0].User_ID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("transactioninsert")]
        public IActionResult TransactionInsert(List<TransactionImports> requestBodyArray)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, requestBodyArray[0].User_ID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, requestBodyArray[0].User_ID.ToString()));
                }

                if (requestBodyArray.Count > 0)
                {
                    DeleteDataFromStagingTable(requestBodyArray[0].User_ID, requestBodyArray[0].FIle_Name, "Transaction");
                }

                foreach (var requestBody in requestBodyArray)
                {
                    Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                    pars.Add("@ExcelRowNumber", requestBody.ExcelRowNumber);
                    pars.Add("@User_ID", requestBodyArray[0].User_ID);
                    pars.Add("@FIle_Name", requestBody.FIle_Name);
                    pars.Add("@Transaction_ID", requestBody.Transaction_ID);
                    pars.Add("@Account_Code", requestBody.Account_Code);
                    pars.Add("@Security_Name", requestBody.Security_Name);
                    pars.Add("@Security_Code", requestBody.Security_Code);
                    pars.Add("@Security_Code_Type", requestBody.Security_Code_Type);
                    pars.Add("@Security_Type", requestBody.Security_Type);
                    pars.Add("@Transaction_Type", requestBody.Transaction_Type);
                    pars.Add("@Client_Description_of_Transaction_Type", requestBody.Client_Description_of_Transaction_Type);
                    if (requestBody.Trade_Date != null && requestBody.Trade_Date.Length < 6)
                        pars.Add("@Trade_Date", DateTime.FromOADate(Convert.ToDouble(requestBody.Trade_Date)).ToShortDateString());
                    else if (requestBody.Trade_Date != null)
                        pars.Add("@Trade_Date", requestBody.Trade_Date);
                    if (requestBody.Acquisition_Trade_Date != null && requestBody.Acquisition_Trade_Date.Length < 6)
                        pars.Add("@Acquisition_Trade_Date", DateTime.FromOADate(Convert.ToDouble(requestBody.Acquisition_Trade_Date)).ToShortDateString());
                    else if (requestBody.Acquisition_Trade_Date != null)
                        pars.Add("@Acquisition_Trade_Date", requestBody.Acquisition_Trade_Date);
                    if (requestBody.Settlement_Date != null && requestBody.Settlement_Date.Length < 6)
                        pars.Add("@Settlement_Date", DateTime.FromOADate(Convert.ToDouble(requestBody.Settlement_Date)).ToShortDateString());
                    else if (requestBody.Settlement_Date != null)
                        pars.Add("@Settlement_Date", requestBody.Settlement_Date);
                    if (requestBody.Acquisition_Settle_Date != null && requestBody.Acquisition_Settle_Date.Length < 6)
                        pars.Add("@Acquisition_Settle_Date", DateTime.FromOADate(Convert.ToDouble(requestBody.Acquisition_Settle_Date)).ToShortDateString());
                    else if (requestBody.Acquisition_Settle_Date != null)
                        pars.Add("@Acquisition_Settle_Date", requestBody.Acquisition_Settle_Date);
                    pars.Add("@Quantity", Util.ReturnDecimalString(requestBody.Quantity, 2));
                    pars.Add("@Price", Util.ReturnDecimalString(requestBody.Price, 2));
                    pars.Add("@Relief_Method", requestBody.Relief_Method);
                    pars.Add("@Trade_Currency", requestBody.Trade_Currency);
                    pars.Add("@Settlement_Currency", requestBody.Settlement_Currency);
                    pars.Add("@Base_Currency", requestBody.Base_Currency);
                    pars.Add("@Trade_FX_Rate", Util.ReturnDecimalString(requestBody.Trade_FX_Rate, 16));
                    pars.Add("@Settlement_FX_Rate", Util.ReturnDecimalString(requestBody.Settlement_FX_Rate, 16));
                    pars.Add("@Trade_to_Base_FX_Rate", Util.ReturnDecimalString(requestBody.Trade_to_Base_FX_Rate, 16));
                    pars.Add("@Gross_Local", Util.ReturnDecimalString(requestBody.Gross_Local, 2));
                    pars.Add("@Gross_Base", Util.ReturnDecimalString(requestBody.Gross_Base, 2));
                    pars.Add("@Net_Local", Util.ReturnDecimalString(requestBody.Net_Local, 2));
                    pars.Add("@Net_Base", Util.ReturnDecimalString(requestBody.Net_Base, 2));
                    pars.Add("@Accrued_Interest_Local", Util.ReturnDecimalString(requestBody.Accrued_Interest_Local, 2));
                    pars.Add("@Accrued_Interest_Base", Util.ReturnDecimalString(requestBody.Accrued_Interest_Base, 2));
                    pars.Add("@Original_Cost_Base", Util.ReturnDecimalString(requestBody.Original_Cost_Base, 2));
                    pars.Add("@Original_Cost_Local", Util.ReturnDecimalString(requestBody.Original_Cost_Local, 2));
                    pars.Add("@Adjusted_Cost_Base", Util.ReturnDecimalString(requestBody.Adjusted_Cost_Base, 2));
                    pars.Add("@Adjusted_Cost_Local", Util.ReturnDecimalString(requestBody.Adjusted_Cost_Local, 2));
                    pars.Add("@Data_Source", requestBody.Data_Source);
                    pars.Add("@Load_Source", requestBody.Load_Source);
                    var data = Helper.callProcedure("import.usp_Load_Transaction_INS", pars, pwsRecConnectionString);
                }

                return requestBodyArray.Any() ? ValidateImport(requestBodyArray[0].User_ID, "Transaction") : Ok( new { error = false, data = "[]" });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = requestBodyArray[0].User_ID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("securityinsert")]
        public IActionResult SecurityInsert(List<SecurityObject> requestBodyArray)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, requestBodyArray[0].User_ID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, requestBodyArray[0].User_ID.ToString()));
                }

                if (requestBodyArray.Count > 0)
                {
                    DeleteDataFromStagingTable(requestBodyArray[0].User_ID, requestBodyArray[0].FIle_Name, "Security");
                }

                foreach (var requestBody in requestBodyArray)
                {
                    Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                    pars.Add("@Data_Source", requestBody.Data_Source);
                    if (requestBody.Dated_Date != null && requestBody.Dated_Date.Length < 6)
                        pars.Add("@Dated_Date", DateTime.FromOADate(Convert.ToDouble(requestBody.Dated_Date)).ToShortDateString());
                    else if (requestBody.Dated_Date != null)
                        pars.Add("@Dated_Date", requestBody.Dated_Date);
                    pars.Add("@Day_Count_Basis", requestBody.Day_Count_Basis);
                    pars.Add("@ExcelRowNumber", requestBody.ExcelRowNumber);
                    pars.Add("@FIle_Name", requestBody.FIle_Name);
                    if (requestBody.First_Pay_Date != null && requestBody.First_Pay_Date.Length < 6)
                        pars.Add("@First_Pay_Date", DateTime.FromOADate(Convert.ToDouble(requestBody.First_Pay_Date)).ToShortDateString());
                    else if (requestBody.First_Pay_Date != null)
                        pars.Add("@First_Pay_Date", requestBody.First_Pay_Date);
                    pars.Add("@Floating_Interest_Rate", Util.ReturnDecimalString(requestBody.Floating_Interest_Rate, 4));
                    pars.Add("@Frequency", requestBody.Frequency);
                    pars.Add("@Income_Currency", requestBody.Income_Currency);
                    pars.Add("@Interest_Rate", Util.ReturnDecimalString(requestBody.Interest_Rate, 4));
                    if (requestBody.Last_Pay_Date != null && requestBody.Last_Pay_Date.Length < 6)
                        pars.Add("@Last_Pay_Date", DateTime.FromOADate(Convert.ToDouble(requestBody.Last_Pay_Date)).ToShortDateString());
                    else if (requestBody.Last_Pay_Date != null)
                        pars.Add("@Last_Pay_Date", requestBody.Last_Pay_Date);
                    pars.Add("@Load_Source", requestBody.Load_Source);
                    if (requestBody.Maturity_Date != null && requestBody.Maturity_Date.Length < 6)
                        pars.Add("@Maturity_Date", DateTime.FromOADate(Convert.ToDouble(requestBody.Maturity_Date)).ToShortDateString());
                    else if (requestBody.Maturity_Date != null)
                        pars.Add("@Maturity_Date", requestBody.Maturity_Date);
                    pars.Add("@Price_Multiplier", requestBody.Price_Multiplier);
                    pars.Add("@Principal_Currency", requestBody.Principal_Currency);
                    pars.Add("@Publically_Viewable", requestBody.Publically_Viewable);
                    if (requestBody.Reset_Date_Start != null && requestBody.Reset_Date_Start.Length < 6)
                        pars.Add("@Reset_Date_Start", DateTime.FromOADate(Convert.ToDouble(requestBody.Reset_Date_Start)).ToShortDateString());
                    else if (requestBody.Reset_Date_Start != null)
                        pars.Add("@Reset_Date_Start", requestBody.Reset_Date_Start);
                    pars.Add("@Security_Code", requestBody.Security_Code);
                    pars.Add("@Security_Code_Type", requestBody.Security_Code_Type);
                    pars.Add("@Security_ID", requestBody.Security_ID);
                    pars.Add("@Security_Name", requestBody.Security_Name);
                    pars.Add("@Security_Type", requestBody.Security_Type);
                    pars.Add("@Unitized", requestBody.Unitized);
                    pars.Add("@User_Group_Code", requestBody.User_Group_Code);
                    pars.Add("@User_ID", requestBodyArray[0].User_ID);
                    var data = Helper.callProcedure("import.usp_Load_Security_INS", pars, pwsRecConnectionString);
                }

                return requestBodyArray.Any() ? ValidateImport(requestBodyArray[0].User_ID, "Security") : Ok( new { error = false, data = "[]" });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = requestBodyArray[0].User_ID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("positioninsert")]
        public IActionResult PositionInsert(List<PositionObject> requestBodyArray)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, requestBodyArray[0].User_ID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, requestBodyArray[0].User_ID.ToString()));
                }

                if (requestBodyArray.Count > 0)
                {
                    DeleteDataFromStagingTable(requestBodyArray[0].User_ID, requestBodyArray[0].FIle_Name, "Position");
                }

                foreach (var requestBody in requestBodyArray)
                {
                    Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                    pars.Add("@Account_Code", requestBody.Account_Code);
                    pars.Add("@Accrued_Interest_Base", Util.ReturnDecimalString(requestBody.Accrued_Interest_Base, 2));
                    pars.Add("@Accrued_Interest_Local", Util.ReturnDecimalString(requestBody.Accrued_Interest_Local, 2));
                    pars.Add("@Base", Util.ReturnDecimalString(requestBody.Base, 2));
                    pars.Add("@Base_Currency", requestBody.Base_Currency);
                    pars.Add("@Data_Source", requestBody.Data_Source);
                    if (requestBody.Date != null && requestBody.Date.Length < 6)
                        pars.Add("@Date", DateTime.FromOADate(Convert.ToDouble(requestBody.Date)).ToShortDateString());
                    else if (requestBody.Date != null)
                        pars.Add("@Date", requestBody.Date);
                    pars.Add("@ExcelRowNumber", requestBody.ExcelRowNumber);
                    pars.Add("@FIle_Name", requestBody.FIle_Name);
                    pars.Add("@Load_Source", requestBody.Load_Source);
                    pars.Add("@Local", Util.ReturnDecimalString(requestBody.Local, 2));
                    pars.Add("@Local_Currency", requestBody.Local_Currency);
                    pars.Add("@Original_Cost_Base", Util.ReturnDecimalString(requestBody.Original_Cost_Base, 2));
                    pars.Add("@Original_Cost_Local", Util.ReturnDecimalString(requestBody.Original_Cost_Local, 2));
                    pars.Add("@Position_ID", requestBody.Position_ID);
                    pars.Add("@Price", Util.ReturnDecimalString(requestBody.Price, 4));
                    pars.Add("@Quantity", Util.ReturnDecimalString(requestBody.Quantity, 2));
                    pars.Add("@Security_Code", requestBody.Security_Code);
                    pars.Add("@Security_Code_Type", requestBody.Security_Code_Type);
                    pars.Add("@Security_Name", requestBody.Security_Name);
                    pars.Add("@Security_Type", requestBody.Security_Type);
                    pars.Add("@Trade_to_Base_FX_Rate", Util.ReturnDecimalString(requestBody.Trade_to_Base_FX_Rate, 16));
                    pars.Add("@User_ID", requestBodyArray[0].User_ID);
                    var data = Helper.callProcedure("import.usp_Load_Position_INS", pars, pwsRecConnectionString);
                }

                return requestBodyArray.Any() ? ValidateImport(requestBodyArray[0].User_ID, "Position") : Ok(new { error = false, data = "[]", positionValidation = "[]" });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = requestBodyArray[0].User_ID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("priceinsert")]
        public IActionResult PriceInsert(List<PriceObject> requestBodyArray)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult result)
                {
                    return result;
                }

                if (handlerHelper.ValidateToken(Request, requestBodyArray[0].User_ID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, requestBodyArray[0].User_ID.ToString()));
                }

                if (requestBodyArray.Count > 0)
                {
                    DeleteDataFromStagingTable(requestBodyArray[0].User_ID, requestBodyArray[0].FIle_Name, "Price");
                }

                foreach (var requestBody in requestBodyArray)
                {
                    Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                    pars.Add("@Data_Source", requestBody.Data_Source);
                    if (requestBody.Date != null && requestBody.Date.Length < 6)
                        pars.Add("@Date", DateTime.FromOADate(Convert.ToDouble(requestBody.Date)).ToShortDateString());
                    else if (requestBody.Date != null)
                        pars.Add("@Date", requestBody.Date);
                    pars.Add("@ExcelRowNumber", requestBody.ExcelRowNumber);
                    pars.Add("@FIle_Name", requestBody.FIle_Name);
                    pars.Add("@Final_or_Estimate", requestBody.Final_or_Estimate);
                    pars.Add("@Load_Source", requestBody.Load_Source);
                    pars.Add("@Price", requestBody.Price);
                    pars.Add("@Principal_Currency", requestBody.Principal_Currency);
                    pars.Add("@Security_Code", requestBody.Security_Code);
                    pars.Add("@User_ID", requestBody.User_ID);
                    var data = Helper.callProcedure("import.usp_Load_Price_INS", pars, pwsRecConnectionString);
                }

                return requestBodyArray.Any() ? ValidateImport(requestBodyArray[0].User_ID, "Price") : Ok( new { error = false, data = "[]", priceValidation = "[]" });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = requestBodyArray[0].User_ID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        public IActionResult ValidateImport(string UserID, string entityType)
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
                pars.Add("@User_ID", UserID);
                if (entityType == "Security")
                {
                    var securityValidation = Helper.callProcedure("import.usp_Load_Security_Edit", pars, pwsRecConnectionString);
                    return Ok( new { error = false, //data = new
                    //{
                    //    securityValidationResponse = securityValidation
                    //},
                    data = JsonConvert.SerializeObject(securityValidation) });
                }

                if (entityType == "Price")
                {
                    var priceValidation = Helper.callProcedure("import.usp_Load_Price_Edit", pars, pwsRecConnectionString);
                    return Ok(new { error = false, //data = new
                    //{
                    //    priceValidationResponse = priceValidation
                    //},
                    data = JsonConvert.SerializeObject(priceValidation) });
                }

                if (entityType == "Transaction")
                {
                    var transactionValidation = Helper.callProcedure("import.usp_Load_Transaction_Edit", pars, pwsRecConnectionString);
                    return Ok( new { error = false, //data = new
                    //{
                    //    transactionValidationResponse = transactionValidation
                    //},
                    data = JsonConvert.SerializeObject(transactionValidation) });
                }

                if (entityType == "Position")
                {
                    var positionValidation = Helper.callProcedure("import.usp_Load_Position_Edit", pars, pwsRecConnectionString);
                    return Ok( new { error = false, //data = new
                    //{
                    //    positionValidationResponse = positionValidation
                    //},
                    data = JsonConvert.SerializeObject(positionValidation) });
                }

                if (entityType == "Account")
                {
                    var accountValidation = Helper.callProcedure("import.usp_Load_Account_Edit", pars, pwsRecConnectionString);
                    return Ok(new { error = false, //data = new
                    //{
                    //    accountValidationResponse = accountValidation
                    //},
                    data = JsonConvert.SerializeObject(accountValidation) });
                }

                return Ok(new { error = false, data = new { generalResponse = "[]" } });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("DeletePreviousStagingData")]
        public IActionResult DeletePreviousStagingData(string UserID)
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
                pars.Add("@User_ID", UserID);
                //DeleteDataFromStagingTable(UserID, null, "Transaction");
                //DeleteDataFromStagingTable(UserID, null, "Security");
                //DeleteDataFromStagingTable(UserID, null, "Price");
                //DeleteDataFromStagingTable(UserID, null, "Position");
                //DeleteDataFromStagingTable(UserID, null, "Account");
                return Ok( new { error = false, data = "[]" });
            }
            catch (Exception ex)
            {
                Helper.AddApiLogs(connectionString, new ApiLogObject { Method = MethodInfo.GetCurrentMethod().Name, Exception = ex.Message, Params = "web api", UserID = UserID.ToString() });
                var exception = "Unknown Error Occurred";
                return StatusCode((int)HttpStatusCode.InternalServerError, new { error = true, exception = exception });
            }
        }

        [HttpPost]
        [Route("SaveData")]
        public IActionResult SaveAllData(string UserID)
        {
            try
            {
                if (handlerHelper.ValidateSource(Request) is IActionResult actionResult)
                {
                    return actionResult;
                }

                if (handlerHelper.ValidateToken(Request, UserID.ToString()) != string.Empty)
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, handlerHelper.ValidateToken(Request, UserID.ToString()));
                }

                Dictionary<string, dynamic> pars = new Dictionary<string, dynamic>();
                pars.Add("@User_ID", UserID);
                var result = Helper.callProcedure("import.usp_All_Tabs_INS", pars, pwsRecConnectionString);
                return Ok( new { error = false, data = result });
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