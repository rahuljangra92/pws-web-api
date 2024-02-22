namespace PWSWebApi.Domains
{
    public class Load_Index
    {
        public string Symbol { get; internal set; }
        public string SymbolName { get; internal set; }
        public string IndexDate { get; internal set; }
        public string DataType { get; internal set; }
        public string DataTypeName { get; internal set; }
        public string IndexValue { get; internal set; }
        public int SecID { get; internal set; }
    }
}
