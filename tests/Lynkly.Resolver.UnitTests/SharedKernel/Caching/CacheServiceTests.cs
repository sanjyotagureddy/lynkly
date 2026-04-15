using Lynkly.Shared.Kernel.Caching.Abstractions;
using Lynkly.Shared.Kernel.Caching.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Resolver.UnitTests.SharedKernel.Caching;

public sealed class CacheServiceTests
{
    [Fact]
    public void CacheKey_Should_Throw_For_Whitespace_Value()
    {
        Assert.Throws<ArgumentException>(() => new CacheKey<string>(" "));
    }

    [Fact]
    public async Task AddKernelCaching_Should_Fallback_To_InMemory_When_Distributed_Is_Not_Configured()
    {
        var services = new ServiceCollection();
        services.AddKernelCaching(options => options.ReadPreference = CacheReadPreference.PreferDistributed);

        await using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();
        var key = new CacheKey<string>("links:abc");

        await cache.SetAsync(key, "https://example.com");
        var value = await cache.GetAsync(key);

        Assert.Equal("https://example.com", value);
    }

    [Fact]
    public async Task AddKernelCaching_Should_Write_To_Multiple_Providers_When_Distributed_Is_Configured()
    {
        var distributedCache = new FakeDistributedCache();

        var services = new ServiceCollection();
        services.AddSingleton<IDistributedCache>(distributedCache);
        services.AddKernelCaching(options => options.ReadPreference = CacheReadPreference.PreferDistributed);

        await using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();
        var key = new CacheKey<string>("links:def");

        await cache.SetAsync(key, "https://example.org");

        var serializedValue = await distributedCache.GetStringAsync("links:def");
        Assert.Equal("\"https://example.org\"", serializedValue);
    }

    [Fact]
    public async Task GetOrCreateAsync_Should_Use_Cache_After_First_Creation()
    {
        var services = new ServiceCollection();
        services.AddKernelCaching();

        await using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();
        var key = new CacheKey<string>("links:ghi");
        var invocationCount = 0;

        var first = await cache.GetOrCreateAsync(
            key,
            _ =>
            {
                invocationCount++;
                return Task.FromResult("https://lynk.ly");
            });

        var second = await cache.GetOrCreateAsync(
            key,
            _ =>
            {
                invocationCount++;
                return Task.FromResult("https://should-not-be-used");
            });

        Assert.Equal("https://lynk.ly", first);
        Assert.Equal("https://lynk.ly", second);
        Assert.Equal(1, invocationCount);
    }

    [Fact]
    public void AddKernelCaching_Should_Throw_When_DistributedOnly_And_IDistributedCache_Is_Not_Registered()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddKernelCaching(options =>
            {
                options.EnableInMemoryProvider = false;
                options.EnableDistributedProvider = true;
            }));

        Assert.Contains("IDistributedCache", exception.Message, StringComparison.Ordinal);
    }

    private sealed class FakeDistributedCache : IDistributedCache
    {
        private readonly Dictionary<string, byte[]> _store = new(StringComparer.Ordinal);

        public byte[]? Get(string key)
        {
            return _store.TryGetValue(key, out var value) ? value : null;
        }

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            return Task.FromResult(Get(key));
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            _store[key] = value;
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            Set(key, value, options);
            return Task.CompletedTask;
        }

        public void Refresh(string key)
        {
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            _store.Remove(key);
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            Remove(key);
            return Task.CompletedTask;
        }
    }
}
