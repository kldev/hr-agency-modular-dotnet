# ADR-0004: `OrganizationId` is carried by every aggregate, projection, query and index, and comes from the signed token

- **Status:** Accepted
- **Date:** 2026-08-30 (`OrganizationId` in `SharedKernel`); 2026-09-01 (taken from the JWT claim)
- **Evidence:** `f1c00ced` start project, `a0e5f16c` feat: add company projection and GET endpoint, `b101b243` feat(company): use organization id from authenticated user, `03a9752a` feat(user): use organization id from authenticated user, `b54b5666` feat(sales): add opportunity integration tests, `e59e85f4` refactor(shared-kernel): guard OrganizationId construction
- **Specs:** [001](../../.shipit/specs/done/001-organizations-and-tenancy.md), [002](../../.shipit/specs/done/002-identity-and-sign-in.md), [003](../../.shipit/specs/done/003-client-companies.md), [015](../../.shipit/specs/done/015-file-service.md), [027](../../.shipit/specs/done/027-reports.md)

## Context
The product is SaaS for several recruitment agencies in one database and one event store. One
agency must never read or change another's data, and every list in the panel is per agency.

## Decision
- `OrganizationId` is a `readonly record struct` in `SharedKernel/Tenant` that refuses
  `Guid.Empty` (`e59e85f4`). Every aggregate, projection and document carries it.
- The API reads it from the JWT claim (`AppClaims.OrganizationId`) through
  `ClaimsPrincipalExtensions.GetAuthenticatedUser`. Commands take it as a plain `Guid`; the
  handler converts with `OrganizationId.From(...)`.
- Every query filters by it, and Marten indexes are declared with `OrganizationId` as the
  leading column (first index in `a0e5f16c`).
- Touching another agency's aggregate is refused; the intended exception is
  `OrganizationAccessDeniedException` -> 403 (added in `b54b5666`).
- Cross-process calls carry the organization as a signed claim, never as a parameter: the file
  service token (`org`, [ADR-0012](0012-separate-file-service.md)) and the reports token.
- The platform owner is outside tenancy: owner accounts have their own email reservation with no
  organization, and owner-only routes use a separate principal (`GetOwner`).

## Alternatives considered
- **`OrganizationId` in the request body.** That was the first shape (`f1c00ced`,
  `CreateCompanyRequest(Guid OrganizationId, ...)`), replaced by the claim in `b101b243` and
  `03a9752a` once JWT sign-in existed (`4c0da676`).
- Marten's built-in multi-tenancy was not used; no record of it being weighed.

## Consequences
- The check "is this aggregate mine" is written in each handler and is opt-in. `.docs/05`
  (local) counted three variants with two HTTP codes. Today ten call sites throw
  `OrganizationAccessDeniedException` (403), but `AssignJobDescriptionRecruiterHandler` still uses
  `OrganizationNotMatchMessage` and `UpdateJobPostHandler`, `ChangeJobPostRecruiterHandler`,
  `UpdateJobDescriptionHandler` throw a literal `"Invalid organization id"` as a 400.
- Not every index follows the rule: `CandidateEmailReservation` is indexed `(Email, OrganizationId)`.
- `OrganizationId` goes on logs and spans (`TenantTelemetryMiddleware`) but never on a metric tag,
  because each tenant would be a new Prometheus series.
- Integration tests fake the claim with `X-Test-OrganizationId` (`TestAuthHandler`).

## Revisit when
1. A handler is found without the ownership check - then move the check into Wolverine middleware
   or a shared `EnsureSameOrganization` so it stops being opt-in.
2. One agency needs data isolation stronger than a column (separate schema or database).
