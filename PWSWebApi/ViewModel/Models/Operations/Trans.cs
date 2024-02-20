using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PWSWebApi.ViewModel.Models.Operations
{
    public class Trans
    {
       public string ColumnTitle { get; set; }
        public string TransID { get; set; }
        public string TransProcessCodeID { get; set; }
        public string ProcessOrd { get; set; }
        public string StaticData { get; set; }
        public string DPTransID { get; set; }
        public string DPLotID { get; set; }
        public string AcctID { get; set; }
        public string SubAcctID { get; set; }
        public string ReliefMethod { get; set; }
        public string SecID { get; set; }
        public string TCodeID { get; set; }
        public string DPTransCodeName { get; set; }
        public string EffectiveDate { get; set; }
        public string TradeDate { get; set; }
        public string SettleDate { get; set; }
        public string AcquisitionTradeDate { get; set; }
        public string AcquisitionSettleDate { get; set; }
        public string Shrs { get; set; }
        public string OFShrs { get; set; }
        public string Price { get; set; }
        public string PriceMult { get; set; }
        public string TradeCCYID { get; set; }
        public string SettleCCYID { get; set; }
        public string BaseCCYID { get; set; }
        public string TradeFXRate { get; set; }
        public string SettleFXRate { get; set; }
        public string BaseFXRate { get; set; }
        public string GrossLocal { get; set; }
        public string GrossBase { get; set; }
        public string Fees { get; set; }
        public string ForeignWithhold { get; set; }
        public string NetLocal { get; set; }
        public string NetBase { get; set; }
        public string AILocal { get; set; }
        public string AIBase { get; set; }
        public string AALocal { get; set; }
        public string AABase { get; set; }
        public string OrigCostLocal { get; set; }
        public string OrigCostBase { get; set; }
        public string BookCostLocal { get; set; }
        public string BookCostBase { get; set; }
        public string RecordCostLocal { get; set; }
        public string RecordCostBase { get; set; }
        public string PurYld { get; set; }
        public string Comment { get; set; }
        public string Final { get; set; }
        public string Stat { get; set; }
        public string LoadSourceID { get; set; }
        public string DPID { get; set; }
        public string OriginalTransID { get; set; }
        public string TransTypeID { get; set; }
        public string TransSubTypeID { get; set; }
        public string LinkAcctID { get; set; }
        public string LinkSubAcctID { get; set; }
        public string UserDefined1 { get; set; }
        public string UserDefined2 { get; set; }
    }
}