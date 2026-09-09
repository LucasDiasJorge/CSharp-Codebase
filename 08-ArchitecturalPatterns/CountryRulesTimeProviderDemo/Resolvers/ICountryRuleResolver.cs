using CountryRulesTimeProviderDemo.Rules;

namespace CountryRulesTimeProviderDemo.Resolvers;

public interface ICountryRuleResolver
{
    string RuleKey { get; }

    int Priority { get; }

    bool AppliesTo(string ruleName);

    CountryRules Resolve();
}
