namespace KafkaOrderingDemo.Kafka;

/// <summary>Endereço do broker e nome do tópico usado pelo exemplo.</summary>
public sealed class KafkaSettings
{
    public KafkaSettings(IConfiguration configuration)
    {
        BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        Topic = configuration["Kafka:Topic"] ?? "pedidos";
        PartitionCount = int.TryParse(configuration["Kafka:PartitionCount"], out int partitions) ? partitions : 3;
    }

    public string BootstrapServers { get; }

    public string Topic { get; }

    /// <summary>
    /// Três partições. Esse número é o teto do paralelismo do grupo: um consumidor a
    /// mais que isso fica sem trabalho.
    /// </summary>
    public int PartitionCount { get; }
}
