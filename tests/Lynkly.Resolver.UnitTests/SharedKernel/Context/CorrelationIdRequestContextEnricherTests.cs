using Lynkly.Resolver.API.Middlewares;
using Lynkly.Shared.Kernel.Context;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using AppContext = Lynkly.Shared.Kernel.Context.AppContext;

namespace Lynkly.Resolver.UnitTests.SharedKernel.Context;

public sealed class CorrelationIdRequestContextEnricherTests
{
    private readonly CorrelationIdRequestContextEnricher _enricher = new();

    // ── EnrichRequest: guard clauses ─────────────────────────────────────────

    [Fact]
    public void EnrichRequest_WithNullHttpContext_ThrowsArgumentNullException()
    {
        var ctx = MakeAppContext();

        Assert.Throws<ArgumentNullException>(() =>
            _enricher.EnrichRequest(null!, ctx));
    }

    [Fact]
    public void EnrichRequest_WithNullAppContext_ThrowsArgumentNullException()
    {
        var httpContext = new DefaultHttpContext();

        Assert.Throws<ArgumentNullException>(() =>
            _enricher.EnrichRequest(httpContext, null!));
    }

    // ── EnrichRequest: correlation logic ────────────────────────────────────

    [Fact]
    public void EnrichRequest_WhenCorrelationIdAlreadySet_DoesNotOverride()
    {
        var httpContext = new DefaultHttpContext();
        var ctx = MakeAppContext(correlationId: "existing-id");

        _enricher.EnrichRequest(httpContext, ctx);

        Assert.Equal("existing-id", ctx.CorrelationId);
    }

    [Fact]
    public void EnrichRequest_WhenHeaderPresent_UsesHeaderValue()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Correlation-Id"] = "header-corr-id";
        var ctx = MakeAppContext();

        _enricher.EnrichRequest(httpContext, ctx);

        Assert.Equal("header-corr-id", ctx.CorrelationId);
    }

    [Fact]
    public void EnrichRequest_WhenHeaderIsWhitespace_GeneratesNewId()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Correlation-Id"] = "   ";
        var ctx = MakeAppContext();

        _enricher.EnrichRequest(httpContext, ctx);

        Assert.NotNull(ctx.CorrelationId);
        Assert.NotEmpty(ctx.CorrelationId);
        Assert.NotEqual("   ", ctx.CorrelationId);
    }

    [Fact]
    public void EnrichRequest_WhenNoHeaderAndNoExistingId_GeneratesNewGuid()
    {
        var httpContext = new DefaultHttpContext();
        var ctx = MakeAppContext();

        _enricher.EnrichRequest(httpContext, ctx);

        Assert.NotNull(ctx.CorrelationId);
        Assert.True(Guid.TryParseExact(ctx.CorrelationId, "N", out _),
            "Expected a 32-character lowercase hex GUID");
    }

    // ── EnrichResponse: guard clauses ────────────────────────────────────────

    [Fact]
    public void EnrichResponse_WithNullHttpContext_ThrowsArgumentNullException()
    {
        var ctx = MakeAppContext();

        Assert.Throws<ArgumentNullException>(() =>
            _enricher.EnrichResponse(null!, ctx));
    }

    [Fact]
    public void EnrichResponse_WithNullAppContext_ThrowsArgumentNullException()
    {
        var httpContext = new DefaultHttpContext();

        Assert.Throws<ArgumentNullException>(() =>
            _enricher.EnrichResponse(httpContext, null!));
    }

    // ── EnrichResponse: header writing ──────────────────────────────────────

    [Fact]
    public void EnrichResponse_WhenCorrelationIdIsSet_WritesResponseHeader()
    {
        var httpContext = new DefaultHttpContext();
        var ctx = MakeAppContext(correlationId: "resp-corr");

        _enricher.EnrichResponse(httpContext, ctx);

        Assert.Equal("resp-corr", httpContext.Response.Headers["X-Correlation-Id"].ToString());
    }

    [Fact]
    public void EnrichResponse_WhenCorrelationIdIsNull_DoesNotWriteHeader()
    {
        var httpContext = new DefaultHttpContext();
        var ctx = MakeAppContext();

        _enricher.EnrichResponse(httpContext, ctx);

        Assert.False(httpContext.Response.Headers.ContainsKey("X-Correlation-Id"));
    }

    [Fact]
    public void EnrichResponse_WhenCorrelationIdIsWhitespace_DoesNotWriteHeader()
    {
        var httpContext = new DefaultHttpContext();
        var ctx = MakeAppContext();
        ctx.CorrelationId = "  ";

        _enricher.EnrichResponse(httpContext, ctx);

        Assert.False(httpContext.Response.Headers.ContainsKey("X-Correlation-Id"));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static AppContext MakeAppContext(string? correlationId = null)
    {
        var ctx = AppContext.Create("app", "req-1", "trace-1", "GET", "/");
        ctx.CorrelationId = correlationId;
        return ctx;
    }
}
