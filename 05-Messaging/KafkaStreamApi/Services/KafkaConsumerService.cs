using Confluent.Kafka;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace KafkaStreamApi.Services;

public class KafkaConsumerService : IDisposable
{
    private readonly ConsumerConfig _consumerConfig;
    private readonly ILogger<KafkaConsumerService> _logger;
    private IConsumer<Ignore, string>? _consumer;

    public KafkaConsumerService(
        string bootstrapServers, 
        string groupId,
        ILogger<KafkaConsumerService> logger)
    {
        if (string.IsNullOrEmpty(bootstrapServers))
            throw new ArgumentException("Bootstrap servers cannot be null or empty", nameof(bootstrapServers));
        
        if (string.IsNullOrEmpty(groupId))
            throw new ArgumentException("Group ID cannot be null or empty", nameof(groupId));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = null,
            AutoOffsetReset = AutoOffsetReset.Latest,
            EnableAutoCommit = false,
            AllowAutoCreateTopics = true,
            EnablePartitionEof = true,
            LogConnectionClose = false
        };
    }

    public async IAsyncEnumerable<string> ConsumeMessagesAsStreamAsync(
        string topic, 
        string groupId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        
        var config = new ConsumerConfig(_consumerConfig)
        {
            GroupId = groupId ?? _consumerConfig.GroupId,
            AutoOffsetReset = AutoOffsetReset.Latest,
            EnableAutoCommit = false,
            AllowAutoCreateTopics = true
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();

        consumer.Subscribe(topic);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ConsumeResult<Ignore, string>? consumeResult = null;
                
                try
                {
                    consumeResult = consumer.Consume(cancellationToken);
                    
                    if (consumeResult.IsPartitionEOF)
                    {
                        _logger.LogDebug("Reached end of partition {Partition}", consumeResult.Partition);
                        await Task.Delay(100, cancellationToken); // Small delay to prevent tight loop
                        continue;
                    }
                }
                catch (ConsumeException e)
                {
                    _logger.LogError(e, "Error consuming message from topic {Topic}", topic);
                    await Task.Delay(1000, cancellationToken); // Delay before retry
                    continue;
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Consumption was cancelled for topic {Topic}", topic);
                    yield break;
                }
                catch (KafkaException e)
                {
                    _logger.LogError(e, "Kafka exception while consuming from topic {Topic}", topic);
                    throw;
                }
                
                if (consumeResult?.Message?.Value != null)
                {
                    _logger.LogDebug("Received message from topic {Topic}, partition {Partition}", 
                        topic, consumeResult.Partition);
                    yield return consumeResult.Message.Value;
                }
            }
        }
        finally
        {
            _logger.LogInformation("Closing consumer for topic {Topic}", topic);
        }
    }

    public void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
        GC.SuppressFinalize(this);
    }
}