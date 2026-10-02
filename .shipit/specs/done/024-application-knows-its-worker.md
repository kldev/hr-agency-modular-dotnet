# 024 · Application knows its worker

**Status:** done · **Built:** 2026-09-22 → 2026-09-22 · **Verify:** `./verify candidates` and `./verify workers` · **Plan:** backend 024 (local, not in git)

## Why
"Register as worker" on an application opened a worker file with the applicant's details, but the
link was kept only on the worker's side, and it pointed at the candidate, not the application. A
recruiter looking at the applications or candidates list could not see who had already been taken
onto the workers' register, nor filter them out - so nobody knew whether somebody else had already
dealt with a person.

## What it promises
- Registering a worker can name the source candidate and the source application
  (`sourceCandidateId`, `sourceApplicationId`); the worker wizard opened from an application passes both.
- An application without its candidate is refused by validation.
- After registration the candidate and that one application carry the `WorkerId`; the candidate's
  other applications do not.
- The applications list and the candidates list show which rows have a worker file, link to it, and
  filter on it (`registeredAsWorker=true|false`, absent = all).
- The receiving side skips the message, with a warning log and without an exception, when the
  candidate or application is unknown or belongs to another organization, when the application
  belongs to a different candidate (the candidate is still marked), when the same worker is already
  recorded (at-least-once delivery), or when the candidate already has a different worker (the first
  stays).
- Registering as a worker changes neither the application status nor the candidate status.

## Surface
- Backend: `src/HrAgencySystem.Workers.Contracts` (`WorkerRegisteredFromRecruitment`),
  `src/HrAgencySystem.Workers/Application/RegisterWorker` (returns the worker event plus the
  integration event), `src/HrAgencySystem.Recruitment/Integration/WorkerRegisteredFromRecruitmentHandler.cs`,
  events `CandidateRegisteredAsWorker`, `JobApplicationRegisteredAsWorker`, `WorkerId` on
  `CandidateProjection` and `JobApplicationProjection`, filter on `GET /api/recruitment/candidates`
  and `GET /api/recruitment/job-applications`.
- Frontend: `frontend/src/features/applications`, `features/candidates` (column, card, toolbar
  filter), `features/workers/components/WorkerFileLink.tsx`, `WorkerWizard`.
- Tests: `tests/HrAgencySystem.UnitTests/Candidates/WorkerRegisteredFromRecruitmentHandlerTests.cs`,
  `tests/HrAgencySystem.UnitTests/Workers/RegisterWorkerHandlerTests.cs`,
  `tests/HrAgencySystem.IntegrationTests.Delivery/Workers/WorkerFromRecruitmentTests.cs`.

## Out of scope for this feature
- Moving the application to `Hired` on registration - not yet; a file may be opened before an
  offer (legalisation takes time). Linking the two facts is a separate decision.
- Creating a worker automatically from a candidate - not yet; a person starts it from the menu.
- A link from the worker back to the application in the worker UI beyond the stored id - not yet.

## Acceptance criteria
- [x] Registration tells recruitment only when a source candidate is given; application without candidate refused — `./verify workers`
- [x] The receiving handler marks candidate and application, and skips foreign, unknown, mismatched, repeated and second-worker messages — `./verify candidates`
- [x] Registering from an application marks both lists and the filter finds/omits them — `./verify workers` (`Registering_a_worker_from_an_application_marks_both_lists`)
- [x] "Register as worker" from an application row carries the applicant's name and contact into the wizard — e2e: `frontend/e2e/workers/register-worker.spec.ts`
- [ ] The "has a worker file" marker and filter on the two lists in the panel — unproven (no e2e or screenshot found)

## Decisions
- `Workers` tells `Recruitment` through an integration event in `Workers.Contracts`; the modules do
  not reference each other: [ADR-0006](../../../docs/adr/0006-contracts-and-shared-kernel-ports.md).
- The integration event is the second element of the handler's returned tuple, so it cascades; the
  first element is only the `InvokeAsync<T>` response: [ADR-0003](../../../docs/adr/0003-wolverine-static-handlers.md).
- Both records get the id because they answer different questions: the candidate "is this person on
  the register", the application "which conversation it came from".
- A message that cannot apply is skipped, not thrown: a retry would not make it better.
- `WorkerId` is an `init` property on both projections, so old documents read as "no worker" with no
  migration: [ADR-0025](../../../docs/adr/0025-learning-project-no-migrations.md).

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-21 | `93effb46` | "Register as worker" from an application row (worker side only) |
| 2026-09-21 | `c94bdafd` | Applicant's name and contact carried into the worker wizard |
| 2026-09-21 | `20b895f0` | Source step skipped when opened from an application |
| 2026-09-22 | `e55873ec` | Candidate and application know the worker opened from them |
| 2026-09-22 | `3a8e28ab` | Front: show and filter which candidates and applications have a worker file |
| 2026-09-22 | `0e66353d` | Minimal module variants for the job board dropped |

## Notes from reconstruction
- The plan worried that the public job board, which then wrote to the same candidate streams through
  a "minimal" event configuration, would meet an unknown event; both new events were added to it.
  Later the same day `0e66353d` moved the board onto internal API routes and removed the minimal
  variants, so the concern disappeared.
- The integration test writes the application straight into the event store; creating one over HTTP
  would need a company, a description and a published post.
- `./verify candidates` does not run the end-to-end integration test, which lives in the Workers
  namespace; `./verify workers` does.
