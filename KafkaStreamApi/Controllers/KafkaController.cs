using KafkaStreamApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Logging;

namespace KafkaStreamApi.Controllers;

[EnableCors("AllowAll")]
[ApiController]
[Route("api/kafka")]
public class KafkaController : ControllerBase
{
    private readonly KafkaConsumerService _kafkaService;
    private readonly ILogger<KafkaController> _logger;

    public KafkaController(
        KafkaConsumerService kafkaService,
        ILogger<KafkaController> logger)
    {
        _kafkaService = kafkaService ?? throw new ArgumentNullException(nameof(kafkaService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("stream/{topic}/{groupId}")]
    public async Task GetKafkaStream(string topic, string groupId, CancellationToken cancellationToken)
    {
        try
        {
            Response.ContentType = "text/event-stream";
            _logger.LogInformation("Starting SSE stream for topic {Topic}", topic);

            await foreach (var message in _kafkaService.ConsumeMessagesAsStreamAsync(topic, groupId, cancellationToken))
            {
                var data = $"data: {System.Text.Json.JsonSerializer.Serialize(message)}\n\n";
                await Response.WriteAsync(data, cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("SSE stream for topic {Topic} was cancelled", topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SSE stream for topic {Topic}", topic);
            throw;
        }
    }

    [HttpGet("stream-ndjson/{topic}/{groupId}")]
    public async Task GetKafkaStreamNdJson(string topic, string groupId, CancellationToken cancellationToken)
    {
        try
        {
            Response.ContentType = "application/x-ndjson";
            _logger.LogInformation("Starting NDJSON stream for topic {Topic}", topic);

            await foreach (var message in _kafkaService.ConsumeMessagesAsStreamAsync(topic, groupId, cancellationToken))
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
        catch (OperationCanceledException)
        {
            _logger.LogInformation("NDJSON stream for topic {Topic} was cancelled", topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in NDJSON stream for topic {Topic}", topic);
            throw;
        }
    }
}