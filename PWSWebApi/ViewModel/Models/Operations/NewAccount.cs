using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PWSWebApi.ViewModel.Models.Operations
{
    public class NewAccount: ParentOps
    {
        //AcctCode,AcctNum,AcctName,AcctNickNamePrimary,BaseCurr,LoadSourceID,UserGroupID,ReliefMethodID,EntityCode,OwnedPct,StartingDate,EndingDate,PortfolioCode
        public int New_AccountsID { get; set; }
        public int? New_Accounts_tmpID { get; set; }
        public string AcctCode { get; set; }
        public string AcctNum { get; set; }
        public string AcctName { get; set; }
        public string AcctNickNamePrimary { get; set; }
        public string BaseCurr { get; set; }
        public string LoadSourceID { get; set; }
        public string UserGroupID { get; set; }
        public string ReliefMethodID { get; set; }
        
        public string TaxLotReliefMethodID { get; set; }
        public string EntityCode { get; set; }
        public string OwnedPct { get; set; }
        public string StartingDate { get; set; }
        public string EndingDate { get; set; }
        public string PortfolioCode { get; set; }

        [JsonProperty("editMode")]
        public bool EditMode { get; set; }
    }
}