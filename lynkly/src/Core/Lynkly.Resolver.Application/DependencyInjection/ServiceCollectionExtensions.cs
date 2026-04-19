using Lynkly.Shared.Kernel.MediatR.Extensions;
using Lynkly.Resolver.Application.UseCases.Links.CreateShortUrl;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Resolver.Application.DependencyInjection;

public static class ModuleRegistration
{
    public static IServiceCollection AddResolverApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddLynklyMediator(typeof(ModuleRegistration).Assembly);
        services.AddSingleton<IShortAliasGenerator, Sha256ShortAliasGenerator>();

        return services;
    }
}
