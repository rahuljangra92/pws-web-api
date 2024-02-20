using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PWSWebApi.Models.DD
{
    public class CorpActType
    {

        public int CorpActsTypeID { get; set; }
        public string CorpActsTypeName { get; set; }
        public int ParentProcessOrder { get; set; }
        public int? ChildProcessOrder { get; set; }
        public int Optional { get; set; }
        public int ExDate { get; set; }
        public int RecordDate { get; set; }
        public int PayDate { get; set; }
        public int DPID { get; set; }
        public int LoadSourceID { get; set; }
        public int ParentSecID { get; set; }
        public int ParentShrRate { get; set; }
        public int ParentCashRate { get; set; }
        public int ParentCostRate { get; set; }
        public int ParentMovementPrice { get; set; }
        public int ParentForeignWithholding { get; set; }
        public int PeriodsPerYear { get; set; }
        public int ChildSecID { get; set; }
        public int ChildShrRate { get; set; }
        public int ChildCashRate { get; set; }
        public int ChildCostRate { get; set; }
        public int ChildMovementPrice { get; set; }
    }
}