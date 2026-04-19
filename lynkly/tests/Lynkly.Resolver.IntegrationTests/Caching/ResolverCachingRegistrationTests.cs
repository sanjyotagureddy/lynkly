using Lynkly.Resolver.Infrastructure.Caching.DependencyInjection;
using Lynkly.Shared.Kernel.Caching.Abstractions;
using Lynkly.Shared.Kernel.Caching.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Resolver.IntegrationTests.Caching;

public sealed class ResolverCachingRegistrationTests
{
    [Fact]
    public async Task AddResolverCaching_UsesInMemoryFallback_WhenRedisConnectionStringIsMissing()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        services.AddResolverCaching(configuration);

        await using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();
        var cacheKey = new CacheKey<string>("links:resolve:fallback");

        await cache.SetAsync(cacheKey, "https://example.com");
        var cachedValue = await cache.GetAsync(cacheKey);

        Assert.Equal("https://example.com", cachedValue);
        Assert.Null(provider.GetService<IDistributedCache>());
        Assert.Equal(
            CacheReadPreference.PreferInMemory,
            provider.GetRequiredService<CacheServiceRegistrationOptions>().ReadPreference);
    }

    [Fact]
    public async Task AddResolverCaching_UsesInMemoryFallback_WhenRedisIsUnavailable()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:lynkly-redis"] = "localhost:0,abortConnect=true,connectTimeout=10,syncTimeout=10"
            })
            .Build();

        services.AddResolverCaching(configuration);

        await using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();
        var cacheKey = new CacheKey<string>("links:resolve:redis-down");

        await cache.SetAsync(cacheKey, "https://example.org");
        var cachedValue = await cache.GetAsync(cacheKey);

        Assert.Equal("https://example.org", cachedValue);
        Assert.Null(provider.GetService<IDistributedCache>());
        Assert.Equal(
            CacheReadPreference.PreferInMemory,
            provider.GetRequiredService<CacheServiceRegistrationOptions>().ReadPreference);
    }
}
