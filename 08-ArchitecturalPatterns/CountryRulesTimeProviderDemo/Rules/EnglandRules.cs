namespace CountryRulesTimeProviderDemo.Rules;

public sealed class EnglandRules : CountryRules
{
    public EnglandRules(TimeProvider timeProvider)
        : base(
            "England",
            "GMT Standard Time",
            "Europe/London",
            timeProvider)
    {
    }
}
