---
name: vertical-slice-scaffold
description: "Use when creating a new feature slice in Lynkly Resolver using Minimal APIs, Clean Architecture, and Vertical Slice Architecture."
---

# Vertical Slice Scaffold

## Purpose

Create consistent feature slices that follow Lynkly standards:

- Minimal API transport
- Application orchestration
- Domain-centric business rules
- Infrastructure through abstractions
- Tests from day one

## Use This Skill When

- User asks to add a new feature/use case.
- User asks to scaffold endpoint + handler + validator.
- User asks for a new slice under CreateLink, ResolveLink, analytics, or similar.

## Required Slice Shape

At minimum, include:

- Endpoint (Minimal API route mapping)
- Request/response contract
- Validator
- Command/query
- Handler
- Tests (unit/integration as appropriate)

Suggested organization:

- src/Core/Lynkly.Resolver.Application/UseCases/<Feature>/<Slice>
- src/Services/Lynkly.Resolver.API/Endpoints/<Feature>
- tests/Lynkly.Resolver.UnitTests/<Feature>
- tests/Lynkly.Resolver.IntegrationTests/<Feature> (if boundary behavior is added)

## Workflow

1. Confirm feature intent, inputs, outputs, and failure cases.
2. Define route and response contract first.
3. Add validator near the slice and keep endpoint thin.
4. Put orchestration in Application handler.
5. Keep Domain rules in Domain types/services.
6. Add abstractions for persistence/cache/messaging as needed.
7. Implement infrastructure adapters only after contracts are clear.
8. Add tests for happy and failure paths.
9. Validate build/tests and coverage impact.

## Quality Gates

- No business logic in API transport code.
- No infrastructure coupling in Domain.
- Messaging side effects are async and non-blocking for redirect path.
- Retry/idempotency/DLQ behavior is considered for message consumers.
- Coverage floor remains >= 80% repository-wide.

## Output Expectations

When using this skill, return:

1. Slice structure created
2. Files added/updated and why
3. Architectural decisions made
4. Validation commands executed
5. Follow-up tasks (if any)

## Dependency Policy

- Prefer BCL, ASP.NET Core, EF Core, and mature open-source NuGet packages.
- Avoid paid SDKs/services by default.
- Keep broker details abstracted so RabbitMQ implementation can later be swapped for Kafka.
