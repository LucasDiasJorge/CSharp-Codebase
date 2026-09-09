using System;
using CountryRulesTimeProviderDemo.Rules;

namespace CountryRulesTimeProviderDemo.Resolvers;

public sealed class KoreaRuleResolver : ICountryRuleResolver
{
    private readonly KoreaRules countryRules;

    public KoreaRuleResolver(KoreaRules countryRules)
    {
        this.countryRules = countryRules;
    }

    public string RuleKey => "korea";

    public int Priority => 100;

    public bool AppliesTo(string ruleName)
    {
        return string.Equals(ruleName, RuleKey, StringComparison.OrdinalIgnoreCase)
            || string.Equals(ruleName, "kr", StringComparison.OrdinalIgnoreCase);
    }

    public CountryRules Resolve()
    {
        return countryRules;
    }
}
