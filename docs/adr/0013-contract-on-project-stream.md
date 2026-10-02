# ADR-0013: The contract lives on the project stream, and compliance is a data catalogue in a shared library

- **Status:** Accepted (compliance part amended on 2026-09-20: moved out of `Projects` into `HrAgencySystem.Compliance`)
- **Date:** 2026-09-20
- **Evidence:** `33797e83` feat(projects): add the project aggregate, events and compliance catalogue,
  `3af2974b` feat(projects): add project use cases for status, team, contacts, emails and contract,
  `f2b37287` feat(projects): compliance checklist from the country catalogue and team assignment,
  `c1e5f904` feat(workers): workers module backend (moves the catalogue into `HrAgencySystem.Compliance`, adds `ComplianceScope`)
- **Specs:** [016-projects-and-compliance](../../.shipit/specs/done/016-projects-and-compliance.md),
  [019-workers-and-assignments](../../.shipit/specs/done/019-workers-and-assignments.md),
  [020-positions](../../.shipit/specs/done/020-positions.md)

## Context
After a sale the agency delivers a service: a project with a contract, contacts and documents,
and, for postings abroad, duties that depend on the country (Belgium, Germany) and on how the
person is engaged. Two rules forced the shape:

- "A project goes `Active` only with a signed contract" must be checked at write time. A separate
  aggregate would answer it from a read model that the async daemon may not have caught up with.
- The law differs per `(country, engagement type)`. Encoding it as branches would mean touching
  handlers for every new country.

## Decision
- `Project` is an event-sourced aggregate (schema `projects`). The contract
  (`ProjectContractRecorded`, `ProjectContractStatusChanged`), contacts (one model plus
  `ContactRole`, assigning a role replaces) and documents (a `FileId` only) are events **on the
  project stream**. One snapshot projection, `ProjectProjection`, serves list and details.
- `ProjectStatusChangePolicy` holds the status graph; going live additionally checks a signed
  contract, a responsible contact and a complete client profile, each reported separately.
- The client profile stays on `Company` (`CompleteCompanyProfile`); only the contract party is frozen
  on the contract. `CompanySnapshotRepository` falls back to replaying the stream when the
  projection lags.
- Compliance is `ComplianceCatalogue`: a dictionary `(country, EngagementType) → requirements`,
  no `if (country == …)`. Poland has no entry on purpose. Since `c1e5f904` it lives in
  `HrAgencySystem.Compliance`, a library with no persistence (the `Files` pattern), read by
  `Projects` and `Workers`. `ComplianceCatalogue.For` requires a `ComplianceScope`
  (`Project` or `Assignment`), so per-person items such as A1 cannot be recorded on a project.

## Alternatives considered
From plan 009 (local, not in git), decision log:
- A separate `Contract` aggregate with `ProjectId` - rejected because the go-live rule would move
  to a lagging read model. Named as the way out if annexes or several contracts appear.
- Separate Marten collections for contracts, contacts and documents - rejected: none has its own
  lifecycle; four collections would need joining into one screen.
- Separate list and details projections - rejected: a second daemon and a second chance to drift.
- Compliance as `if` branches, a rules engine, or its own aggregate - rejected; it is a checklist.
- First version kept the catalogue inside `Projects/Domain/Compliance` (`33797e83`); the move to a
  shared library came with `Workers` (`c1e5f904`), because modules may not reference each other.

## Consequences
- A project has exactly one contract; an annex is a document plus a changed `ValidTo`.
- Every document is an event; fine for tens per project, not for thousands.
- Adding a country is a catalogue row plus enum values. Uncertain requirements were left out of the
  enum rather than modelled from unofficial sources.
- The replay fallback in snapshot repositories is load bearing (profile completed seconds before
  the contract is recorded).

## Revisit when
- A project needs several contracts or annexes with their own dates.
- A project accumulates hundreds of documents.
- Compliance has to compute deadlines or send reminders instead of being ticked by hand.
