using Newtonsoft.Json;
using PWSWebApi.Controllers;
using PWSWebApi.ViewModel.Models.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PWSWebApi.ViewModel
{
    public class VMImpotData
    {
        public string UserID { get; set; }

        public bool notExcelImport { get; set; } // indicating whether the call came from an excel import or not
        public string JSON { get; set; }

        [JsonProperty("gridColumnsID")]
        public string GridColumnSId { get; set; }

        [JsonProperty("gridName")]
        public string GridName { get; set; }

        public List<List<string>> Operations { get; set; }

        public List<ColumnsFromExcel> Cols { get; set; }

        public bool validateOnly { get; set; }

        public List<NewAccount> newAccts { get; set; }

        public List<Trans> trans { get; set; }

        public List<ReportModel> rptModels { get; set; }

        public List<PerfStatic> perfStatic { get; set; }

        public string PerfStaticKeepAction { get; set; }

        public  List<ComparePerfStats> visualGroupingPermToTemp { get; set; }
        public List<ComparePerfStats> visualGroupingTempToPerm { get; set; }
        public bool perfStaticDateContinuityWarningIgnore { get; set; }
    }

    public class ColumnsFromExcel
    {
        public string name { get; set; }
        public string key { get; set; }

    }

    public class PerfStaticDeletion
    {
        public int PerfStaticID { get; set; }
        public List<int> PerfStatic_tmpIDs { get; set; }
    }

}