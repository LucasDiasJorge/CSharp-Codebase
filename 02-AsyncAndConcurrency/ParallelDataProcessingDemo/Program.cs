using Microsoft.Extensions.Logging;
using ParallelDataProcessingDemo.Demo;

// Logger de console em linha unica: cada medicao vira uma linha estruturada, e a tabela
// comparativa e impressa depois, ja com os ganhos calculados.
using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(LogLevel.Information);
    builder.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss.fff ";
    });
});

Console.WriteLine("ParallelDataProcessingDemo - sequencial, Task.WhenAll e Parallel.ForEachAsync");

ParallelDemoRunner runner = new ParallelDemoRunner(loggerFactory.CreateLogger<ParallelDemoRunner>());
await runner.RunAllAsync();

Console.WriteLine();
Console.WriteLine("Fim dos cenarios.");
