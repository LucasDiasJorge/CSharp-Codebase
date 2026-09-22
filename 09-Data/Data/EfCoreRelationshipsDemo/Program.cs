using EfCoreRelationshipsDemo.Data;
using EfCoreRelationshipsDemo.Demo;
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

Console.WriteLine("EfCoreRelationshipsDemo - 1:1, 1:N, N:N, owned types, N+1 e tracking");

QueryCounter counter = new QueryCounter();
RelationshipsDemoRunner runner = new RelationshipsDemoRunner(counter, loggerFactory.CreateLogger<RelationshipsDemoRunner>());

await runner.RunAllAsync();

await Task.Delay(200);
Console.WriteLine();
Console.WriteLine("Fim dos cenarios.");
