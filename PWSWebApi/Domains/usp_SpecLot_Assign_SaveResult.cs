using System;

namespace PWSWebApi.Domains
{
    public class usp_SpecLot_Assign_SaveResult
    {
        public int? NewEditTransID { get; set; }
        public int? TransID { get; set; }
        public int? TaxlotMasterID { get; set; }
        public decimal? AdjOFShares { get; set; }
        public decimal? AdjShares { get; set; }
        public decimal? AdjLocalCost { get; set; }
        public decimal? AdjBaseCost { get; set; }
        public decimal? AdjLocalMV { get; set; }
        public decimal? AdjBaseMV { get; set; }
        public decimal? LocalCostAssigned { get; set; }
        public decimal? BaseCostAssigned { get; set; }
        public decimal? LocalBookAssigned { get; set; }
        public decimal? BaseBookAssigned { get; set; }
        public decimal? LocalMVAssigned { get; set; }
        public decimal? BaseMVAssigned { get; set; }
        public decimal? TotalSharesAdj { get; set; }
        public decimal? TotalLocalCostAdj { get; set; }
        public decimal? TotalBaseCostAdj { get; set; }
        public decimal? TotalLocalMVAdj { get; set; }
        public decimal? TotalBaseMVAdj { get; set; }
        public decimal? RemainingShares { get; set; }
        public decimal? RemainingNetLocal { get; set; }
        public decimal? RemainingNetBase { get; set; }
        public DateTime? LoadDateTime { get; set; }
        public string Stat { get; set; }
        public DateTime? RetiredDateTime { get; set; }
        public int? EditUserID { get; set; }
    }
}
