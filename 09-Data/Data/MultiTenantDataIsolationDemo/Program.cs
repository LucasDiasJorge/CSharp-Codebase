using Microsoft.Extensions.Logging;
using MultiTenantDataIsolationDemo.Demo;

using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(LogLevel.Information);
    builder.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss.fff ";
    });
});

Console.WriteLine("MultiTenantDataIsolationDemo - query filters, indice composto e as formas de furar o isolamento");

MultiTenantDemoRunner runner = new MultiTenantDemoRunner(loggerFactory.CreateLogger<MultiTenantDemoRunner>());
await runner.RunAllAsync();

await Task.Delay(200);
Console.WriteLine();
Console.WriteLine("Fim dos cenarios.");
