using Lynkly.Shared.Kernel.Caching.Abstractions;

namespace Lynkly.Resolver.Application.UseCases.Links;

internal static class LinkCacheKeys
{
    public static CacheKey<string> ResolveDestinationByAlias(string alias)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);
        return new CacheKey<string>($"links:resolve:{alias.Trim().ToLowerInvariant()}");
    }
}
