using Confluent.Kafka;
using System.Runtime.CompilerServices;

namespace KafkaStreamApi.Services;

public class KafkaConsumerService
{
    private readonly ConsumerConfig _consumerConfig;

    public KafkaConsumerService(string bootstrapServers, string groupId)
    {
        if (string.IsNullOrEmpty(bootstrapServers))
            throw new ArgumentException("Bootstrap servers cannot be null or empty", nameof(bootstrapServers));
        
        if (string.IsNullOrEmpty(groupId))
            throw new ArgumentException("Group ID cannot be null or empty", nameof(groupId));

        _consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Latest,
            EnableAutoCommit = false,
            AllowAutoCreateTopics = true
        };
    }

    public async IAsyncEnumerable<string> ConsumeMessagesAsStreamAsync(
        string topic, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
        consumer.Subscribe(topic);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ConsumeResult<Ignore, string>? consumeResult = null;
                try
                {
                    consumeResult = consumer.Consume(cancellationToken);
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Error consuming message: {e.Error.Reason}");
                    continue;
                }
                
                if (consumeResult?.Message?.Value != null)
                {
                    yield return consumeResult.Message.Value;
                }
            }
        }
        finally
        {
            consumer.Close();
        }
    }
}