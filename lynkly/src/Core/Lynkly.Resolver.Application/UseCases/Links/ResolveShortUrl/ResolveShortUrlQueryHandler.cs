using System.Text;
using Lynkly.Resolver.Application.Abstractions.Persistence;
using Lynkly.Resolver.Application.UseCases.Links;
using Lynkly.Shared.Kernel.Caching.Abstractions;
using Lynkly.Shared.Kernel.Core.Helpers.Security;
using Lynkly.Shared.Kernel.MediatR.Abstractions;
using Lynkly.Shared.Kernel.Security.Encryption;

namespace Lynkly.Resolver.Application.UseCases.Links.ResolveShortUrl;

public sealed class ResolveShortUrlQueryHandler(
    ILinkReadRepository repository,
    IEncryptionService encryptionService,
    ICacheService cacheService,
    TimeProvider? timeProvider = null)
    : IRequestHandler<ResolveShortUrlQuery, ResolveShortUrlResult?>
{
    private readonly ILinkReadRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IEncryptionService _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    public async Task<ResolveShortUrlResult?> Handle(ResolveShortUrlQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Alias);

        var normalizedAlias = request.Alias.Trim().ToLowerInvariant();
        var cacheKey = LinkCacheKeys.ResolveDestinationByAlias(normalizedAlias);
        var cachedDestination = await _cacheService.GetAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrWhiteSpace(cachedDestination))
        {
            return new ResolveShortUrlResult(cachedDestination);
        }

        var encryptedDestination = await _repository.GetEncryptedDestinationByAliasAsync(
            normalizedAlias,
            _timeProvider.GetUtcNow(),
            cancellationToken);

        if (string.IsNullOrWhiteSpace(encryptedDestination))
        {
            return null;
        }

        var decryptedDestination = Encoding.UTF8.GetString(
            _encryptionService.Decrypt(SecurityHelper.FromBase64ToBytes(encryptedDestination)));

        var cacheDuration = request.CacheDurationSeconds is > 0
            ? TimeSpan.FromSeconds(request.CacheDurationSeconds.Value)
            : LinkCachingDefaults.DefaultCacheDuration;

        await _cacheService.SetAsync(
            cacheKey,
            decryptedDestination,
            new CacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheDuration
            },
            cancellationToken);

        return new ResolveShortUrlResult(decryptedDestination);
    }
}
