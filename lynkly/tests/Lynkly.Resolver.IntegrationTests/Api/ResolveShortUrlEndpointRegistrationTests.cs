using Lynkly.Resolver.API.Endpoints.Links;
using Lynkly.Resolver.Application.UseCases.Links.ResolveShortUrl;
using Lynkly.Shared.Kernel.MediatR.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Resolver.IntegrationTests.Api;

public sealed class ResolveShortUrlEndpointRegistrationTests
{
    [Fact]
    public void ResolveShortUrlEndpoint_RegistersGetRouteUsingIEndpointPattern()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton<IMediator, FakeMediator>();
        var app = builder.Build();

        var endpoint = new ResolveShortUrlEndpoint();
        endpoint.MapEndpoints(app);

        var routeEndpoint = Assert.Single(
            ((IEndpointRouteBuilder)app).DataSources
                .SelectMany(dataSource => dataSource.Endpoints)
                .OfType<RouteEndpoint>(),
            candidate => candidate.RoutePattern.RawText == "/{alias}");

        var methods = routeEndpoint.Metadata.GetMetadata<HttpMethodMetadata>();
        Assert.NotNull(methods);
        Assert.Contains("GET", methods!.HttpMethods);
    }

    [Fact]
    public async Task ResolveShortUrlEndpoint_ForwardsHeaderOverrideToMediator()
    {
        var fakeMediator = new FakeMediator();
        var (routeEndpoint, serviceProvider) = BuildResolveRouteEndpoint(fakeMediator);
        var httpContext = CreateHttpContext(serviceProvider, "promo", "45");

        await routeEndpoint.RequestDelegate!(httpContext);

        Assert.NotNull(fakeMediator.LastResolveQuery);
        Assert.Equal("promo", fakeMediator.LastResolveQuery!.Alias);
        Assert.Equal(45, fakeMediator.LastResolveQuery.CacheDurationSeconds);
        Assert.Equal(StatusCodes.Status302Found, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task ResolveShortUrlEndpoint_UsesDefaultCacheDuration_WhenHeaderMissing()
    {
        var fakeMediator = new FakeMediator();
        var (routeEndpoint, serviceProvider) = BuildResolveRouteEndpoint(fakeMediator);
        var httpContext = CreateHttpContext(serviceProvider, "promo");

        await routeEndpoint.RequestDelegate!(httpContext);

        Assert.NotNull(fakeMediator.LastResolveQuery);
        Assert.Equal("promo", fakeMediator.LastResolveQuery!.Alias);
        Assert.Null(fakeMediator.LastResolveQuery.CacheDurationSeconds);
        Assert.Equal(StatusCodes.Status302Found, httpContext.Response.StatusCode);
    }

    private static (RouteEndpoint Endpoint, IServiceProvider Services) BuildResolveRouteEndpoint(FakeMediator mediator)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton<IMediator>(mediator);
        var app = builder.Build();

        var endpoint = new ResolveShortUrlEndpoint();
        endpoint.MapEndpoints(app);

        var routeEndpoint = Assert.Single(
            ((IEndpointRouteBuilder)app).DataSources
                .SelectMany(dataSource => dataSource.Endpoints)
                .OfType<RouteEndpoint>(),
            candidate => candidate.RoutePattern.RawText == "/{alias}");

        return (routeEndpoint, app.Services);
    }

    private static DefaultHttpContext CreateHttpContext(IServiceProvider serviceProvider, string alias, string? cacheExpiryHeader = null)
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            Response =
            {
                Body = new MemoryStream()
            }
        };

        httpContext.Request.Method = "GET";
        httpContext.Request.Path = $"/{alias}";
        httpContext.Request.RouteValues["alias"] = alias;
        if (cacheExpiryHeader is not null)
        {
            httpContext.Request.Headers["X-Cache-Expiry"] = cacheExpiryHeader;
        }

        return httpContext;
    }

    private sealed class FakeMediator : IMediator
    {
        public ResolveShortUrlQuery? LastResolveQuery { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object? response = request switch
            {
                ResolveShortUrlQuery query => CaptureAndReturn(query),
                _ => default(TResponse)
            };

            return Task.FromResult((TResponse)response!);
        }

        private ResolveShortUrlResult CaptureAndReturn(ResolveShortUrlQuery query)
        {
            LastResolveQuery = query;
            return new ResolveShortUrlResult("https://example.com");
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task Publish(INotification notification, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public async IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }
    }
}
