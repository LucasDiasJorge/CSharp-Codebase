using System.Threading.Channels;
using ChannelProducerConsumer.Channels;
using ChannelProducerConsumer.Models;
using Microsoft.Extensions.Logging;

namespace ChannelProducerConsumer.Demo;

/// <summary>
/// Executa os cenarios em sequencia, contrastando canal bounded, canal unbounded,
/// descarte por politica e o efeito do numero de consumidores sobre a ordem.
/// </summary>
public sealed class ChannelDemoRunner
{
    private const int BoundedCapacity = 3;

    private static readonly TimeSpan FastProducerInterval = TimeSpan.FromMilliseconds(20);
    private static readonly TimeSpan SlowConsumerTime = TimeSpan.FromMilliseconds(150);

    /// <summary>
    /// O provider de console do <c>Microsoft.Extensions.Logging</c> grava em uma fila processada
    /// por thread propria. Sem esta pausa, o resumo escrito direto no <see cref="Console"/>
    /// apareceria antes dos logs do cenario que ele resume.
    /// </summary>
    private static readonly TimeSpan LogFlushDelay = TimeSpan.FromMilliseconds(100);

    private readonly ILoggerFactory loggerFactory;

    public ChannelDemoRunner(ILoggerFactory loggerFactory)
    {
        this.loggerFactory = loggerFactory;
    }

    public async Task RunAllAsync()
    {
        await RunBoundedBackpressureAsync();
        await RunUnboundedGrowthAsync();
        await RunDropOldestAsync();
        await RunOrderedCompletionAsync();
        await RunConsumerCountEffectOnOrderAsync();
    }

    /// <summary>
    /// Cenario 1: canal bounded com produtor rapido e consumidor lento. A fila nunca passa
    /// da capacidade e o excedente vira espera do produtor — isso e backpressure.
    /// </summary>
    private async Task RunBoundedBackpressureAsync()
    {
        PrintHeader($"1. Canal bounded (capacidade {BoundedCapacity}) com backpressure");

        Channel<TelemetryReading> channel = Channel.CreateBounded<TelemetryReading>(
            new BoundedChannelOptions(BoundedCapacity)
            {
                // Wait e o padrao: WriteAsync so retorna quando abrir vaga.
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            });

        ProducerConsumerCoordinator coordinator = CreateCoordinator(FastProducerInterval, SlowConsumerTime);
        ChannelRunSummary summary = await coordinator.RunAsync(
            "bounded/Wait",
            channel,
            producerCount: 1,
            consumerCount: 1,
            readingsPerProducer: 10,
            CancellationToken.None);

        await PrintResultAsync(summary);
    }

    /// <summary>
    /// Cenario 2: mesmos tempos, canal unbounded. O produtor nunca espera, mas a fila
    /// cresce sem limite — a memoria passa a ser o unico freio.
    /// </summary>
    private async Task RunUnboundedGrowthAsync()
    {
        PrintHeader("2. Canal unbounded: sem espera, mas a fila cresce");

        Channel<TelemetryReading> channel = Channel.CreateUnbounded<TelemetryReading>(
            new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false
            });

        ProducerConsumerCoordinator coordinator = CreateCoordinator(FastProducerInterval, SlowConsumerTime);
        ChannelRunSummary summary = await coordinator.RunAsync(
            "unbounded",
            channel,
            producerCount: 1,
            consumerCount: 1,
            readingsPerProducer: 10,
            CancellationToken.None);

        await PrintResultAsync(summary);
    }

    /// <summary>
    /// Cenario 3: bounded com <see cref="BoundedChannelFullMode.DropOldest"/>. O produtor
    /// segue livre, mas itens somem em silencio: WriteAsync devolve sucesso mesmo assim.
    /// </summary>
    private async Task RunDropOldestAsync()
    {
        PrintHeader($"3. Canal bounded (capacidade {BoundedCapacity}) com DropOldest");

        Channel<TelemetryReading> channel = Channel.CreateBounded<TelemetryReading>(
            new BoundedChannelOptions(BoundedCapacity)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = false,
                SingleWriter = false
            });

        ProducerConsumerCoordinator coordinator = CreateCoordinator(FastProducerInterval, SlowConsumerTime);
        ChannelRunSummary summary = await coordinator.RunAsync(
            "bounded/DropOldest",
            channel,
            producerCount: 1,
            consumerCount: 1,
            readingsPerProducer: 10,
            CancellationToken.None);

        await PrintResultAsync(summary);
    }

    /// <summary>
    /// Cenario 4: varios produtores e varios consumidores. O writer so e completado depois
    /// que todos os produtores terminam, entao nenhuma leitura se perde.
    /// </summary>
    private async Task RunOrderedCompletionAsync()
    {
        PrintHeader("4. Conclusao ordenada com varios produtores");

        Channel<TelemetryReading> channel = Channel.CreateBounded<TelemetryReading>(
            new BoundedChannelOptions(BoundedCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            });

        ProducerConsumerCoordinator coordinator = CreateCoordinator(
            FastProducerInterval,
            TimeSpan.FromMilliseconds(80));

        ChannelRunSummary summary = await coordinator.RunAsync(
            "3 produtores / 2 consumidores",
            channel,
            producerCount: 3,
            consumerCount: 2,
            readingsPerProducer: 6,
            CancellationToken.None);

        await PrintResultAsync(summary);
    }

    /// <summary>
    /// Cenario 5: o mesmo trabalho com 1 e com 3 consumidores. O canal entrega em FIFO nos
    /// dois casos, mas o processamento concorrente desfaz a ordem na saida.
    /// </summary>
    private async Task RunConsumerCountEffectOnOrderAsync()
    {
        PrintHeader("5. Numero de consumidores versus ordem de saida");

        ChannelRunSummary singleConsumer = await RunOrderScenarioAsync("1 consumidor", consumerCount: 1);
        await PrintResultAsync(singleConsumer);

        ChannelRunSummary multipleConsumers = await RunOrderScenarioAsync("3 consumidores", consumerCount: 3);
        await PrintResultAsync(multipleConsumers);
    }

    private async Task<ChannelRunSummary> RunOrderScenarioAsync(string scenarioName, int consumerCount)
    {
        Channel<TelemetryReading> channel = Channel.CreateBounded<TelemetryReading>(
            new BoundedChannelOptions(BoundedCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            });

        ProducerConsumerCoordinator coordinator = CreateCoordinator(
            FastProducerInterval,
            TimeSpan.FromMilliseconds(60));

        return await coordinator.RunAsync(
            scenarioName,
            channel,
            producerCount: 2,
            consumerCount: consumerCount,
            readingsPerProducer: 6,
            CancellationToken.None);
    }

    private ProducerConsumerCoordinator CreateCoordinator(TimeSpan producerInterval, TimeSpan consumerProcessingTime)
    {
        ReadingProducer producer = new ReadingProducer(
            loggerFactory.CreateLogger<ReadingProducer>(),
            producerInterval);

        ReadingConsumer consumer = new ReadingConsumer(
            loggerFactory.CreateLogger<ReadingConsumer>(),
            consumerProcessingTime);

        return new ProducerConsumerCoordinator(
            loggerFactory.CreateLogger<ProducerConsumerCoordinator>(),
            producer,
            consumer);
    }

    private static void PrintHeader(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"=== {title} ===");
    }

    private static async Task PrintResultAsync(ChannelRunSummary summary)
    {
        await Task.Delay(LogFlushDelay);
        Console.WriteLine($">> {summary.Describe()}");
    }
}
