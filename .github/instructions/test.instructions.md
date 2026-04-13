---
description: "Use when adding, updating, or reviewing tests under tests/** in Lynkly Resolver."
applyTo: "tests/**"
---

# Test Instructions

These instructions apply to all test code under `tests/**` in this repository.

## Testing Goals

1. Validate behavior, not implementation details.
2. Keep tests deterministic, isolated, and readable.
3. Cover happy paths, failure paths, and edge cases.
4. Protect the architecture by testing boundaries, contracts, and integration points.
5. Keep code coverage at a minimum of 80% for the repository, with higher coverage expected for core domain and critical application slices.

## Required Test Strategy

Use the right test level for the change:

1. Unit tests for pure domain and application logic.
2. Integration tests for EF Core, Redis, messaging, and API boundaries.
3. Architecture tests for dependency direction and layer boundaries.
4. Contract tests when external APIs or message contracts are involved.
5. Performance tests for hot-path redirects, latency, and throughput-sensitive code.

## Best Practices

1. Prefer Arrange-Act-Assert structure where it keeps tests clear.
2. Give tests names that describe behavior and expectation.
3. Keep one reason to fail per test when practical.
4. Use fixed clocks, seeded data, and controlled inputs.
5. Avoid shared mutable state between tests.
6. Prefer test doubles at boundaries; do not mock internals unnecessarily.
7. Prefer integration coverage over over-mocking persistence or messaging layers.
8. Test the public behavior of a slice end to end when the slice is small enough to do so cleanly.
9. Keep test helpers simple, reusable, and local to the test project unless truly shared.

## Coverage Rules

1. Maintain at least 80% overall code coverage.
2. Aim for 100% coverage on critical domain logic and high-value application slices.
3. Do not use coverage as a substitute for meaningful assertions.
4. Exclude generated code, trivial boilerplate, and framework bootstrap only when the repo's tooling explicitly permits it.
5. If a change lowers coverage, add tests in the same change unless explicitly out of scope.

## Messaging and Resilience Tests

1. Verify retry behavior for transient failures.
2. Verify poison-message handling and DLQ routing.
3. Verify idempotency for duplicate deliveries.
4. Verify event publication does not block critical request paths.
5. Use broker-agnostic assertions where possible so RabbitMQ can be replaced later without rewriting the test intent.

## Persistence Tests

1. Validate EF Core mappings, query behavior, and migration safety.
2. Prefer real database-backed integration tests for persistence behavior.
3. Verify schema changes with migrations rather than ad-hoc setup.
4. Assert transaction boundaries and data consistency where relevant.

## API Tests

1. Verify request validation, status codes, and response contracts.
2. Keep endpoint tests thin and focused on transport behavior.
3. Ensure routes remain versioned and predictable.
4. Test failure responses as deliberately as success responses.

## What to Avoid

1. Avoid flaky timing-based tests.
2. Avoid tests that depend on external networks or paid services.
3. Avoid asserting implementation details that make refactoring difficult.
4. Avoid large, opaque test setups that hide the behavior under test.
5. Avoid lowering the coverage floor unless the change is explicitly a test-exception review.

## Completion Checklist

1. Tests for the changed behavior are added or updated.
2. Relevant test types are chosen correctly for the change.
3. The coverage floor is preserved at 80% or better.
4. The test suite remains deterministic and maintainable.
5. Any new failure paths introduced by the change are covered.
