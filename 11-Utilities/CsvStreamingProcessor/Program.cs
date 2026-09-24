using CsvStreamingProcessor.Demo;
using Microsoft.Extensions.Logging;

using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss.fff ";
    });
});

CsvDemoRunner runner = new CsvDemoRunner(loggerFactory.CreateLogger<CsvDemoRunner>());

runner.RunAll();

await Task.Delay(200);

Console.WriteLine();
Console.WriteLine("Fim dos cenarios.");
