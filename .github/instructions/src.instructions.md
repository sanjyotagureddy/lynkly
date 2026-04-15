---
description: "Use when changing files under src/** in Lynkly Resolver."
applyTo: "src/**"
---

# Source Layer Instructions

These instructions apply to all source code under `src/**` in this repository.

## Repository Context

This repository is a .NET 10 URL shortener built with Minimal APIs, Clean Architecture, Vertical Slice Architecture (VSA), EF Core, and an event-driven analytics pipeline.

Follow the intended boundaries:

- `src/Services/Lynkly.Resolver.API`: composition root, HTTP surface, middleware, endpoint mapping.
- `src/Core/Lynkly.Resolver.Domain`: entities, value objects, enums, rules, domain events.
- `src/Core/Lynkly.Resolver.Application`: use cases, contracts, DTOs, orchestrators, validation.
- `src/Infrastructure/Lynkly.Resolver.Persistence`: EF Core DbContext, mappings, repositories, migrations.
- `src/Infrastructure/Lynkly.Resolver.Caching`: Redis abstractions and cache services.
- `src/Infrastructure/Lynkly.Resolver.Messaging`: broker integration, producers, consumers, events, outbox.
- `src/Workers/Lynkly.Resolver.AnalyticsWorker`: async consumers and analytics aggregation.
- `shared/Lynkly.Shared.Kernel.*`: reusable primitives that must remain NuGet-ready.

## Dependency Direction

Preserve these boundaries:

- API -> Application -> Domain
- Infrastructure -> Application/Domain through interfaces only
- Shared.Kernel -> no application-specific dependencies
- Domain -> framework-light and persistence-agnostic

## Non-Negotiable Development Rules

1. Keep changes minimal and scoped to the request.
2. Do not refactor unrelated files in the same change.
3. Preserve existing public contracts unless explicitly asked to change them.
4. Keep naming and folder placement aligned with the repo structure.
5. Do not bypass middleware, validation, logging, tracing, or error handling conventions.
6. Use only open-source NuGet packages and Microsoft packages by default; avoid paid SDKs, paid services, and proprietary dependencies unless explicitly requested.
7. Prefer library choices that can be replaced or extracted later without redesigning the application.
8. Keep business logic inside Domain and Application, not in endpoints, middleware, or infrastructure glue.
9. If a solution can be built with the BCL, EF Core, ASP.NET Core, or a mature open-source NuGet package, prefer that over custom plumbing.

## Technology Defaults

Use these defaults unless the task explicitly says otherwise:

- .NET 10
- ASP.NET Core Minimal APIs
- EF Core for persistence
- Redis for hot-path caching
- RabbitMQ as the current message broker, but keep abstractions broker-agnostic so Kafka can be swapped in later
- OpenTelemetry for tracing, metrics, and logs
- Testcontainers for integration test dependencies when helpful

Do not introduce paid observability, paid queues, paid auth providers, or paid infrastructure products as required dependencies.

## Architecture Rules

1. Treat Minimal API endpoints as thin transport adapters.
2. Use VSA so each feature owns its endpoint, request/response contract, validation, handler, and tests.
3. Keep Application focused on orchestration, policy, and use-case logic.
4. Keep Domain pure: no EF Core, no ASP.NET Core, no Redis, no messaging clients.
5. Keep Infrastructure isolated behind interfaces and registrations.
6. Keep Shared.Kernel small, stable, and reusable enough to become a NuGet package later if needed.
7. Prefer explicit abstractions over premature generic frameworks.

## API Layer Rules

When adding or changing endpoints:

1. Implement Minimal API endpoints or feature endpoint classes consistently with the repo style.
2. Keep route mappings versioned and predictable.
3. Keep request validation near the feature, not only in the transport layer.
4. Return clear, explicit HTTP status codes and response contracts.
5. Keep endpoint methods short; delegate to Application use cases.
6. Avoid Controllers unless a specific framework requirement makes them necessary.

## Application Layer Rules

1. Model features as use cases, not service bags.
2. Keep each slice self-contained: command/query, validator, handler, response contract, tests.
3. Keep infrastructure details out of Application logic.
4. Inject abstractions, not concrete services.
5. Prefer explicit orchestration over implicit side effects.
6. Keep mapping logic simple and local to the slice unless it is truly shared.

## Domain Layer Rules

1. Put the business model in Domain: entities, value objects, rules, and domain events.
2. Keep domain methods intention-revealing and side-effect free where possible.
3. Use strongly typed identifiers and value objects when they improve correctness.
4. Do not leak persistence or transport concerns into Domain.
5. Keep shared constants and domain errors centralized and reusable.

## Infrastructure Layer Rules

1. Keep EF Core DbContext, entity configuration, repositories, and migrations in Infrastructure.
2. Use migrations for schema changes; do not use runtime schema mutation.
3. Keep caching, messaging, and external integrations behind interfaces.
4. Keep outbox and consumer implementations idempotent and retry-safe.
5. Keep broker-specific code isolated so RabbitMQ can be replaced later without changing business logic.

## Shared.Kernel Rules

1. Shared.Kernel is for reusable primitives only.
2. Do not add feature-specific code there.
3. Favor general-purpose abstractions, results, exceptions, validation helpers, logging helpers, and persistence primitives.
4. Keep these packages free of infrastructure coupling so they can be extracted into NuGet packages later.
5. If a helper depends on Lynkly-specific behavior, it does not belong in Shared.Kernel.

## Eventing, Retry, and DLQ Rules

1. Treat the event pipeline as broker-agnostic at the application boundary.
2. RabbitMQ is the current implementation choice, but messaging abstractions must not prevent a future Kafka swap.
3. Redirect paths must never wait on event publication.
4. Use the outbox pattern for reliable event dispatch.
5. Producers should use bounded retries with exponential backoff and jitter.
6. Consumers should be idempotent and should route poison messages to a DLQ after bounded retries.
7. Treat DLQ metrics as operational signals, not as optional logging.

## Testing and Validation Rules

Before considering a change complete, run and pass the relevant validation:

1. `dotnet restore`
2. `dotnet build`
3. `dotnet test`

If a change affects persistence, also validate:

1. EF Core migrations build and apply cleanly.
2. Startup and registration paths still work.
3. Integration tests cover the changed persistence behavior.

If a change affects messaging, also validate:

1. Producer and consumer tests.
2. Retry and DLQ behavior.
3. Idempotency and duplicate-delivery handling.

## Testing Standards

1. Prefer unit tests for domain and pure application logic.
2. Prefer integration tests for EF Core, Redis, and messaging boundaries.
3. Prefer architecture tests to enforce layer boundaries and dependency direction.
4. Use deterministic tests only: fixed clocks, seeded data, isolated dependencies.
5. Test both happy paths and failure paths.
6. Avoid flaky tests; zero tolerance in protected branches.
7. Aim for 100% coverage on core domain and critical application slices.
8. Use coverage as a gate, but verify quality with meaningful assertions and mutation testing where it matters.

## Design and Development Standards

1. Keep code small, direct, and explicit.
2. Prefer composition over inheritance unless the domain truly needs inheritance.
3. Prefer clear names over clever abstractions.
4. Keep side effects isolated.
5. Avoid over-engineering: build the simplest thing that satisfies the requirement and remains maintainable.
6. Do not introduce new architectural patterns without a strong reason.
7. Do not move business rules into middleware, handlers, or infrastructure wrappers just to reduce perceived coupling.

## Access Modifier and Test Visibility Rules

1. Keep implementation types internal by default.
2. Keep interfaces public when they define cross-assembly contracts.
3. Make classes public only when they are true external contracts for another assembly.
4. When internal implementation types must be unit-tested from a separate test project, expose them via `InternalsVisibleTo` in the project under test.
5. Add `InternalsVisibleTo` in `Properties/AssemblyInfo.cs` of the production project, not in test projects.

## What to Avoid

1. Do not introduce paid dependencies, paid SDKs, or paid cloud services as required parts of the design.
2. Do not add vendor lock-in where an open-source or built-in option works well.
3. Do not put business logic in endpoints, middleware, or infrastructure plumbing.
4. Do not create cross-layer dependencies that break the architecture.
5. Do not ignore build, test, or migration failures in final reporting.
6. Do not expand Shared.Kernel with application-specific code.

## Change Checklists

### Adding a new API feature

1. Add a new vertical slice under the feature area.
2. Keep the endpoint thin.
3. Put orchestration in the Application layer.
4. Keep persistence and messaging behind Infrastructure abstractions.
5. Add tests for success, failure, and validation paths.

### Adding or changing persistence

1. Update EF Core configuration in Infrastructure.
2. Add a migration with a clear name.
3. Verify the migration applies cleanly.
4. Add or update integration tests.

### Adding or changing messaging

1. Keep the broker behind an interface.
2. Avoid broker-specific assumptions in Domain or Application.
3. Add retry, idempotency, and DLQ handling.
4. Test duplicate delivery and failure recovery.

### Adding reusable shared code

1. Add only genuinely reusable primitives to Shared.Kernel.
2. Keep the API small and stable.
3. Ensure the code has no project-specific dependencies.
4. Write it as if it could become its own NuGet package.
