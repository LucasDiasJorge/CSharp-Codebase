using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using UnifiedCacheSdk.Abstractions;
using UnifiedCacheSdk.Core;
using UnifiedCacheSdk.Options;
using UnifiedCacheSdk.Providers;

namespace UnifiedCacheSdk.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUnifiedCache(
        this IServiceCollection services,
        Action<UnifiedCacheOptions> configureOptions,
        Action<CacheBuilder>? configureBuilder = null)
    {
        services.AddOptions<UnifiedCacheOptions>()
            .Configure(configureOptions)
            .PostConfigure(options =>
            {
                if (string.IsNullOrWhiteSpace(options.ServiceId))
                    throw new ArgumentException("UnifiedCacheOptions.ServiceId deve ser informado.");
            });

        services.AddSingleton<ICacheKeyBuilder, CacheKeyBuilder>();

        var builder = new CacheBuilder(services);
        configureBuilder?.Invoke(builder);
        if (!builder.HasProvider)
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheProvider, MemoryCacheProvider>();
        }

        return services;
    }

    public sealed class CacheBuilder(IServiceCollection services)
    {
        public bool HasProvider { get; private set; }

        public CacheBuilder UseMemoryCache()
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheProvider, MemoryCacheProvider>();
            HasProvider = true;
            return this;
        }

        public CacheBuilder UseRedis(string configuration)
        {
            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(configuration));
            services.AddSingleton<ICacheProvider, RedisCacheProvider>();
            HasProvider = true;
            return this;
        }

        public CacheBuilder UseExistingRedis(IConnectionMultiplexer multiplexer)
        {
            services.AddSingleton(multiplexer);
            services.AddSingleton<ICacheProvider, RedisCacheProvider>();
            HasProvider = true;
            return this;
        }
    }
}
