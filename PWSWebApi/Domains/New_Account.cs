using System;

namespace PWSWebApi.Domains
{
    public class New_Account
    {
        public int New_AccountsID { get; set; }
        public string AcctCode { get; set; }
        public string AcctNum { get; set; }
        public string AcctName { get; set; }
        public string AcctNickNamePrimary { get; set; }
        public string BaseCurr { get; set; }
        public int? LoadSourceID { get; set; }
        public int? UserGroupID { get; set; }
        public int? TaxLotReliefMethodID { get; set; }
        public string EntityCode { get; set; }
        public decimal? OwnedPct { get; set; }
        public DateTime? StartingDate { get; set; }
        public DateTime? EndingDate { get; set; }
        public string PortfolioCode { get; set; }
        public int? UserID { get; set; }
        public DateTime? LoadDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public DateTime? RetiredDateTime { get; set; }
        public int? AcctID { get; set; }
    }
}
