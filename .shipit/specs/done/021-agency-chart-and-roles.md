# 021 · Agency chart and roles

**Status:** done · **Built:** 2026-09-21 → 2026-10-01 · **Verify:** `./verify agency` · **Plan:** backend 015, 020 (local, not in git)

## Why
Approving somebody's hours (and, later, their leave) starts with "the supervisor approves first" -
and the system had no idea who anybody's supervisor was. That answer is not a field to type per
person: it follows from the company's chart of departments and sections, which everybody already
has in their head. Separately, the organization roles were labels only: any signed-in member could
create users or grant themselves `Admin`.

## What it promises
- A member of the organization sees the agency's chart at `/app/org-structure`: units of kind `Board`,
  `Department` or `Section`, nested, each with members (with an optional title) and an optional head.
- There is exactly one top unit; it cannot be moved or archived.
- A unit cannot be moved under itself or under any of its own sections; moving a unit moves the
  supervisor of everybody in it.
- A person belongs to at most one unit; adding somebody who sits elsewhere is refused.
- A head must be a member of the unit; the head cannot be taken out while heading it.
- A unit can be archived only when it has no people and no active units under it.
- A supervisor is never stored. It is the head of the person's unit; for the head themselves, or a
  unit with no head of its own, the question goes up the tree until somebody is found. The top
  person has no supervisor (`null`). Somebody outside the chart has none either.
- `GET /api/org-structure/supervisor/{userId}` and `/subordinates/{userId}` answer those questions;
  the user card shows the unit and the computed supervisor, read-only.
- The roles `HumanResources`, `Finance` and `Administration` exist next to `Admin`, `Recruiter`,
  `HiringManager`, `Interviewer`, `Sales`.
- Creating, editing and re-roling users requires the `Admin` organization role (`AdminPolicy`); so
  do impersonation and setting somebody else's avatar (see 023).
- A chart read right after a change reflects it (no lagging snapshot hides a just-assigned head).

## Surface
- Backend: `src/HrAgencySystem.Agency` (`Domain/OrgStructure`, `SupervisorPolicy`, `OrgStructureId`,
  `OrgStructureProjection`, `IOrgStructureQueryRepository`), `src/HrAgencySystem.Identity/Domain/OrganizationRole.cs`,
  `src/HrAgencySystem.Api/Auth/AdminPolicy.cs`. Endpoints `GET /api/org-structure`,
  `POST /api/org-structure/units`, `PUT .../units/{id}`, `PUT .../units/{id}/parent`,
  `POST .../units/{id}/archive`, `.../units/{id}/head`, `.../units/{id}/members`,
  `GET /api/org-structure/supervisor/{userId}`, `GET /api/org-structure/subordinates/{userId}`.
- Frontend: `frontend/src/features/org-structure`, route `/app/org-structure`, unit and supervisor on
  `/app/users/$id`.
- Seeder: a full chart with named heads; operations sections deliberately without heads.
- Tests: `tests/HrAgencySystem.UnitTests/Agency` (`OrgUnitHandlerTests`, `SupervisorPolicyTests`,
  `SupervisorReachTests`), `tests/HrAgencySystem.IntegrationTests.Agency/Agency/OrgStructureTests.cs`.

## Out of scope for this feature
- A per-person `ReportsTo` override - not yet; added only if a real exception appears.
- Deputies and escalation when a supervisor is away - not yet (belonged to leave requests, plan 016,
  which was never implemented).
- Several roles per user - not yet; one role per user stays.
- Matrix reporting (two supervisors) - not ever for this product.
- Department costs and budgets - not ever here (that is reporting).
- `Admin` gates outside user management - not yet; a review of the whole API by role is its own plan.

## Acceptance criteria
- [x] A unit without its own head is answered for from above; the top has nobody — `./verify agency`
- [x] Moving a unit moves everybody's supervisor; moving under its own section is refused — `./verify agency`
- [x] Exactly one top unit; nobody belongs to two units — `./verify agency`
- [x] Archive refuses a unit with people or units under it, and the top unit — `./verify agency`
- [x] The chart is read whole and opens for a real organization — `./verify agency`
- [x] A department can be reorganised in the UI and survives a reload — e2e: `frontend/e2e/organization/org-structure.spec.ts`
- [x] Chart screen — evidence: `docs/screenshots/organization-structure.png`
- [x] Non-admins are refused impersonation and setting another person's avatar — `./verify identity`
- [ ] Non-admins are refused `POST /api/users`, `PUT /api/users/{id}`, `PUT /api/users/{id}/role` — unproven (no test found; integration clients are all `Admin`)
- [ ] The supervisor shown on the user card — unproven (no e2e found)

## Decisions
- The agency as an employer is its own module, not `Organization` (the tenant) or `Workers`:
  [ADR-0015](../../../docs/adr/0015-agency-separate-from-workers.md).
- One `OrgStructure` aggregate per organization instead of one per unit: every rule spans several
  units, and checking them on an in-memory list in one transaction cannot be outrun by a lagging
  projection; a cycle let in once would loop the supervisor walk forever.
- Hence no membership reservation document and no `SharedKernel` port, unlike
  [ADR-0005](../../../docs/adr/0005-uniqueness-reservations.md) / [ADR-0006](../../../docs/adr/0006-contracts-and-shared-kernel-ports.md).
- The chart's stream id is derived from the organization id, not equal to it (`ac324a31`):
  [ADR-0004](../../../docs/adr/0004-tenant-on-every-aggregate.md).
- A role is not a supervisor: roles say what one may do, the chart says whose requests pass through whom.

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-21 | `4db534bd` | Agency module: chart with a computed supervisor |
| 2026-09-21 | `b973aa0b` | Integration tests for the supervisor rule and cross-unit rules |
| 2026-09-21 | `ac324a31` | Chart gets its own derived stream id |
| 2026-09-21 | `fe1eda85` | Roles `HumanResources`, `Finance`, `Administration` |
| 2026-09-21 | `10cd7ab8` | Front: chart, unit and computed supervisor on the user card |
| 2026-09-22 | `f0c4682c` | Front: chart named Structure, layout fixes |
| 2026-09-22 | `eaf308dd` | `AdminPolicy` on creating, editing and re-roling users |
| 2026-10-01 | `3f18078f` | Chart read with `FetchLatest` (PR #20) |

## Notes from reconstruction
- Plan 015 put the chart in `Organization` with one aggregate per unit, a `Path` column, a
  membership reservation and a `SharedKernel` port; the code has none of these (see Decisions).
- Marten rebuilt the aggregate without running field initialisers, so a `List` field was `null` on
  the first `Apply`; reads go through a property with `?? []`.
- The stream fallback read the projection first and replayed only when it was missing, so a stale
  snapshot could hide a just-assigned head; `3f18078f` replaced it with `FetchLatest`.
- `AdminPolicy` was introduced by plan 020 (impersonation), not 015, because the role endpoints were
  open to every member. CLAUDE.md says payroll and rates are the only places a role decides
  anything; `AdminPolicy` (and the forms design policy) are further ones.
- The chart's write endpoints carry no role policy: any signed-in member of the organization can
  create, move and archive units or name heads - and so, indirectly, choose who approves whose hours.
