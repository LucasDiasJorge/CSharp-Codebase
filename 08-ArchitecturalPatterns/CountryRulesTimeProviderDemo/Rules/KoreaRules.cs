namespace CountryRulesTimeProviderDemo.Rules;

public sealed class KoreaRules : CountryRules
{
    public KoreaRules(TimeProvider timeProvider)
        : base(
            "Korea",
            "Korea Standard Time",
            "Asia/Seoul",
            timeProvider)
    {
    }
}
