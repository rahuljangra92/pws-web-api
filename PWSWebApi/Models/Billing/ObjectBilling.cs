using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PWSWebApi.Models.Billing
{

    public class ObjectBilling
    {
        public int UserID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string UserGroupIDs { get; set; }
        public string RptPortIDs { get; set; }
        public string FeeStructureAssignmentIDsBYDates { get; set; }
        public string Category { get; set; }
        public string FeeStructureAssignmentIDs { get; set; }
    }
}