# ADR-0006: Modules never reference each other: integration events in `*.Contracts`, data through `SharedKernel` ports, and `SharedKernel` stays small

- **Status:** Accepted
- **Date:** 2026-09-02 (first snapshot port); 2026-09-07 (first `*.Contracts` project)
- **Evidence:** `7a47fbd4` feat: added check for non existing organization, `02d85709` feat(job-description): add recruiter snapshot to projections, `3b2702cc` feat(job-description): add company snapshot, `b1f4dbca` test(identity): add user suggestions integration tests, `2c114b2e` feat(company): add recruitment integrations, `5612e407` refactor(teams): move TeamRole into its own contracts project
- **Specs:** [004](../../.shipit/specs/done/004-job-descriptions.md), [005](../../.shipit/specs/done/005-job-posts-and-channels.md), [014](../../.shipit/specs/done/014-teams.md), [016](../../.shipit/specs/done/016-projects-and-compliance.md), [019](../../.shipit/specs/done/019-workers-and-assignments.md), [020](../../.shipit/specs/done/020-positions.md), [029](../../.shipit/specs/done/029-sales-workspace-and-tasks.md)

## Context
Inside one process ([ADR-0001](0001-modular-monolith.md)) nothing stops a module from calling
another's repository or reading its projection, and then the boundaries exist only on paper.
Yet modules do need each other: a job description names a recruiter (Identity) and a company
(Company); a company counts its job posts (Recruitment); a project knows which positions are staffed (Workers).

## Decision
- A module's `.csproj` may reference only `SharedKernel`, `*.Contracts` projects, and libraries
  without persistence (`Compliance`, `Files`, `Feeds`, `Reports.ReadModel`). Hosts and
  `PlatformSeeder` are the only projects that reference several modules.
- **Notifications** travel as integration events in a contracts project per producer
  (`Recruitment.Contracts`, `Projects.Contracts`, `Workers.Contracts`, `Teams.Contracts`,
  `Tasks.Contracts`). The consumer has a `[WolverineHandler]` in `Integration/` that translates the
  message into its own domain event. Producers send through `OutgoingMessages`
  ([ADR-0003](0003-wolverine-static-handlers.md)).
- **Data** is read through ports in `SharedKernel/Snapshots` and `SharedKernel/Port`
  (`IUserSnapshotRepository`, `ICompanySnapshotRepository`, `IJobDescriptionSnapshotRepository`,
  `IProjectSnapshotRepository`, `IPositionSnapshotRepository`, `ILegalEntitySnapshotRepository`,
  `ITeamSnapshotRepository`, `IWorkerSnapshotRepository`, `IOpportunitySnapshotRepository`,
  `IOrganizationChecker`). The owning module implements the port. The snapshot is copied into the
  event, so history shows the data as it was.
- `SharedKernel` holds only exceptions, `OrganizationId`, `IClock`, shared value objects, paging
  and ports.

## Alternatives considered
- **Marker interfaces for messaging in `SharedKernel`** (`ICommand`, `IEvent`,
  `IIntegrationEvent` in `f1c00ced`). Removed in `b1f4dbca`; integration events got their own
  contracts project instead (`2c114b2e`).
- **Ports named `*Service`** (`IUserSnapshotService`, `ICompanySnapshotService` in
  `02d85709`/`3b2702cc`), renamed to `*Repository` in `b1f4dbca`.
- **A shared enum inside the producing module**: `TeamRole` moved out of `Teams/Domain` into
  `Teams.Contracts` (`5612e407`) so `Identity` could use it without referencing `Teams`.
- `.docs/05` (local) proposed one shared validation collector in `SharedKernel` as the only
  justified growth; it was not built.

## Consequences
- Integration tests replace ports with fakes (`tests/.../Infrastructure/Snapshots`), so one
  module can be tested without the others.
- `SharedKernel` grew anyway: 19 value objects, `ContactDataFactory` (`afbb4781`),
  `PostalAddress` (`c5289625`), `FieldValidationException` (`302828ec`) and nine snapshot ports.
- `Agency` exposes no port at all; a need for one is treated as a sign that something is filed
  under the wrong group of people ([ADR-0015](0015-agency-separate-from-workers.md)).
- The counters `Recruitment` sends to `Company` use the older producer shape described in
  ADR-0003 and are not covered by a test.

## Revisit when
1. A port starts carrying behaviour (a decision) rather than data.
2. `SharedKernel` gains a concept only two modules use - it belongs in a contracts project then.
