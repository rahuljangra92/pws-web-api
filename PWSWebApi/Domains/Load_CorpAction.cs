namespace PWSWebApi.Domains
{
    public class Load_CorpAction
    {
        public string Identifier { get; internal set; }
        public string IdentifierType { get; internal set; }
        public string CUSIP { get; internal set; }
        public string RIC { get; internal set; }
        public string Ticker { get; internal set; }
        public string ISIN { get; internal set; }
        public string CorporateActionsType { get; internal set; }
        public string CorporateActionsID { get; internal set; }
        public string CurrencyCode { get; internal set; }
        public string DividendRate { get; internal set; }
        public string DividendExDate { get; internal set; }
        public string DividendPayDate { get; internal set; }
        public string RecordDate { get; internal set; }
        public string DividendCurrency { get; internal set; }
        public string DividendCurrencyDescription { get; internal set; }
        public string DividendFrequency { get; internal set; }
        public string DividendFrequencyDescription { get; internal set; }
        public string DividendTaxMarker { get; internal set; }
        public string DividendTaxMarkerDescription { get; internal set; }
        public string DividendTypeMarker { get; internal set; }
        public string DividendTypeMarkerDescription { get; internal set; }
        public string UserDefinedIdentifier { get; internal set; }
    }
}
