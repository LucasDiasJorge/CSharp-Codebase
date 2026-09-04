using CancellationTokenPipeline.Demo;
using Microsoft.Extensions.Logging;

// Logger de console em linha unica com timestamp: o horario e o que torna visivel
// em que ponto do pipeline cada cancelamento chegou.
using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(LogLevel.Information);
    builder.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss.fff ";
    });
});

Console.WriteLine("CancellationTokenPipeline - propagacao de cancelamento entre etapas assincronas");

PipelineDemoRunner runner = new PipelineDemoRunner(loggerFactory);
await runner.RunAllAsync();

Console.WriteLine();
Console.WriteLine("Fim dos cenarios.");
