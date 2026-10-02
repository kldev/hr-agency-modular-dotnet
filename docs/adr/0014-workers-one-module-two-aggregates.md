# ADR-0014: Workers is one module with two aggregates - the person (`Worker`) and the posting (`Assignment`)

- **Status:** Accepted
- **Date:** 2026-09-20
- **Evidence:** `c1e5f904` feat(workers): workers module backend,
  `607378a5` feat(workers): hold an assignment against a project position, and count the staffing both ways,
  `93effb46` feat(front): register, edit and maintain workers from the register and from an application,
  `e55873ec` feat(recruitment): let a candidate and an application know the worker opened from them
- **Specs:** [019-workers-and-assignments](../../.shipit/specs/done/019-workers-and-assignments.md),
  [020-positions](../../.shipit/specs/done/020-positions.md),
  [024-application-knows-its-worker](../../.shipit/specs/done/024-application-knows-its-worker.md)

## Context
Projects could only tick per-person duties (A1, Limosa, local contract) at project level, which is
wrong: an A1 is issued for one person, one period and one posting company. The same person often
moves from one project to another, or to the same client through another of our companies. If the
person and the posting were one record, every move would either overwrite history or create a
second file for the same human.

## Decision
- One module, `HrAgencySystem.Workers`, schema `workers`, two aggregates:
  - `Worker` - the person: name, birth date, citizenship, identity document, personal documents,
    permits, and a status pipeline (`Recruitment → ContractPreparation → Legalisation →
    Onboarding → Employed`, plus `ProjectChange` and `Terminated`) with an owner per stage
    (`WorkerStatusChangePolicy.OwnerOf`).
  - `Assignment` - one posting: project, delivering company, position, period, engagement type;
    states `Planned → Active → Completed | Interrupted`, and `Planned → DidNotStart`.
- No command repoints an assignment; a move ends one and opens another.
- "Domestic or foreign" is data (citizenship, work country), not a module split.
  `LegalisationPolicy` is a list of free movement countries.
- `WorkerProjection` is a `MultiStreamProjection` (`WorkerProjector`) fed by the worker stream and
  every assignment stream, so the register row carries `CurrentWorkCountry` and can be filtered with
  `workCountry` / `excludeWorkCountry` and still be paged.
- One person, one file: reservations on the identity document and on the e-mail, plus a projection
  lookup on name and phone. Overlapping assignments are refused by a projection query
  (`HasOverlappingAssignment`), because date ranges cannot be a unique index.
- Per-person compliance is recorded on the assignment (`ComplianceScope.Assignment`).

## Alternatives considered
Plan 014 (local, not in git) decided **two modules from day one**, `Workers.Domestic` and
`Workers.Foreign`, arguing that documents, statuses and owners differ too much; it left open how to
avoid the same person existing twice (option A: a thin shared identity port; option B: a new file
per category). The code did neither: one module, one `Worker`, with the domestic/foreign difference
expressed as data. The plan also asked whether the engagement type belongs on the project or on the
posting; the code puts it on the assignment, and `IProjectSnapshotRepository` deliberately does not
carry it.

## Consequences
- A person's employment history is the list of their assignments; nothing to maintain.
- The front end still shows two registers (here / abroad) by filtering on work country; the API
  never decides which country is home.
- The overlap check reads a projection, so two simultaneous requests can both pass; this limit is
  documented on the port.
- Everything Wolverine builds must be `public` (an internal repository injected into a handler
  throws `InvalidServiceLocationException` at runtime).

## Revisit when
- Domestic and foreign workflows diverge so much that most handlers branch on the work country.
- Concurrent planning makes the projection-based overlap check produce real double bookings.
