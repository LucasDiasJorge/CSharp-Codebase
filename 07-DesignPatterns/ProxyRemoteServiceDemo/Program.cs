using Microsoft.Extensions.Logging;
using ProxyRemoteServiceDemo.Demo;

using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(LogLevel.Information);
    builder.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss.fff ";
    });
});

Console.WriteLine("ProxyRemoteServiceDemo - Proxy virtual, de protecao e de telemetria sobre o mesmo contrato");

ProxyDemoRunner runner = new ProxyDemoRunner(loggerFactory);
await runner.RunAllAsync();

// Pausa para a fila do provider de console esvaziar antes da linha final.
await Task.Delay(200);

Console.WriteLine();
Console.WriteLine("Fim dos cenarios.");
