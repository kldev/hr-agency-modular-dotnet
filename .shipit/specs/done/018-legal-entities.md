# 018 · Legal entities

**Status:** done · **Built:** 2026-09-20 → 2026-09-22 · **Verify:** `./verify legal-entities` · **Plan:** backend 012 (local, not in git)

## Why
An agency rarely trades as a single company. One organization works through several registered
companies, and it is a specific one that signs the contract, issues the invoices and carries the
legal duties in the country of work (licence, notifications, authorised recipient). The system had
no way to say which of the agency's own companies delivers a project.

## What it promises
- A member can record one of the agency's own companies: trading name, full registered name, tax id
  (required), optional EU VAT number, registered address, description, president (name, optional
  e-mail) and the date it started trading.
- Bank accounts are kept per purpose (`Incoming`, `Outgoing`) and currency; a second account for the
  same purpose and currency is refused. A malformed IBAN is refused and spacing is normalised; the
  IBAN checksum is not computed.
- A tax id is used once inside an organization.
- Editing replaces the details and the whole list of accounts.
- Closing an entity records its last trading day; closing twice, or a last day before it started,
  is refused. Whether it is active is computed from the dates, never stored.
- The list can show only entities still trading; the project wizard picks from those.
- Every project is delivered by one legal entity, frozen as a snapshot on the project (rules in
  [016](016-projects-and-compliance.md)). A closed entity stays on the projects it ran.
- Another organization's entity is simply not there.

## Surface
- Backend: `src/HrAgencySystem.LegalEntities` (aggregate `LegalEntity`, value objects `TaxId`,
  `VatNumber`, `President`, `LegalEntityBankAccount`; `LegalEntityTaxIdReservation`),
  port `SharedKernel/Snapshots/ILegalEntitySnapshotRepository`.
- API: `POST|GET /api/legal-entities`, `GET|PUT /api/legal-entities/{id}`, `POST /api/legal-entities/{id}/close`.
- Frontend: `frontend/src/features/legal-entities`, routes `/app/legal-entities`, `/app/legal-entities/$id`;
  create/edit as a wizard in a dialog; picker in the project wizard.
- Tests: `tests/HrAgencySystem.UnitTests/LegalEntities`, `tests/HrAgencySystem.IntegrationTests.Delivery/LegalEntities`,
  project side in `IntegrationTests.Delivery/Projects/ProjectLegalEntityTests.cs`.

## Out of scope for this feature
- Invoices (not yet). The model is ready: an invoice would pick the `Incoming` account in its
  currency from the delivering entity and freeze it. The snapshot does not carry accounts until then.
- Continuing a project on another entity by copying it (`CopyProject`, `ContinuedFromProjectId`) -
  designed in plan 012, not built.
- No `.Contracts` project: `Projects` is the only consumer and reads through a SharedKernel port.

## Acceptance criteria
- [x] Entity recorded with registered details; accounts per purpose and currency; duplicate pair refused — `./verify legal-entities`
- [x] Tax id unique per organization — `./verify legal-entities`
- [x] Edit replaces details and accounts — `./verify legal-entities`
- [x] Close records the last day; twice or before start refused — `./verify legal-entities`
- [x] Active-only list; foreign entity not visible — `./verify legal-entities`
- [x] Factory reports every fault at once; president e-mail optional but validated; IBAN normalised — `./verify legal-entities`
- [x] A project names its delivering entity; a closed or foreign entity cannot take it — `./verify projects`
- [ ] Legal entity screens and wizard in the panel — unproven (no test found)

## Decisions
- Tax id uniqueness through a reservation document: [ADR-0005](../../../docs/adr/0005-uniqueness-reservations.md).
- Consumed by `Projects` through `ILegalEntitySnapshotRepository`, no module reference:
  [ADR-0006](../../../docs/adr/0006-contracts-and-shared-kernel-ports.md).
- Tenant scoped aggregate and reservation: [ADR-0004](../../../docs/adr/0004-tenant-on-every-aggregate.md).
- Named `LegalEntity`, because `Company` already means the client; a "this is us" flag on `Company`
  would mix client and agency in one table (plan 012).
- `TaxId`/`VatNumber` copied from `Company` instead of moved into `SharedKernel` (~40 lines each).
- The president is not a `ContactPerson`, which requires an e-mail; it is known from the register by name.
- No IBAN checksum: a slightly wrong mod-97 rejects good accounts, which is worse than a typo the first transfer catches.

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-20 | `b7bb9431` | Legal entities with bank accounts per purpose and currency |
| 2026-09-20 | `680da48d` | Every project delivered by a legal entity |
| 2026-09-20 | `f1bbbb95` | Manage entities in the panel, pick one for a project |
| 2026-09-21 | `9e4c32aa` | Edit in a wizard dialog instead of a drawer |
| 2026-09-22 | `d2ead7c1` | API client regenerated |

## Notes from reconstruction
- Plan 012 lists four open questions for the owner (contacts and team in a copy, hiding closed
  entities, required description). Only "a closed entity cannot take a new project" is visible in
  code; the copy questions became moot because `CopyProject` was never built.
- Also the workers module freezes which of these companies posted a person (`AssignmentPlanned`).
