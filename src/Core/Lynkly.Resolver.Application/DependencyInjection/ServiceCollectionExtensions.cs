using Lynkly.Resolver.Domain.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Resolver.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddResolverApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddResolverDomain();

        return services;
    }
}
