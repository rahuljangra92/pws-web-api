using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using PWSWebApi.Models.DD;

namespace PWSWebApi.Models
{
    public class CorpActionSearchRequest
    {
        [JsonProperty("pwsIdentification")]
        public bool PwsIdentification { get; set; }
        public List<CorpActType> CorpActTypes { get; set; } = new List<CorpActType>();
        public string ActionStatus { get; set; }
        public string ParentCashRate { get; set; }
        public string CashRate { get; set; }
        public string ChildCashRate { get; set; }
        public string ParentCostRate { get; set; }
        public string ChildCostRate { get; set; }
        public string ParentMovementPrice { get; set; }
        public string ChildMovementPrice { get; set; }
        public string ChildProcessOrder { get; set; }
        public string ChildRequired { get; set; }
        public string ChildSecID { get; set; }
        public string ChildSecurityName { get; set; }
        public string ChildShrRate { get; set; }
        public string CorpActsChildID { get; set; }
        public int CorpActsChildIDNum => Convert.ToInt32(CorpActsChildID);
        public string CorpActsID { get; set; }
        public string CorpActsTypeID { get; set; }
        public string CorpactsTypeName { get; set; }
        public string CostRate { get; set; }
        public string Currency { get; set; }
        public string DPID { get; set; }
        public string DPName { get; set; }
        public string EditUserID { get; set; }
        public DateTime? ExDate { get; set; }
        public object ForeignWithholding { get; set; }
        public DateTime? LoadDateTime { get; set; }
        public string LoadSourceID { get; set; }
        public string LoadSourceName { get; set; }
        public string MovementPrice { get; set; }
        public string Optional { get; set; }
        public DateTime? PayDate { get; set; }
        public string PctCost { get; set; }
        public string PctValue { get; set; }
        public string PrincCCYID { get; set; }
        public string ParentProcessOrder { get; set; }
        public string ProcessOrder { get; set; }
        public DateTime? RecordDate { get; set; }

        public string ParentSecID { get; set; }
        public string SecID { get; set; }
        public string SecurityName { get; set; }
        public string ParentShrRate { get; set; }
        public string ShrRate { get; set; }
        public string Stat { get; set; }
        public string Trial_CorpActionsChildID { get; set; }
        public string Trial_CorpActionsID { get; set; }
        public int? Trial_CorpActionsIDAsNum => Convert.ToInt32(Trial_CorpActionsID);
        public string UserID { get; set; }

        public bool IsParent => !IsChildRecord; 

        public bool IsParentWithChildren { get; set; }

        public bool IsChildRowFromUI { get; set; }
        public bool IsChildRecord => !string.IsNullOrEmpty(CorpActsChildID) || IsChildRowFromUI; // non-db row, gets set in the middle tier

        public bool LastItemInGroup { get; set; } // non-db row, gets set in the middle tier

        public string GridSecurityName => IsParent ? SecurityName : ChildSecurityName; // non-db row
        public string GridCorpActionTypeWithID => IsParent ? CorpactsTypeName : null;
        //public string GridCorpActionTypeWithID => IsParent ? (PwsIdentification ? CorpactsTypeName : $"{CorpActsTypeID}-{CorpactsTypeName}") : null;
        public string GridShowOptional => IsParent ? (Optional == "1" ? "Yes" : "No") : null; 
        public string GridExDate => IsParent && ExDate != null ? Convert.ToDateTime(ExDate).ToString("MM/dd/yyyy") : null;
        public string GridPayDate => IsParent && PayDate != null ? Convert.ToDateTime(PayDate).ToString("MM/dd/yyyy") : null;
        public string GridRecordDate => IsParent && RecordDate != null ? Convert.ToDateTime(RecordDate).ToString("MM/dd/yyyy") : null;
        public string GridCurrency => IsParent ? Currency : null;
        public string GridProcessOrder => IsParent ? ProcessOrder : null;
        public string GridShrRate => IsParent && !string.IsNullOrEmpty(ShrRate) ? Convert.ToDecimal(ShrRate).ToString("N5") : (!string.IsNullOrEmpty(ChildShrRate) ? Convert.ToDecimal(ChildShrRate).ToString("N5") : null);
        public string GridCashRate => IsParent && !string.IsNullOrEmpty(CashRate) ? Convert.ToDecimal(CashRate).ToString("N5") : (!string.IsNullOrEmpty(ChildCashRate) ? Convert.ToDecimal(ChildCashRate).ToString("N5") : null);
        public string GridCostRate => IsParent && !string.IsNullOrEmpty(CostRate) ? Convert.ToDecimal(CostRate).ToString("N5") : (!string.IsNullOrEmpty(ChildCostRate) ? Convert.ToDecimal(ChildCostRate).ToString("N5") : null);
        public string GridMovementPrice => IsParent && !string.IsNullOrEmpty(MovementPrice) ? Convert.ToDecimal(MovementPrice).ToString("N5") : (!string.IsNullOrEmpty(ChildMovementPrice) ? Convert.ToDecimal(ChildMovementPrice).ToString("N5") : null);
        public string GridPercentValue => IsParent && IsParentWithChildren ? PctValue : null;
        public string GridPercentCost => IsParent && IsParentWithChildren ? PctCost : null;

        public string key { get; set; } // needed for antd Table component 

        public bool IsEditing { get; set; } // needed for keeping track in the UI if this row is in edit more or not

        public string Periodicity { get; set; } // set up in the UI based on parent
        
        public string GridPeriodicity { get; set; }

        // this is logic for PeriodicityDropdown's visibiloty based on the corpactytypeids. Same logic is also built in ui
        public bool ShouldShowPeriodicityDropdown => (new List<string> { "1", "11", "25" }).Contains(CorpActsTypeID);

        public bool IsChildRecordRequiredBeforeSaving { get; set; } // set from the UI
        public bool ConfirmDelete { get; set; }
        public bool ConfirmCancel { get; set; }
        public bool IsDeleted { get; set; }
        public CorpActionSearchRequest MakeCopy()
        {
            return this.MemberwiseClone() as CorpActionSearchRequest;
        }

        public ChildForPosting Child1 { get; set; }
        public ChildForPosting Child2 { get; set; }
        public ChildForPosting Child3 { get; set; }
        public ChildForPosting Child4 { get; set; }
        public ChildForPosting Child5 { get; set; }
        public ChildForPosting Child6 { get; set; }
        public ChildForPosting Child7 { get; set; }
    }


    public class ChildForPosting
    {
        public string ChildCashRate { get; set; }
        public string ChildCostRate { get; set; }
        public string ChildShrRate { get; set; }
        public string ChildMovementPrice { get; set; }
        public string ChildProcessOrder { get; set; }
        public string ChildRequired { get; set; }
        public string ChildSecID { get; set; }
        public string ChildSecurityName { get; set; }
        public string CorpActsChildID { get; set; }
    }
}