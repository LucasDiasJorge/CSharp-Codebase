using Confluent.Kafka;

namespace KafkaOrderingDemo.Kafka;

/// <summary>
/// Sobe N consumidores no mesmo grupo, lê o tópico do início e relata o que cada um
/// recebeu. É com isso que dá para ver as duas coisas ao mesmo tempo: a distribuição das
/// partições entre os consumidores e a ordem dentro de cada partição.
/// </summary>
public sealed class GroupConsumerRunner
{
    private readonly KafkaSettings _settings;
    private readonly ILogger<GroupConsumerRunner> _logger;

    public GroupConsumerRunner(KafkaSettings settings, ILogger<GroupConsumerRunner> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task<GroupRunReport> RunAsync(string groupId, int consumerCount, TimeSpan duration, CancellationToken cancellationToken)
    {
        List<ConsumerReport> reports = new List<ConsumerReport>();
        List<Task<ConsumerReport>> tasks = new List<Task<ConsumerReport>>();

        using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(duration);

        for (int index = 0; index < consumerCount; index++)
        {
            int consumerIndex = index;
            tasks.Add(Task.Run(() => ConsumeAsync(groupId, consumerIndex, timeoutSource.Token), CancellationToken.None));
        }

        foreach (Task<ConsumerReport> task in tasks)
        {
            reports.Add(await task.ConfigureAwait(false));
        }

        return new GroupRunReport(groupId, consumerCount, _settings.PartitionCount, reports);
    }

    private ConsumerReport ConsumeAsync(string groupId, int consumerIndex, CancellationToken cancellationToken)
    {
        ConsumerConfig config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,

            // Todos os consumidores da mesma execucao compartilham o GroupId: e isso que
            // faz o broker DIVIDIR as particoes entre eles em vez de entregar tudo a
            // todos. Grupos diferentes recebem cada um uma copia completa.
            GroupId = groupId,

            // Grupo novo comeca do inicio do topico, para o exemplo ser repetivel.
            AutoOffsetReset = AutoOffsetReset.Earliest,

            EnableAutoCommit = false,
            SessionTimeoutMs = 6000
        };

        using IConsumer<string?, string> consumer = new ConsumerBuilder<string?, string>(config).Build();
        consumer.Subscribe(_settings.Topic);

        List<ConsumedRecord> records = new List<ConsumedRecord>();
        HashSet<int> partitions = new HashSet<int>();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ConsumeResult<string?, string>? result = consumer.Consume(TimeSpan.FromMilliseconds(500));

                if (result?.Message is null)
                {
                    continue;
                }

                partitions.Add(result.Partition.Value);
                records.Add(new ConsumedRecord(
                    result.Partition.Value,
                    result.Offset.Value,
                    result.Message.Key,
                    result.Message.Value));
            }
        }
        catch (OperationCanceledException)
        {
            // Fim da janela de leitura.
        }
        finally
        {
            // Close faz o consumidor sair do grupo de forma limpa e dispara o rebalance
            // na hora. Sem isso, o grupo espera o session timeout para perceber a saida.
            consumer.Close();
        }

        _logger.LogInformation(
            "Consumidor {Indice} do grupo {Grupo} leu {Quantidade} mensagens das particoes [{Particoes}].",
            consumerIndex,
            groupId,
            records.Count,
            string.Join(", ", partitions.Order()));

        return new ConsumerReport(consumerIndex, partitions.Order().ToList(), records);
    }
}

public sealed record ConsumedRecord(int Partition, long Offset, string? Key, string Value);

public sealed record ConsumerReport(int ConsumerIndex, IReadOnlyList<int> AssignedPartitions, IReadOnlyList<ConsumedRecord> Records)
{
    public int MessageCount => Records.Count;

    /// <summary>Consumidor sem partição não recebe nada — o caso de excesso de consumidores.</summary>
    public bool IsIdle => AssignedPartitions.Count == 0;
}

public sealed record GroupRunReport(string GroupId, int ConsumerCount, int PartitionCount, IReadOnlyList<ConsumerReport> Consumers);
