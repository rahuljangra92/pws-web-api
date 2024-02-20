using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PWSWebApi.ViewModel.Models.Administration
{
    public class ReportModel
    {
        public string RowOrder { get; set; }
        public int RowOrderNum { get; set; }
        public string ParentRowOrder { get; set; }

        [JsonProperty("Level")]
        public string Levels { get; set; }

        public int LevelsNum { get; set; }
        public string AssetClass { get; set; }
        public string ModelTitle { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string AttribTypeID { get; set; }

        public string AttrbType { get; set; }
        public string AttribNameID { get; set; }
        public string AttribOrder { get; set; }
        public string UserGroupID { get; set; }

        public string UniqueRowID { get; set; }

        public string ParentUniqueID { get; set; }

        public List<int> parentLevels { get; set; }

        public bool CanMoveUp { get; set; }

        public bool CanMoveDown { get; set; }

        public string SuperParentRowOrder { get; set; }

    }
}