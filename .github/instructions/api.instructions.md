---
description: "Use when changing API host and endpoints under src/Services/Lynkly.Resolver.API/**."
applyTo: "src/Services/Lynkly.Resolver.API/**"
---

# API Instructions

## Scope

Applies to:

- `src/Services/Lynkly.Resolver.API/**`

## Endpoint Design

1. Use ASP.NET Core Minimal APIs consistently.
2. Keep endpoints thin: transport concerns only.
3. Delegate use-case behavior to Application layer handlers/services.
4. Keep endpoints explicit about request/response contracts.

## Routing and Contracts

1. Keep route naming and versioning predictable.
2. Return explicit status codes for success and failure paths.
3. Keep validation close to the slice and return clear problem details.
4. Avoid breaking contract changes unless explicitly requested.

## Middleware and Composition Root

1. Keep middleware ordering intentional and stable.
2. Register services in coherent order by layer responsibility.
3. Do not bypass tracing, logging, validation, auth, or error handling conventions.
4. Keep startup code readable and explicit.

## Performance and Reliability

1. Avoid heavy work inside request handlers, especially redirect paths.
2. Ensure redirect flow remains low-latency and non-blocking.
3. Move analytics side effects to asynchronous event processing.
4. Handle transient dependency failures with graceful responses.

## Security

1. Enforce HTTPS and secure headers as configured by the project.
2. Validate inputs and destination URL policies strictly.
3. Keep authn/authz checks explicit on management endpoints.
4. Never hardcode secrets, tokens, or environment-specific values.

## Testing Expectations

1. Add or update endpoint tests for changed routes and contracts.
2. Cover success, validation failure, and dependency-failure behavior.
3. Ensure API tests assert status codes and response shapes.

## Validation Checklist

1. `dotnet restore`
2. `dotnet build`
3. `dotnet test`
