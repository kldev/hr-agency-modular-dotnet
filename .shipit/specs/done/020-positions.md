# 020 · Positions

**Status:** done · **Built:** 2026-09-21 → 2026-09-22 · **Verify:** `./verify projects` and `./verify workers` · **Plan:** backend 019 / frontend 011 (local, not in git)

## Why
An assignment's position used to be free text typed per person. The same role came in five
spellings, a project did not know which roles it had, and nobody could say "this role is two people
short". Delivery staff need the roles of a project written down once, with the terms a contract with
the worker needs (duties, contract type, rate, hours, workplace), and every posting held against one.

## What it promises
- A role (position) opens inside a project with an internal name, a contract name (defaults to the
  internal one), work description, duties, required qualifications, contract type, proposed rate,
  weekly hours, start time, schedule, workplace address (empty = the project's), payout day,
  probation and notice periods, target headcount and a suggested engagement type.
- Two open roles in one project cannot share a name (case-insensitive); the same name in another
  project is fine. A rate needs its currency; half an address is a validation error; all problems
  are reported at once.
- Roles are archived, not deleted. An archived role leaves the picker and the default list, frees its
  name, stays in history, and can be restored unless its name was taken meanwhile.
- A register at `/app/positions` lists roles across projects, with the project and the client, a
  search and an "include archived" switch.
- An assignment is planned against a `positionId` from that project; free text is gone. The
  assignment freezes the role's names, so renaming a role later does not rewrite a past contract -
  but the rename reaches the postings' displayed names through an integration event.
- A role counts the people held against it (`assigned / planned`, and a "short" chip only when a
  target exists). Exceeding the target is allowed - a warning, not a block.
- Several people planned onto one project at once are all counted.
- A role of another organization is not found.

## Surface
- Backend: `src/HrAgencySystem.Projects` (`Domain/ProjectPosition.cs`, events
  `ProjectPositionOpened/Updated/Archived/Restored/Staffed/Unstaffed` on the project stream,
  `ProjectPositionProjection`), `IPositionSnapshotRepository` port for `Workers`,
  `src/HrAgencySystem.Workers.Contracts` (staffing integration events). Endpoints
  `POST /api/projects/{projectId}/positions`, `PUT /api/projects/{projectId}/positions/{positionId}`,
  `.../archive`, `.../restore`, `GET /api/positions`, `GET /api/positions/{id}`,
  `GET /api/suggestion/positions[/{id}]`.
- Frontend: `frontend/src/features/positions` (register, wizard), `components/ui/pickers/PositionsPicker.tsx`,
  Positions tab on `/app/projects/$id`, route `/app/positions`; position picker in the assignment wizard.
- Tests: `tests/HrAgencySystem.UnitTests/Projects/ProjectPositionHandlerTests.cs`,
  `tests/HrAgencySystem.IntegrationTests.Delivery/Projects/ProjectPositionTests.cs`,
  `tests/HrAgencySystem.IntegrationTests.Delivery/Workers/AssignmentPositionTests.cs`.

## Out of scope for this feature
- Client demand ("15 people in two months") - a different entity owned by sales, not yet.
- An organization-wide dictionary of trades - not yet; the plan's first draft proposed it and was dropped.
- Per-person contract terms copied from the role (`Assignment.ContractTerms`) - not yet.
- A position-change event on a running assignment - open; `UpdateAssignment` treats a change as a correction.
- A details page per role - not ever as planned: the wizard is the editor.
- Country filter and sortable "missing headcount" on the register - not yet.

## Acceptance criteria
- [x] A project carries several roles; names unique per project, free across projects — `./verify projects`
- [x] Opening validates rate currency and address, and collects every problem at once — `./verify projects`
- [x] Archive hides the role and frees its name; restore refuses a taken name — `./verify projects`
- [x] A role says how many people it still needs — `./verify projects`
- [x] A role of another organization is not found — `./verify projects`
- [x] A role counts the people held against it; renames reach the postings — `./verify workers`
- [x] Concurrent planning onto one project loses no count — `./verify workers` (`Several_people_planned_onto_one_project_at_once_are_all_counted`)
- [x] A role can be opened through the wizard and picked when planning an assignment — e2e: `frontend/e2e/projects/project.spec.ts`
- [x] Positions tab exists — evidence: `docs/screenshots/project-positions.png`
- [ ] The `/app/positions` register with "include archived" — unproven (no e2e or screenshot found)

## Decisions
- Positions live on the project aggregate, so name uniqueness is checked inside one stream with no
  reservation document: [ADR-0013](../../../docs/adr/0013-contract-on-project-stream.md), and the
  contrast with [ADR-0005](../../../docs/adr/0005-uniqueness-reservations.md).
- Staffing crosses modules as integration events: [ADR-0006](../../../docs/adr/0006-contracts-and-shared-kernel-ports.md).
- The assignment freezes a `PositionSnapshot`, like the posting legal entity: a contract states what was signed.
- `WorkRate` (amount, currency, Hourly/Daily/Monthly, Gross/Net) instead of `SalaryRange`: an agency quotes one rate.
- `WorkerContractType` is a new list, distinct from `EmploymentType` (job ads) and `EngagementType` (client side).
- Staffing events are appended to the project stream exclusively (`a780a729`); plain appends lost seats.

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-21 | `2501a5fe` | Positions as roles inside a project, with contract terms |
| 2026-09-21 | `7ca84042` | List, search, archive positions across projects; suggestions |
| 2026-09-21 | `c5efeb72` | Positions exposed to other modules, listed on the project |
| 2026-09-21 | `607378a5` | Assignment held against a position; staffing counted both ways |
| 2026-09-21 | `a780a729` | Exclusive appends so no seat is silently lost |
| 2026-09-21 | `b0d70b31` | Same race fixed for job post counts on the company stream |
| 2026-09-21 | `4fe3ec37` | PR #7 merged |
| 2026-09-21 | `b544df30` | Front: register and project tab |
| 2026-09-21 | `a312824e` | Front: pick the role instead of typing it |
| 2026-09-22 | `737cdbdd` | OpenAPI descriptions of the position request |

## Notes from reconstruction
- The plan's first version made the position an organization dictionary; it was rewritten to a role
  inside a project before any code.
- The position port takes `projectId` first: looking the project up through `ProjectProjection` lagged,
  so a role opened a second earlier was "unknown". Unknown id, other project and other organization
  now share one refusal message.
- The seeder exposed the lost-update race (5 of 12 `Staffed` lost); a test fake that returned a new
  company id on every call had hidden the same race for job posts (`b0d70b31`).
- Plan 019 said the next plan (021) would add per-person contract terms; 021 became time sheet mail
  and `ContractTerms` does not exist.
