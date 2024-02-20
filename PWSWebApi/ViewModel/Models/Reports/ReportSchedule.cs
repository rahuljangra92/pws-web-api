using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PWSWebApi.ViewModel.Models.Reports
{
    public class ReportSchedule
    {
        public string ModelTitle { get; set; }
        public string selectedGroupingStyle { get; set; }
        public string selectedGroupingStyleName { get; set; }
        public string periodicityID { get; set; }
        public string periodicityName { get; set; }
        public DateTime BookEndDate { get; set; }
        public string BookTitle { get; set; }

        public string BookName { get; set; }
        public string BookID { get; set; }
        public string currentReport { get; set; }
        public string LogoID { get; set; }
        public string WaitForBusinessDays { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int UserID { get; set; }
        public int UserPrefTypeID { get; set; }

        public string UserPrefTypeName{ get; set; }
        public string DelimParams { get; set; }

        public string selectedReportBook { get; set; }

        public bool isNewBook { get; set; }
    }
}