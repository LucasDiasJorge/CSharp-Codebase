using System.Collections.Concurrent;
using System.Threading.Channels;
using ChannelProducerConsumer.Models;
using Microsoft.Extensions.Logging;

namespace ChannelProducerConsumer.Channels;

/// <summary>
/// Consumidor que drena o canal com <see cref="ChannelReader{T}.ReadAllAsync"/>.
///
/// O laco termina sozinho quando o writer e completado e a fila esvazia; nao ha
/// sentinela, flag de parada nem verificacao manual de fim de fila.
/// </summary>
public sealed class ReadingConsumer
{
    private readonly ILogger<ReadingConsumer> logger;
    private readonly TimeSpan processingTime;

    public ReadingConsumer(ILogger<ReadingConsumer> logger, TimeSpan processingTime)
    {
        this.logger = logger;
        this.processingTime = processingTime;
    }

    /// <param name="consumptionLog">
    /// Registro compartilhado na ordem real de consumo. Varios consumidores escrevem nele,
    /// entao precisa ser uma colecao concorrente.
    /// </param>
    public async Task<int> ConsumeAsync(
        ChannelReader<TelemetryReading> reader,
        int consumerId,
        ConcurrentQueue<TelemetryReading> consumptionLog,
        CancellationToken cancellationToken)
    {
        int consumedCount = 0;

        await foreach (TelemetryReading reading in reader.ReadAllAsync(cancellationToken))
        {
            await Task.Delay(processingTime, cancellationToken);
            consumptionLog.Enqueue(reading);
            consumedCount++;
        }

        logger.LogInformation("[consumidor {ConsumerId}] processou {Count} leituras", consumerId, consumedCount);
        return consumedCount;
    }
}
