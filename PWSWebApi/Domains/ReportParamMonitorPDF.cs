using System;

namespace PWSWebApi.Domains
{
    public class ReportParamMonitorPDF
    {
        public int ReportRefID;

        public int ReportBookID;

        public string ReportName;

        public int RptTemplateNameID;

        public string RptTemplateName;

        public DateTime StartDate;

        public DateTime EndDate;

        public string DelimParams;

        public string LogoID;

        public DateTime ReconCompleteTime;

        public DateTime DataMissingStartTime;

        public string FileSavedName;

        public DateTime FileSavedDate;

        public DateTime CreateDateTime;

        public int UserID;

        public int UserPrefTypeID;
    }
}
