namespace CountryRulesTimeProviderDemo.Rules;

public sealed class BrazilRules : CountryRules
{
    public BrazilRules(TimeProvider timeProvider)
        : base(
            "Brazil",
            "E. South America Standard Time",
            "America/Sao_Paulo",
            timeProvider)
    {
    }
}
