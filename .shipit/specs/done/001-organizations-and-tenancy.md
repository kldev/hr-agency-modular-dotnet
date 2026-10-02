# 001 · Organizations and tenancy

**Status:** done · **Built:** 2026-08-30 → 2026-09-19 · **Verify:** `./verify organizations` · **Plan:** none

## Why
HR Agency is one system shared by many recruitment agencies. The platform owner needs to open an
agency (an organization) with a public slug and the e-mail domains its people sign in with, and
every agency must see only its own data. Without a tenant boundary nothing else in the product can
be built.

## What it promises
- The platform owner can create an organization with a name (max 250 characters), a slug (max 100
  characters) and at least one e-mail domain. Name and slug are trimmed; the slug is lower-cased.
- Invalid name and slug are reported together in one 400, not one at a time.
- A slug already used by another organization is refused ("An organization slug already exits");
  two concurrent creates with the same slug produce one organization.
- An organization without any non-empty e-mail domain is refused ("No email domains specified").
- The owner can change the slug alone (same slug again is refused) or update name, slug, domains
  and info together; an unknown organization answers 404.
- The owner can list organizations, read one by id or by slug, and create or edit users inside any
  organization (but not move a user to another organization).
- Every organization route is owner-only: an organization member gets 403.
- Every aggregate, projection and query of the other modules carries an `OrganizationId` taken
  from the signed-in user's token, never from the request body.
- The owner panel lists organizations (table and card list), with search and an edit form.

## Surface
- Backend: `src/HrAgencySystem.Organization` (aggregate `Organization`, `OrganizationSlugReservation`,
  `OrganizationProjection`, `OrganizationChecker` implementing the `IOrganizationChecker` port);
  `SharedKernel/Tenant/OrganizationId.cs` (a struct that refuses `Guid.Empty`).
- API (group requires the `Owner` platform role): `POST|GET /api/organization`,
  `GET|PUT /api/organization/{organizationId}`, `PUT /api/organization/{organizationId}/slug`,
  `GET /api/organization/{slug}`, `GET|POST /api/organization/users`,
  `PUT /api/organization/users/{userId}`.
- Tenant plumbing: `Api/Auth/AppUserAuthenticated.cs`, `ClaimsPrincipalExtensions.cs` (claim
  `organizationId`), `OrganizationAccessDeniedException` → 403 in `GlobalExceptionHandler`.
- Frontend: `frontend/src/platform-owner/features/organizations`, routes `/admin/organizations`,
  `/admin/organizations/$id`.
- Tests: `tests/HrAgencySystem.UnitTests/Organizations`,
  `tests/HrAgencySystem.IntegrationTests.Platform/Organization`,
  `tests/HrAgencySystem.IntegrationTests.Persistence/Organization`.

## Out of scope for this feature
- Deactivating or deleting an organization: there is no status, every organization is "active"
  for the feed scheduler (not yet).
- Self-service sign-up of an agency: only the platform owner creates organizations (not ever, by design).
- Billing, plans or per-tenant limits (not ever in this learning project).
- Uniqueness of e-mail domains across organizations is not enforced (not decided).

## Acceptance criteria
- [x] Create with valid data; name and slug normalized — `./verify organizations`
- [x] Missing/too long name or slug → 400 with all errors at once — `./verify organizations`
- [x] Duplicate slug refused; concurrent creates allow only one — `./verify organizations`
- [x] Create without e-mail domains → 400 — `./verify organizations`
- [x] Slug change: valid, empty, too long, duplicate, unknown organization (404), normalized — `./verify organizations`
- [x] Slug reservation: unique across organizations, case/whitespace-insensitive lookup — `./verify organizations`
- [x] Owner edits a user of any organization, cannot use another user's e-mail, organization roles get 403 — `./verify organizations`
- [x] Cross-tenant reads and writes are refused in other modules (users, teams, suggestions, interviews) — `./verify identity`, `./verify teams`, `./verify suggestions`, `./verify interviews`
- [x] Owner's organization register renders the seeded tenants — e2e: `frontend/e2e/reports/reports.spec.ts`
- [x] Screen — evidence: `docs/screenshots/owner-organizations.png`
- [ ] Full update (`PUT /api/organization/{id}`) with name, domains and info — unproven (no test found)
- [ ] Read by slug and organization slice — unproven (no test found)

## Decisions
- Tenant id on every aggregate, projection and index, from the JWT claim: [ADR-0004](../../../docs/adr/0004-tenant-on-every-aggregate.md).
- Slug uniqueness through a reservation document with a unique index: [ADR-0005](../../../docs/adr/0005-uniqueness-reservations.md).
- Other modules ask "does this organization exist / what is its slug" only through the `IOrganizationChecker` port: [ADR-0006](../../../docs/adr/0006-contracts-and-shared-kernel-ports.md).
- `OrganizationName`/`OrganizationSlug` with `TryCreate`: [ADR-0007](../../../docs/adr/0007-value-objects-trycreate.md).
- Event-sourced aggregate on Marten: [ADR-0002](../../../docs/adr/0002-marten-event-store.md).
- E-mail domains live on the organization so a user can sign in without typing the slug (`be630d2d`).
- `OrganizationId` refuses an empty `Guid` at construction (`e59e85f4`), so a missing claim fails
  loudly instead of silently reading "no tenant".

## History
| Date | Commit | Change |
|---|---|---|
| 2026-08-30 | `f1c00ced` | Project start: organization create |
| 2026-08-30 | `d44687c2` | Slug update |
| 2026-08-30 | `7a47fbd4` | Check for a non-existing organization |
| 2026-08-31 | `5820e004` | Value object tests |
| 2026-08-31 | `df3445a2` | Create organization endpoint tests |
| 2026-09-01 | `b101b243` | Companies take the organization from the authenticated user |
| 2026-09-06 | `be630d2d` | Sign-in without slug, organization found by e-mail domain |
| 2026-09-08 | `97a2c72a` | Organization slice |
| 2026-09-11 | `15da2f4b` | Owner creates users inside an organization |
| 2026-09-11 | `c8c5c6f4` | Owner panel: organizations search |
| 2026-09-12 | `985910a8` | Owner panel: organization update |
| 2026-09-15 | `564729fc` | Organization card list |
| 2026-09-19 | `e59e85f4` | Guarded `OrganizationId`, slug reservation tests |

## Notes from reconstruction
- Changing a slug inserts a new reservation and never removes the old one, so the old slug stays
  taken and still resolves to the organization. `OrganizationChecker.GetSlug` picks the first
  reservation without ordering, so after a rename it may hand out the old slug (job post creation
  stores that slug).
- Tenant checks are done per module (`ValidateAggregateUpdate`, `ValidateOrganization`, query
  filters), not by one central filter, and some handlers skip them - see specs 004 and 005.
- The error constant is spelled `SlugAlreadyExitsMessage` ("exits") and tests assert on it.
- `OrganizationReportProjection` (feeds the reports schema) was added later by `940e05a2` - see spec 027.
