using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace KafkaOrderingDemo.Kafka;

/// <summary>
/// Produtor do exemplo. O que interessa aqui é a chave: ela é o único mecanismo que
/// decide a partição e, portanto, o único que garante ordem.
/// </summary>
public sealed class OrderProducer : IDisposable
{
    private readonly IProducer<string?, string> _producer;
    private readonly KafkaSettings _settings;
    private readonly ILogger<OrderProducer> _logger;

    public OrderProducer(KafkaSettings settings, ILogger<OrderProducer> logger)
    {
        _settings = settings;
        _logger = logger;

        ProducerConfig config = new ProducerConfig
        {
            BootstrapServers = settings.BootstrapServers,

            // Espera a confirmacao de todas as replicas em sincronia. Com acks=1 uma
            // troca de lider pode perder mensagens ja confirmadas.
            Acks = Acks.All,

            // Sem isto, uma retentativa interna pode reordenar mensagens da mesma
            // particao — e a garantia de ordem por chave vai embora justamente no
            // momento de instabilidade em que ela mais importa.
            EnableIdempotence = true
        };

        _producer = new ProducerBuilder<string?, string>(config).Build();
    }

    /// <summary>
    /// Publica e devolve a partição e o offset atribuídos. Sem a chave, o produtor
    /// distribui entre as partições e a ordem relativa se perde.
    /// </summary>
    public async Task<DeliveryResult<string?, string>> ProduceAsync(string? key, string value, CancellationToken cancellationToken)
    {
        DeliveryResult<string?, string> result = await _producer.ProduceAsync(
            _settings.Topic,
            new Message<string?, string> { Key = key, Value = value },
            cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Chave {Chave} -> particao {Particao}, offset {Offset}.",
            key ?? "(sem chave)",
            result.Partition.Value,
            result.Offset.Value);

        return result;
    }

    /// <summary>Cria o tópico com o número de partições esperado, se ainda não existir.</summary>
    public async Task EnsureTopicAsync(CancellationToken cancellationToken)
    {
        using IAdminClient admin = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = _settings.BootstrapServers
        }).Build();

        Metadata metadata = admin.GetMetadata(TimeSpan.FromSeconds(10));

        foreach (TopicMetadata topic in metadata.Topics)
        {
            if (topic.Topic == _settings.Topic && topic.Partitions.Count > 0)
            {
                return;
            }
        }

        try
        {
            await admin.CreateTopicsAsync(
            [
                new TopicSpecification
                {
                    Name = _settings.Topic,
                    NumPartitions = _settings.PartitionCount,
                    ReplicationFactor = 1
                }
            ]).ConfigureAwait(false);

            _logger.LogInformation("Topico {Topico} criado com {Particoes} particoes.", _settings.Topic, _settings.PartitionCount);
        }
        catch (CreateTopicsException ex) when (ex.Results.Count > 0 && ex.Results[0].Error.Code == ErrorCode.TopicAlreadyExists)
        {
            // Corrida entre instancias: alguem criou primeiro, o que e o resultado desejado.
        }
    }

    public void Dispose()
    {
        // Flush antes de fechar: mensagens em buffer que nao forem enviadas se perdem
        // silenciosamente no descarte do produtor.
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
