using DatabaseMigrationsDemo.Demo;
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

Console.WriteLine("DatabaseMigrationsDemo - aplicar, reverter e evoluir schema com migrations");

MigrationsDemoRunner runner = new MigrationsDemoRunner(loggerFactory.CreateLogger<MigrationsDemoRunner>());
await runner.RunAllAsync();

await Task.Delay(200);
Console.WriteLine();
Console.WriteLine("Fim dos cenarios.");
