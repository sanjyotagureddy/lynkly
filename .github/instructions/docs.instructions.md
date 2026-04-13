---
description: "Use when updating architecture and operational documentation under docs/**."
applyTo: "docs/**"
---

# Documentation Instructions

## Scope

Applies to documentation changes under:

- `docs/**`

## Documentation Standards

1. Keep docs implementation-aware but architecture-centric.
2. Explain both happy path and failure behavior.
3. Keep terminology consistent with code and event contracts.
4. Favor concise, actionable guidance over abstract prose.

## Required Content for New Features

1. Feature overview and intent.
2. End-to-end request/event flow.
3. Error/failure modes and fallback behavior.
4. Observability: logs, traces, metrics, and alerts.
5. Security and abuse considerations.
6. Testing strategy for the feature.

## Event and Messaging Docs

1. Document event names, schema fields, and versioning assumptions.
2. Document retry behavior, DLQ policy, and replay strategy.
3. Keep broker-specific details isolated from core event semantics.

## Infra and Operations Docs

1. Document dependency requirements and local setup.
2. Include migration and rollback notes for persistence changes.
3. Include runbook-style troubleshooting steps when relevant.
4. Keep examples copy/paste friendly and environment-safe.

## Quality Rules

1. Do not reference paid tooling as a hard requirement.
2. Keep architecture docs aligned with Clean Architecture + VSA boundaries.
3. Keep broker language RabbitMQ-current but Kafka-portable.
4. Update docs in the same change when behavior or contracts change.
