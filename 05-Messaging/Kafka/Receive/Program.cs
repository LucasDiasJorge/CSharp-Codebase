using Confluent.Kafka;

class Program
{
    static void Main(string[] args)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "127.0.0.1:9092",
            GroupId = "test-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
            
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe("test");

        try
        {
            while (true)
            {
                var consumeResult = consumer.Consume();
                Console.WriteLine($"Received message: {consumeResult.Message.Value} at {consumeResult.TopicPartitionOffset}");
            }
        }
        catch (OperationCanceledException)
        {
            // The consumer was stopped via cancellation token
            Console.WriteLine("Consumer stopped");
        }
        finally
        {
            consumer.Close();
        }
    }
}