using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;

namespace PWSWebApi.Domains.DBContext
{
    public class CommonSPs
    {
        private readonly PWSRecDataContext _pWSRecDataContext;
        public CommonSPs(PWSRecDataContext pWSRecDataContext)
        {
            _pWSRecDataContext = pWSRecDataContext;
        }

        public async Task<int> CallUspLoadTermsAndConditionsInsAsync(string identifier, string identifierType, string loadSourceID, string dPID, string userID, string cUSIP, string iSIN, string ticker, string rIC, string sEDOL, string valoren, string princCCYID, string assetType, string assetTypeDescription, string assetSubType, string assetSubTypeDescription, string dayCountCode, string dayCountCodeDescription, string issueDate, string issuePrice, string accrualDate, string maturityDate, string lastCouponDate, string firstCouponDate, string couponRate, string couponType, string couponTypeDescription, string couponFrequency, string couponFrequencyDescription, string couponCurrency, string originalIssueDiscountFlag, string denominationIncrement, string nextCallDate, string nextCallPrice, string iSOCountryCode, string stateCode, string moodysRating, string industryDescription, string industrySectorDescription, string tRBCEconomicSectorCode, string tRBCEconomicSectorCodeDescription, string tRBCIndustryCode, string tRBCIndustryCodeDescription, string hybridFlag, string parValue, string securityDescription, string currencyCode, string currencyCodeDescription, string dividendCurrency, string dividendCurrencyDescription, string investmentType, string exchangeCode, string exchangeDescription, string domicile, string issuerName, string issuerOrgID, string eTFType, string convertibleFlag, string floatIndexType, string couponResetFrequency, string couponResetFrequencyDescription, string couponResetRuleCode, string couponResetRuleCodeDescription, string previousCouponDate, string sharesAmountType, string sharesAmount, string refinitivClassificationScheme, string refinitivClassificationSchemeDescription, string geographicalFocus, string lipperGlobalClassification, string fundManagerBenchmark, string userDefinedIdentifier)
        {
            var result = await _pWSRecDataContext.Database.ExecuteSqlInterpolatedAsync($@"
            EXEC tr.usp_Load_TermsAndConditions_INS
            {identifier}, {identifierType}, {loadSourceID}, {dPID}, {userID}, {cUSIP}, {iSIN}, {ticker}, {rIC}, {sEDOL}, {valoren}, {princCCYID}, {assetType}, {assetTypeDescription}, {assetSubType}, {assetSubTypeDescription}, {dayCountCode}, {dayCountCodeDescription}, {issueDate}, {issuePrice}, {accrualDate}, {maturityDate}, {lastCouponDate}, {firstCouponDate}, {couponRate}, {couponType}, {couponTypeDescription}, {couponFrequency}, {couponFrequencyDescription}, {couponCurrency}, {originalIssueDiscountFlag}, {denominationIncrement}, {nextCallDate}, {nextCallPrice}, {iSOCountryCode}, {stateCode}, {moodysRating}, {industryDescription}, {industrySectorDescription}, {tRBCEconomicSectorCode}, {tRBCEconomicSectorCodeDescription}, {tRBCIndustryCode}, {tRBCIndustryCodeDescription}, {hybridFlag}, {parValue}, {securityDescription}, {currencyCode}, {currencyCodeDescription}, {dividendCurrency}, {dividendCurrencyDescription}, {investmentType}, {exchangeCode}, {exchangeDescription}, {domicile}, {issuerName}, {issuerOrgID}, {eTFType}, {convertibleFlag}, {floatIndexType}, {couponResetFrequency}, {couponResetFrequencyDescription}, {couponResetRuleCode}, {couponResetRuleCodeDescription}, {previousCouponDate}, {sharesAmountType}, {sharesAmount}, {refinitivClassificationScheme}, {refinitivClassificationSchemeDescription}, {geographicalFocus}, {lipperGlobalClassification}, {fundManagerBenchmark}, {userDefinedIdentifier}");

            return result;
        }

        public async Task<List<Intermediate_GetIdentifier>> CallUspPriceGetIdentifiersAsync()
        {
            var result = await _pWSRecDataContext.Intermediate_GetIdentifiers
                .FromSqlInterpolated($"EXEC tr.usp_Price_GetIdentifiers")
                .ToListAsync();

            return result;
        }

        public Task<List<Intermediate_GetIdentifier>> GetIdentifiersFromStoredProcedure()
        {
            return _pWSRecDataContext.Intermediate_GetIdentifiers
                .FromSqlInterpolated($"EXEC tr.usp_CorpAction_GetIdentifiers")
                .ToListAsync();
        }

    }
}
