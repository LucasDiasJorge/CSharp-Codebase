using KafkaStreamApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using Microsoft.AspNetCore.Cors;

namespace KafkaStreamApi.Controllers;

[EnableCors("AllowAll")]
[ApiController]
[Route("api/kafka")]
public class KafkaController : ControllerBase
{
    private readonly KafkaConsumerService _kafkaService;

    public KafkaController(KafkaConsumerService kafkaService)
    {
        _kafkaService = kafkaService;
    }

    [HttpGet("stream/{topic}")]
    public async Task GetKafkaStream(string topic, CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";
        
        await foreach (var message in _kafkaService.ConsumeMessagesAsStreamAsync(topic, cancellationToken))
        {
            var data = $"data: {System.Text.Json.JsonSerializer.Serialize(message)}\n\n";
            await Response.WriteAsync(data, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }

    [HttpGet("stream-ndjson/{topic}")]
    public async Task GetKafkaStreamNdJson(string topic, CancellationToken cancellationToken)
    {
        Response.ContentType = "application/x-ndjson";
        
        await foreach (var message in _kafkaService.ConsumeMessagesAsStreamAsync(topic, cancellationToken))
        {
            var jsonMessage = System.Text.Json.JsonSerializer.Serialize(new 
            { 
                Message = message,
                Timestamp = DateTime.UtcNow
            });
            await Response.WriteAsync(jsonMessage + "\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }
}