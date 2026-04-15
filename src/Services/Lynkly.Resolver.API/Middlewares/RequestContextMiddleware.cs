using System.Diagnostics;
using System.Security.Claims;
using Lynkly.Shared.Kernel.Context;
using Microsoft.Extensions.Hosting;
using AppContext = Lynkly.Shared.Kernel.Context.AppContext;

namespace Lynkly.Resolver.API.Middlewares;

internal sealed class RequestContextMiddleware(
    RequestDelegate next,
    IHostEnvironment environment,
    IEnumerable<IRequestContextEnricher> enrichers)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var appContext = BuildAppContext(httpContext, environment.ApplicationName);

        foreach (var enricher in enrichers)
        {
            enricher.EnrichRequest(httpContext, appContext);
        }

        using (RequestContextScope.BeginScope(appContext))
        {
            httpContext.Response.OnStarting(() =>
            {
                foreach (var enricher in enrichers)
                {
                    enricher.EnrichResponse(httpContext, appContext);
                }

                return Task.CompletedTask;
            });

            await next(httpContext);
        }
    }

    private static AppContext BuildAppContext(HttpContext httpContext, string applicationName)
    {
        var request = httpContext.Request;

        var correlationId = request.Headers.TryGetValue("X-Correlation-Id", out var correlationValue)
                            && !string.IsNullOrWhiteSpace(correlationValue.ToString())
            ? correlationValue.ToString()
            : null;

        var traceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;
        var method = string.IsNullOrWhiteSpace(request.Method) ? HttpMethods.Get : request.Method;
        var path = request.Path.HasValue ? request.Path.Value! : "/";
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = httpContext.User.Identity?.Name;
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = request.Headers.UserAgent.ToString();

        return AppContext.Create(
            applicationName,
            httpContext.TraceIdentifier,
            traceId,
            method,
            path,
            correlationId,
            userId,
            userName,
            clientIp,
            userAgent);
    }
}
