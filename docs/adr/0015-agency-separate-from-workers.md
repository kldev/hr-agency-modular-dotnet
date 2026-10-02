# ADR-0015: The agency as an employer is its own module; the supervisor is computed from the chart

- **Status:** Accepted
- **Date:** 2026-09-21
- **Evidence:** `4db534bd` feat(agency): the agency's own chart, with the supervisor computed rather than written down,
  `ac324a31` fix(agency): give the chart its own stream, derived from the organization rather than equal to it,
  `fe1eda85` feat(identity): add the payroll, finance and administration roles the agency actually has,
  `7e99563e` feat(agency): rcp backend (`PayrollPolicy`),
  `59cba475` feat(agency): put a rate on the contract and export the settled month to Excel (`RatesPolicy`),
  `3f18078f` fix(agency): read the org chart with FetchLatest so a lagging snapshot cannot hide a just assigned head
- **Specs:** [021-agency-chart-and-roles](../../.shipit/specs/done/021-agency-chart-and-roles.md),
  [022-time-sheets-and-settlement](../../.shipit/specs/done/022-time-sheets-and-settlement.md)

## Context
Time sheets and leave need "the supervisor approves first". Nothing in the system knew who reports
to whom. `Workers` describes people sent to clients; the agency's own staff are a different group,
settled with a different party. `Organization` is the tenant row every module scopes by.

## Decision
- New module `HrAgencySystem.Agency`, schema `agency`: the org chart, `AgencyEmployment` and time
  sheets. It reads `Identity` through `IUserSnapshotRepository`; no other module has a port into it.
- The chart is **one aggregate per organization** (`OrgStructure`), stream id derived from the
  organization id (`OrgStructureId`, `ac324a31`), with units `Board`/`Department`/`Section`, nesting,
  members and an optional head. One root, no move under its own subtree, one person in one unit
  - all checked in memory in the same transaction.
- A supervisor is **never stored**. `SupervisorPolicy` walks up the tree at the time of the
  question: the head of my unit unless it is me, otherwise ask the parent. A unit without a head
  passes the question upwards.
- `OrganizationRole` decides only two things: `PayrollPolicy` (`HumanResources` + `Admin`) settles
  approved months, and `RatesPolicy` (`HumanResources` + `Finance` + `Admin`) sees and sets rates.
  Approving hours has no role-shaped door; only the supervisor from the chart can approve.

## Alternatives considered
Plan 015 (local, not in git) proposed:
- the chart in the `Organization` module - replaced by a separate `Agency` module (`### uwagi do
  planu`): the tenant and the company-as-employer change for different reasons;
- an `OrgUnit` aggregate per unit with an `OrgUnitMembershipReservation` document and a `Path`
  column - replaced by one aggregate per organization, because each rule spans several units and a
  lagging projection could let a cycle in, after which walking up never ends;
- an `IOrgStructureRepository` port in `SharedKernel` - dropped; its only consumers live in `Agency`.
- A stored `ReportsTo` per person was listed as "not now", only as a future override.

## Consequences
- Moving a person or a unit needs no fix-up of anybody's supervisor.
- Marten rebuilds the aggregate without running field initialisers; list fields are read through
  `?? []` (a trap that cost a test run, per plan 015 notes).
- The chart read must include unprocessed events: `3f18078f` switched to
  `FetchLatest<OrgStructureProjection>` after a flaky approval test read a stale head.
- If something wants to ask `Agency` about the chart to decide something about a `Worker`, it was
  filed under the wrong group of people.

## Revisit when
- Someone needs a matrix (two supervisors) or explicit per-person overrides.
- Charts grow to thousands of units, making one stream per organization too heavy.
- A third role-based rule appears, suggesting a real permission model.
