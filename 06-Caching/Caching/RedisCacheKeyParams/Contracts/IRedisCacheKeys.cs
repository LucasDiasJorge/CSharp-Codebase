namespace RedisCacheKeyParams.Contracts;

/// <summary>
/// Define a stable contract for cache key composition.
/// </summary>
public interface IRedisCacheKeys
{
    string Namespace { get; }

    string Join(params object[] parts);
}
