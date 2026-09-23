using BinarySearchBoundariesDemo.Demo;
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

Console.WriteLine("BinarySearchBoundariesDemo - busca exata, lower bound e upper bound derivados do mesmo invariante");

BinarySearchDemoRunner runner = new BinarySearchDemoRunner(loggerFactory.CreateLogger<BinarySearchDemoRunner>());
runner.RunAll();

await Task.Delay(200);
Console.WriteLine();
Console.WriteLine("Fim dos cenarios.");
