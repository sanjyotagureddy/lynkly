# Technology Stack

This document is the source of truth for the Lynkly Resolver technology stack.

## Core Platform

- Runtime: .NET 10
- Web framework: ASP.NET Core Minimal APIs
- Architecture: Clean Architecture + Vertical Slice Architecture
- Domain model: framework-independent

## Persistence and Data Access

- ORM: Entity Framework Core
- Primary relational database: PostgreSQL
- Redirect cache: Redis
- Pattern: cache-aside for slug resolution
- Notes: keep redirect lookup data compact and prefer append-only analytics storage

## Data Model and Table Structure

Design the persistence model for global scale, tenant isolation, and fast redirects.

- Use `tenant_id` as the first partitioning and ownership key wherever tenant data exists.
- Use a surrogate primary key for core records, preferably a time-sortable identifier such as UUIDv7 or ULID.
- Keep the redirect lookup path separate from analytics-heavy data.
- Avoid making the public slug or alias the primary key if the value may change, be domain-scoped, or be reused across tenants.

Recommended core tables:

- `tenants`: tenant identity and lifecycle metadata.
- `custom_domains`: owned redirect domains and verification state.
- `links`: canonical link record with `link_id`, `tenant_id`, destination target, status, timestamps, and lifecycle fields.
- `link_aliases`: redirect lookup table with `tenant_id`, optional `domain_id`, `alias`, and `link_id`; enforce uniqueness on the logical lookup scope.
- `link_clicks`: append-only click events, partitioned by time, optimized for write throughput and retention policies.
- `link_rollups`: precomputed analytics aggregates by link and time bucket for fast reporting.
- `outbox_messages`: durable event dispatch records for reliable messaging.

Design notes:

- Put unique indexes on the redirect lookup keys instead of on a wide canonical table.
- Keep `link_clicks` write-optimized and avoid joins on the redirect path.
- Rebuild rollups from click history if needed; treat rollups as derived state.
- Prefer soft-delete or status flags for operational records rather than immediate hard deletes when auditability matters.
- Partition click and rollup tables by time so retention, archival, and reprocessing stay manageable.
- Treat `Blocked` as an admin/abuse lifecycle state. Redirects must not resolve blocked links and should return `410 Gone` from the public redirect surface.
- Keep `status` indexed for operational queries, and consider partial indexes around active redirect lookups so blocked or archived rows do not burden the hot path.

## Messaging and Background Processing

- Broker: RabbitMQ
- Messaging library: MassTransit
- Reliability pattern: outbox for publish durability
- Consumer behavior: idempotent handlers, bounded retries, DLQ on final failure
- Portability: keep broker-specific code behind application-facing abstractions so Kafka remains a later option

## Resilience

- Outbound HTTP resilience: Polly v8 through Microsoft.Extensions.Http.Resilience
- Dependency boundaries: apply retries, timeouts, and circuit breakers only at infrastructure edges
- Redirect path rule: never block redirects on downstream publish or telemetry failures

## Validation

- Request and command validation: FluentValidation
- Domain invariants: enforce in value objects and entities, not only at the transport layer
- Failure shape: return consistent ProblemDetails responses for validation failures

## Authentication and Authorization

- Public redirect endpoint: anonymous
- Management APIs: OAuth2 / OpenID Connect with JWT bearer tokens
- Authorization model: policy-based and claim-driven
- Identity provider: Keycloak
- Identity provider role: external IdP preferred over custom auth

## Testing and Fakes

- Test framework: xUnit
- Test data generation: Bogus
- Test doubles: hand-written fakes first
- Mocking library: NSubstitute if interaction verification is needed
- Avoid: mixing multiple mocking libraries in the same test suite
- Integration dependencies: Testcontainers for database, cache, and broker-backed tests

## Observability

- Tracing, metrics, and logs: OpenTelemetry
- Cross-cutting correlation: trace IDs through API, cache, broker, and worker paths

## Approved Baseline Packages

The exact package versions should be pinned in the solution once projects exist, but the approved library families are:

- Microsoft.AspNetCore.*
- Microsoft.EntityFrameworkCore.*
- Microsoft.Extensions.Http.Resilience
- FluentValidation
- Polly
- MassTransit
- StackExchange.Redis
- OpenTelemetry.*
- Bogus
- xUnit
- Testcontainers
- NSubstitute

## Package Management

- Use Central Package Management for all .NET projects.
- Store package versions in Directory.Packages.props at the repository root.
- Use Directory.Build.props for shared build properties.
- Keep CentralPackageVersionOverrideEnabled set to false to avoid per-project version drift.

## Implementation Rules

- Keep runtime concerns at infrastructure boundaries.
- Keep business rules in Domain and Application.
- Keep redirect latency independent from messaging and analytics.
- Prefer explicit, small abstractions over large framework wrappers.
- Do not add Moq and NSubstitute together; standardize on one mocking approach.
