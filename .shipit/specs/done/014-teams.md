# 014 · Teams

**Status:** done · **Built:** 2026-09-19 → 2026-09-23 · **Verify:** `./verify teams` · **Plan:** backend 006, 007, 008 (local, not in git)

## Why
In an agency the work is carried by a team - usually a sales person, a recruiter and an operations
person - not by one individual. Recruiters rotate often: "team X handles this" survives a rotation,
"one named recruiter handles this" does not. Until this feature every piece of work was pinned to a single user.

## What it promises
- Any signed-in member of the organization can create a named team with its whole roster at once; each member has a team
  role (`Sales`, `Recruiter`, `Operations`, `Lead`). A team without members, the same person twice, a
  blank name or a member from another organization is refused.
- Members can be added, removed and given another role; a team can be renamed. Removing the last
  member is refused, and so is giving somebody the role they already hold or renaming to the same name.
- A person belongs to at most one team at a time; joining a second one is refused.
- A user can be created straight into a team (team + role); an unknown team or a team without a role
  is refused. The user list and details show the team, and follow a rename or a removal.
- Being added to a team or having your role changed by somebody else sends a mail; doing it to
  yourself sends nothing; removal sends nothing.
- Another organization's team cannot be read or changed.
- The panel has a teams list with roster preview, a team details page with membership management,
  and user screens with a "change team" action.

## Surface
- Backend: `src/HrAgencySystem.Teams` (aggregate `Team`, reservation `ITeamMembershipReservationRepository`),
  `src/HrAgencySystem.Teams.Contracts` (`TeamRole`, `TeamInfo`, `IntegrationEvents`, `IntegrationCommands`),
  `TeamMembershipChanged` handled in `Identity` (`UserProjection.Team`).
- API: `POST|GET /api/teams`, `GET /api/teams/{id}`, `PUT /api/teams/{id}/name`,
  `POST /api/teams/{id}/members`, `DELETE /api/teams/{id}/members/{userId}`,
  `PUT /api/teams/{id}/members/{userId}/role`, `GET /api/suggestion/teams`.
- Frontend: `frontend/src/features/teams`, `frontend/src/features/users`, routes `/app/teams`, `/app/teams/$id`.
- Tests: `tests/HrAgencySystem.UnitTests/Teams`, `tests/HrAgencySystem.IntegrationTests.Platform/Teams`,
  `tests/HrAgencySystem.IntegrationTests.Platform/Users/UserTeamTests.cs`.

## Out of scope for this feature
- Dissolving or archiving a team - with "never leave a team empty" a team, once created, stays forever (not yet; shape described in plan 006).
- Unique team names (not ever unless people get confused - a duplicate name breaks no invariant).
- Membership history as a read model (the `Team` stream holds it; no reader yet).
- The team on `UserSnapshot` (not ever - it would freeze membership in every event).
- Guaranteed ordering of `TeamMembershipChanged` on the local queue (accepted risk, plan 007).

## Acceptance criteria
- [x] Create with full roster; empty, duplicate person, blank name, foreign member refused — `./verify teams`
- [x] Add, remove (not the last), change role, rename; foreign team refused — `./verify teams`
- [x] One team per person enforced by a reservation; seat released on removal — `./verify teams`
- [x] Handover mails only when somebody else made the change — `./verify teams`
- [x] User created into a team; team shown on the user, follows rename and removal — `./verify identity`
- [x] Create a team and add a member in the panel — e2e: `frontend/e2e/organization/teams.spec.ts`
- [x] Teams list — evidence: `docs/screenshots/teams.png`

## Decisions
- Own module; team role is a split of work, independent of `OrganizationRole` (a permission) - plan 006.
- `Teams.Contracts` and a membership integration event, because `Identity` keeps a copy of the team
  name and role: [ADR-0006](../../../docs/adr/0006-contracts-and-shared-kernel-ports.md).
- "One person, one team" is a reservation document with a unique index:
  [ADR-0005](../../../docs/adr/0005-uniqueness-reservations.md).
- Membership mails: [ADR-0010](../../../docs/adr/0010-email-over-rabbitmq.md).
- A rename fans out one event per member so the copy on the user never goes stale (teams are 3-10 people).
- `CreateUser` validates the team synchronously through a port, so a bad team id fails the request
  instead of dead-lettering after a 201 (plan 007).

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-19 | `d65a698b` | Teams module, named teams and role-tagged members |
| 2026-09-20 | `5612e407` | `TeamRole` moved into `Teams.Contracts` |
| 2026-09-20 | `30860843` | One team per person, membership change events |
| 2026-09-20 | `a144390c` | Create a user straight into a team |
| 2026-09-20 | `b7582b50` | Teams list, details and membership in the panel |
| 2026-09-20 | `b1052102` | User create/edit/role/team actions |
| 2026-09-20 | `b38c46af` | Team moves correct after a failed step, stable member row keys |
| 2026-09-23 | `ccd48368` | E2E suite incl. teams |

## Notes from reconstruction
- Plan 007 reversed two decisions of plan 006 (no `Teams.Contracts`, no membership event) once
  `Identity` started keeping a copy of the team.
- Plan 007 found that the first tuple element is not cascaded when an endpoint calls
  `InvokeAsync<T>` - the event must also go into `OutgoingMessages`.
- "Never remove the last member" also blocks moving the only member to another team; the panel's
  change-team drawer explains this instead of failing (plan 008 notes).
- The owner panel got its own `OrganizationUserForm` because the team picker is scoped to the
  caller's organization, not the one the owner is managing.
