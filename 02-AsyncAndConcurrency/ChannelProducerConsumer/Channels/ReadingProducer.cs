using System.Diagnostics;
using System.Threading.Channels;
using ChannelProducerConsumer.Models;
using Microsoft.Extensions.Logging;

namespace ChannelProducerConsumer.Channels;

/// <summary>
/// Produtor de leituras. O ponto observavel aqui e o tempo gasto dentro de
/// <see cref="ChannelWriter{T}.WriteAsync"/>: em um canal bounded cheio essa chamada
/// nao retorna ate abrir vaga, e e assim que o backpressure chega ao produtor.
/// </summary>
public sealed class ReadingProducer
{
    private const int SequenceBlockPerProducer = 1000;

    private readonly ILogger<ReadingProducer> logger;
    private readonly TimeSpan intervalBetweenReadings;

    public ReadingProducer(ILogger<ReadingProducer> logger, TimeSpan intervalBetweenReadings)
    {
        this.logger = logger;
        this.intervalBetweenReadings = intervalBetweenReadings;
    }

    /// <summary>
    /// Recebe o <see cref="Channel{T}"/> inteiro, e nao apenas o writer, para poder consultar
    /// <see cref="ChannelReader{T}.Count"/> e mostrar a fila crescendo (ou nao) durante a escrita.
    /// Em codigo de producao o produtor normalmente conhece so o <see cref="ChannelWriter{T}"/>.
    /// </summary>
    public async Task<ProducerReport> ProduceAsync(
        Channel<TelemetryReading> channel,
        int producerIndex,
        int readingCount,
        CancellationToken cancellationToken)
    {
        string sensorId = $"sensor-{producerIndex}";
        Stopwatch blockedStopwatch = new Stopwatch();
        int writtenCount = 0;
        int maxQueueDepth = 0;

        for (int index = 1; index <= readingCount; index++)
        {
            // Faixa propria de numeros por produtor: permite conferir a ordem relativa
            // de cada sensor sem precisar de um contador compartilhado.
            int sequenceNumber = (producerIndex * SequenceBlockPerProducer) + index;
            TelemetryReading reading = new TelemetryReading(sequenceNumber, sensorId, 20.0 + index);

            blockedStopwatch.Start();
            await channel.Writer.WriteAsync(reading, cancellationToken);
            blockedStopwatch.Stop();

            writtenCount++;
            maxQueueDepth = Math.Max(maxQueueDepth, ReadQueueDepth(channel.Reader));

            await Task.Delay(intervalBetweenReadings, cancellationToken);
        }

        logger.LogInformation(
            "[{SensorId}] escreveu {Count} leituras; bloqueado por {Blocked}ms; fila maxima vista {Depth}",
            sensorId,
            writtenCount,
            blockedStopwatch.Elapsed.TotalMilliseconds,
            maxQueueDepth);

        return new ProducerReport(writtenCount, blockedStopwatch.Elapsed, maxQueueDepth);
    }

    private static int ReadQueueDepth(ChannelReader<TelemetryReading> reader)
    {
        // Nem todo canal sabe se contar; canais criados por CreateBounded/CreateUnbounded sabem.
        return reader.CanCount ? reader.Count : 0;
    }
}
