namespace PWSWebApi.Domains
{
    public class Load_EOD
    {
        public string IdentifierType { get; internal set; }
        public string Identifier { get; internal set; }
        public string Ticker { get; internal set; }
        public string RIC { get; internal set; }
        public string CurrencyCode { get; internal set; }
        public string BidPrice { get; internal set; }
        public string AskPrice { get; internal set; }
        public string MidPrice { get; internal set; }
        public string ClosePrice { get; internal set; }
        public string UniversalClosePrice { get; internal set; }
        public string TradeDate { get; internal set; }
        public string MaturityDate { get; internal set; }
        public string PutCallIndicator { get; internal set; }
        public string UserDefinedIdentifier { get; internal set; }
    }
}
