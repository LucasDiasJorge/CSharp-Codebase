using System;
using CountryRulesTimeProviderDemo.Rules;

namespace CountryRulesTimeProviderDemo.Resolvers;

public sealed class BrazilRuleResolver : ICountryRuleResolver
{
    private readonly BrazilRules countryRules;

    public BrazilRuleResolver(BrazilRules countryRules)
    {
        this.countryRules = countryRules;
    }

    public string RuleKey => "brazil";

    public int Priority => 100;

    public bool AppliesTo(string ruleName)
    {
        return string.Equals(ruleName, RuleKey, StringComparison.OrdinalIgnoreCase)
            || string.Equals(ruleName, "br", StringComparison.OrdinalIgnoreCase);
    }

    public CountryRules Resolve()
    {
        return countryRules;
    }
}
