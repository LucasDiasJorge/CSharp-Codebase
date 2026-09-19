using CommandUndoRedoDemo.Demo;
using CommandUndoRedoDemo.Editor;
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

Console.WriteLine("CommandUndoRedoDemo - padrao Command com historico, undo e redo");

TextDocument document = new TextDocument();
CommandHistory history = new CommandHistory(loggerFactory.CreateLogger<CommandHistory>(), limit: 10);
EditorDemoRunner runner = new EditorDemoRunner(document, history, loggerFactory.CreateLogger<EditorDemoRunner>());

runner.RunAll();

// O provider de console do logging grava em fila propria; sem esta pausa a linha final
// sai no meio das mensagens dos cenarios.
await Task.Delay(200);

Console.WriteLine();
Console.WriteLine("Fim dos cenarios.");
