using LruCacheDataStructureDemo.Demo;
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

Console.WriteLine("LruCacheDataStructureDemo - dicionario + lista duplamente ligada para O(1) em leitura, escrita e remocao");

LruDemoRunner runner = new LruDemoRunner(loggerFactory.CreateLogger<LruDemoRunner>());
runner.RunAll();

await Task.Delay(200);
Console.WriteLine();
Console.WriteLine("Fim dos cenarios.");
