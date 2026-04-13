---
description: "Use when staging, committing, or preparing change history (commit messages, commit scope, and PR readiness)."
---

# Git and Commit Instructions

## Scope

Applies whenever work includes git staging, commit creation, commit message writing, or preparing PR history.

## Commit Quality Rules

1. Keep commits small, cohesive, and single-purpose.
2. Do not mix unrelated refactors with feature or bug-fix commits.
3. Preserve behavior-focused history: each commit should explain what changed and why.
4. Prefer multiple focused commits over one large mixed commit.
5. Do not amend or rewrite shared history unless explicitly requested.

## Message Format

Use clear, descriptive commit messages.
Prefer Conventional Commit style when practical.

Recommended format:

- `<type>(<scope>): <summary>`

Examples:

- `feat(links): add create-link use case with validator`
- `fix(redirect): prevent cache stampede on slug miss`
- `refactor(messaging): isolate broker adapter behind abstraction`
- `test(analytics): add retry and DLQ integration coverage`
- `docs(architecture): document outbox and idempotency rules`

## Commit Message Content Rules

1. Use imperative mood in the summary line.
2. Keep summary concise and specific.
3. Add a body when context is needed, including rationale and risks.
4. Reference issue IDs when available.
5. Avoid vague summaries such as "update code" or "fix stuff".

## Staging Rules

1. Stage only files relevant to the intended commit.
2. Review staged diff before commit.
3. Exclude generated artifacts, local configs, and secrets.
4. Do not include unrelated formatting-only changes unless they are the commit purpose.

## Validation Before Commit

Run relevant checks before committing:

1. `dotnet restore`
2. `dotnet build`
3. `dotnet test`

If full validation is not possible, clearly document what was skipped and why.

## Architecture and Safety Guardrails

1. Do not commit changes that break architecture boundaries.
2. Do not commit code with hardcoded credentials or sensitive values.
3. Keep Shared.Kernel commits generic and reusable (NuGet-ready mindset).
4. For messaging changes, ensure retries, idempotency, and DLQ behavior are covered.

## PR Readiness Expectations

Before opening a PR:

1. Ensure commit history is understandable and reviewable.
2. Ensure tests and documentation are updated with behavior changes.
3. Ensure coverage floor remains at least 80% repository-wide.
4. Ensure PR summary explains both what changed and why.
