namespace InvoiceThrottlingApi.Services;

using InvoiceThrottlingApi.Models;
using System.Threading.RateLimiting;

public interface IInvoiceProcessor
{
    Task<InvoiceProcessingResult> ProcessInvoiceAsync(Invoice invoice);
    Task<BatchProcessingResult> ProcessBatchAsync(List<Invoice> invoices, CancellationToken cancellationToken = default);
    Task<BatchProcessingResult> ProcessBatchWithSemaphoreAsync(List<Invoice> invoices, int maxConcurrency, CancellationToken cancellationToken = default);
    Task<BatchProcessingResult> ProcessBatchWithRateLimiterAsync(List<Invoice> invoices, int requestsPerSecond, CancellationToken cancellationToken = default);
}

public class InvoiceProcessor : IInvoiceProcessor
{
    private readonly ILogger<InvoiceProcessor> _logger;
    private static int _processedCount = 0;

    public InvoiceProcessor(ILogger<InvoiceProcessor> logger)
    {
        _logger = logger;
    }

    public async Task<InvoiceProcessingResult> ProcessInvoiceAsync(Invoice invoice)
    {
        try
        {
            _logger.LogInformation("Processando nota fiscal {InvoiceNumber} - Cliente: {Customer}", 
                invoice.Number, invoice.CustomerName);

            await Task.Delay(Random.Shared.Next(10, 50));

            Interlocked.Increment(ref _processedCount);

            return new InvoiceProcessingResult(
                InvoiceId: invoice.Id,
                Status: InvoiceStatus.Processed,
                ProcessedAt: DateTime.Now
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar nota fiscal {InvoiceNumber}", invoice.Number);
            return new InvoiceProcessingResult(
                InvoiceId: invoice.Id,
                Status: InvoiceStatus.Failed,
                ProcessedAt: DateTime.Now,
                ErrorMessage: ex.Message
            );
        }
    }

    public async Task<BatchProcessingResult> ProcessBatchAsync(List<Invoice> invoices, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.Now;
        var tasks = invoices.Select(inv => ProcessInvoiceAsync(inv));
        var results = await Task.WhenAll(tasks);

        return CalculateBatchResult(invoices.Count, results, DateTime.Now - startTime);
    }

    public async Task<BatchProcessingResult> ProcessBatchWithSemaphoreAsync(List<Invoice> invoices, int maxConcurrency, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.Now;
        var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        var results = new List<InvoiceProcessingResult>();

        var tasks = invoices.Select(async invoice =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                return await ProcessInvoiceAsync(invoice);
            }
            finally
            {
                semaphore.Release();
            }
        });

        var processedResults = await Task.WhenAll(tasks);
        results.AddRange(processedResults);

        return CalculateBatchResult(invoices.Count, results, DateTime.Now - startTime);
    }

    public async Task<BatchProcessingResult> ProcessBatchWithRateLimiterAsync(List<Invoice> invoices, int requestsPerSecond, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.Now;
        var results = new List<InvoiceProcessingResult>();

        var rateLimiterOptions = new FixedWindowRateLimiterOptions
        {
            PermitLimit = requestsPerSecond,
            Window = TimeSpan.FromSeconds(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = invoices.Count
        };

        using var rateLimiter = new FixedWindowRateLimiter(rateLimiterOptions);

        var tasks = invoices.Select(async invoice =>
        {
            using var lease = await rateLimiter.AcquireAsync(permitCount: 1, cancellationToken);
            
            if (lease.IsAcquired)
            {
                return await ProcessInvoiceAsync(invoice);
            }
            else
            {
                _logger.LogWarning("Nota fiscal {InvoiceNumber} rejeitada por throttling", invoice.Number);
                return new InvoiceProcessingResult(
                    InvoiceId: invoice.Id,
                    Status: InvoiceStatus.ThrottledRejected,
                    ProcessedAt: DateTime.Now,
                    ErrorMessage: "Limite de taxa excedido"
                );
            }
        });

        var processedResults = await Task.WhenAll(tasks);
        results.AddRange(processedResults);

        return CalculateBatchResult(invoices.Count, results, DateTime.Now - startTime);
    }

    private BatchProcessingResult CalculateBatchResult(int total, IEnumerable<InvoiceProcessingResult> results, TimeSpan duration)
    {
        var resultsList = results.ToList();
        return new BatchProcessingResult(
            TotalInvoices: total,
            Processed: resultsList.Count(r => r.Status == InvoiceStatus.Processed),
            Failed: resultsList.Count(r => r.Status == InvoiceStatus.Failed),
            ThrottledRejected: resultsList.Count(r => r.Status == InvoiceStatus.ThrottledRejected),
            Duration: duration,
            Results: resultsList
        );
    }
}
