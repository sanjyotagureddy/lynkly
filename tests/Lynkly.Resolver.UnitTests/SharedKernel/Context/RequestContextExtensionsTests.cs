using Lynkly.Resolver.API.Extensions;
using Lynkly.Resolver.API.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Lynkly.Resolver.UnitTests.SharedKernel.Context;

public sealed class RequestContextExtensionsTests
{
    // ── AddRequestContextSupport ─────────────────────────────────────────────

    [Fact]
    public void AddRequestContextSupport_WithNullServices_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            RequestContextExtensions.AddRequestContextSupport(null!));
    }

    [Fact]
    public void AddRequestContextSupport_RegistersCorrelationIdEnricher()
    {
        var services = new ServiceCollection();

        services.AddRequestContextSupport();

        using var provider = services.BuildServiceProvider();
        var enrichers = provider.GetServices<IRequestContextEnricher>().ToList();

        Assert.Single(enrichers);
        Assert.IsType<CorrelationIdRequestContextEnricher>(enrichers[0]);
    }

    [Fact]
    public void AddRequestContextSupport_CalledTwice_DoesNotDuplicateEnricher()
    {
        var services = new ServiceCollection();

        services.AddRequestContextSupport();
        services.AddRequestContextSupport();

        using var provider = services.BuildServiceProvider();
        var enrichers = provider.GetServices<IRequestContextEnricher>().ToList();

        Assert.Single(enrichers);
    }

    // ── UseRequestContext ────────────────────────────────────────────────────

    [Fact]
    public void UseRequestContext_WithNullApplicationBuilder_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            RequestContextExtensions.UseRequestContext(null!));
    }

    [Fact]
    public void UseRequestContext_WithValidBuilder_ReturnsBuilder()
    {
        var appBuilder = Substitute.For<IApplicationBuilder>();
        appBuilder.ApplicationServices.Returns(
            new ServiceCollection()
                .AddRequestContextSupport()
                .BuildServiceProvider());

        appBuilder
            .Use(Arg.Any<Func<RequestDelegate, RequestDelegate>>())
            .Returns(appBuilder);

        var result = appBuilder.UseRequestContext();

        Assert.NotNull(result);
    }
}
