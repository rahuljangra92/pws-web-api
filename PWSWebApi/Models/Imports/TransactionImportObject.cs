using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PWSWebApi.Models.Imports
{
    public class DataSourceObject
    {
        public int ExcelRowNumber { get; set; }
        [JsonProperty("AcctID")]
        public int AcctID;

        [JsonProperty("AcctName")]
        public string AcctName;

        [JsonProperty("AcctCode")]
        public string AcctCode;

        [JsonProperty("Base_Currency")]
        public string BaseCurrency;

        [JsonProperty("Account_Number")]
        public string AccountNumber;

        [JsonProperty("AcctNickNamePrimary")]
        public string AcctNickNamePrimary;

        [JsonProperty("Data_Source")]
        public object DataSource;

        [JsonProperty("Load_Source")]
        public string LoadSource;

        [JsonProperty("UserGroupCode")]
        public string UserGroupCode;

        [JsonProperty("Tax_Lot_Relief_Method")]
        public string TaxLotReliefMethod;

        [JsonProperty("Benchmark")]
        public string Benchmark;

        [JsonProperty("Performance_Start_Date")]
        public int PerformanceStartDate;

        [JsonProperty("Manager")]
        public string Manager;

        [JsonProperty("key")]
        public int Key;

        [JsonProperty("EntityID")]
        public int? EntityID;

        [JsonProperty("Entity_Name")]
        public string EntityName;

        [JsonProperty("Entity_Description")]
        public string EntityDescription;

        [JsonProperty("EntityCode")]
        public string EntityCode;

        [JsonProperty("PortfolioID")]
        public int? PortfolioID;

        [JsonProperty("PortfolioName")]
        public string PortfolioName;

        [JsonProperty("Portfolio_Description")]
        public string PortfolioDescription;

        [JsonProperty("PortfolioCode")]
        public string PortfolioCode;

        [JsonProperty("OwnershipID")]
        public int? OwnershipID;

        [JsonProperty("Pct_Own")]
        public int? PctOwn;

        [JsonProperty("Account_or_Entity")]
        public string AccountOrEntity;

        [JsonProperty("Acct_or_EntityCode")]
        public string AcctOrEntityCode;

        [JsonProperty("Starting_Date")]
        public int? StartingDate;

        [JsonProperty("Ending_Date")]
        public object EndingDate;

        [JsonProperty("AcctOrEntityOrder")]
        public object AcctOrEntityOrder;

        [JsonProperty("PortfolioOwnID")]
        public int? PortfolioOwnID;

        [JsonProperty("UserGroup_ID")]
        public int? UserGroupID;

        [JsonProperty("UserGroupName")]
        public string UserGroupName;

        [JsonProperty("SecID")]
        public int? SecID;

        [JsonProperty("SecName")]
        public string SecName;

        [JsonProperty("SecCode")]
        public string SecCode;

        [JsonProperty("SecType")]
        public string SecType;

        [JsonProperty("Unitized")]
        public string Unitized;

        [JsonProperty("Principal")]
        public string Principal;

        [JsonProperty("Income")]
        public string Income;

        [JsonProperty("__EMPTY_1")]
        public string EMPTY1;

        [JsonProperty("SecCode_1")]
        public string SecCode1;

        [JsonProperty("Interest_Rate")]
        public string InterestRate;

        [JsonProperty("DatedDate")]
        public int? DatedDate;

        [JsonProperty("FirstPayDate")]
        public int? FirstPayDate;

        [JsonProperty("LastPayDate")]
        public int? LastPayDate;

        [JsonProperty("MaturityDate")]
        public int? MaturityDate;

        [JsonProperty("DayCountBasis")]
        public int? DayCountBasis;

        [JsonProperty("Frequency")]
        public string Frequency;

        [JsonProperty("FloatingInterestRate")]
        public int? FloatingInterestRate;

        [JsonProperty("ResetDateStart")]
        public int? ResetDateStart;

        [JsonProperty("Transaction_ID")]
        public int? TransactionID;

        [JsonProperty("Security_Name")]
        public string SecurityName;

        [JsonProperty("Transaction_Type")]
        public string TransactionType;

        [JsonProperty("Trade_Date")]
        public string TradeDate;

        [JsonProperty("Acquisition_Settle_Date")]
        public int? AcquisitionSettleDate;

        [JsonProperty("Quantity")]
        public double? Quantity;

        [JsonProperty("Price")]
        public double? Price;

        [JsonProperty("Trade_Currency")]
        public string TradeCurrency;

        [JsonProperty("Settlement_Currency")]
        public string SettlementCurrency;

        [JsonProperty("Trade_FX_Rate")]
        public double? TradeFXRate;

        [JsonProperty("Settlement_FX_Rate")]
        public double? SettlementFXRate;

        [JsonProperty("Trade_to_Base_FX_Rate")]
        public double? TradeToBaseFXRate;

        [JsonProperty("Gross_Local")]
        public double? GrossLocal;

        [JsonProperty("Gross_Base")]
        public double? GrossBase;

        [JsonProperty("Net_Local")]
        public double? NetLocal;

        [JsonProperty("Net_Base")]
        public double? NetBase;

        [JsonProperty("Accrued_Interest_Local")]
        public int? AccruedInterestLocal;

        [JsonProperty("Accrued_Interest_Base")]
        public int? AccruedInterestBase;

        [JsonProperty("Original_Cost_Base")]
        public double? OriginalCostBase;

        [JsonProperty("Original_Cost_Local")]
        public double? OriginalCostLocal;

        [JsonProperty("Adjusted_Cost_Base")]
        public int? AdjustedCostBase;

        [JsonProperty("Adjusted_Cost_Local")]
        public int? AdjustedCostLocal;

        [JsonProperty("Position_ID")]
        public int? PositionID;

        [JsonProperty("Date")]
        public object Date;

        [JsonProperty("Local_Currency")]
        public string LocalCurrency;

        [JsonProperty("Local")]
        public double? Local;

        [JsonProperty("Base")]
        public double? Base;

        [JsonProperty("Original_Cost_Local_1")]
        public double? OriginalCostLocal1;

        [JsonProperty("Final_or_Estimate")]
        public string FinalOrEstimate;

        [JsonProperty("Principal_Currency")]
        public string PrincipalCurrency;
    }

    public class Column
    {
        [JsonProperty("title")]
        public string Title;

        [JsonProperty("dataIndex")]
        public string DataIndex;

        [JsonProperty("key")]
        public string Key;
    }

    public class TransactionImportObjectAggregation
    {
        [JsonProperty("dataSource")]
        public List<DataSourceObject> DataSource;

        [JsonProperty("workSheetName")]
        public string WorkSheetName;

        [JsonProperty("columns")]
        public List<Column> Columns;

        public string WorkbookName { get; set; }
    }

    public class TransactionImportObject
    {
        public int UserID { get; set; }
        public List<TransactionImportObjectAggregation> Records { get; set; }

        public List<ErrorExcelObject> Errors { get; set; }
    }


    public class ErrorExcelObject
    {
        public string SheetName { get; set; }
        public string WorkbookName { get; set; }

        public string Error { get; set; }

        public int RowNumber { get; set; }
    }

}