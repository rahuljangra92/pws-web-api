using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PWSWebApi.Models.PartnershipTracking
{

    public class ObjectPartnershipTracking
    {
        public string UserID { get; set; }
        public string AcctID_SubAcctIDs { get; set; }
        public string EOD_StartDateStart { get; set; }
        public string EOD_StartDateEnd { get; set; }
        public string PartnersAcctIDs { get; set; }
        public string EntityID { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string ByPartner { get; set; }
        public string ByPartnership { get; set; }
        public string PeriodicityID { get; set; }
        public string TrialBalance { get; set; }
        public string Effectivedate { get; set; }

        public string Test { get; set; }
        public string TransSets { get; set; }

        public string Unmatched { get; set; }

        public string Matched { get; set; }

        public string EffectiveStartDate { get; set; }

        public string EffectiveEndDate { get; set; }

        public string Unmatch { get; set; }

        public string PartnershipFlow { get; set; }

        public string PartnershipTransID { get; set; }
        public string PartnerTransID { get; set; }
        public string PartnershipTransIDs { get; set; }
        public string PartnerTransIDs { get; set; }

        public string MatchingTransID { get; set; }

        public string CreateMatch { get; set; }

        public string NonRAFlow { get; set; }
        public string PartnershipAcctID { get; set; }
        public string ExTrackedSecID { get; set; }
        public string NewTcodeID { get; set; }

        public string SettleCCYID { get; set; }
        public string Final { get; set; }
        public string TcodeID { get; set; }
        public string SecID { get; set; }
        public string SubAcctID { get; set; }
        public string AcctID { get; set; }
    }
}