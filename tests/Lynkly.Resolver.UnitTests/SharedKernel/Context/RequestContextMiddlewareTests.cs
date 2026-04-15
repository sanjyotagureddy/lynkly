using Lynkly.Resolver.API.Middlewares;
using Lynkly.Shared.Kernel.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using AppContext = Lynkly.Shared.Kernel.Context.AppContext;

namespace Lynkly.Resolver.UnitTests.SharedKernel.Context;

public sealed class RequestContextMiddlewareTests
{
    // ── InvokeAsync: guard clause ────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_WithNullHttpContext_ThrowsArgumentNullException()
    {
        var middleware = BuildMiddleware();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            middleware.InvokeAsync(null!));
    }

    // ── InvokeAsync: calls next ──────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_WithValidContext_CallsNextDelegate()
    {
        var nextCalled = false;
        var middleware = BuildMiddleware(next: _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(new DefaultHttpContext());

        Assert.True(nextCalled);
    }

    // ── InvokeAsync: request enricher invocation ─────────────────────────────

    [Fact]
    public async Task InvokeAsync_CallsEnricher_EnrichRequest()
    {
        var enricher = Substitute.For<IRequestContextEnricher>();
        var httpContext = new DefaultHttpContext();
        var middleware = BuildMiddleware(enrichers: new[] { enricher });

        await middleware.InvokeAsync(httpContext);

        enricher.Received(1).EnrichRequest(
            Arg.Is(httpContext),
            Arg.Any<AppContext>());
    }

    // ── InvokeAsync: response enricher invocation ────────────────────────────

    [Fact]
    public async Task InvokeAsync_CallsEnricher_EnrichResponse_OnResponseStart()
    {
        var enricher = Substitute.For<IRequestContextEnricher>();

        // DefaultHttpContext does not fire OnStarting callbacks via StartAsync, so
        // we swap in a custom IHttpResponseFeature that lets us fire them on demand.
        var httpContext = new DefaultHttpContext();
        var trackingFeature = new TrackingHttpResponseFeature();
        httpContext.Features.Set<IHttpResponseFeature>(trackingFeature);

        var middleware = BuildMiddleware(
            enrichers: new[] { enricher },
            next: async _ =>
            {
                await trackingFeature.FireOnStartingAsync();
            });

        await middleware.InvokeAsync(httpContext);

        enricher.Received(1).EnrichResponse(
            Arg.Any<HttpContext>(),
            Arg.Any<AppContext>());
    }

    // ── InvokeAsync: ambient scope ───────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_SetsRequestContextScope_DuringPipeline()
    {
        AppContext? capturedDuring = null;

        var middleware = BuildMiddleware(next: _ =>
        {
            capturedDuring = RequestContextScope.Current;
            return Task.CompletedTask;
        });

        Assert.Null(RequestContextScope.Current);

        await middleware.InvokeAsync(new DefaultHttpContext());

        Assert.NotNull(capturedDuring);
    }

    [Fact]
    public async Task InvokeAsync_ClearsRequestContextScope_AfterPipeline()
    {
        var middleware = BuildMiddleware();

        await middleware.InvokeAsync(new DefaultHttpContext());

        Assert.Null(RequestContextScope.Current);
    }

    // ── InvokeAsync: AppContext population ───────────────────────────────────

    [Fact]
    public async Task InvokeAsync_PopulatesMethod_FromRequest()
    {
        AppContext? captured = null;
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "DELETE";

        var middleware = BuildMiddleware(next: _ =>
        {
            captured = RequestContextScope.Current;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(httpContext);

        Assert.Equal("DELETE", captured!.Method);
    }

    [Fact]
    public async Task InvokeAsync_PopulatesPath_FromRequest()
    {
        AppContext? captured = null;
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/test";

        var middleware = BuildMiddleware(next: _ =>
        {
            captured = RequestContextScope.Current;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(httpContext);

        Assert.Equal("/api/test", captured!.Path);
    }

    [Fact]
    public async Task InvokeAsync_PopulatesCorrelationId_WhenHeaderPresent()
    {
        AppContext? captured = null;
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Correlation-Id"] = "corr-from-header";

        var middleware = BuildMiddleware(next: _ =>
        {
            captured = RequestContextScope.Current;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(httpContext);

        Assert.Equal("corr-from-header", captured!.CorrelationId);
    }

    [Fact]
    public async Task InvokeAsync_CorrelationIdIsNull_WhenHeaderAbsent()
    {
        AppContext? captured = null;
        var httpContext = new DefaultHttpContext();

        // No enrichers registered, so CorrelationId stays as-built from BuildAppContext
        var middleware = BuildMiddleware(
            enrichers: Enumerable.Empty<IRequestContextEnricher>(),
            next: _ =>
            {
                captured = RequestContextScope.Current;
                return Task.CompletedTask;
            });

        await middleware.InvokeAsync(httpContext);

        Assert.Null(captured!.CorrelationId);
    }

    [Fact]
    public async Task InvokeAsync_UsesGet_WhenRequestMethodIsEmpty()
    {
        AppContext? captured = null;
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = string.Empty;

        var middleware = BuildMiddleware(next: _ =>
        {
            captured = RequestContextScope.Current;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(httpContext);

        Assert.Equal(HttpMethods.Get, captured!.Method);
    }

    [Fact]
    public async Task InvokeAsync_UsesRootPath_WhenRequestPathIsEmpty()
    {
        AppContext? captured = null;
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = PathString.Empty;

        var middleware = BuildMiddleware(next: _ =>
        {
            captured = RequestContextScope.Current;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(httpContext);

        Assert.Equal("/", captured!.Path);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static RequestContextMiddleware BuildMiddleware(
        Func<HttpContext, Task>? next = null,
        IEnumerable<IRequestContextEnricher>? enrichers = null,
        string applicationName = "TestApp")
    {
        var env = Substitute.For<IHostEnvironment>();
        env.ApplicationName.Returns(applicationName);

        RequestDelegate nextDelegate = next is not null
            ? ctx => next(ctx)
            : _ => Task.CompletedTask;

        return new RequestContextMiddleware(
            nextDelegate,
            env,
            enrichers ?? new[] { new CorrelationIdRequestContextEnricher() });
    }

    /// <summary>
    /// A minimal <see cref="IHttpResponseFeature"/> that captures registered
    /// <c>OnStarting</c> callbacks and exposes a public method to fire them.
    /// DefaultHttpContext's built-in body feature does not fire OnStarting
    /// callbacks on StartAsync, so we replace the feature in tests that need it.
    /// </summary>
    private sealed class TrackingHttpResponseFeature : IHttpResponseFeature
    {
        private readonly List<(Func<object, Task> Callback, object State)> _onStarting = new();

        public int StatusCode { get; set; } = StatusCodes.Status200OK;
        public string? ReasonPhrase { get; set; }
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public bool HasStarted { get; private set; }

        // Not used in this context; required by the interface.
        public Stream Body
        {
            get => Stream.Null;
            set { }
        }

        public void OnStarting(Func<object, Task> callback, object state)
            => _onStarting.Add((callback, state));

        public void OnCompleted(Func<object, Task> callback, object state) { }

        /// <summary>Fires all registered OnStarting callbacks in registration order.</summary>
        public async Task FireOnStartingAsync()
        {
            HasStarted = true;
            foreach (var (callback, state) in _onStarting)
            {
                await callback(state);
            }
        }
    }
}
