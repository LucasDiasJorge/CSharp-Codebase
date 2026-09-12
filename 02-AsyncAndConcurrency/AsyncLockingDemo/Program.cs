using AsyncLockingDemo.Demo;
using Microsoft.Extensions.Logging;

// Console em linha unica com timestamp: o horario de cada linha mostra quanto tempo o
// cenario bloqueante passou sem conseguir agendar nada na thread pool.
using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(LogLevel.Information);
    builder.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss.fff ";
    });
});

Console.WriteLine("AsyncLockingDemo - race condition e exclusao mutua compativel com async");
Console.WriteLine("A execucao leva cerca de 15 segundos; a demora do cenario 2 faz parte da licao.");
Console.WriteLine();

ContentionRunner contentionRunner = new ContentionRunner(loggerFactory.CreateLogger<ContentionRunner>());
ReleasePitfallDemo pitfallDemo = new ReleasePitfallDemo(loggerFactory.CreateLogger<ReleasePitfallDemo>());
AsyncLockingDemoRunner runner = new AsyncLockingDemoRunner(contentionRunner, pitfallDemo, loggerFactory.CreateLogger<AsyncLockingDemoRunner>());

using CancellationTokenSource cancellationSource = new CancellationTokenSource();
Console.CancelKeyPress += (sender, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationSource.Cancel();
};

await runner.RunAsync(cancellationSource.Token);

Console.WriteLine();
Console.WriteLine("Fim dos cenarios.");
