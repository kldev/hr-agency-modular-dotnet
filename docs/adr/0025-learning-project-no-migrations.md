# ADR-0025: A learning project that never ships - no data migrations or event versioning, model quality is the point

- **Status:** Accepted
- **Date:** 2026-08-30 (schema auto-creation from the first commit)
- **Evidence:** `f1c00ced` start project (`AutoCreate.CreateOrUpdate`),
  `c1e5f904` feat(workers): workers module backend (renames `Assignment` to `Placement` and moves `EngagementType` without upcasters),
  `ac324a31` fix(agency): give the chart its own stream, derived from the organization rather than equal to it,
  `59cba475` feat(agency): put a rate on the contract and export the settled month to Excel (new nullable field on existing events),
  `cc99a125` feat(sales): opportunity snapshot port, task activity from a done task and last activity on an opportunity (enum value inserted mid-list)
- **Specs:** all; notably [016-projects-and-compliance](../../.shipit/specs/done/016-projects-and-compliance.md),
  [021-agency-chart-and-roles](../../.shipit/specs/done/021-agency-chart-and-roles.md),
  [029-sales-workspace-and-tasks](../../.shipit/specs/done/029-sales-workspace-and-tasks.md)

## Context
HR Agency is a training ground for a modular monolith on Marten and Wolverine. It never runs in
production, has no SLA and no data anybody needs to keep. Event sourcing normally makes every change
of an event's shape a versioning problem; here that cost buys nothing.

## Decision
- Marten creates and updates its schema at startup (`AutoCreate.CreateOrUpdate`); there are no
  migration scripts and no upcasters in `src/`.
- Event, command and stream-id shapes change freely; a stale local database is reset and reseeded.
- The saved effort goes into the model: domain rules, module boundaries and tests are treated as if
  the system were real ("the quality of the model is the point").
- Architectural experiments are allowed when written down in a plan first (`.plans/`).

## Alternatives considered
None recorded. Domain doc 01 and backend-improvements doc 05 (local, not in git) list event
versioning and data migration under "deliberately not done": upcasters would cost time and teach
nothing that cannot be read.

## Consequences
- Changes that would need a migration elsewhere landed as plain commits: the project's
  `Assignment` became `Placement` and `EngagementType` moved to `HrAgencySystem.Compliance`
  (`c1e5f904`); the chart's stream id changed from "equal to the organization id" to derived
  (`ac324a31`); `WorkRate?` was added to employment events relying on old events reading as `null`
  (`59cba475`).
- Traps are real on an old database: inserting `SalesActivityType.Task` before `Other` shifted the
  stored integer, so an unreset database shows old "Other" activities as "Task" (plan 029 notes).
- Changing a fixed stream-id namespace would orphan every derived stream (`AgencyStreamId` comment).
- Personal data rules (GDPR) are not built; the plans still avoid spreading personal data across
  projections, and logs carry identifiers only.

## Revisit when
- The system, or a fork of it, is deployed with data somebody must keep - then event versioning,
  upcasters and schema migrations become mandatory.
- Enum values stored as integers need to stay stable across a shared demo database.
