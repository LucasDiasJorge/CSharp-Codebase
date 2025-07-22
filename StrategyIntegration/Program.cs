using StrategyIntegration.IntegrationClasses;

namespace StrategyIntegration;

class Program
{
    static void Main(string[] args)
    {
        IntegrationStrategy integrationStrategy = new IntegrationStrategy(new FirstIntegration());
        integrationStrategy.ExecuteIntegration(new Dictionary<string, object> { { "key1", "value1" } }, "Destination1");
        integrationStrategy.SetStrategy(new SecondIntegration());
        integrationStrategy.ExecuteIntegration(new Dictionary<string, object> { { "key2", "value2" } }, "Destination2");
    }
}
