using Lynkly.Resolver.Application.Abstractions.Persistence;
using Lynkly.Resolver.Domain.Links;
using Microsoft.EntityFrameworkCore;

namespace Lynkly.Resolver.Infrastructure.Persistence.Internal;

internal sealed class LinkReadRepository(AppDbContext dbContext) : ILinkReadRepository
{
    private readonly AppDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<string?> GetEncryptedDestinationByAliasAsync(
        string alias,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);
        var normalizedAlias = alias.Trim().ToLowerInvariant();

        return await _dbContext.LinkAliases
            .AsNoTracking()
            .Where(linkAlias => linkAlias.Alias == normalizedAlias)
            .Join(
                _dbContext.Links.AsNoTracking(),
                linkAlias => linkAlias.LinkId,
                link => link.Id,
                (_, link) => new
                {
                    link.DestinationUrl,
                    link.Status,
                    link.ExpiresAtUtc
                })
            .Where(link => link.Status == LinkStatus.Active)
            .Where(link => !link.ExpiresAtUtc.HasValue || link.ExpiresAtUtc > utcNow)
            .Select(link => link.DestinationUrl)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
