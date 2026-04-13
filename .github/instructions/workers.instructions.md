---
description: "Use when changing worker and background processing code under src/Workers/**."
applyTo: "src/Workers/**"
---

# Workers Instructions

## Scope

Applies to background consumers and processors:

- `src/Workers/Lynkly.Resolver.AnalyticsWorker/**`

## Core Principles

1. Workers must be resilient, idempotent, and restart-safe.
2. Background failures must not impact redirect path availability.
3. Keep worker behavior focused on asynchronous processing concerns.

## Consumer Behavior

1. Treat every message as potentially duplicated.
2. Implement idempotency keys/deduplication checks.
3. Acknowledge messages only after successful processing.
4. Use bounded retries with exponential backoff and jitter.
5. Route poison messages to DLQ after retry limits are reached.

## Processing and Aggregation

1. Keep processors deterministic and side-effect aware.
2. Update analytics aggregates in safe, retry-tolerant ways.
3. Ensure partial failures are observable and recoverable.
4. Prefer small, composable processing units over large handlers.

## Reliability and Operations

1. Emit structured logs for consumption lifecycle and failures.
2. Publish metrics for throughput, lag, retries, and DLQ depth.
3. Expose health/readiness signals suitable for orchestration.
4. Support graceful shutdown and in-flight work completion.

## Broker Portability

1. Keep worker logic broker-agnostic at the application boundary.
2. RabbitMQ specifics belong in infrastructure adapters.
3. Avoid assumptions that would block a future Kafka adapter.

## Testing Expectations

1. Add tests for retry behavior and DLQ transitions.
2. Add tests for duplicate message handling (idempotency).
3. Add integration tests for consumer + persistence interaction.
4. Verify failure and recovery paths, not only happy paths.

## Validation Checklist

1. `dotnet restore`
2. `dotnet build`
3. `dotnet test`
