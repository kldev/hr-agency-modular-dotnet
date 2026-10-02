# 002 · Identity and sign-in

**Status:** done · **Built:** 2026-08-31 → 2026-09-25 · **Verify:** `./verify identity` · **Plan:** none

## Why
Recruiters, sales people and HR need their own accounts inside their agency, and the platform owner
needs a separate account above all agencies. Every request after sign-in has to carry who the person
is, which organization they belong to and what role they hold, so that the rest of the API can decide
on its own without asking `Identity` again.

## What it promises
- The platform owner is a separate account type (`PlatformOwner`, role `Owner`) with its own
  e-mail reservation; a second owner with the same e-mail is refused. Only an owner can create
  another owner.
- A user belongs to exactly one organization and has one `OrganizationRole`: `Admin`, `Recruiter`,
  `HiringManager`, `Interviewer`, `Sales`, `HumanResources`, `Finance` or `Administration`.
  The internal `System` role is not accepted through the API.
- An e-mail address is unique within an organization; a second user with the same e-mail in the same
  organization is refused, and the unique index defeats concurrent requests.
- Name, e-mail, job title and phone are validated and all errors come back in one 400. A password
  shorter than 4 characters is refused.
- The password is stored only as a BCrypt hash, on the e-mail reservation document - never in an event.
- `POST /api/auth/login` takes e-mail, password and an optional organization slug. Without a slug the
  organization is found by the e-mail's domain. A wrong password or unknown account answers 401
  "Invalid login or password" (the same answer for both).
  before any account lookup; an empty list lets everybody in.
- A successful sign-in returns a signed JWT (default 6 hours) carrying user id, e-mail, full name,
  role and `organizationId`.
- Every endpoint is closed unless it opts out (`AllowAnonymous`).
- Creating, editing and re-roling users requires the `Admin` role; changing to the role a person
  already has is refused; a user of another organization is never touched.
- The panel has a sign-in page for members (`/login`) and one for the owner (`/owner`), and a users
  list with create/edit in a drawer.

## Surface
- Backend: `src/HrAgencySystem.Identity` (aggregates `User`, `PlatformOwner`; documents
  `UserEmailReservation`, `OwnerEmailReservation`; `BCryptPasswordHasher`; `Infrastructure/IAM`
  with `JwtTokenService`, `AppClaims`, `JwtConfig`; `PasswordPolicyValidator`).
- API: `POST /api/auth/login`, `POST /api/owner/login`, `GET /api/user/me`, `GET /api/owner/me`,
  `POST|GET /api/users`, `GET|PUT /api/users/{userId}`, `PUT /api/users/{userId}/role`,
  `POST|GET /api/owners`, `GET /api/owners/{ownerId}`; `Api/Auth/AdminPolicy.cs`.
- Frontend: `frontend/src/features/auth`, `frontend/src/features/users`,
  `frontend/src/platform-owner/features/auth`, `.../users`; routes `/login`, `/owner`,
  `/app/users`, `/admin/users`.
- Tests: `tests/HrAgencySystem.UnitTests/Identity`, `tests/HrAgencySystem.IntegrationTests.Platform/{Auth,Users,Owner}`.

## Out of scope for this feature
- Refresh tokens, sign-out and password reset - spec 013. My profile and avatars - specs 017 and 023.
  Impersonation - spec 023. Rate limits on sign-in (not yet).
- External identity providers, SSO, MFA (not yet; nothing in plans).
- A password policy beyond a minimum length of 4 (learning project; not decided).
- A user in two organizations at once (not ever: the organization is part of the identity).

## Acceptance criteria
- [x] Login returns a token for valid credentials, 401 for unknown user or wrong password — `./verify identity`
- [x] Login with slug, and without slug by e-mail domain; unknown domain is refused — `./verify identity`
- [x] Token carries the claims the API binds on and expires after the configured hours — `./verify identity`
- [x] Login over HTTP issues tokens — `./verify identity` (`RefreshTokenEndpointTests`)
- [x] Create user: validation of every field, duplicate e-mail refused, unknown organization refused — `./verify identity`
- [x] Create owner; duplicate owner e-mail refused; only an owner creates owners — `./verify identity`
- [x] Change role; same role refused; another organization's user untouched — `./verify identity`
- [x] Users list and single user scoped to the caller's organization — `./verify identity`
- [x] Password minimum length — `./verify identity`
- [x] Member signs in through the page and lands in the panel — e2e: `frontend/e2e/auth/login.spec.ts`
- [x] Screen — evidence: `docs/screenshots/login.png`
- [ ] Owner sign-in (`POST /api/owner/login`) — unproven (no test found)
- [ ] Password stored only as a BCrypt hash — unproven (no test of `BCryptPasswordHasher`)

## Decisions
- One API host, `Identity` is a module like the others: [ADR-0001](../../../docs/adr/0001-modular-monolith.md).
- `organizationId` claim is the tenant for every other module: [ADR-0004](../../../docs/adr/0004-tenant-on-every-aggregate.md).
- E-mail uniqueness per organization via reservation document: [ADR-0005](../../../docs/adr/0005-uniqueness-reservations.md).
- Other modules read people through `IUserSnapshotRepository`: [ADR-0006](../../../docs/adr/0006-contracts-and-shared-kernel-ports.md).
- `Email`, `FirstName`, `LastName` with `TryCreate`: [ADR-0007](../../../docs/adr/0007-value-objects-trycreate.md).
- Fallback-deny authorization, `AuthorizationException` → 401: [ADR-0008](../../../docs/adr/0008-endpoint-per-file-thin-api.md).
- Option validation at startup (`JwtConfig.MinSecretBytes = 32`): [ADR-0024](../../../docs/adr/0024-quality-gates.md).
- The hash sits on the reservation document, not the stream, so a password never lands in the
  event store (`cefa9b0d`).
- Role and place in the org chart are two separate axes (comment on `OrganizationRole`); the chart is spec 021.

## History
| Date | Commit | Change |
|---|---|---|
| 2026-08-31 | `83298a42` | Identity domain |
| 2026-08-31 | `78b45f66` | Owner and user create handlers |
| 2026-08-31 | `d6b8676a` | E-mail unique within an organization |
| 2026-08-31 | `cefa9b0d` | Password hash stored with the e-mail reservation |
| 2026-09-01 | `4c0da676` | JWT login for users and owners, fallback authorization policy |
| 2026-09-06 | `be630d2d` | Login without slug, organization by e-mail domain |
| 2026-09-07 | `b71f73b6` | `System` role hidden from the API |
| 2026-09-10 | `99d18478` | Panel: authentication store |
| 2026-09-12 | `7950f1f5` | Panel: create user |
| 2026-09-19 | `b51d87d2` | User update flow, identity ids as structs |
| 2026-09-20 | `7ab7ab64` | Change a user's role |
| 2026-09-21 | `fe1eda85` | `HumanResources`, `Finance`, `Administration` roles |
| 2026-09-22 | `eaf308dd` | Create/edit/re-role behind the `Admin` policy |

## Notes from reconstruction
- Until `eaf308dd` any authenticated member could call `PUT /api/users/{id}/role` and make
  themselves `Admin`; the policy comment says so explicitly.
- Login by domain takes the first organization whose domain list contains the domain; domains are
  not unique across organizations, so two agencies sharing a domain would make the result arbitrary.
- The minimum password length is 4; `3e2f19de` aligned the message with that number rather than
  raising it.
