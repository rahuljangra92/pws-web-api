using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PWSWebApi.ViewModel.Models.Operations
{
    public class ReportModel
    {
        public string RowOrder { get; set; }
        public string ParentRowOrder { get; set; }
        public string Levels { get; set; }
        public string ModelTitle { get; set; }
        public string Description { get; set; }
        public string AttribTypeID { get; set; }
        public string AttribNameID { get; set; }
        public string AttribOrder { get; set; }
        public string UserGroupID { get; set; }
        public string UserUD { get; set; }

    }
}