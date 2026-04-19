# Lynkly Resolver Wiki

This wiki is the fast entry point for understanding and contributing to Lynkly Resolver.

## What Lynkly Resolver Is

Lynkly Resolver is a .NET 10 URL shortener built with:

- ASP.NET Core Minimal APIs
- Clean Architecture
- Vertical Slice Architecture (VSA)
- Event-driven analytics processing

Core rule: keep redirects fast and move analytics work to asynchronous processing.

## Start Here

1. Read the [project README](/README.md) for system goals and architecture.
2. Review the [technology stack](/docs/architecture/tech-stack.md) for approved tooling choices.
3. Use the structure below when navigating source code:
   - `src/Services/Lynkly.Resolver.API`: HTTP surface and composition root
   - `src/Core/Lynkly.Resolver.Application`: use-case orchestration
   - `src/Core/Lynkly.Resolver.Domain`: business model and rules
   - `src/Infrastructure/*`: persistence, caching, and messaging adapters
   - `src/Workers/Lynkly.Resolver.AnalyticsWorker`: async analytics consumption
   - `shared/Lynkly.Shared.Kernel.*`: reusable NuGet-ready primitives

## Local Validation Commands

Run these before submitting changes:

```bash
dotnet restore
dotnet build
dotnet test
```

## Operational Expectations

- Redirect flow must not block on analytics/event publication.
- Messaging consumers must be idempotent and retry-safe.
- Poison messages should flow to DLQ after bounded retries.
- Keep broker abstractions Kafka-portable even when implementing with RabbitMQ.

## Security and Abuse Baseline

- Enforce strict URL validation for destination links.
- Keep management APIs behind OAuth2/OIDC + JWT.
- Use rate limiting and abuse protections on public-facing flows.

## Testing Expectations

- Unit tests for domain and application logic.
- Integration tests for persistence, caching, messaging, and API boundaries.
- Cover happy paths and failure paths, especially retries and DLQ behavior.

