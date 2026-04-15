# Lynkly.Shared.Kernel.Context

A framework-agnostic, NuGet-ready library for ambient per-request context propagation.

---

## Overview

| Type | Responsibility |
|------|---------------|
| `AppContext` | Immutable-by-contract data model capturing request metadata (app name, request/trace IDs, HTTP method/path, optional user/client info, extensible key/value items). Created via a BCL-only static factory — **no ASP.NET Core dependency**. |
| `RequestContextScope` | `AsyncLocal<AppContext>`-based ambient scope. Establishes and restores context cleanly across `await` chains. |

> **Design principle** — The shared library carries zero ASP.NET Core dependencies so it remains extractable as a standalone NuGet package.
> The responsibility of reading `HttpContext` and calling `AppContext.Create(...)` belongs exclusively to the API-project middleware (`RequestContextMiddleware`).

---

## How Validation Works

`AppContext.Create(...)` enforces guards on every required parameter:

| Parameter | Rule | Exception |
|-----------|------|-----------|
| `applicationName` | Must not be `null` or whitespace | `ArgumentException` |
| `requestId` | Must not be `null` or whitespace | `ArgumentException` |
| `traceId` | Must not be `null` or whitespace | `ArgumentException` |
| `method` | Must not be `null` or whitespace | `ArgumentException` |
| `path` | Must not be `null` or whitespace | `ArgumentException` |

Optional parameters (`correlationId`, `userId`, `userName`, `clientIp`, `userAgent`) may be `null`.

`AppContext.SetItem(key, value)` guards:

| Parameter | Rule | Exception |
|-----------|------|-----------|
| `key` | Must not be `null` or whitespace | `ArgumentException` |

`AppContext.TryGetItem(key, out value)` with a `null` or whitespace key **silently returns `false`** rather than throwing.

`RequestContextScope.BeginScope(appContext)` guards:

| Parameter | Rule | Exception |
|-----------|------|-----------|
| `appContext` | Must not be `null` | `ArgumentNullException` |

---

## Exception Flow

```
AppContext.Create(null!, ...)
    └─► ArgumentException("Application name is required.", "applicationName")

AppContext.Create("app", null!, ...)
    └─► ArgumentException("Request ID is required.", "requestId")

AppContext.Create("app", "req", null!, ...)
    └─► ArgumentException("Trace ID is required.", "traceId")

AppContext.Create("app", "req", "trace", null!, ...)
    └─► ArgumentException("HTTP method is required.", "method")

AppContext.Create("app", "req", "trace", "GET", null!)
    └─► ArgumentException("Request path is required.", "path")

ctx.SetItem(null!, "value")
    └─► ArgumentException("Item key is required.", "key")

RequestContextScope.BeginScope(null!)
    └─► ArgumentNullException("appContext")
```

---

## Example Usage

### 1 — Create an `AppContext` (pure BCL)

```csharp
var ctx = AppContext.Create(
    applicationName: "lynkly-resolver",
    requestId:       "00-abc123def456-01",
    traceId:         "abc123def456",
    method:          "GET",
    path:            "/r/my-link",
    correlationId:   "corr-7f3a",        // optional
    userId:          "user-42",          // optional
    userName:        "alice",            // optional
    clientIp:        "192.168.1.100",    // optional
    userAgent:       "Mozilla/5.0"       // optional
);
```

### 2 — Establish an ambient scope

```csharp
using (RequestContextScope.BeginScope(ctx))
{
    await SomeAsyncOperation();
    // Anywhere inside the async call stack:
    var ambient = RequestContextScope.Current; // same ctx instance
}
// scope is disposed; Current returns to its previous value (null if none)
```

### 3 — Attach custom key/value items

```csharp
ctx.SetItem("tenant-id", "acme-corp");
ctx.SetItem("feature-flag", "new-ui");

if (ctx.TryGetItem("tenant-id", out var tenantId))
{
    // tenantId == "acme-corp"
}
```

Keys are **case-insensitive** — `"Tenant-Id"` and `"tenant-id"` refer to the same slot.

### 4 — Nested scopes

```csharp
var outer = AppContext.Create("outer-svc", "r1", "t1", "GET", "/");
var inner = AppContext.Create("inner-svc", "r2", "t2", "POST", "/sub");

using (RequestContextScope.BeginScope(outer))
{
    // Current == outer

    using (RequestContextScope.BeginScope(inner))
    {
        // Current == inner
    }

    // Current == outer (restored)
}

// Current == null (restored)
```

---

## API Middleware Integration (`Lynkly.Resolver.API`)

The API layer bridges `HttpContext` → `AppContext` without coupling the shared library to ASP.NET Core.

### Registration

```csharp
// Program.cs
builder.Services.AddRequestContextSupport();   // registers IRequestContextEnricher(s)
// ...
app.UseRequestContext();                       // early in the pipeline, before auth
```

### What the middleware does

1. Reads `HttpContext` fields (method, path, headers, user claims, connection info).
2. Calls `AppContext.Create(...)` with primitive values — the shared library is never handed an `HttpContext`.
3. Runs all registered `IRequestContextEnricher` implementations (`EnrichRequest`).
4. Establishes `RequestContextScope.BeginScope(appContext)` for the lifetime of the request.
5. Registers `OnStarting` callbacks to run `EnrichResponse` before response headers are sent.
6. Tears down the scope on completion.

### Default enricher: `CorrelationIdRequestContextEnricher`

| Situation | Behaviour |
|-----------|-----------|
| `appContext.CorrelationId` already set | no-op |
| `X-Correlation-Id` request header present and non-whitespace | copies header value to `appContext.CorrelationId` |
| No header, no existing value | generates a new 32-char lowercase hex `Guid` |
| **Response** — `CorrelationId` is set | writes `X-Correlation-Id` response header |
| **Response** — `CorrelationId` is null/whitespace | header is not written |

### Custom enrichers

```csharp
internal sealed class TenantEnricher : IRequestContextEnricher
{
    public void EnrichRequest(HttpContext httpContext, AppContext appContext)
    {
        if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantId))
        {
            appContext.SetItem("tenant-id", tenantId.ToString());
        }
    }

    public void EnrichResponse(HttpContext httpContext, AppContext appContext) { }
}

// Registration (before AddRequestContextSupport so TryAddEnumerable works):
services.AddSingleton<IRequestContextEnricher, TenantEnricher>();
services.AddRequestContextSupport();
```

---

## Design Principles

| Principle | Detail |
|-----------|--------|
| **No ASP.NET Core dependency in the shared library** | `AppContext` and `RequestContextScope` use only BCL types; the library is NuGet-extractable. |
| **Ambient propagation** | `AsyncLocal<T>` carries context across `await` chains without explicit parameter injection. |
| **Scope nesting** | Inner scopes correctly restore the outer scope on disposal. Double-dispose is a no-op. |
| **Extensibility** | Register additional `IRequestContextEnricher` implementations to attach domain or infrastructure metadata. |
| **Validation at the boundary** | Guards are applied at object construction, not scattered through consumers. |
