---
description: "Use when changing reusable shared packages under shared/** (Lynkly.Shared.Kernel.*)."
applyTo: "shared/**"
---

# Shared.Kernel Instructions

## Scope

Applies to all reusable shared packages:

- `shared/Lynkly.Shared.Kernel.*`

## Design Intent

1. Shared.Kernel contains only reusable, general-purpose primitives.
2. Code here must be suitable for future NuGet extraction.
3. APIs should be small, stable, and easy to reason about.

## What Belongs Here

1. Abstractions and contracts used across multiple services.
2. Result/Error primitives and common exception types.
3. Validation helpers and guard abstractions.
4. Messaging/persistence abstractions that are implementation-agnostic.
5. Utility extensions that are generic and side-effect safe.

## What Must Not Be Added

1. Lynkly feature-specific business rules.
2. Endpoint-specific DTOs or request/response models.
3. Infrastructure-specific implementations (EF, Redis, RabbitMQ clients).
4. Project-local assumptions that cannot generalize.

## Dependency Rules

1. Avoid dependencies on service-layer projects.
2. Avoid dependencies that force infrastructure coupling.
3. Prefer BCL and Microsoft packages where possible.
4. Use open-source packages only when clearly justified and stable.

## API Stability Rules

1. Treat public types as long-lived contracts.
2. Prefer additive, backward-compatible changes.
3. Document and justify any breaking change explicitly.
4. Keep naming consistent and domain-neutral.

## Testing Expectations

1. Add focused unit tests for public behavior and edge cases.
2. Keep tests deterministic and minimal.
3. Validate no hidden coupling to service-specific concerns.

## Validation Checklist

1. `dotnet restore`
2. `dotnet build`
3. `dotnet test`
