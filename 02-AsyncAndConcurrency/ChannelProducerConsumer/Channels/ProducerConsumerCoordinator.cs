using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;
using ChannelProducerConsumer.Models;
using Microsoft.Extensions.Logging;

namespace ChannelProducerConsumer.Channels;

/// <summary>
/// Liga produtores e consumidores sobre um canal e cuida da conclusao ordenada.
///
/// A sequencia importa: os consumidores comecam primeiro e ficam esperando, os produtores
/// terminam, so entao o writer e completado, e por fim os consumidores drenam o que sobrou.
/// Completar o writer antes de todos os produtores terminarem faz as escritas seguintes
/// lancarem <see cref="ChannelClosedException"/>.
/// </summary>
public sealed class ProducerConsumerCoordinator
{
    private readonly ILogger<ProducerConsumerCoordinator> logger;
    private readonly ReadingProducer producer;
    private readonly ReadingConsumer consumer;

    public ProducerConsumerCoordinator(
        ILogger<ProducerConsumerCoordinator> logger,
        ReadingProducer producer,
        ReadingConsumer consumer)
    {
        this.logger = logger;
        this.producer = producer;
        this.consumer = consumer;
    }

    public async Task<ChannelRunSummary> RunAsync(
        string scenarioName,
        Channel<TelemetryReading> channel,
        int producerCount,
        int consumerCount,
        int readingsPerProducer,
        CancellationToken cancellationToken)
    {
        ConcurrentQueue<TelemetryReading> consumptionLog = new ConcurrentQueue<TelemetryReading>();
        Stopwatch stopwatch = Stopwatch.StartNew();

        // 1. Consumidores primeiro: ReadAllAsync espera sem custo enquanto o canal esta vazio.
        List<Task<int>> consumerTasks = new List<Task<int>>(consumerCount);
        for (int consumerId = 1; consumerId <= consumerCount; consumerId++)
        {
            consumerTasks.Add(consumer.ConsumeAsync(channel.Reader, consumerId, consumptionLog, cancellationToken));
        }

        // 2. Produtores em paralelo.
        List<Task<ProducerReport>> producerTasks = new List<Task<ProducerReport>>(producerCount);
        for (int producerIndex = 1; producerIndex <= producerCount; producerIndex++)
        {
            producerTasks.Add(producer.ProduceAsync(channel, producerIndex, readingsPerProducer, cancellationToken));
        }

        ProducerReport[] producerReports = await Task.WhenAll(producerTasks);

        // 3. So agora o canal pode ser fechado para escrita.
        channel.Writer.Complete();
        logger.LogInformation("writer completado apos {Count} produtores terminarem", producerCount);

        // 4. Os consumidores encerram sozinhos ao drenar o que restou.
        int[] consumedCounts = await Task.WhenAll(consumerTasks);

        // 5. Completion so conclui quando o canal esta fechado e vazio.
        await channel.Reader.Completion;
        stopwatch.Stop();

        return BuildSummary(scenarioName, producerReports, consumedCounts, consumptionLog, stopwatch.Elapsed);
    }

    private static ChannelRunSummary BuildSummary(
        string scenarioName,
        IReadOnlyList<ProducerReport> producerReports,
        IReadOnlyList<int> consumedCounts,
        ConcurrentQueue<TelemetryReading> consumptionLog,
        TimeSpan totalElapsed)
    {
        int producedCount = 0;
        TimeSpan blockedTime = TimeSpan.Zero;
        int maxQueueDepth = 0;

        foreach (ProducerReport report in producerReports)
        {
            producedCount += report.WrittenCount;
            blockedTime += report.BlockedTime;
            maxQueueDepth = Math.Max(maxQueueDepth, report.MaxQueueDepth);
        }

        int consumedCount = 0;
        foreach (int count in consumedCounts)
        {
            consumedCount += count;
        }

        return new ChannelRunSummary(
            scenarioName,
            producedCount,
            consumedCount,
            blockedTime,
            totalElapsed,
            maxQueueDepth,
            IsOrderPreservedPerSensor(consumptionLog));
    }

    /// <summary>
    /// O canal e FIFO, mas essa garantia so chega ate a saida do reader: com varios
    /// consumidores em paralelo, dois itens do mesmo sensor podem ser concluidos fora de ordem.
    /// Por isso a verificacao e feita por sensor, sobre a ordem real de consumo.
    /// </summary>
    private static bool IsOrderPreservedPerSensor(ConcurrentQueue<TelemetryReading> consumptionLog)
    {
        Dictionary<string, int> lastSequenceBySensor = new Dictionary<string, int>();

        foreach (TelemetryReading reading in consumptionLog)
        {
            if (lastSequenceBySensor.TryGetValue(reading.SensorId, out int lastSequence)
                && reading.SequenceNumber < lastSequence)
            {
                return false;
            }

            lastSequenceBySensor[reading.SensorId] = reading.SequenceNumber;
        }

        return true;
    }
}
