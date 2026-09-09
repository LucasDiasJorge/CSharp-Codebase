using System.Collections.Generic;

namespace CountryRulesTimeProviderDemo.Contracts;

public sealed record CountryTimeErrorResponse(
    string RequestedRule,
    string Message,
    IReadOnlyCollection<string> SupportedRules);
