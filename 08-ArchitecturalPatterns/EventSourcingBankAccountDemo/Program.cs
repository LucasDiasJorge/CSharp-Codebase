using EventSourcingBankAccountDemo.Demo;
using EventSourcingBankAccountDemo.Store;
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

Console.WriteLine("EventSourcingBankAccountDemo - estado derivado de eventos, concorrencia otimista e snapshots");

EventStore store = new EventStore(loggerFactory.CreateLogger<EventStore>());
EventSourcingDemoRunner runner = new EventSourcingDemoRunner(store, loggerFactory.CreateLogger<EventSourcingDemoRunner>());

runner.RunAll();

// Pausa para a fila do provider de console esvaziar antes da linha final.
await Task.Delay(200);

Console.WriteLine();
Console.WriteLine("Fim dos cenarios.");
