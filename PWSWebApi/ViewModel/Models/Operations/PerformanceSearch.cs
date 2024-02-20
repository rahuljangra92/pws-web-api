using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PWSWebApi.ViewModel.Models.Operations
{
    public class PerformanceSearch
    {
        public decimal BMV { get; set; }
        public decimal BegFlow { get; set; }
        public string CurrCode { get; set; }
        public decimal EMV { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal GIndex { get; set; }
        public decimal GrossReturn { get; set; }
        public decimal NIndex { get; set; }
        public decimal NetFlow { get; set; }
        public decimal NetReturn { get; set; }
        public int PerfAttribID { get; set; }
        public int RowOrder { get; set; }
        public string Title { get; set; }

    }

    public class PerfSearchSortOrder
    {
        public string SortingOrder { get; set; }
        public string FieldName { get; set; }
    }
}