# 006 · Candidates and applications

**Status:** done · **Built:** 2026-09-03 → 2026-09-23 · **Verify:** `./verify candidates` · **Plan:** none

## Why
A recruiter works a pipeline: people apply to posts, get screened, interviewed, tested, offered,
hired or turned down. The agency must keep one record per person across all their applications, see
where every application stands, write notes along the way and tag people by language and skill so
they can be found again for the next role.

## What it promises
- Applying to a post (`POST /api/recruitment/job-posting/{id}/apply`) is accepted only while the
  post is `Published`. E-mail and phone are validated; all errors come back together.
- An applicant is matched to an existing candidate of the same organization by e-mail; otherwise a
  new candidate is created. The same e-mail can exist once per organization and again in another one.
- Every application records the source (career page, job boards, referral, direct sourcing, ...).
- A new application sends the post's recruiter an e-mail.
- A member can also create and edit a candidate directly; a candidate of another organization is
  neither returned nor updated.
- An application moves `Applied → Screening → Interview | Assessment → Offer`; `Hired`, `Rejected`
  and `Withdrawn` can be set from any open status. Going back to `Applied`, skipping steps, or
  rejecting/withdrawing a closed application is refused. Moving to `Interview` needs the interview id.
- A status change can carry a note, which is stored as an application note.
- Notes can be added to and deleted from an application; the list of notes is readable.
- Candidates and applications can be tagged from a fixed catalogue (languages, IT skills, production
  skills, driving licences), one tag or a list at a time, and untagged the same way.
- The panel lists candidates and applications (search, filters, paging), shows details pages, has
  create/edit drawers, an action menu for notes and status, a tag picker, and a kanban board of
  applications where dropping a card on a reachable stage opens the status drawer preselected.

## Surface
- Backend: `src/HrAgencySystem.Recruitment` - `Domain/Candidates`, `Domain/Applications`
  (`JobApplicationStatusChangePolicy`), `Application/Candidates`, `Application/JobApplications`,
  `Infrastructure/CandidateResolver.cs`, documents `JobApplicationNote`, `Tag`,
  `CandidateEmailReservation`; `TagSeeder`.
- API: `POST .../job-posting/{id}/apply`; `POST|GET /api/recruitment/candidates`,
  `GET|PUT .../candidates/{id}`, `.../candidates/{id}/tag|tag-list|tag/remove|tag/{tagId}`;
  `GET /api/recruitment/job-applications`, `GET|PUT .../{id}`, `PUT .../{id}/status`,
  `GET .../{id}/notes`, `POST .../{id}/note`, `DELETE .../{id}/note/{noteId}`, tag routes as for
  candidates; `GET /api/suggestion/tags`.
- Frontend: `frontend/src/features/candidates`, `frontend/src/features/applications`; routes
  `/app/candidates`, `/app/candidates/$id`, `/app/applications` (`?view=kanban`), `/app/applications/$id`.
- Tests: `tests/HrAgencySystem.UnitTests/{Candidates,Applications}`,
  `tests/HrAgencySystem.UnitTests/JobPostings/Handlers/ApplyToJobApplicationHandlerTests.cs`,
  `tests/HrAgencySystem.IntegrationTests.Recruitment/Candidates`.

## Out of scope for this feature
- Reactivating a withdrawn application: a handler exists but no endpoint (not yet; `TODO` in the enum).
- Blocking or archiving a candidate: `CandidateStatus.Blocked/Archived` exist, no command sets them (not yet).
- Agency-defined tags: the catalogue is seeded and shared by all organizations (not decided).
- Interviews - spec 009. History timelines - spec 026. Linking an application to a worker - spec 024.
- CV upload and parsing (not yet).

## Acceptance criteria
- [x] Apply: refused for a post that is not published, invalid e-mail/phone, unknown company — `./verify job-posts`
- [x] Apply starts the application stream for a resolved candidate — `./verify job-posts`
- [x] Applying through the public board creates the application — `./verify job-board`
- [x] Candidate create/update; duplicate e-mail refused, allowed in another organization; other organization invisible — `./verify candidates`
- [x] Candidate slice: empty, paging, page size, has-more — `./verify candidates`
- [x] Status transitions allowed and refused by the aggregate; interview id required — `./verify candidates`
- [x] Status change with note writes a note, without note writes none; organization mismatch refused — `./verify candidates`
- [x] Apply, status change and notes over HTTP (seen through the timeline) — `./verify timeline`
- [x] Tag catalogue search by text and category — `./verify suggestions`
- [x] Lists render seeded data — e2e: `frontend/e2e/overview/main-views.spec.ts`
- [x] Kanban: unreachable stage opens nothing, `Rejected` opens the drawer preselected — e2e: `frontend/e2e/kanban/kanban-boards.spec.ts`
- [x] Screens — evidence: `docs/screenshots/candidates.png`, `docs/screenshots/applications.png`, `docs/screenshots/applications-kanban.png`, `docs/screenshots/applications-kanban-drop.png`
- [ ] New application e-mail to the recruiter — unproven at handler level (template render only, `./verify emails`)
- [ ] Deleting a note — unproven (no test found)
- [ ] Tagging and untagging candidates and applications — unproven (no test found)

## Decisions
- Candidate and application are separate event streams: [ADR-0002](../../../docs/adr/0002-marten-event-store.md).
- Candidate e-mail per organization via reservation document: [ADR-0005](../../../docs/adr/0005-uniqueness-reservations.md).
- New-application mail as a message over RabbitMQ: [ADR-0010](../../../docs/adr/0010-email-over-rabbitmq.md).
- `ShortNote`, `Email`, `PhoneNumber` with `TryCreate`: [ADR-0007](../../../docs/adr/0007-value-objects-trycreate.md).
- Transitions are a policy (`JobApplicationStatusChangePolicy`) checked again in the aggregate's
  `Apply`, so a replayed or forged event cannot put an application in an impossible state.
- The kanban reuses the domain-free board from the sales pipeline (`fc4af8d6`).

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-03 | `8bf6b5a0` | Job application domain, Recruitment module |
| 2026-09-04 | `943e926f` | Create candidate and candidate application handlers |
| 2026-09-04 | `3ecb9310` | Candidate applies to a job post |
| 2026-09-04 | `458072f2` | Candidate tagging |
| 2026-09-05 | `ddc29450` | Job application projection |
| 2026-09-05 | `1d420ccc` | Application tags |
| 2026-09-07 | `98709bc5` | Change application status |
| 2026-09-07 | `1164a448` | Add/delete application note |
| 2026-09-08 | `9d124ec5` | Candidate update endpoint and tests |
| 2026-09-11 | `e1f910d2` | Panel: candidates table |
| 2026-09-11 | `f7641848` | Panel: applications table |
| 2026-09-12 | `a1d56ea2` | Panel: notes and status from the action menu |
| 2026-09-15 | `15612443` | Panel: tagging |
| 2026-09-19 | `0666512f` | New-application e-mail over RabbitMQ |
| 2026-09-23 | `fc4af8d6` | Panel: applications kanban with drag and drop |

## Notes from reconstruction
- `ReactivateJobApplicationHandler` is reachable from no endpoint and does not check the organization.
- `(_, Hired) => true` comes before the final-status check, so a `Rejected` or `Withdrawn`
  application can still be moved to `Hired`.
- The candidate e-mail reservation index is `(Email, OrganizationId)`, not led by `OrganizationId`
  as CLAUDE.md asks of indexes.
- `Tag` documents carry no `OrganizationId`; the catalogue is platform-wide.
