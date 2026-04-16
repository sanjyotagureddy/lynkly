using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Shared.Kernel.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKernelCore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services;
    }
}
