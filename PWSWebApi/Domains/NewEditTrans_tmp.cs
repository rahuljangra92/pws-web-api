using System;

namespace PWSWebApi.Domains
{
    public class NewEditTrans_tmp
    {
        public int NewEditTrans_tmpID { get; set; }
        public int UIID { get; set; }
        public int? TransID { get; set; }
        public int? TransProcessCodeID { get; set; }
        public decimal? ProcessOrd { get; set; }
        public bool? StaticData { get; set; }
        public string DPTransID { get; set; }
        public string DPLotID { get; set; }
        public int? AcctID { get; set; }
        public int? SubAcctID { get; set; }
        public int? ReliefMethod { get; set; }
        public int? SecID { get; set; }
        public int? TCodeID { get; set; }
        public string DPTransCodeName { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? TradeDate { get; set; }
        public DateTime? SettleDate { get; set; }
        public DateTime? AcquisitionTradeDate { get; set; }
        public DateTime? AcquisitionSettleDate { get; set; }
        public decimal? Shrs { get; set; }
        public decimal? OFShrs { get; set; }
        public decimal? Price { get; set; }
        public decimal? PriceMult { get; set; }
        public int? TradeCCYID { get; set; }
        public int? SettleCCYID { get; set; }
        public int? BaseCCYID { get; set; }
        public decimal? TradeFXRate { get; set; }
        public decimal? SettleFXRate { get; set; }
        public decimal? BaseFXRate { get; set; }
        public decimal? GrossLocal { get; set; }
        public decimal? GrossBase { get; set; }
        public decimal? Fees { get; set; }
        public decimal? ForeignWithhold { get; set; }
        public decimal? NetLocal { get; set; }
        public decimal? NetBase { get; set; }
        public decimal? AILocal { get; set; }
        public decimal? AIBase { get; set; }
        public decimal? AALocal { get; set; }
        public decimal? AABase { get; set; }
        public decimal? OrigCostLocal { get; set; }
        public decimal? OrigCostBase { get; set; }
        public decimal? BookCostLocal { get; set; }
        public decimal? BookCostBase { get; set; }
        public decimal? RecordCostLocal { get; set; }
        public decimal? RecordCostBase { get; set; }
        public decimal? PurYld { get; set; }
        public string Comment { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? Final { get; set; }
        public int? DPID { get; set; }
        public int? UserID { get; set; }
        public DateTime? LoadDateTime { get; set; }
        public string Stat { get; set; }
        public DateTime? RetiredDateTime { get; set; }
        public int? LoadSourceID { get; set; }
        public int? OriginalTransID { get; set; }
        public int? TransTypeID { get; set; }
        public int? TransSubTypeID { get; set; }
        public int? LinkAcctID { get; set; }
        public string UserDefined1 { get; internal set; }
        public string UserDefined2 { get; internal set; }
        public int LinkSubAcctID { get; internal set; }
    }
}
