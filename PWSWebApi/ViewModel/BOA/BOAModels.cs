using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PWSWebApi.ViewModel.BOA
{
    public class TokenObject
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public string ExpiresIn { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("transactionIdentification")] // gets set when there is an error
        public string TransactionIdentification { get; set; } // gets set when there is an error


        [JsonProperty("apiName")]
        public string ApiName { get; set; } // gets set when there is an error


        [JsonProperty("reason.reasonCode")]
        public string ReasonCode { get; set; } // gets set when there is an error

        [JsonProperty("reason")]
        public ErrorReason Reason { get; set; } // gets set when there is an error

        //  [JsonProperty("errors")]
        // public List<ErrorFromBOA> Errors {get;set;} // gets set when there is an error
    }

    public class VMBalanceInputObject
    {
        [JsonProperty("balanceAsOfDate")]
        public string BalanceAsOfDate { get; set; }

        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }

        [JsonProperty("bankId")]
        public string BankID { get; set; }

        [JsonProperty("accounts")]
        public List<InputAccount> Accounts { get; set; }


    }


    public class VMBalanceInputObjectForBOAParams
    {
        [JsonProperty("balanceAsOfDate")]
        public string BalanceAsOfDate { get; set; }

        [JsonProperty("accounts")]
        public List<InputAccount> Accounts { get; set; }


    }

    public class InputAccount
    {
        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }

        [JsonProperty("bankId")]
        public string BankID { get; set; }
    }


    public class ErrorReason
    {
        [JsonProperty("reasonCode")]
        public string ReasonCode { get; set; }

        [JsonProperty("reasonDescription")]
        public string ReasonDescription { get; set; }


    }


    public class ErrorFromBOA
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }


    }

    public class VMBalanceInquiry
    {
        [JsonProperty("accountBalances")]
        public List<AccountBalance> AccountBalances { get; set; }



        [JsonProperty("error")]
        public string Error { get; set; } // gets set when there is an error

        [JsonProperty("status")]
        public string Status { get; set; } // gets set when there is an error

        [JsonProperty("transactionIdentification")]
        public string TransactionIdentification { get; set; } // gets set when there is an error


        [JsonProperty("apiName")]
        public string ApiName { get; set; } // gets set when there is an error


        [JsonProperty("reason")]
        public ErrorReason Reason { get; set; } // gets set when there is an error

        [JsonProperty("errors")]
        public List<ErrorFromBOA> Errors { get; set; } // gets set when there is an error
    }

    public class AccountBalance
    {
        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }

        [JsonProperty("bankId")]
        public string BankID { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("balances")]
        public List<Balance> balances { get; set; }
    }


    public class Balance
    {

        [JsonProperty("transactionDescription")]
        public string TransactionDescription { get; set; }

        [JsonProperty("transactionCode")]
        public string TransactionCode { get; set; }


        [JsonProperty("amount")]
        public string Amount { get; set; }


        [JsonProperty("lastUpdatedDate")]
        public string LastUpdatedDate { get; set; }

    }



    #region transaction

    public class VMTransInputObjectForBOAParams
    {

        [JsonProperty("fromDate")]
        public string FromDate { get; set; }


        [JsonProperty("toDate")]
        public string ToDate { get; set; }

        [JsonProperty("accounts")]
        public List<InputAccount> Accounts { get; set; }


    }


    public class VMTransactionInputObject
    {
        [JsonProperty("fromDate")]
        public string FromDate { get; set; }

        [JsonProperty("toDate")]
        public string ToDate { get; set; }

        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }

        [JsonProperty("bankId")]
        public string BankID { get; set; }


        [JsonProperty("accounts")]
        public List<InputAccount> Accounts { get; set; }


    }


    //this one is needed for JSON response form
    public class AccountTransaction
    {
        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }

        [JsonProperty("bankId")]
        public string BankID { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }


        [JsonProperty("transactions")]
        public List<TransactionInfo> Transactions { get; set; }

    }

    public class VMTransactionInquiry
    {

        [JsonProperty("accountTransactions")]
        public List<AccountTransaction> AccountTransactions { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; } // gets set when there is an error

        [JsonProperty("status")]
        public string Status { get; set; } // gets set when there is an error

        [JsonProperty("transactionIdentification")]
        public string TransactionIdentification { get; set; } // gets set when there is an error


        [JsonProperty("apiName")]
        public string ApiName { get; set; } // gets set when there is an error


        [JsonProperty("reason")]
        public ErrorReason Reason { get; set; } // gets set when there is an error

        [JsonProperty("errors")]
        public List<ErrorFromBOA> Errors { get; set; } // gets set when there is an error
    }

    public class TransactionInfo
    {
        [JsonProperty("asOfDate")]
        public string AsOfDate { get; set; }
        [JsonProperty("transactionCode")]
        public string TransactionCode { get; set; }
        [JsonProperty("transactionDescription")]
        public string TransactionDescription { get; set; }

        [JsonProperty("transactionType")]
        public string TransactionType { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("creditDebitIndicator")]
        public string CreditDebitIndicator { get; set; }

        [JsonProperty("availabilityImmediateAmount")]
        public string AvailabilityImmediateAmount { get; set; }

        [JsonProperty("availability1DayAmount")]
        public string Availability1DayAmount { get; set; }

        [JsonProperty("availability2PlusDayAmount")]
        public string Availability2PlusDayAmount { get; set; }

        [JsonProperty("itemCount")]
        public string ItemCount { get; set; }

        [JsonProperty("valueDate")]
        public string ValueDate { get; set; }

        [JsonProperty("customerReference")]
        public string CustomerReference { get; set; }

        [JsonProperty("bankReference")]
        public string BankReference { get; set; }

        [JsonProperty("detailText")]
        public string DetailText { get; set; }
    }

    #endregion
}