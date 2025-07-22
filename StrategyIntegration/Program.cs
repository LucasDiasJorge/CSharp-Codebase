using StrategyIntegration.IntegrationClasses;

namespace StrategyIntegration;

class Program
{
    static void Main(string[] args)
    {
        IntegrationStrategy integrationStrategy = new IntegrationStrategy(new FirstIntegration());
        PrintResponse(integrationStrategy.ExecuteIntegration(new Dictionary<string, object> { { "key1", "value1" } }, "Destination1"));
        integrationStrategy.SetStrategy(new SecondIntegration());
        PrintResponse(integrationStrategy.ExecuteIntegration(new Dictionary<string, object> { { "key2", "value2" } }, "Destination2"));
    }

    static void PrintResponse(Response response)
    {
        Console.WriteLine($"Response Id: {response.Id}, Message: {response.Message}, At: {response.Timestamp}");
    }
}
