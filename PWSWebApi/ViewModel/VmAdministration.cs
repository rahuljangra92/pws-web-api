using PWSWebApi.ViewModel.Models.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace PWSWebApi.ViewModel
{
    public class VmAdministration
    {
        public List<ReportModel> reportModels { get; set; }

        public List<ReportModel> backUpReportModels { get; set; }

        public string Action { get; set; }

        public ReportModel newRow { get; set; }
    }
}