using FluentValidation;
using Lynkly.Resolver.Application.UseCases.Links.CreateShortUrl;
using Lynkly.Resolver.API.Endpoints.Links;
using Lynkly.Shared.Kernel.MediatR.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Lynkly.Resolver.IntegrationTests.Api;

public sealed class CreateShortUrlEndpointRegistrationTests
{
    [Fact]
    public void CreateShortUrlEndpoint_RegistersPostRouteUsingIEndpointPattern()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton<IMediator, FakeMediator>();
        builder.Services.AddSingleton<IValidator<CreateShortUrlCommand>, CreateShortUrlCommandValidator>();
        var app = builder.Build();

        var endpoint = new CreateShortUrlEndpoint();
        endpoint.MapEndpoints(app);

        var routeEndpoint = Assert.Single(
            ((IEndpointRouteBuilder)app).DataSources
                .SelectMany(dataSource => dataSource.Endpoints)
                .OfType<RouteEndpoint>(),
            candidate => candidate.RoutePattern.RawText is "/api/v1/links/" or "/api/v1/links");

        var methods = routeEndpoint.Metadata.GetMetadata<HttpMethodMetadata>();
        Assert.NotNull(methods);
        Assert.Contains("POST", methods!.HttpMethods);
    }

    private sealed class FakeMediator : IMediator
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(default(TResponse)!);
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
