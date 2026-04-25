namespace InvoiceThrottlingApi.Models;

public record Invoice(
    string Id,
    string Number,
    DateTime IssueDate,
    decimal TotalAmount,
    string CustomerName,
    string CustomerDocument,
    List<InvoiceItem> Items,
    InvoiceStatus Status = InvoiceStatus.Pending
);

public record InvoiceItem(
    string ProductCode,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);

public enum InvoiceStatus
{
    Pending,
    Processing,
    Processed,
    Failed,
    ThrottledRejected
}

public record InvoiceProcessingResult(
    string InvoiceId,
    InvoiceStatus Status,
    DateTime ProcessedAt,
    string? ErrorMessage = null,
    int RetryAttempt = 0
);

public record BatchProcessingResult(
    int TotalInvoices,
    int Processed,
    int Failed,
    int ThrottledRejected,
    TimeSpan Duration,
    List<InvoiceProcessingResult> Results
);
