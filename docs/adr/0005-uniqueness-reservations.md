# ADR-0005: Uniqueness across aggregates is a reservation document with a unique index, written in the same transaction as the event

- **Status:** Accepted
- **Date:** 2026-08-30
- **Evidence:** `f1c00ced` start project, `d6b8676a` feat(identity): enforce email uniqueness within organization, `cefa9b0d` feat(identity): hash password stored with email reservation, `e59e85f4` refactor(shared-kernel): guard OrganizationId construction and cover slug reservations with tests, `30860843` feat(teams): keep a person on at most one team, `c1e5f904` feat(workers): workers module backend
- **Specs:** [001](../../.shipit/specs/done/001-organizations-and-tenancy.md), [002](../../.shipit/specs/done/002-identity-and-sign-in.md), [003](../../.shipit/specs/done/003-client-companies.md), [006](../../.shipit/specs/done/006-candidates-and-applications.md), [014](../../.shipit/specs/done/014-teams.md), [018](../../.shipit/specs/done/018-legal-entities.md), [019](../../.shipit/specs/done/019-workers-and-assignments.md)

## Context
An event-sourced aggregate only guards its own stream. Rules such as "one company per tax id in
an agency" or "one user per e-mail in an agency" span streams, and the async read models
([ADR-0002](0002-marten-event-store.md)) lag, so checking a projection would let two concurrent
requests both pass.

## Decision
- Each such rule has a reservation document in the owning module's
  `Infrastructure/Persistence/` with a unique Marten index, e.g. `CompanyTaxIdReservation`,
  `OrganizationSlugReservation`, `UserEmailReservation`, `OwnerEmailReservation`,
  `CandidateEmailReservation`, `LegalEntityTaxIdReservation`, `TeamMembershipReservation`,
  `WorkerEmailReservation`, `WorkerIdentityDocumentReservation`, `FormCodeReservation`.
- The handler first asks the reservation repository and throws `BusinessRuleException` with a
  `public const` message for a friendly 400. It then writes the reservation in the same Marten
  session as the event; the unique index defeats the race, and the resulting
  `DocumentAlreadyExistsException` maps to 409 (`MapDocumentErrors`, present since `f1c00ced`).
- Reservations are scoped by `OrganizationId`, except the two that are platform-wide by nature:
  the organization slug (it is the public board address) and the platform owner's e-mail.

## Alternatives considered
- **Derive the stream id from the unique key** instead of reserving: chosen for time sheets
  (one stream per person and month, [ADR-0016](0016-timesheet-stream-per-person-month.md)), where
  the key never changes.
- **A projection lookup** where a unique index cannot express the rule: overlapping assignment
  periods (`IAssignmentsQueryRepository.HasOverlappingAssignment`) and the "same name plus phone"
  duplicate check for workers. Both accept the lag; the port documents the limit.
- **A check on a list inside one stream**: system field codes in forms live on one catalogue
  stream per organization, so no reservation is needed there.

## Consequences
- The friendly check and the index are both required: without the first, users get a bare 409;
  without the second, concurrent requests create duplicates.
- A reservation can carry more than the key: the user's password hash is stored on the e-mail
  reservation (`cefa9b0d`), which is what sign-in reads.
- Changing the unique value (organization slug, user e-mail) must move the reservation in the
  same transaction; the slug cases are covered by tests since `e59e85f4`.
- Integration tests must clear reservation documents between tests (`DatabaseCleaner`).

## Revisit when
1. A rule needs uniqueness over a range or a combination a unique index cannot express - then a
   projection check plus a documented lag, as for assignments.
2. Reservations start to drift from the aggregates (a reservation without a stream) - then a
   repair job or a different design is due.
