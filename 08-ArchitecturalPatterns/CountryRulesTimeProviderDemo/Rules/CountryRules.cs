using System;

namespace CountryRulesTimeProviderDemo.Rules;

public abstract class CountryRules
{
    private readonly TimeProvider timeProvider;
    private readonly TimeZoneInfo timeZoneInfo;

    public string CountryName { get; }

    public string TimeZoneId => timeZoneInfo.Id;

    public DateTimeOffset InitializedAt { get; }

    protected CountryRules(
        string countryName,
        string windowsTimeZoneId,
        string ianaTimeZoneId,
        TimeProvider timeProvider)
    {
        CountryName = countryName;
        this.timeProvider = timeProvider;
        timeZoneInfo = ResolveTimeZone(windowsTimeZoneId, ianaTimeZoneId);
        InitializedAt = GetCountryNow();
    }

    public DateTimeOffset GetCountryNow()
    {
        DateTimeOffset utcNow = timeProvider.GetUtcNow();
        DateTimeOffset localNow = TimeZoneInfo.ConvertTime(utcNow, timeZoneInfo);
        return localNow;
    }

    public void PrintTimeProviderNow()
    {
        DateTimeOffset now = GetCountryNow();
        Console.WriteLine($"{CountryName}: InitializedAt={InitializedAt:yyyy-MM-dd HH:mm:ss zzz} | TimeProviderNow={now:yyyy-MM-dd HH:mm:ss zzz}");
    }

    private static TimeZoneInfo ResolveTimeZone(string windowsTimeZoneId, string ianaTimeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(windowsTimeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(ianaTimeZoneId);
        }
    }
}
