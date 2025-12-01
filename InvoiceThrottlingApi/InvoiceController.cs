namespace InvoiceThrottlingApi.Controllers;

using InvoiceThrottlingApi.Models;
using InvoiceThrottlingApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/[controller]")]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceGenerator _generator;
    private readonly IInvoiceProcessor _processor;
    private readonly ILogger<InvoiceController> _logger;

    public InvoiceController(
        IInvoiceGenerator generator,
        IInvoiceProcessor processor,
        ILogger<InvoiceController> logger)
    {
        _generator = generator;
        _processor = processor;
        _logger = logger;
    }

    [HttpGet("generate/{count}")]
    public ActionResult<List<Invoice>> GenerateInvoices(int count = 1000)
    {
        if (count <= 0 || count > 10000)
        {
            return BadRequest("Count deve estar entre 1 e 10000");
        }

        var invoices = _generator.GenerateInvoices(count);
        _logger.LogInformation("Geradas {Count} notas fiscais", count);
        
        return Ok(new { 
            Total = invoices.Count, 
            TotalAmount = invoices.Sum(i => i.TotalAmount),
            Invoices = invoices 
        });
    }

    [HttpPost("process/unlimited")]
    public async Task<ActionResult<BatchProcessingResult>> ProcessUnlimited([FromBody] List<Invoice> invoices)
    {
        if (invoices == null || !invoices.Any())
        {
            return BadRequest("Lista de notas fiscais vazia");
        }

        _logger.LogInformation("Iniciando processamento ilimitado de {Count} notas fiscais", invoices.Count);
        var result = await _processor.ProcessBatchAsync(invoices);
        
        return Ok(result);
    }

    [HttpPost("process/semaphore/{maxConcurrency}")]
    public async Task<ActionResult<BatchProcessingResult>> ProcessWithSemaphore(
        [FromBody] List<Invoice> invoices,
        int maxConcurrency = 50)
    {
        if (invoices == null || !invoices.Any())
        {
            return BadRequest("Lista de notas fiscais vazia");
        }

        if (maxConcurrency <= 0 || maxConcurrency > 500)
        {
            return BadRequest("maxConcurrency deve estar entre 1 e 500");
        }

        _logger.LogInformation(
            "Iniciando processamento com Semaphore (max {MaxConcurrency}) de {Count} notas fiscais", 
            maxConcurrency, invoices.Count);
        
        var result = await _processor.ProcessBatchWithSemaphoreAsync(invoices, maxConcurrency);
        
        return Ok(result);
    }

    [HttpPost("process/ratelimit/{requestsPerSecond}")]
    public async Task<ActionResult<BatchProcessingResult>> ProcessWithRateLimit(
        [FromBody] List<Invoice> invoices,
        int requestsPerSecond = 100)
    {
        if (invoices == null || !invoices.Any())
        {
            return BadRequest("Lista de notas fiscais vazia");
        }

        if (requestsPerSecond <= 0 || requestsPerSecond > 1000)
        {
            return BadRequest("requestsPerSecond deve estar entre 1 e 1000");
        }

        _logger.LogInformation(
            "Iniciando processamento com Rate Limiter ({RequestsPerSecond} req/s) de {Count} notas fiscais", 
            requestsPerSecond, invoices.Count);
        
        var result = await _processor.ProcessBatchWithRateLimiterAsync(invoices, requestsPerSecond);
        
        return Ok(result);
    }

    [HttpPost("process/demo1000")]
    public async Task<ActionResult<object>> ProcessDemo1000()
    {
        _logger.LogInformation("Iniciando demo com 1000 notas fiscais");
        
        var invoices = _generator.GenerateInvoices(1000);
        
        var unlimitedResult = await _processor.ProcessBatchAsync(invoices.Take(1000).ToList());
        
        await Task.Delay(1000);
        
        var semaphoreResult = await _processor.ProcessBatchWithSemaphoreAsync(
            invoices.Take(1000).ToList(), 
            maxConcurrency: 50);
        
        await Task.Delay(1000);
        
        var rateLimitResult = await _processor.ProcessBatchWithRateLimiterAsync(
            invoices.Take(1000).ToList(), 
            requestsPerSecond: 100);

        return Ok(new
        {
            TotalInvoices = 1000,
            Results = new
            {
                Unlimited = new
                {
                    unlimitedResult.Processed,
                    unlimitedResult.Failed,
                    unlimitedResult.ThrottledRejected,
                    DurationMs = unlimitedResult.Duration.TotalMilliseconds
                },
                Semaphore = new
                {
                    MaxConcurrency = 50,
                    semaphoreResult.Processed,
                    semaphoreResult.Failed,
                    semaphoreResult.ThrottledRejected,
                    DurationMs = semaphoreResult.Duration.TotalMilliseconds
                },
                RateLimit = new
                {
                    RequestsPerSecond = 100,
                    rateLimitResult.Processed,
                    rateLimitResult.Failed,
                    rateLimitResult.ThrottledRejected,
                    DurationMs = rateLimitResult.Duration.TotalMilliseconds
                }
            }
        });
    }

    [HttpGet("api-limited")]
    [EnableRateLimiting("fixed")]
    public IActionResult GetWithRateLimit()
    {
        return Ok(new { Message = "Este endpoint tem rate limiting de 10 requisições por minuto", Timestamp = DateTime.Now });
    }
}
