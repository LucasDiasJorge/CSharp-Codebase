using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using IdempotencyCacheApi.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace IdempotencyCacheApi.Services;

public sealed class IdempotencyService : IIdempotencyService
{
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _cacheEntryOptions;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks;

    public IdempotencyService(IMemoryCache memoryCache, IOptions<IdempotencyCacheOptions> options)
    {
        _memoryCache = memoryCache;
        _locks = new ConcurrentDictionary<string, SemaphoreSlim>(StringComparer.Ordinal);

        IdempotencyCacheOptions cacheOptions = options.Value;
        TimeSpan absoluteExpiration = TimeSpan.FromMinutes(Math.Max(1, cacheOptions.AbsoluteExpirationMinutes));
        TimeSpan slidingExpiration = TimeSpan.FromMinutes(Math.Max(1, cacheOptions.SlidingExpirationMinutes));

        _cacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration,
            SlidingExpiration = slidingExpiration
        };
    }

    public async Task<IdempotencyExecutionResult> ExecuteAsync(
        string idempotencyKey,
        PaymentRequest request,
        Func<CancellationToken, Task<PaymentResponse>> handler,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return IdempotencyExecutionResult.Conflict("Idempotency-Key header is required.");
        }

        string normalizedKey = idempotencyKey.Trim();
        string requestHash = ComputeRequestHash(request);
        SemaphoreSlim keyLock = _locks.GetOrAdd(normalizedKey, CreateKeyLock);

        await keyLock.WaitAsync(cancellationToken);

        try
        {
            if (_memoryCache.TryGetValue(normalizedKey, out IdempotencyCacheEntry? existingEntry))
            {
                if (existingEntry is null)
                {
                    _memoryCache.Remove(normalizedKey);
                }
                else if (!string.Equals(existingEntry.RequestHash, requestHash, StringComparison.Ordinal))
                {
                    return IdempotencyExecutionResult.Conflict(
                        "The informed idempotency key was already used with a different request payload.");
                }
                else
                {
                    PaymentResponse replayResponse = new PaymentResponse
                    {
                        TransactionId = existingEntry.Response.TransactionId,
                        OrderId = existingEntry.Response.OrderId,
                        Amount = existingEntry.Response.Amount,
                        Currency = existingEntry.Response.Currency,
                        Description = existingEntry.Response.Description,
                        ProcessedAtUtc = existingEntry.Response.ProcessedAtUtc,
                        IsReplay = true
                    };

                    return IdempotencyExecutionResult.Replay(replayResponse);
                }
            }

            PaymentResponse response = await handler(cancellationToken);

            IdempotencyCacheEntry cacheEntry = new IdempotencyCacheEntry
            {
                RequestHash = requestHash,
                Response = response,
                CachedAtUtc = DateTimeOffset.UtcNow
            };

            _memoryCache.Set(normalizedKey, cacheEntry, _cacheEntryOptions);

            return IdempotencyExecutionResult.Executed(response);
        }
        finally
        {
            keyLock.Release();
        }
    }

    public bool Invalidate(string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return false;
        }

        string normalizedKey = idempotencyKey.Trim();
        bool exists = _memoryCache.TryGetValue(normalizedKey, out IdempotencyCacheEntry? _);

        if (!exists)
        {
            return false;
        }

        _memoryCache.Remove(normalizedKey);

        if (_locks.TryRemove(normalizedKey, out SemaphoreSlim? semaphoreSlim))
        {
            semaphoreSlim.Dispose();
        }

        return true;
    }

    private static SemaphoreSlim CreateKeyLock(string cacheKey)
    {
        _ = cacheKey;
        return new SemaphoreSlim(1, 1);
    }

    private static string ComputeRequestHash(PaymentRequest request)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(request.OrderId.Trim().ToUpperInvariant());
        builder.Append('|');
        builder.Append(request.Amount.ToString("F2", CultureInfo.InvariantCulture));
        builder.Append('|');
        builder.Append(request.Currency.Trim().ToUpperInvariant());
        builder.Append('|');
        builder.Append((request.Description ?? string.Empty).Trim());

        string normalizedPayload = builder.ToString();
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedPayload));

        return Convert.ToHexString(hashBytes);
    }
}
