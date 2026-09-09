using System;
using System.Collections.Generic;
using System.Linq;
using CountryRulesTimeProviderDemo.Contracts;
using CountryRulesTimeProviderDemo.Resolvers;
using CountryRulesTimeProviderDemo.Rules;
using Microsoft.Extensions.Logging;

namespace CountryRulesTimeProviderDemo.Services;

public sealed class CountryTimeService : ICountryTimeService
{
    private readonly IReadOnlyList<ICountryRuleResolver> resolvers;
    private readonly UnsupportedCountryRuleResolver fallbackResolver;
    private readonly ILogger<CountryTimeService> logger;

    public CountryTimeService(
        IEnumerable<ICountryRuleResolver> resolvers,
        UnsupportedCountryRuleResolver fallbackResolver,
        ILogger<CountryTimeService> logger)
    {
        this.resolvers = resolvers
            .OrderBy(resolver => resolver.Priority)
            .ToList();
        this.fallbackResolver = fallbackResolver;
        this.logger = logger;
    }

    public bool TryGetNow(
        string ruleName,
        out CountryTimeResponse? response,
        out CountryTimeErrorResponse? errorResponse)
    {
        response = null;
        errorResponse = null;

        ICountryRuleResolver? selectedResolver = SelectResolver(ruleName);
        if (selectedResolver is null)
        {
            IReadOnlyCollection<string> supportedRules = GetSupportedRules();
            errorResponse = fallbackResolver.CreateError(ruleName, supportedRules);

            logger.LogWarning(
                "Rule nao suportada recebida: {Rule}. Regras suportadas: {Rules}",
                ruleName,
                string.Join(", ", supportedRules));

            return false;
        }

        CountryRules countryRules = selectedResolver.Resolve();
        DateTimeOffset timeProviderNow = countryRules.GetCountryNow();

        response = new CountryTimeResponse(
            countryRules.CountryName,
            selectedResolver.RuleKey,
            countryRules.TimeZoneId,
            countryRules.InitializedAt,
            timeProviderNow);

        logger.LogInformation(
            "Rule {RuleKey} resolveu horario para {CountryName} em {TimeZoneId}",
            selectedResolver.RuleKey,
            countryRules.CountryName,
            countryRules.TimeZoneId);

        return true;
    }

    public IReadOnlyCollection<string> GetSupportedRules()
    {
        string[] supportedRules = resolvers
            .OrderBy(resolver => resolver.Priority)
            .Select(resolver => resolver.RuleKey)
            .ToArray();

        return supportedRules;
    }

    private ICountryRuleResolver? SelectResolver(string ruleName)
    {
        ICountryRuleResolver? resolver = resolvers.FirstOrDefault(candidate => candidate.AppliesTo(ruleName));
        return resolver;
    }
}
