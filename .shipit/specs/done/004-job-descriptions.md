# 004 · Job descriptions

**Status:** done · **Built:** 2026-09-01 → 2026-09-17 · **Verify:** `./verify job-descriptions` · **Plan:** frontend 002, 003 (local, not in git); backend none

## Why
Before anything is posted, a recruiter has to agree with the client what the role actually is:
title, duties, requirements, location, contract type and pay. That internal description is the one
source the candidate-facing posts are written from, and it needs an owner (the recruiter) and a
status so the team knows which positions are still being filled.

## What it promises
- A member can create a job description for a client company of their organization: title, summary,
  description, lists of responsibilities, requirements and skills, location, country, employment type,
  work mode, salary range (currency, min, max) and the responsible recruiter. It starts as `Draft`.
- Invalid input is reported in one 400 with all errors: missing or too long title/description,
  invalid list entries, negative salary, minimum above maximum, invalid country code.
- An unknown organization or creator is refused.
- The company and the recruiter are frozen into the read model as snapshots (name, e-mail), so the
  list shows them without asking other modules.
- The content can be edited; the recruiter is changed through a separate action. Both refuse a
  description of another organization.
- Status can be set to `Open`, `OnHold`, `Closed` or `Cancelled`. Setting the current status again
  writes nothing; going back to `Draft` is refused.
- Every status change is kept in a status history, readable per organization.
- One job description = one position; a second position is a second job description.
- The panel lists job descriptions, shows a details page, creates them through a step-by-step wizard
  (`/app/job-descriptions/add`) and edits them through a wizard that reuses the same steps
  (`/app/job-descriptions/edit/$id`); company and recruiter are read-only there.

## Surface
- Backend: `src/HrAgencySystem.JobDescription` (aggregate `JobDescription`, `JobDescriptionProjection`,
  `StatusChangeHistoryProjection`, `JobDescriptionSnapshotRepository` implementing the
  `IJobDescriptionSnapshotRepository` port used by Recruitment).
- API: `POST|GET /api/job-description`, `GET|PUT /api/job-description/{id}`,
  `PUT /api/job-description/{id}/{status}`, `PUT /api/job-description/{id}/assign-recruiter`,
  `GET /api/job-description/status`.
- Frontend: `frontend/src/features/job-descriptions` (`wizards/create`, `wizards/edit`, change-recruiter
  drawer); routes `/app/job-descriptions`, `/$id`, `/add`, `/edit/$id`.
- Tests: `tests/HrAgencySystem.UnitTests/JobDescriptions`,
  `tests/HrAgencySystem.IntegrationTests.Recruitment/JobDescriptions`.

## Out of scope for this feature
- A status transition policy: any status except `Draft` can follow any other, so a `Closed` or
  `Cancelled` description can be reopened (not decided).
- "Every job description has at least one job post" is not enforced (not yet).
- A "position" entity above the job description (not ever, per CLAUDE.md).
- A link from a won sales opportunity to the job description (not yet).

## Acceptance criteria
- [x] Create with valid data starts as `Draft`; every invalid field reported — `./verify job-descriptions`
- [x] Unknown organization or user refused — `./verify job-descriptions`
- [x] Update content with the same validation — `./verify job-descriptions`
- [x] Assign recruiter; unknown recruiter refused — `./verify job-descriptions`
- [x] Open, put on hold, close, cancel; same status writes nothing; `Draft` refused — `./verify job-descriptions`
- [x] Status history and list over HTTP — `./verify job-descriptions`
- [x] Create through the wizard and land on the details page — e2e: `frontend/e2e/job-descriptions/create-job-description.spec.ts`
- [x] Screens — evidence: `docs/screenshots/job-description-wizard.png`, `docs/screenshots/job-description-review.png`, `docs/screenshots/job-description-created.png`
- [ ] Status change on another organization's description is refused — unproven, and in fact not checked (see notes)
- [ ] Edit wizard saves and returns to the details page — unproven (no test found; plan 002 says it was never tried by hand)

## Decisions
- Event-sourced aggregate, projections built from events: [ADR-0002](../../../docs/adr/0002-marten-event-store.md).
- `[AggregateHandler]` for update, status and recruiter: [ADR-0003](../../../docs/adr/0003-wolverine-static-handlers.md).
- Company and recruiter read through `ICompanySnapshotRepository`/`IUserSnapshotRepository`;
  Recruitment reads the description through `IJobDescriptionSnapshotRepository`: [ADR-0006](../../../docs/adr/0006-contracts-and-shared-kernel-ports.md).
- `JobTitle`, `LongText`, `EntryText`, `SalaryRange` with `TryCreate`: [ADR-0007](../../../docs/adr/0007-value-objects-trycreate.md).
- Wizard steps, schema and helpers built on the shared `FormWizard`: [ADR-0022](../../../docs/adr/0022-frontend-tanstack-start-orval.md).
- Recruiter change is a separate drawer, not a wizard step, so an edit sends exactly one `PUT`
  (frontend plan 002 note 1).
- The create wizard did nothing on submit because `location` was validated on no step; frontend
  plan 003 diagnosed it and `dfdce630`/`afd728ac` fixed validation and the review step.

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-01 | `25eabd47` | Domain and value objects |
| 2026-09-02 | `e545b56d` | Create handler |
| 2026-09-02 | `02d85709` | Recruiter snapshot in projections |
| 2026-09-02 | `e5e286d3` | Change status handler with tests |
| 2026-09-02 | `8f114544` | Status history projection |
| 2026-09-02 | `2cecf49b` | API endpoints |
| 2026-09-02 | `3b2702cc` | Company snapshot |
| 2026-09-03 | `3ee2c96f` | Get job description slice |
| 2026-09-04 | `6e3fc2b8` | Projections rewritten in `EventProjection` style |
| 2026-09-13 | `44925c90` | Panel: details page |
| 2026-09-16 | `6b2c949f` | Panel: create wizard |
| 2026-09-17 | `afd728ac` | Review step improved |
| 2026-09-17 | `988cc386` | Panel: edit wizard and recruiter drawer |

## Notes from reconstruction
- `ChangeJobDescriptionStatusHandler` receives `OrganizationId` but never compares it with the
  aggregate's; update and assign-recruiter do. A member who knows another tenant's description id
  can change its status.
- The status endpoint takes the status as a route segment (`/{id}/{status}`), unlike every other
  status change in the API, which uses a body on `/status`.
- No policy object for transitions, unlike job posts and applications.
