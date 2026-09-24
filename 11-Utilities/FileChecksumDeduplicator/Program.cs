using FileChecksumDeduplicator.Demo;
using Microsoft.Extensions.Logging;

using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss.fff ";
    });
});

DeduplicatorDemoRunner runner = new DeduplicatorDemoRunner(loggerFactory.CreateLogger<DeduplicatorDemoRunner>());

runner.RunAll();

await Task.Delay(200);

Console.WriteLine();
Console.WriteLine("Fim dos cenarios.");
