---
name: pr-review-gate
description: "Use when reviewing pull requests, pre-merge checks, release readiness, or when the user asks for a review of changes in Lynkly Resolver."
---

# PR Review Gate

## Purpose

Run a consistent, high-signal review workflow for Lynkly Resolver changes with emphasis on:

- Architecture boundaries
- Correctness and regressions
- Retry, idempotency, and DLQ safety
- Test completeness and coverage policy
- Operational readiness and observability

## Use This Skill When

- User asks for a review.
- User asks if a change is ready to merge.
- User asks for release readiness or production-readiness check.
- A change touches critical flows (redirect path, persistence, messaging, worker processing).

## Review Workflow

1. Identify changed files and map each file to its layer.
2. Validate architecture direction:
   - API -> Application -> Domain
   - Infrastructure depends on abstractions, not business logic leakage.
3. Check behavior risks first:
   - Bugs, regressions, race conditions, null/edge cases, backward compatibility.
4. If messaging changed, verify:
   - Outbox use
   - Bounded retries with backoff
   - Idempotent consumers
   - DLQ routing and replay safety
5. If persistence changed, verify:
   - EF mappings/migrations
   - query performance and data consistency impact
6. If API changed, verify:
   - status codes, contracts, validation, versioned routes
7. Evaluate tests:
   - unit + integration relevance
   - failure path coverage
   - repository coverage floor remains >= 80%
8. Report findings ordered by severity with specific file references.

## Output Format

Produce findings first, then brief summary:

1. Critical findings
2. High findings
3. Medium findings
4. Low findings
5. Open questions/assumptions
6. Short merge readiness summary

If no findings exist, explicitly say no blocking findings were discovered and call out any residual risk/testing gaps.

## Lynkly-Specific Guardrails

- Do not accept business logic in endpoint handlers or infrastructure adapters.
- Redirect path must remain non-blocking and low-latency.
- Messaging must remain broker-portable (RabbitMQ now, Kafka-compatible design).
- Shared.Kernel additions must remain generic and NuGet-ready.
- Do not recommend paid dependencies unless explicitly requested.
