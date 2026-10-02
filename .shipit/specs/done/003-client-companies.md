# 003 · Client companies

**Status:** done · **Built:** 2026-08-30 → 2026-09-21 · **Verify:** `./verify companies` · **Plan:** backend 009 (company profile part, local, not in git)

## Why
Everything an agency does is for a client company: sales deals, job descriptions, job posts,
projects. A sales person must be able to add a company in seconds from a business card, while the
people who later sign a contract need its legal name, registered address and bank details. Both
halves have to live on one record so that nobody retypes them per project.

## What it promises
- A member can create a company with a name (max 250), a two-letter country code, a tax id
  (required, max 50), a registration number, an industry and a website, optionally with a first
  contact person. All validation errors come back together in one 400.
- A tax id is unique within an organization; the same tax id is allowed in another organization;
  two concurrent creates with the same tax id produce one company.
- A company can be edited (name, country, registration number, industry, website), and found by tax id.
- Contact people (name, position, e-mail, phone) can be added, edited and removed; a company's
  contacts are listed on its page.
- A separate "complete profile" step stores the paperwork half: legal name, registered address,
  VAT number, IBAN/BIC and legal representative. Every field is optional; half an address, an
  invalid VAT number or a BIC without an IBAN is refused, every problem reported at once.
- `IsProfileComplete` is computed (legal name + registered address + tax id), never stored. The
  moment it first becomes true is recorded once as `CompanyProfileCompleted`; a company from another
  organization answers 403.
- A company knows how many job posts it has and how many are published, counted from Recruitment's
  integration events; several posts published at once are all counted.
- The panel lists companies (table and card list, search), shows a details page with contacts, and
  has create/edit drawers and a complete-profile wizard.

## Surface
- Backend: `src/HrAgencySystem.Company` (aggregate `Company`, `CompanyProfile`, document
  `CompanyContact`, `CompanyTaxIdReservation`, `CompanyProjection`, `CompanySnapshotRepository`,
  `Integration/JobPost*IntegrationEventHandler`).
- API: `POST|GET /api/companies`, `GET|PUT /api/companies/{companyId}`,
  `GET /api/companies/find-by-tax/{taxId}`, `GET /api/companies/{companyId}/contacts`,
  `PUT /api/companies/{companyId}/profile`, `POST /api/company-contacts/{companyId}`,
  `GET|PUT|DELETE /api/company-contacts/{contactId}`; pickers under `/api/suggestion/companies`,
  `/api/suggestion/company-contacts`.
- Frontend: `frontend/src/features/companies`, `frontend/src/features/company-contacts`; routes
  `/app/companies`, `/app/companies/$id`.
- Tests: `tests/HrAgencySystem.UnitTests/Companies`, `tests/HrAgencySystem.IntegrationTests.Sales/Companies`.

## Out of scope for this feature
- Deactivating or deleting a company: `CompanyStatus.Inactive` exists but no command sets it (not yet).
- Requiring the profile when a lead is created (not ever - plan 009 decision: it would kill fast lead entry).
- Contacts as an event-sourced history: a contact is a plain document, deleted for good (not decided).
- Checking a tax id or VAT number against an official register (not yet).

## Acceptance criteria
- [x] Create with valid data; invalid fields → 400 with all errors — `./verify companies`
- [x] Duplicate tax id refused; allowed across tenants; concurrent creates → one company — `./verify companies`
- [x] Value objects: name, country code, tax id, registration number limits — `./verify companies`
- [x] Complete profile: full, partial, empty, half address, invalid VAT, BIC without IBAN, all errors at once — `./verify companies`
- [x] `CompanyProfileCompleted` raised once, only on the transition — `./verify companies`
- [x] Lead without profile is valid but incomplete; another organization → 403 — `./verify companies`
- [x] Job posts published at once are all counted on the company — `./verify companies`
- [x] Company suggestions scoped to the organization — `./verify suggestions`
- [x] Companies list renders seeded data — e2e: `frontend/e2e/overview/main-views.spec.ts`
- [x] Screen — evidence: `docs/screenshots/companies.png`
- [ ] Update company (`PUT /api/companies/{id}`) — unproven (no test found)
- [ ] Contact create, edit, delete — unproven (no test found)
- [ ] Find by tax id — unproven (no test found)

## Decisions
- Event-sourced company stream; contacts as plain Marten documents: [ADR-0002](../../../docs/adr/0002-marten-event-store.md).
- Tax id uniqueness via reservation document: [ADR-0005](../../../docs/adr/0005-uniqueness-reservations.md).
- Recruitment tells `Company` about posts through `Recruitment.Contracts`; other modules read a
  company through `ICompanySnapshotRepository`: [ADR-0006](../../../docs/adr/0006-contracts-and-shared-kernel-ports.md).
- Value objects `CompanyName`, `TaxId`, `VatNumber`, `BankAccount`: [ADR-0007](../../../docs/adr/0007-value-objects-trycreate.md).
- The client of a project is `Company` with an optional profile - no `Client`/`Contractor` entity
  (plan 009: "contractor" means the opposite party in English).
- `CompanySnapshotRepository` falls back to replaying the stream when the projection lags, because
  the profile is usually completed seconds before a contract is recorded: [ADR-0013](../../../docs/adr/0013-contract-on-project-stream.md).

## History
| Date | Commit | Change |
|---|---|---|
| 2026-08-30 | `a0e5f16c` | Company projection and GET endpoint |
| 2026-08-31 | `a0174cf9` | Get company |
| 2026-09-01 | `b101b243` | Organization taken from the authenticated user |
| 2026-09-07 | `2c114b2e` | Job post counts from Recruitment integration events |
| 2026-09-08 | `d9d35a85` | Company contact document |
| 2026-09-08 | `16099bb5` | Contact create/update handlers |
| 2026-09-08 | `710a3eed` | Update company |
| 2026-09-11 | `49bf5fc3` | Panel: create company drawer |
| 2026-09-12 | `1851d2ce` | Optional contact when creating a company |
| 2026-09-12 | `6ecc5d8f` | Panel: company details page |
| 2026-09-14 | `6864ca59` | Panel: companies card list |
| 2026-09-20 | `a1753f36` | Optional registration and billing profile with completeness flag |
| 2026-09-20 | `bb54923d` | Complete-profile wizard (with the projects UI) |
| 2026-09-21 | `b0d70b31` | Job post counting takes the stream exclusively, so a batch is not half lost |

## Notes from reconstruction
- The integration handlers on the Recruitment side (`JobPostCreatedHandler`,
  `ActiveJobPostChangedHandler`) still contain `await Task.Delay(100)` before returning the event.
- `IsProfileComplete` also checks `TaxId is not null`, but `TaxId` is required at creation, so that
  half of the condition can never fail today.
- `CompanyProjection.ApplicantsCount` is declared but nothing ever increments it - it is always 0.
- The contact create path has no unit or integration test, although the panel uses it on every
  company page.
