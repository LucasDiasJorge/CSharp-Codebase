using Microsoft.Extensions.Logging;
using ObserverStockAlertsDemo.Demo;

using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(LogLevel.Information);
    builder.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss.fff ";
    });
});

Console.WriteLine("ObserverStockAlertsDemo - padrao Observer a mao, com event do C#, e suas armadilhas");

ObserverDemoRunner runner = new ObserverDemoRunner(loggerFactory);
runner.RunAll();

// Pausa para a fila do provider de console esvaziar antes da linha final.
await Task.Delay(200);

Console.WriteLine();
Console.WriteLine("Fim dos cenarios.");
