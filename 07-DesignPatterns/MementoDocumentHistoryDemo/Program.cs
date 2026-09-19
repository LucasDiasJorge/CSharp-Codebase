using MementoDocumentHistoryDemo.Demo;
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

Console.WriteLine("MementoDocumentHistoryDemo - padrao Memento: salvar e restaurar sem expor o estado");

MementoDemoRunner runner = new MementoDemoRunner(loggerFactory);
runner.RunAll();

// Pausa para a fila do provider de console esvaziar antes da linha final.
await Task.Delay(200);

Console.WriteLine();
Console.WriteLine("Fim dos cenarios.");
