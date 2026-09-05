using ChannelProducerConsumer.Demo;
using Microsoft.Extensions.Logging;

// Logger de console em linha unica com timestamp: o horario mostra quando cada produtor
// destravou e ajuda a enxergar o backpressure acontecendo.
using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(LogLevel.Information);
    builder.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss.fff ";
    });
});

Console.WriteLine("ChannelProducerConsumer - produtores e consumidores com System.Threading.Channels");

ChannelDemoRunner runner = new ChannelDemoRunner(loggerFactory);
await runner.RunAllAsync();

Console.WriteLine();
Console.WriteLine("Fim dos cenarios.");
