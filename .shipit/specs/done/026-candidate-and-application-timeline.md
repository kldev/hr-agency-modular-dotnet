# 026 · Candidate and application timeline

**Status:** done · **Built:** 2026-09-23 → 2026-09-23 · **Verify:** `./verify timeline` · **Plan:** backend 026 (local, not in git)

## Why
A recruiter opening a candidate saw only the current state: the status of each application, the
tags, the notes. "What happened with this person, and when" - which application moved where, when
an interview was scheduled or cancelled, when they were registered as a worker - was in the event
store but nowhere on screen.

## What it promises
- A candidate's timeline shows everything that happened to the person and to each of their
  applications and interviews, newest first; each application entry names the job post.
- An application's timeline shows only that application and its interviews - not another application
  of the same candidate, not facts about the person (such as their tags).
- Entries cover: candidate created/updated/tagged/untagged, application created/updated/tagged,
  status changes with `from → to`, note added, interview scheduled/rescheduled/cancelled/completed/
  no-show/other status, interviewer and format changes, registered as a worker.
- A status change shows up even when no `JobApplicationStatusChanged` was raised (scheduling an
  interview moves the application to `Interview`); a status change from the panel is not shown twice.
- That a note was added is shown; what it says never is - note text is not stored in the read model.
  Deleted notes are not shown.
- Entries at the same instant are ordered by event sequence, so the order is stable.
- Paging is by cursor ("load more"): no gaps or duplicates when pages are joined; a cursor the API did
  not issue is a 400. Default page size 20.
- Another organization gets an empty page, never a 403 that would confirm the id exists.
- The candidate page has `Profile | Timeline` tabs and the application page `Details | Timeline`,
  the tab in `?tab=`; mutations that add history refresh the timeline.

## Surface
- Backend: `src/HrAgencySystem.Recruitment/Projections/Timeline` (`TimelineEntry`, `TimelineEntryType`,
  `TimelineProjection : EventProjection`, async), `ITimelineQueryRepository` with a keyset cursor,
  indexes `idx_timeline_candidate`, `idx_timeline_application`. Endpoints
  `GET /api/recruitment/candidates/{candidateId}/timeline?after=&pageSize=`,
  `GET /api/recruitment/job-applications/{jobApplicationId}/timeline?after=&pageSize=`.
- Frontend: `frontend/src/features/timeline` (`RecruitmentTimeline.tsx`, `useTimeline.ts`), tabs in
  `features/candidates/pages/CandidateDetailsPage.tsx` and `features/applications/pages/ApplicationDetailsPage.tsx`.
- Tests: `tests/HrAgencySystem.IntegrationTests.Recruitment/Timeline/TimelineTests.cs`,
  `tests/HrAgencySystem.UnitTests/Applications/Timeline` (cursor).

## Out of scope for this feature
- "Note deleted" entries - not ever (user's decision).
- Note text on the timeline - not ever.
- The previous date of a rescheduled interview - not yet.
- A link to the interview - not yet; the interview details route is a placeholder.
- An applications list tab on the candidate page - not yet.
- Timelines for workers, projects or deals - not yet.

## Acceptance criteria
- [x] The candidate timeline aggregates every application, newest first — `./verify timeline`
- [x] Status transitions include the one scheduling made — `./verify timeline`
- [x] Interviews appear with their application and post — `./verify timeline`
- [x] Notes appear as added, never with their text — `./verify timeline`
- [x] Same instant ordered by event sequence — `./verify timeline`
- [x] Application timeline shows only that application — `./verify timeline`
- [x] Empty for another organization — `./verify timeline`
- [x] Cursor paging without gaps or duplicates; a foreign cursor refused — `./verify timeline`
- [ ] Reactivation `Withdrawn → Screening` shown — unproven (the API has no reactivate endpoint; the projection handles the event)
- [ ] Tag and data-edit entries — unproven (no integration test; would need seeded tags)
- [ ] The Timeline tabs in the panel — unproven (no e2e or screenshot found)

## Decisions
- A dedicated async `EventProjection`, one document per relevant event, rather than reading
  `mt_events` at query time (no index across streams by time, and notes would be loaded):
  [ADR-0002](../../../docs/adr/0002-marten-event-store.md).
- Context (organization, candidate, post) comes from the first event of the same stream, not from
  other projections that may lag; `from` comes from the aggregate rebuilt to the version before the event.
- Keyset paging on `(OccurredAt, Sequence)`, the first non-offset paging in the codebase; the slice type
  stays in `Recruitment` to keep `SharedKernel` small: [ADR-0006](../../../docs/adr/0006-contracts-and-shared-kernel-ports.md).
- The organization is the leading column of both indexes and comes from the token:
  [ADR-0004](../../../docs/adr/0004-tenant-on-every-aggregate.md).
- History is complete backwards because the new async projection replays from position 0.

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-23 | `2f9416cc` | "Add note" now appends its event to the application stream |
| 2026-09-23 | `823cbb0a` | Candidate and job application timeline (backend) |
| 2026-09-23 | `32ff5710` | Timeline tabs on candidate and application details |
| 2026-09-24 | `381fe564` | Merged with PR #8 |

## Notes from reconstruction
- Found while building: the "Add note" handler returned `JobApplicationNoteAdded` but never appended
  it - with `InvokeAsync<T>` a returned event is only the response. Notes added that way before the
  fix have no event and are missing from the timeline until a reseed.
- Generated index names exceeded PostgreSQL's 63-character limit, hence the hand-written names.
- `OccurredAt` is stored at microsecond precision so a cursor built from a read value compares
  exactly with the boundary row.
- Integration test fakes used to invent a random candidate for every application; they now return the
  real one when the test created it.
- Registering as a worker produces two entries on the candidate timeline (candidate and application
  events of the same click); accepted in the plan.
