namespace PWSWebApi.Domains
{
    public class Usp_GridColumnsResult
    {
        public int GridColumnsID { get; set; }
        public string GridName { get; set; }
        public string ColumnTitle { get; set; }
        public string MinLen { get; set; }
        public string MaxLen { get; set; }
        public string Datatype { get; set; }
        public string Decimals { get; set; }
        public string Req { get; set; }
        public string Formula { get; set; }
        public string LookUpID { get; set; }
        public string LookUpIDDep { get; set; }
    }
}
