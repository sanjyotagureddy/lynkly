namespace Lynkly.Resolver.Application.Abstractions.Persistence;

public interface ILinkReadRepository
{
    Task<string?> GetEncryptedDestinationByAliasAsync(
        string alias,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken);
}
