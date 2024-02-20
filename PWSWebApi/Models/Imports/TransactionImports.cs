using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PWSWebApi.Models.Imports
{
    public class TransactionImports
    {
        public string accountNumber { get; set; }
        public string bankId { get; set; }
        public string currency { get; set; }
        public string asOfDate { get; set; }
        public string transactionCode { get; set; }
        public string transactionDescription { get; set; }
        public string transactionType { get; set; }
        public string amount { get; set; }
        public string creditDebitIndicator { get; set; }
        public string availabilityImmediateAmount { get; set; }
        public string availability1DayAmount { get; set; }
        public string availability2PlusDayAmount { get; set; }
        public string itemCount { get; set; }
        public string valueDate { get; set; }
        public string customerReference { get; set; }
        public string bankReference { get; set; }
        public string detailText { get; set; }
        public string ExcelRowNumber { get; set; }
        public string Transaction_ID { get; set; }
        public string Account_Code { get; set; }
        public string Security_Name { get; set; }
        public string Security_Code { get; set; }
        public string Security_Code_Type { get; set; }
        public string Security_Type { get; set; }
        public string Transaction_Type { get; set; }
        public string Client_Description_of_Transaction_Type { get; set; }
        public string Trade_Date { get; set; }
        public string Acquisition_Trade_Date { get; set; }
        public string Settlement_Date { get; set; }
        public string Acquisition_Settle_Date { get; set; }
        public string Quantity { get; set; }
        public string Price { get; set; }
        public string Relief_Method { get; set; }
        public string Trade_Currency { get; set; }
        public string Settlement_Currency { get; set; }
        public string Base_Currency { get; set; }
        public string Trade_FX_Rate { get; set; }
        public string Settlement_FX_Rate { get; set; }
        public string Trade_to_Base_FX_Rate { get; set; }
        public string Gross_Local { get; set; }
        public string Gross_Base { get; set; }
        public string Net_Local { get; set; }
        public string Net_Base { get; set; }
        public string Accrued_Interest_Local { get; set; }
        public string Accrued_Interest_Base { get; set; }
        public string Original_Cost_Base { get; set; }
        public string Original_Cost_Local { get; set; }
        public string Adjusted_Cost_Base { get; set; }
        public string Adjusted_Cost_Local { get; set; }
        public string Data_Source { get; set; }
        public string Load_Source { get; set; }
        public string User_ID { get; set; }
        public string FIle_Name { get; set; }

    }
}