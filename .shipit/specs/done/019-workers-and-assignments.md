# 019 · Workers and assignments

**Status:** done · **Built:** 2026-09-20 → 2026-09-23 · **Verify:** `./verify workers` · **Plan:** backend 014, 018 (local, not in git)

## Why
Before this, compliance confirmed per-person duties (an A1 certificate, a host country filing) as a
single tick on the project, because there was no record of the people we send to a client. Delivery
and legalisation staff need one file per person, a history of where that person worked, and a
register of who holds which posting document for which period. A person moving from one project to
another must stay the same record.

## What it promises
- Delivery staff can register a worker (name, birth date, citizenship, identity document, contact,
  address) and edit them later with the same wizard.
- One person, one file: a second file with the same identity document, the same e-mail address, or
  the same name plus phone is refused, each with its own message. The same document number in
  another organization is a different person.
- A worker moves through a pipeline `Recruitment → ContractPreparation → Legalisation → Onboarding
  → Employed` (plus `ProjectChange`, `Terminated`); each stage has an owning department.
  `Legalisation` applies only to a citizen of a country outside free movement; such a person cannot
  skip it.
- Personal documents and work authorisations (permit, residence title, visa) are attached to the
  worker; files go to the file service and only a `FileId` is kept.
- An assignment puts one worker on one project for one period. It has five states: `Planned →
  Active → Completed | Interrupted`, and `Planned → DidNotStart`.
- Nobody holds two overlapping assignments; the days free up once a posting has finished.
- No command moves an assignment to another project: moving somebody ends one assignment and plans
  another, and the first stays intact.
- Starting an assignment whose worker's paperwork is unfinished is refused.
- An assignment freezes which of our legal entities posted the person and where.
- Per-person requirements (A1, Limosa, local employment contract) are recorded on the assignment, not
  the project; recording an A1 against a project is refused with a message saying where it belongs.
  Local employment never asks for an A1.
- A document recorded as compliance proof cannot be removed.
- A worker or assignment from another organization answers 404, not 403.
- The panel shows two registers (people working here, people working abroad) split by the current
  work country, each as a table or a kanban board where dropping a card opens the status drawer.

## Surface
- Backend: `src/HrAgencySystem.Workers` (aggregates `Worker`, `Assignment`; `WorkerProjection` as a
  multi-stream projection), `src/HrAgencySystem.Workers.Contracts`, shared law in
  `src/HrAgencySystem.Compliance`. Endpoints `GET|POST /api/workers`, `GET|PUT /api/workers/{id}`,
  `PUT /api/workers/{id}/status`, `/api/workers/{id}/documents[/{documentId}[/content]]`,
  `/api/workers/{id}/work-authorisations[/{id}]`, `GET|POST /api/assignments`,
  `GET|PUT /api/assignments/{id}`, `PUT /api/assignments/{id}/status`,
  `GET /api/assignments/{id}/compliance/catalogue`, `PUT /api/assignments/{id}/compliance/{requirement}`,
  `/api/assignments/{id}/documents...`, suggestions `/api/suggestion/workers[/{id}]`.
- Frontend: `frontend/src/features/workers`, `features/assignments`, `features/compliance`; routes
  `/app/workers`, `/app/workers/$id` (tabs in `?tab=`), `/app/workers-abroad`, `/app/assignments`,
  `/app/assignments/$id`; `?view=kanban` on the registers.
- Seeder: legal entities, projects, workers and assignments (`PlatformSeeder`).
- Tests: `tests/HrAgencySystem.UnitTests/Workers`, `tests/HrAgencySystem.IntegrationTests.Delivery/Workers`.

## Out of scope for this feature
- Repointing an assignment to another project - not ever (end one, open another).
- History / timeline of a worker beyond the list of assignments - not yet.
- Notifications about expiring documents - not yet; the list shows the date and a chip.
- Bulk status changes and a register export - not yet, nobody asked.
- Editing document metadata in the UI - the endpoint exists, the drawer does not.
- Searching by identity document number - not ever: the number must not land in a URL or a log.
- Hours of people placed with clients - a future separate module, not `Agency`.

## Acceptance criteria
- [x] A worker can be registered and shows up in the register — `./verify workers`
- [x] An applicant can be registered as a worker through the wizard and documents attached — e2e: `frontend/e2e/workers/register-worker.spec.ts`
- [x] A third-country national is flagged for legalisation and cannot skip it — `./verify workers`
- [x] A second file for the same document / the same person is refused; same document in another organization is allowed — `./verify workers`
- [x] Overlapping assignments are refused; days free up after a finished posting — `./verify workers`
- [x] Moving somebody leaves the first posting intact — `./verify workers`
- [x] Starting with unfinished paperwork is refused — `./verify workers`
- [x] A1 is per assignment, refused on the project; local employment asks for no A1 — `./verify workers`
- [x] Proof documents cannot be removed — `./verify workers`
- [x] Cross-organization access answers 404 — `./verify workers`
- [x] The assignments register lists seeded postings — e2e: `frontend/e2e/projects/delivery-register.spec.ts`
- [x] A worker can be planned onto a project from the project page — e2e: `frontend/e2e/projects/project.spec.ts`
- [x] Kanban drop opens the status drawer; the abroad board shows the other half — e2e: `frontend/e2e/kanban/kanban-boards.spec.ts`
- [x] Register screens exist — evidence: `docs/screenshots/workers.png`, `worker-wizard.png`, `worker-documents.png`, `assignments.png`, `workers-abroad-kanban.png`
- [ ] The manual walk-through (Ukrainian citizen → legalisation → permit → posting to DE → A1 → end → second posting) — unproven (listed as still open in plan 018)

## Decisions
- Worker vs Assignment, one module, multi-stream projection: [ADR-0014](../../../docs/adr/0014-workers-one-module-two-aggregates.md).
- Compliance as a catalogue in a shared library: [ADR-0013](../../../docs/adr/0013-contract-on-project-stream.md).
- Identity document and e-mail uniqueness via reservations: [ADR-0005](../../../docs/adr/0005-uniqueness-reservations.md).
- Documents through the file service, `FileId` only: [ADR-0012](../../../docs/adr/0012-separate-file-service.md).
- Overlap check reads the projection, because a unique index cannot express date ranges.
- Repositories Wolverine injects must be `public`, or generated code falls back to service location and throws.
- The responsible department is a pure function of the status, so the UI does not show it (`156aa089`).

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-20 | `c1e5f904` | Workers module backend: worker, assignment, pipeline, A1 register |
| 2026-09-21 | `d3b17f60` | Single-item suggestions for workers and projects |
| 2026-09-21 | `0ef523a6` | Seeder: legal entities, projects, workers, assignments |
| 2026-09-21 | `6a12e26a` | Shared compliance vocabulary and checklist on the front |
| 2026-09-21 | `b8353bc4` | Read-only register and assignment screens |
| 2026-09-21 | `93effb46` | Register/edit workers, also from an application |
| 2026-09-21 | `e9ae3caf` | Tabbed worker details (experiment) |
| 2026-09-21 | `93a4a297` | Plan/edit/document assignments; tabbed layout becomes the only one |
| 2026-09-21 | `156aa089` | Responsible department dropped from the register UI |
| 2026-09-23 | `ccd48368` | Playwright suite with documentation screenshots |
| 2026-09-23 | `fc4af8d6` | Kanban boards for workers and workers abroad |

## Notes from reconstruction
- Plan 014 proposed two modules, `Workers.Domestic` and `Workers.Foreign`; the code has one module
  where "foreign" is data (citizenship, work country).
- Plan 018 proposed registration by wizard and editing by drawer; the code uses one wizard for both.
- Two detail layouts were built side by side to compare; tabs won and the stacked one was deleted.
- Plan 018 notes that `CompanySnapshotRepository`'s stream fallback only fires when the projection is
  missing, not when it is stale - CLAUDE.md promises more than that code did at the time.
- The front validated `position` to 150 characters while the backend allows 200; fixed in review.
