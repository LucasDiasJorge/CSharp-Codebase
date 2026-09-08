using System.Runtime.CompilerServices;
using AsyncStreamsDemo.Models;
using Microsoft.Extensions.Logging;

namespace AsyncStreamsDemo.Sources;

/// <summary>
/// Fonte de leituras. Expoe a mesma coleta em duas formas — stream assincrono e lista
/// materializada — alem de duas variantes de cancelamento, uma correta e uma quebrada.
/// </summary>
public sealed class SensorFeed
{
    private readonly ILogger<SensorFeed> logger;
    private readonly TimeSpan intervalBetweenReadings;

    private int producedCount;

    public SensorFeed(ILogger<SensorFeed> logger, TimeSpan intervalBetweenReadings)
    {
        this.logger = logger;
        this.intervalBetweenReadings = intervalBetweenReadings;
    }

    /// <summary>
    /// Quantas leituras a fonte chegou a produzir. Em um pipeline preguiçoso esse numero
    /// fica bem abaixo do total pedido, porque o consumidor para antes.
    /// </summary>
    public int ProducedCount => Volatile.Read(ref producedCount);

    public void ResetCounters()
    {
        Volatile.Write(ref producedCount, 0);
    }

    /// <summary>
    /// Iterador assincrono: cada `yield return` entrega uma leitura assim que ela existe,
    /// sem esperar o restante do lote.
    ///
    /// O <c>finally</c> roda quando o consumidor encerra a enumeracao — inclusive quando ele
    /// sai por `break`, porque `await foreach` chama `DisposeAsync` na saida.
    /// </summary>
    public async IAsyncEnumerable<SensorReading> StreamAsync(
        int readingCount,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[fonte] iniciando stream de {Count} leituras", readingCount);

        try
        {
            for (int sequence = 1; sequence <= readingCount; sequence++)
            {
                await Task.Delay(intervalBetweenReadings, cancellationToken);

                Interlocked.Increment(ref producedCount);
                logger.LogInformation("[fonte] produziu leitura {Sequence}", sequence);
                yield return new SensorReading(sequence, 20.0 + sequence, DateTimeOffset.UtcNow);
            }
        }
        finally
        {
            logger.LogInformation("[fonte] enumeracao encerrada, recursos liberados");
        }
    }

    /// <summary>
    /// Mesma coleta, porem materializada: nada volta ao chamador antes do ultimo item.
    /// Serve de contraponto para medir o tempo ate a primeira leitura.
    /// </summary>
    public async Task<IReadOnlyList<SensorReading>> CollectAllAsync(
        int readingCount,
        CancellationToken cancellationToken = default)
    {
        List<SensorReading> readings = new List<SensorReading>(readingCount);

        for (int sequence = 1; sequence <= readingCount; sequence++)
        {
            await Task.Delay(intervalBetweenReadings, cancellationToken);
            readings.Add(new SensorReading(sequence, 20.0 + sequence, DateTimeOffset.UtcNow));
        }

        logger.LogInformation("[fonte] lista completa devolvida com {Count} leituras", readings.Count);
        return readings;
    }

    /// <summary>
    /// Variante **quebrada** de proposito: o parametro de cancelamento nao tem
    /// <see cref="EnumeratorCancellationAttribute"/>.
    ///
    /// Sem o atributo, o token passado por <c>WithCancellation</c> nao chega ate aqui — o
    /// parametro fica com o valor capturado na chamada, normalmente <c>default</c>. O stream
    /// segue produzindo mesmo depois de o consumidor pedir para parar.
    ///
    /// Este metodo produz o aviso <c>CS8425</c> na compilacao, **de proposito**: o compilador
    /// aponta exatamente o defeito que o cenario 3 demonstra. Nao adicione o atributo aqui —
    /// e no <c>StreamAsync</c> acima que esta a versao correta.
    /// </summary>
    public async IAsyncEnumerable<SensorReading> StreamIgnoringCancellationAsync(
        int readingCount,
        CancellationToken cancellationToken = default)
    {
        for (int sequence = 1; sequence <= readingCount; sequence++)
        {
            await Task.Delay(intervalBetweenReadings, cancellationToken);

            logger.LogInformation("[fonte sem atributo] produziu leitura {Sequence}", sequence);
            yield return new SensorReading(sequence, 20.0 + sequence, DateTimeOffset.UtcNow);
        }
    }
}
