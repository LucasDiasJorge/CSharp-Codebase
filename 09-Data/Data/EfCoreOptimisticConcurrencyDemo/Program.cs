using EfCoreOptimisticConcurrencyDemo.Demo;
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

Console.WriteLine("EfCoreOptimisticConcurrencyDemo - deteccao e resolucao de conflitos de atualizacao");

ConcurrencyDemoRunner runner = new ConcurrencyDemoRunner(loggerFactory.CreateLogger<ConcurrencyDemoRunner>());
await runner.RunAllAsync();

await Task.Delay(200);
Console.WriteLine();
Console.WriteLine("Fim dos cenarios.");
