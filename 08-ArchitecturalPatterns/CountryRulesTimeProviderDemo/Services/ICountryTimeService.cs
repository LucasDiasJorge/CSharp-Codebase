using System.Collections.Generic;
using CountryRulesTimeProviderDemo.Contracts;

namespace CountryRulesTimeProviderDemo.Services;

public interface ICountryTimeService
{
    bool TryGetNow(
        string ruleName,
        out CountryTimeResponse? response,
        out CountryTimeErrorResponse? errorResponse);

    IReadOnlyCollection<string> GetSupportedRules();
}
