using System;
using System.Collections.Generic;
using CountryRulesTimeProviderDemo.Contracts;

namespace CountryRulesTimeProviderDemo.Resolvers;

public sealed class UnsupportedCountryRuleResolver
{
    public CountryTimeErrorResponse CreateError(
        string requestedRule,
        IReadOnlyCollection<string> supportedRules)
    {
        string safeRequestedRule = string.IsNullOrWhiteSpace(requestedRule)
            ? "(empty)"
            : requestedRule;

        return new CountryTimeErrorResponse(
            safeRequestedRule,
            "Rule nao suportada. Use uma das rules disponiveis.",
            supportedRules);
    }
}
