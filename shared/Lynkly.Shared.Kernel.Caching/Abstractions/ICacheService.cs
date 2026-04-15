namespace Lynkly.Shared.Kernel.Caching.Abstractions;

public interface ICacheService
{
    Task<TValue?> GetAsync<TValue>(
        CacheKey<TValue> key,
        CancellationToken cancellationToken = default);

    Task SetAsync<TValue>(
        CacheKey<TValue> key,
        TValue value,
        CacheEntryOptions? options = null,
        CancellationToken cancellationToken = default);

    Task RemoveAsync<TValue>(
        CacheKey<TValue> key,
        CancellationToken cancellationToken = default);

    Task<TValue> GetOrCreateAsync<TValue>(
        CacheKey<TValue> key,
        Func<CancellationToken, Task<TValue>> factory,
        CacheEntryOptions? options = null,
        CancellationToken cancellationToken = default);
}
