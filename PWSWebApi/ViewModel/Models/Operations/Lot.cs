using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PWSWebApi.ViewModel.Models.Operations
{
    public class Lot
    {
        public string AccountName { get; set; }
        public string AcctID { get; set; }
        public string AcquisitionTradeDate { get; set; }
        public string BaseCCYID { get; set; }
        public string BaseCost { get; set; }
        public string BaseCurr { get; set; }
        public string BaseUnitCost { get; set; }
        public string Effectivedate { get; set; }
        public string LocalCost { get; set; }
        public string LocalCurr { get; set; }
        public string LocalUnitCost { get; set; }
        public string LotShrs { get; set; }
        public string NetBase { get; set; }
        public string NetLocal { get; set; }
        public string Ord { get; set; }
        public string SecID { get; set; }
        public string SecurityTitle { get; set; }
        public string Shrs { get; set; }
        public string SignCost { get; set; }
        public string SignCostPlusMinus { get; set; }
        public string SignShares { get; set; }
        public string SignSharesPlusMinus { get; set; }
        public string SubAcctID { get; set; }
        public string TaxLotMasterID { get; set; }
        public string TradeCCYID { get; set; }
        public string TransID { get; set; }
        public string calcBaseCost { get; set; }
        public string calcLocalCost { get; set; }
        public string calcShares { get; set; }
        public string editBaseCost { get; set; }
        public string editLocalCost { get; set; }
        public string editShares { get; set; }
        public string sequencyKey { get; set; }

        public string Stat { get; set; }

        public string SpecLot_Assign_EditID { get; set; }
        public string NewEditTransID { get; set; }
    }
}