# 009 · Interviews

**Status:** done · **Built:** 2026-09-07 → 2026-09-14 · **Verify:** `./verify interviews` · **Plan:** none

## Why
Between screening and an offer, recruiters, hiring managers and client staff meet candidates. The
team needs to book an interview against an application, say who runs it, where and how it happens,
move it when plans change, record how it went, and see everybody's interviews on a list and a
calendar in their own time zone.

## What it promises
- A member can schedule an interview for an application of their organization: local date and time
  plus an IANA time zone (default `Europe/Warsaw`), interviewer, format (`Online`, `OnSite`, `Phone`),
  type (`Hr`, `Technical`, `Client`, `Final`), optional location, meeting url and note.
- The time is stored as an instant read in the given time zone, so 10:00 in Warsaw stays 10:00 there
  across daylight saving.
- The interview starts as `Planned`, carries the candidate, the post title and the company, and
  moves the application to `Interview` (no separate status change is needed).
- An interview can be rescheduled (new time, zone, location, meeting url; status becomes
  `Rescheduled`), get a new interviewer, a new format, or a new status: `Planned`, `Confirmed`,
  `InProgress`, `Completed`, `Canceled`, `NoShow`, `Rescheduled`.
- A note given with scheduling, rescheduling, a status or an interviewer change is also written as
  a note on the application.
- Every change refuses an interview of another organization.
- Interviews can be listed and filtered by candidate, application, interviewer, creator, status and
  date range, newest first, with paging; a single interview of another organization is 404 with
  ProblemDetails.
- The calendar reads a date range (`fromDate`..`toDate` inclusive, interpreted in the caller's time
  zone) without paging.
- The panel has an interviews table, a details page, a schedule drawer from the application, actions
  for reschedule/interviewer/format/status, and a calendar view.

## Surface
- Backend: `src/HrAgencySystem.Recruitment/Domain/Interviews` (`Interview`, `InterviewFormat`,
  `InterviewStatus`, `InterviewType`), `Application/Interviews/*`, `Events/Interviews`,
  `IInterviewsQueryRepository` and its Marten implementation.
- API: `POST /api/interviews/schedule`, `GET /api/interviews`, `GET /api/interviews/range`,
  `GET /api/interviews/{id}`, `PUT /api/interviews/{id}/reschedule|status|format|interviewer`.
- Frontend: `frontend/src/features/interviews`, `frontend/src/features/calendar`; routes
  `/app/interviews`, `/app/interviews/$id`, `/app/calendar`.
- Tests: `tests/HrAgencySystem.IntegrationTests.Recruitment/Interviews`,
  `tests/HrAgencySystem.IntegrationTests.Persistence/Interviews`.

## Out of scope for this feature
- E-mails or calendar invitations to the candidate or interviewer - no interview mail exists (not yet).
- A status transition policy for interviews: any status can follow any other (not decided).
- Checking the interviewer's availability or overlapping interviews (not yet).
- Scorecards or structured interview feedback beyond a free note (not yet).

## Acceptance criteria
- [x] Get one interview for the right organization; another organization → 404 with ProblemDetails; unknown id → 404 — `./verify interviews`
- [x] List filters (candidate, application, interviewer, creator, status, from/to and ranges, inclusive bounds), combined filters and paging — `./verify interviews`
- [x] Query repository: tenant isolation, each filter, ordering by schedule descending — `./verify interviews`
- [x] Scheduling over HTTP creates the interview and moves the application to `Interview` — `./verify timeline`
- [ ] Reschedule, change status, format and interviewer — unproven (no test found)
- [ ] Cross-tenant refusal on changes (`ValidateAggregateUpdate`) — unproven for interviews (no test found)
- [ ] Calendar range endpoint and its time zone handling — unproven (no test found)
- [ ] Interviews table and calendar screens — unproven (no e2e or screenshot found)

## Decisions
- Interview is its own event stream; scheduling also appends to the application stream: [ADR-0002](../../../docs/adr/0002-marten-event-store.md).
- Static handlers, `[AggregateHandler]` for every change after scheduling: [ADR-0003](../../../docs/adr/0003-wolverine-static-handlers.md).
- Every query filtered by `OrganizationId`; a foreign interview is "not found": [ADR-0004](../../../docs/adr/0004-tenant-on-every-aggregate.md).
- Interviewer and creator read through `IUserSnapshotRepository`: [ADR-0006](../../../docs/adr/0006-contracts-and-shared-kernel-ports.md).
- Endpoint per file, ProblemDetails mapping: [ADR-0008](../../../docs/adr/0008-endpoint-per-file-thin-api.md).
- Local time + IANA zone in the request, an instant in the store, so the calendar is correct across
  daylight saving (`ScheduleInterviewRequest` description).
- Interviews live in `Recruitment`, not a module of their own, because they belong to an application.

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-07 | `71d65a56` | Interview domain and events |
| 2026-09-07 | `6530112b` | Query repository with tests |
| 2026-09-07 | `5f10a976` | Slice over HTTP tested |
| 2026-09-07 | `c2b5a5ed` | Get endpoint tests |
| 2026-09-07 | `77270745` | Interview seed scenario |
| 2026-09-11 | `34de153e` | Panel: interviews table |
| 2026-09-12 | `aa8b4bd8` | Panel: schedule interview |
| 2026-09-13 | `bb7521e7` | Change interviewer endpoint |
| 2026-09-13 | `ac2c58d1` | Fix: stream event applied properly |
| 2026-09-13 | `c6982816` | Reschedule endpoint |
| 2026-09-13 | `b83f7e26` | Panel: interviews calendar |
| 2026-09-13 | `5d6961da` | Panel: location and meeting url |
| 2026-09-14 | `4d8028b0` | Calendar loads through a server function |

## Notes from reconstruction
- Scheduling does not look at the application's status. It appends `JobApplicationInterviewScheduled`,
  whose `Apply` throws for a status that cannot reach `Interview` (for example `Applied` or `Rejected`),
  so such an application stream might no longer load - read from the code, not reproduced.
- `ChangeInterviewStatusHandler` accepts the status the interview already has and writes an event anyway.
- The write side of interviews has no unit tests; only reads are tested directly.
