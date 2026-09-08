using AsyncStreamsDemo.Demo;
using Microsoft.Extensions.Logging;

// Logger de console em linha unica com timestamp: e pelo horario que se ve o item chegando
// ao consumidor no mesmo instante em que a fonte o produz.
using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(LogLevel.Information);
    builder.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss.fff ";
    });
});

Console.WriteLine("AsyncStreamsDemo - IAsyncEnumerable, await foreach e cancelamento");

AsyncStreamsDemoRunner runner = new AsyncStreamsDemoRunner(loggerFactory);
await runner.RunAllAsync();

Console.WriteLine();
Console.WriteLine("Fim dos cenarios.");
