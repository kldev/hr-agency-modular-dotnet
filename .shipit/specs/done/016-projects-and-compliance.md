# 016 · Projects and compliance

**Status:** done · **Built:** 2026-09-20 → 2026-09-24 · **Verify:** `./verify projects` · **Plan:** backend 009; frontend 009 (local, not in git)

## Why
After a sale the agency delivers a service: it signs a contract with the client, names who answers
for the work, keeps the paperwork, and - when people work in Belgium or Germany - has to prove a list
of legal obligations. Before this module none of that had a home; the account manager kept it in
their head and in mail.

## What it promises
- A project is created for a client company, delivered by one of the agency's own legal entities,
  with a country of work, an engagement type (`PostingOfWorkers`, `TemporaryAgencyWork`,
  `Outsourcing`, `LocalEmployment`), a workplace and a period. It starts as `Draft`.
- Statuses `Draft → Active → Suspended → Completed/Cancelled`; a final status does not restart, a
  draft can be cancelled outright, a draft cannot jump to `Completed`, the same status is refused.
- Going live needs a signed contract, a responsible contact and a complete client profile (read live
  from `Company`, not from creation); each missing piece is reported separately.
- One contract per project: number, dates, status (`Draft, Signed, Terminated, Expired`). Recording it
  freezes the client as it is now and refuses an incomplete client profile; terminating keeps the
  original signature date.
- Contacts are one model with roles; assigning a role replaces whoever held it. A live project
  cannot lose its responsible contact. Invoice and document e-mail recipients are kept apart.
- The delivering legal entity can change while the project is a draft and is settled once it
  starts; an entity of another organization or one that has stopped trading is refused.
- The compliance checklist comes from a catalogue keyed by (country, engagement type). Belgium and
  Germany have entries; Poland has none, which is the correct answer. A requirement outside the
  catalogue, or one that belongs to a person (A1, Limosa) rather than the project, is refused; a
  numbered requirement needs its number when confirmed; the next expiry shows on the project.
- Documents are attached, described and removed through the file service; one recorded as
  compliance proof cannot be removed.
- A project may optionally name the deal it was sold as; a deal of another company or organization is refused.
- Everything is tenant scoped: another organization's project answers 404, changing it 403.

## Surface
- Backend: `src/HrAgencySystem.Projects` (aggregate `Project`, `ProjectProjection`,
  `ProjectStatusChangePolicy`), `src/HrAgencySystem.Compliance` (`ComplianceCatalogue`,
  `EngagementType`, `ComplianceScope`), `src/HrAgencySystem.Projects.Contracts`.
- API: `/api/projects` (`POST`, `GET`, `GET|PUT /{id}`, `/status`, `/legal-entity`, `/team`,
  `/contacts/{role}`, `/emails/{purpose}`, `/contract`, `/contract/status`,
  `/compliance/{requirement}`, `/compliance/catalogue`, `/documents...`).
- Frontend: `frontend/src/features/{projects,compliance,contracts}`, routes `/app/projects`, `/app/projects/$id`
  (tabs), create/edit wizard in a wide dialog.
- Tests: `tests/HrAgencySystem.UnitTests/{Projects,Compliance}`, `tests/HrAgencySystem.IntegrationTests.Delivery/Projects`.

## Out of scope for this feature
- A won deal does not create a project automatically (not ever - a human decides; plan 009).
- Per-person requirements live on the worker's assignment, see [019](019-workers-and-assignments.md).
- No `Archived`/`OnHold` status (archiving is a list filter); no annexes or several contracts (not yet).
- No reminder mails for expiring documents, no integration with Limosa or the German notification portal (not yet).
- Copying a project onto another legal entity (`CopyProject`, plan 012) was designed but not built.

## Acceptance criteria
- [x] Create, validation, tenant isolation, catalogue attached on create — `./verify projects`
- [x] Lifecycle and go-live preconditions — `./verify projects`
- [x] Contract recording, freezing, termination — `./verify projects`
- [x] Contacts replace per role; responsible contact kept on a live project — `./verify projects`
- [x] Catalogue per country and engagement type; scopes never overlap — `./verify projects`
- [x] Compliance items: outside catalogue, per-person, missing number refused — `./verify projects`
- [x] Documents: attach, read back, foreign read/attach refused, proof cannot be removed — `./verify projects`
- [x] Delivering entity rules; optional deal link — `./verify projects`
- [x] Create a project, take it live and post a worker — e2e: `frontend/e2e/projects/project.spec.ts`
- [x] Compliance checklist of a hired-out project — e2e: `frontend/e2e/projects/delivery-register.spec.ts`
- [x] Screens — evidence: `docs/screenshots/project.png`, `docs/screenshots/project-compliance.png`, `docs/screenshots/project-contract.png`, `docs/screenshots/project-wizard.png`

## Decisions
- Contract on the project stream; compliance as a data catalogue in a shared library:
  [ADR-0013](../../../docs/adr/0013-contract-on-project-stream.md).
- Documents through the file service: [ADR-0012](../../../docs/adr/0012-separate-file-service.md).
- Event-sourced project, one snapshot projection for list and details:
  [ADR-0002](../../../docs/adr/0002-marten-event-store.md).
- `Compliance` is a library both `Projects` and `Workers` reference, since modules may not reference
  each other: [ADR-0006](../../../docs/adr/0006-contracts-and-shared-kernel-ports.md).
- The client profile stays on `Company` (no new "client" entity); `CompanySnapshotRepository` falls
  back to replaying the stream, because the profile is usually completed seconds before the contract.
- Only requirements confirmed by an official source entered the catalogue (plan 009).

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-20 | `33797e83` | Project aggregate, events, compliance catalogue |
| 2026-09-20 | `3af2974b` | Status, team, contacts, emails, contract use cases |
| 2026-09-20 | `c0dbcf1f` | Projects API with read models |
| 2026-09-20 | `87502c50` | Lifecycle, contacts and contract end to end tests |
| 2026-09-20 | `01a65642` | Compliance items against the catalogue |
| 2026-09-20 | `a592e12b` | Project documents through the file service |
| 2026-09-20 | `bb54923d` | List, details, drawers, create wizard |
| 2026-09-20 | `680da48d` | Every project delivered by a legal entity |
| 2026-09-21 | `6a12e26a` | Compliance vocabulary shared with assignments |
| 2026-09-21 | `b8994c23` | Details on tabs, editing moved into the wizard |
| 2026-09-24 | `f495bb91` | Optional link to the deal and a people count |

## Notes from reconstruction
- Plan 009 put the opportunity link out of scope for v1; `f495bb91` added it as an optional,
  validated link from the project side. CLAUDE.md still says nothing links the two.
- Plan 009 called the missing worker entity "the biggest deliberate hole": compliance v1 covered only
  project-level duties. Workers (plan 014) closed it by adding `ComplianceScope`.
- The project's `Placement` used to be called `Assignment`, the same name as a worker's posting; it was renamed together with the workers backend (`c1e5f904`), which also introduced `ComplianceScope`.
