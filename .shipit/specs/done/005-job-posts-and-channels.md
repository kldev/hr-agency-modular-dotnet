# 005 · Job posts and channels

**Status:** done · **Built:** 2026-09-03 → 2026-09-20 · **Verify:** `./verify job-posts` · **Plan:** frontend 004 (local, not in git); backend none

## Why
The internal job description is not what a candidate should read. A recruiter writes a polished,
candidate-facing post from it - often more than one, one per language - and publishes it on job
boards. The agency needs to know which posts are live, where they were published and which
recruiter handles the applications.

## What it promises
- A member can create a job post from a job description of their organization. The post copies
  nothing on the server: title, summary, description, lists, location, country, employment type,
  work mode, salary range, language code and recruiter are sent explicitly and may differ from the
  description. The company is taken from the description.
- Several posts may describe the same description (for example one per `LanguageCode`).
- Each post gets a readable slug `company-title-location-xxxx` (diacritics removed, 4 characters of
  the id) and a public url `{organization slug}/{post slug}`.
- A post starts as `Draft`. Allowed changes: `Draft → Published`, `Published → Closed | Archived`,
  `Archived → Published`. `Closed` is final; the same status again is refused.
- "Post to channel" records that the post went out on `CareerPage`, `PracujPl`, `Olx`, `PracaPl`,
  `Rocketjobs`, `JustJoinIt`, `NoFluffJobs`, `Linkedin`, `Indeed` or `Other`. It does not publish
  anything; a post not yet published becomes `Published`, a closed one is refused.
- Content can be edited (all validation errors at once); a post of another organization is refused.
- The recruiter can be changed. The new recruiter gets an e-mail only when they are somebody other
  than the person making the change.
- Publishing and unpublishing are announced to `Company` (published post count) and to the feed read
  model.
- The panel lists posts, shows a details page with channels, creates a post from a description or
  copies an existing one "to a new language", and edits it in a wizard.

## Surface
- Backend: `src/HrAgencySystem.Recruitment/Domain/JobPostings` (`JobPost`, `JobPostStatusChangePolicy`,
  `ChannelPost`), `Application/JobPosting/*`, `Projections/JobPostProjection.cs`;
  `HrAgencySystem.Recruitment.Contracts` (`JobPostCreatedIntegrationEvent`,
  `JobPostActiveChangedIntegrationEvent`).
- API: `POST|GET /api/recruitment/job-posting`, `GET|PUT /api/recruitment/job-posting/{id}`,
  `PUT .../{id}/status`, `PUT .../{id}/change-recruiter`, `PUT .../{id}/channel`;
  picker `/api/suggestion/job-posts`.
- Frontend: `frontend/src/features/job-posts` (`wizards/create`, `wizards/edit`, post-to-channel
  drawer); routes `/app/jobs`, `/app/jobs/$id`, `/app/jobs/add?jobDescriptionId=|fromJobPostId=`,
  `/app/jobs/edit/$id`.
- Tests: `tests/HrAgencySystem.UnitTests/JobPostings`, `tests/HrAgencySystem.IntegrationTests.Recruitment/JobPosts`.

## Out of scope for this feature
- Real integration with job boards: `PostToChannel` only records a fact (not yet).
- One post per language per description is not enforced (frontend plan 004, "out of scope").
- Applying to a post - spec 006. Feeds built from published posts - spec 007.
- Translation or AI-written posts (not ever, per frontend plan 004).

## Acceptance criteria
- [x] Create with valid data; unknown organization, recruiter, creator, description or company refused — `./verify job-posts`
- [x] Invalid content → 400 with all errors (create and update) — `./verify job-posts`
- [x] Posting to a channel is refused in a final status and does not change status by itself in the domain — `./verify job-posts`
- [x] Recruiter change; empty recruiter → 400 — `./verify job-posts`
- [x] Handover rule: mail only when the new recruiter is somebody else — `./verify job-posts`
- [x] Published posts counted on the company — `./verify companies`
- [x] Published/unpublished state reaches the feed read model — `./verify feeds`
- [x] Job postings list renders seeded data — e2e: `frontend/e2e/overview/main-views.spec.ts`
- [x] Screen — evidence: `docs/screenshots/job-postings.png`
- [ ] Status change endpoint and `JobPostStatusChangePolicy` transitions — unproven (no test found)
- [ ] Post to channel over HTTP publishes a draft — unproven (no test found)
- [ ] Copy to a new language in the panel — unproven (no test found)

## Decisions
- Event-sourced post stream, `[AggregateHandler]` handlers: [ADR-0002](../../../docs/adr/0002-marten-event-store.md), [ADR-0003](../../../docs/adr/0003-wolverine-static-handlers.md).
- Recruitment reads the description through `IJobDescriptionSnapshotRepository` and tells `Company`
  through integration events: [ADR-0006](../../../docs/adr/0006-contracts-and-shared-kernel-ports.md).
- `PostTitle`, `LanguageCode`, `SalaryRange` with `TryCreate`: [ADR-0007](../../../docs/adr/0007-value-objects-trycreate.md).
- Handover e-mail returned as `OutgoingMessages`, sent by the worker: [ADR-0010](../../../docs/adr/0010-email-over-rabbitmq.md).
- A post's wording deliberately differs from the description, so the post stores its own copy
  instead of pointing at the description's text.
- The mail takes the post title from the aggregate, not the async projection (`a37f9c4d`), because
  the projection may not have caught up.

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-03 | `bbf3a348` | Job posting events |
| 2026-09-03 | `ad4b2817` | Job post domain |
| 2026-09-03 | `1eaebf0b` | Projection and update events |
| 2026-09-03 | `9a9edd9c` | Create endpoint |
| 2026-09-04 | `b0c45b17` | Update handler with tests |
| 2026-09-04 | `99fadbd2` | Change recruiter |
| 2026-09-04 | `63beac4c` | Job post url |
| 2026-09-05 | `7a5cec14` | Post to channel endpoint |
| 2026-09-07 | `2c114b2e` | Integration events to `Company` |
| 2026-09-09 | `60cfdd54` | Post to channel refactored, publishes a draft |
| 2026-09-13 | `96030901` | Panel: job posts table |
| 2026-09-13 | `432e7ec6` | Panel: post to channel |
| 2026-09-17 | `a686677b` | Panel: create/edit wizard, copy to new language |
| 2026-09-19 | `73678dec` | Handover e-mail on recruiter change |
| 2026-09-20 | `a37f9c4d` | Mail title from the aggregate |

## Notes from reconstruction
- `PostToChannelHandler` checks that the organization exists but never compares it with the post's
  organization; update, status and recruiter changes do.
- `Archived` can go back to `Published` while `Closed` is final - the opposite of the enum comment,
  which calls `Archived` "permanently archived".
- The API enum spells `Rocketjobs` and `Linkedin`, while `CandidateSource` spells `RocketJobs` and `LinkedIn`.
