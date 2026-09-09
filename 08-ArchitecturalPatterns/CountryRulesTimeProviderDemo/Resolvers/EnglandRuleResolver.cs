using System;
using CountryRulesTimeProviderDemo.Rules;

namespace CountryRulesTimeProviderDemo.Resolvers;

public sealed class EnglandRuleResolver : ICountryRuleResolver
{
    private readonly EnglandRules countryRules;

    public EnglandRuleResolver(EnglandRules countryRules)
    {
        this.countryRules = countryRules;
    }

    public string RuleKey => "england";

    public int Priority => 100;

    public bool AppliesTo(string ruleName)
    {
        return string.Equals(ruleName, RuleKey, StringComparison.OrdinalIgnoreCase)
            || string.Equals(ruleName, "uk", StringComparison.OrdinalIgnoreCase);
    }

    public CountryRules Resolve()
    {
        return countryRules;
    }
}
