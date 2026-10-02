# 023 · Impersonation and avatars

**Status:** done · **Built:** 2026-09-22 → 2026-09-22 · **Verify:** `./verify identity` · **Plan:** backend 020 (local, not in git)

## Why
To check that a time sheet approval works, one has to be the supervisor from the chart, and being a
supervisor is not a role anybody can grant themselves - so the only way in was to know somebody
else's password. An administrator needs to sign in as a member of their own agency. Profile pictures
existed only "for yourself": nobody could set one for a colleague, and nobody but the owner ever saw
one, so lists showed initials only.

## What it promises
- An administrator can sign in as any other member of their organization, other administrators
  included, without a password: `POST /api/user/impersonate/{userId}`.
- The issued token is the target's identity plus an `impersonatedBy` claim naming the administrator.
  It is short-lived (`Jwt:ImpersonationExpiresInMinutes`, default 30) and comes with no refresh
  token, so the session cannot be extended.
- Signing in as yourself is refused. A user of another organization, an unknown id and a system
  account all answer the same 404. A caller who is not an administrator is refused.
- Every issued impersonation token is logged as a warning naming both ids; `ImpersonatedBy` is on
  every log line and span of the impersonated session.
- There is no "return to my account": the administrator signs out and back in. The panel says so
  before confirming, and shows a banner while impersonating.
- The entry is in the user row/header menu and next to heads and members in the chart; absent for
  non-admins, disabled with a hint on the administrator's own row.
- An administrator can upload or remove another member's avatar
  (`POST|DELETE /api/users/{userId}/avatar`); a non-administrator cannot.
- Any member of the organization can see a colleague's avatar (`GET /api/users/{userId}/avatar`);
  another organization's picture is simply not there.
- `GET /api/users/avatars` lists who in the organization has a picture (user id + file id), so lists
  show faces without a request per row and can bust the browser cache.
- Setting a picture for somebody who does not exist never reaches the storage.

## Surface
- Backend: `src/HrAgencySystem.Identity/Application/Users/Impersonate`, `Infrastructure/IAM/JwtTokenService.cs`,
  `JwtConfig.cs`; `src/HrAgencySystem.Api/Endpoints/Auth/Maps/MapImpersonate.cs`,
  `Endpoints/User/Maps/MapUploadAvatarFor.cs`, `MapRemoveAvatarFor.cs`, `MapDownloadAvatarFor.cs`,
  `MapGetAvatars.cs`; `Api/Auth/AppUserAuthenticated.cs` (`ImpersonatedBy`), `TenantTelemetryMiddleware`.
- Frontend: `frontend/src/features/users/components/ImpersonateUserDialog.tsx`,
  `components/layout/ImpersonationBanner.tsx`, `components/ui/Avatar.tsx`, avatars in the users table
  and the chart's unit panel.
- Tests: `tests/HrAgencySystem.IntegrationTests.Platform/Auth/ImpersonateEndpointTests.cs`,
  `tests/HrAgencySystem.IntegrationTests.Platform/Users/AvatarTests.cs`.

## Out of scope for this feature
- Returning to the administrator's session - not ever as designed (a second parallel session state).
- Forbidding a chain of impersonations - not yet; one line when it proves real.
- Ending an impersonation early, an audit screen or a domain event - not yet (`Audit` is an empty project).
- Recording who set a picture (`SetByUserId`) - not yet.
- `AvatarFileId` on the user projection - not ever: avatars are deliberately not events.
- Paging the avatar catalogue - not yet; fine for tens of people.
- `Admin` gates on other modules - not yet.

## Acceptance criteria
- [x] Token is the target's plus the administrator's name; short-lived with nothing to refresh — `./verify identity`
- [x] Refuses yourself, allows another administrator — `./verify identity`
- [x] 404 for another organization and for an unknown user; non-admin refused — `./verify identity`
- [x] Administrator sets and removes somebody else's picture; non-admin cannot — `./verify identity`
- [x] Colleagues see a picture; another organization's is not there — `./verify identity`
- [x] Catalogue lists only people with a picture — `./verify identity`
- [ ] The impersonation dialog, banner and hard reload in the panel — unproven (no e2e found)
- [ ] The warning log line on every impersonation — unproven (no test found)

## Decisions
- The gate is the `Admin` organization role via `AdminPolicy`, put on user management at the same
  time, since otherwise anyone could grant themselves `Admin` first: see
  [021](021-agency-chart-and-roles.md) and [ADR-0008](../../../docs/adr/0008-endpoint-per-file-thin-api.md).
- The target is read through the organization-scoped user query, which yields 404 across tenants:
  [ADR-0004](../../../docs/adr/0004-tenant-on-every-aggregate.md).
- Avatars stay files in the file service, not events: [ADR-0012](../../../docs/adr/0012-separate-file-service.md).
- The front stores the token like the owner session (access token only) and does a full page reload
  instead of clearing query caches, because several query keys carry no user id.
- Avatar commands take an explicit `ModifiedBy` (the acting administrator), not the target (`11d5e0b7`).

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-22 | `eaf308dd` | `AdminPolicy` on user create, edit, role change |
| 2026-09-22 | `11d5e0b7` | Avatar commands name who changes the picture |
| 2026-09-22 | `e81da69b` | Admin sets anybody's avatar; colleagues can see it |
| 2026-09-22 | `126d2f8f` | Impersonation endpoint and token |
| 2026-09-22 | `ae052ada` | Front: `Avatar` primitive, one-token session |
| 2026-09-22 | `3da15185` | Front: log in as a member, set their picture, faces in lists |
| 2026-09-22 | `c110c797` | Impersonation moved off the anonymous `/api/auth` prefix |
| 2026-09-22 | `43364af0` | Client regenerated after the route move |

## Notes from reconstruction
- Plan 020 put the endpoint at `POST /api/auth/impersonate/{userId}`. Everything under `/api/auth`
  is anonymous and the front's proxy sends no bearer there, so the bearer never arrived; the route
  moved to `/api/user/impersonate/{userId}` (`c110c797`).
- A chain of impersonations stays possible: the check is "my role is `Admin`", not "I am not
  impersonating somebody already" - accepted in the plan as a known gap.
- The `ModifiedBy` the avatar commands now carry is not persisted anywhere; the plan fixed only its
  name and meaning.
