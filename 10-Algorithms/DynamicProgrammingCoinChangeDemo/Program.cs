using DynamicProgrammingCoinChangeDemo.Demo;
using Microsoft.Extensions.Logging;

using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(LogLevel.Information);
    builder.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss.fff ";
    });
});

Console.WriteLine("DynamicProgrammingCoinChangeDemo - recursao, memoization e tabulation sobre a mesma recorrencia");

CoinChangeDemoRunner runner = new CoinChangeDemoRunner(loggerFactory.CreateLogger<CoinChangeDemoRunner>());
runner.RunAll();

await Task.Delay(200);
Console.WriteLine();
Console.WriteLine("Fim dos cenarios.");
