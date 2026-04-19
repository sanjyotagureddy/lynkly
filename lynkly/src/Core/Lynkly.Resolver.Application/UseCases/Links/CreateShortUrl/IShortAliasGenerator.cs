using Lynkly.Resolver.Domain.Links;

namespace Lynkly.Resolver.Application.UseCases.Links.CreateShortUrl;

public interface IShortAliasGenerator
{
    string Generate(TenantId tenantId, string originalUrl, int attempt);
}
