using InvoiceThrottlingApi.Services;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IInvoiceGenerator, InvoiceGenerator>();
builder.Services.AddScoped<IInvoiceProcessor, InvoiceProcessor>();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });

    options.AddSlidingWindowLimiter("sliding", opt =>
    {
        opt.PermitLimit = 20;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 4;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });

    options.AddTokenBucketLimiter("token", opt =>
    {
        opt.TokenLimit = 100;
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
        opt.TokensPerPeriod = 50;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 20;
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            Error = "Muitas requisições. Tente novamente mais tarde.",
            RetryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter) 
                ? retryAfter.TotalSeconds 
                : null
        }, cancellationToken);
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRateLimiter();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    Project = "Invoice Throttling API",
    Description = "API para demonstração de técnicas de throttling no processamento de 1000 notas fiscais",
    Endpoints = new[]
    {
        "GET /api/invoice/generate/{count} - Gera N notas fiscais",
        "POST /api/invoice/process/unlimited - Processa sem limite de concorrência",
        "POST /api/invoice/process/semaphore/{maxConcurrency} - Processa com Semaphore",
        "POST /api/invoice/process/ratelimit/{requestsPerSecond} - Processa com Rate Limiter",
        "POST /api/invoice/process/demo1000 - Demo completa com 1000 NFs usando todas as estratégias",
        "GET /api/invoice/api-limited - Endpoint com rate limiting (10 req/min)"
    },
    Swagger = "/swagger"
}));

app.Run();
