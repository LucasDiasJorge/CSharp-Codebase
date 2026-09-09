namespace CountryRulesTimeProviderDemo.Contracts;

public sealed record CountryTimeResponse(
    string Country,
    string Rule,
    string TimeZoneId,
    DateTimeOffset InitializedAt,
    DateTimeOffset TimeProviderNow);
