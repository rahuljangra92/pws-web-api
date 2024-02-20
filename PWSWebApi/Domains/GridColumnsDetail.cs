namespace PWSWebApi.Domains
{
    public class GridColumnsDetail
    {
        public int GridColumnsDetailID { get; set; }
        public int? GridColumnsID { get; set; }
        public int? ColumnOrder { get; set; }
        public string ColumnTitle { get; set; }
        public int? MinLen { get; set; }
        public int? MaxLen { get; set; }
        public string Datatype { get; set; }
        public int? Req { get; set; }
        public string Formula { get; set; }
        public int? LookUpID { get; set; }
        public int? Decimals { get; set; }
        public string LookUpIDDep { get; set; }
    }
}
