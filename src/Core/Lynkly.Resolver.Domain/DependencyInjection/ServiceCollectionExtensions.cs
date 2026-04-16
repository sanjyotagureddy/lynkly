using Lynkly.Shared.Kernel.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Resolver.Domain.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddResolverDomain(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddKernelCore();

        return services;
    }
}
