using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PWSWebApi.ViewModel.Models.Operations
{
    public class PerfStatic
    {
        public int PerfStatic_tmpID { get; set; }

        public string PerfAttribDescr { get; set; }

        public string PerfAttribID { get; set; }

        public string BeginDate { get; set; }

        public string EndDate { get; set; }

        public DateTime BeginDateFormatted { get; set; }

        public DateTime EndDateFormatted { get; set; }
        public string BMV { get; set; }

        public string EMV { get; set; }
        public string BegFlow { get; set; }
        public string NetFlow { get; set; }

        public string MgtFee { get; set; }

        public string GRet { get; set; }

        public string NRet { get; set; }

        public string Stat { get; set; }

        public string DPID { get; set; }

        public string LoadSourceID { get; set; }

    }
}