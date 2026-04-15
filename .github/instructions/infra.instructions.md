---
description: "Use when changing infrastructure code under src/Infrastructure/** (persistence, caching, messaging)."
applyTo: "src/Infrastructure/**"
---

# Infrastructure Instructions

## Scope

Applies to infrastructure adapters only:

- `src/Infrastructure/Lynkly.Resolver.Persistence/**`
- `src/Infrastructure/Lynkly.Resolver.Caching/**`
- `src/Infrastructure/Lynkly.Resolver.Messaging/**`

## Core Rules

1. Infrastructure implements interfaces from Application/Domain; it must not own business rules.
2. Keep broker-specific and storage-specific details isolated to Infrastructure.
3. Prefer explicit adapters and registrations over hidden magic.
4. Keep implementations replaceable (RabbitMQ today, Kafka later).
5. Keep infrastructure implementation types internal by default.
6. Expose only the public contracts needed for composition, options, or cross-assembly integration.
7. When internal infrastructure types must be unit-tested from a separate test project, expose them with `InternalsVisibleTo` in `Properties/AssemblyInfo.cs` of the production project.

## EF Core and Persistence

1. Keep DbContext, entity configurations, repositories, and migrations in Persistence.
2. Use explicit EF Core configurations; avoid leaky conventions when behavior matters.
3. Add migrations for schema changes and give migrations clear, intent-based names.
4. Validate migration creation and application paths for every persistence change.
5. Keep queries predictable; avoid accidental N+1 patterns and unbounded scans.
6. Keep mapping helpers, converters, and repository internals internal unless they must be public for EF Core or assembly composition.

## Caching

1. Use cache-aside for read-heavy paths unless explicitly justified otherwise.
2. Define deterministic cache key strategies; avoid ad-hoc key formats.
3. Set explicit TTL choices and document invalidation behavior.
4. Use negative caching carefully for not-found scenarios.
5. Do not cache sensitive data without encryption and policy approval.

## Messaging, Outbox, Retry, and DLQ

1. Keep message contracts stable and version-aware.
2. Use outbox for reliable dispatch of integration events.
3. Producers must use bounded retries with exponential backoff and jitter.
4. Consumers must be idempotent and safe under duplicate delivery.
5. Route poison messages to DLQ after bounded retries.
6. Ensure broker outages do not block critical request paths.

## Reliability and Observability

1. Add structured logs for key infrastructure transitions and failures.
2. Emit traces/metrics for DB, cache, queue, retries, and DLQ.
3. Add health checks for infrastructure dependencies where appropriate.
4. Ensure failure modes degrade gracefully and are observable.

## Dependency and Package Policy

1. Use open-source NuGet and Microsoft packages by default.
2. Do not introduce paid SDKs/services or proprietary lock-in without explicit approval.
3. Prefer mature, community-supported libraries over custom plumbing when reasonable.

## Validation Checklist

1. `dotnet restore`
2. `dotnet build`
3. `dotnet test`
4. Persistence changes: migration build/apply verification.
5. Messaging changes: retry, DLQ, and idempotency tests.
