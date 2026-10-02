# Open - what is known to be missing or rough today

State checked against the code on 2026-10-02. Each line says what is missing and where it shows. Ordered by the cost
of leaving it as it is. When one of these gets picked up, it becomes a numbered spec in `specs/` first.

## Authorization gaps (most expensive first)

| What | Where it shows | Spec |
|---|---|---|
| The org chart write endpoints have no role policy | any member can create/move units, add members and **assign themselves as head** - and the head approves the hours of the people below ([ADR-0015](../docs/adr/0015-agency-separate-from-workers.md)); `src/HrAgencySystem.Api/Endpoints/OrgStructure/` | [021](specs/done/021-agency-chart-and-roles.md) |
| Changing a job description's status does not check the tenant | `ChangeJobDescriptionStatusHandler` never compares the caller's organization with the aggregate's: anyone who knows the id of another agency's description can close or cancel it | [004](specs/done/004-job-descriptions.md) |
| Posting a job post to a channel does not check the tenant | `PostToChannelHandler` checks that the caller's organization exists, not that the post belongs to it - and posting can republish the post | [005](specs/done/005-job-posts-and-channels.md) |
| Starting and changing an agency employment has no role policy | only the rate is hidden/kept by `RatesPolicy`; contract type, period and hours can be changed by any member | [022](specs/done/022-time-sheets-and-settlement.md) |
| Non-admins being refused on user create/edit/re-role is untested | every integration client runs as Admin | [021](specs/done/021-agency-chart-and-roles.md) |

## Gaps in the product

| What | Where it shows | Spec |
|---|---|---|
| Opportunity stages have no transition policy | `Won` can go back to `New`; the kanban has no drag and drop | [010](specs/done/010-sales-opportunities.md) |
| A team can never be dissolved | no team status, and the last member cannot be removed | [014](specs/done/014-teams.md) |
| "Waiting for the projection" is a fixed 1.5 s sleep | `frontend/src/hooks/useProjectionWait.ts` does not poll the read model; a slow daemon still shows stale data | [011](specs/done/011-agency-panel-foundation.md) |
| A job description does not have to have a job post | a position can be "open" with nothing published | [004](specs/done/004-job-descriptions.md) |
| Two job posts in the same language under one description are not refused | the natural fix is a reservation on `(jobDescriptionId, languageCode)`, the pattern already exists ([ADR-0005](../docs/adr/0005-uniqueness-reservations.md)) | [005](specs/done/005-job-posts-and-channels.md) |
| A `Closed`/`Archived` job post can still be edited | `UpdateJobPostHandler` has no status guard | [005](specs/done/005-job-posts-and-channels.md) |
| An application cannot come back from a final stage | `Reactivate` exists, the transition policy has no way back (a `TODO` in `JobApplicationStatus`) | [006](specs/done/006-candidates-and-applications.md) |
| `GET /api/recruitment/job-posting` cannot filter by job description, and there is no job description suggestion | "the posts of this description" cannot be listed, a new post cannot be started from the posts list | [005](specs/done/005-job-posts-and-channels.md) |
| No job board integration | `PostToChannel` records a publication, it does not publish | [005](specs/done/005-job-posts-and-channels.md) |
| Leave requests | planned (backend plan 016), never built; the agency module has hours but no absences | [022](specs/done/022-time-sheets-and-settlement.md) |
| The sales kanban has no read model of its own | the board costs 7 `?stage=` queries plus `/totals`; `PipelineStageSummary` cannot honour the filters the board needs | [010](specs/done/010-sales-opportunities.md) |
| Nobody can enter hours on somebody else's behalf | the owner of a month is the only writer | [022](specs/done/022-time-sheets-and-settlement.md) |
| A rate has no history | the settlement export uses the rate in force at export time | [022](specs/done/022-time-sheets-and-settlement.md) |

## Rough edges in the code

| What | Why it matters |
|---|---|
| Commands carry ids as plain `Guid` | swapping `companyId` and `organizationId` compiles and passes unit tests, because the test builds the command the same way; only an integration test catches it |
| The tenant check is written per handler, three different ways | one rule, two HTTP codes (400 and 403) for the same attempt, and a handler that forgets it simply has none |
| The literal `"Invalid organization id"` in three handlers | `OrganizationId.OrganizationNotMatchMessage` exists for it |
| Value object factories accumulate errors by hand | a missing `errors.Add` lets a `null!` through silently ([ADR-0007](../docs/adr/0007-value-objects-trycreate.md)) |
| `JobPostId.From` does not refuse `Guid.Empty` | `CompanyId.From` and `OrganizationId.From` do |
| `HrAgencySystem.Audit` is an empty project in the solution | it suggests an audit that does not exist; either delete it or build it as an event-driven exercise (it would need every module to publish integration events) |
| `Recruitment` is the largest module | job posts, candidates, applications, interviews and tags in one place; the feeds already left it ([ADR-0009](../docs/adr/0009-feeds-own-read-model-and-worker.md)) |
| `AutoCreate.CreateOrUpdate` in every environment | fine for a project that never ships ([ADR-0025](../docs/adr/0025-learning-project-no-migrations.md)); a real deployment would need migrations |

## Fixed since the last review (kept so they are not "rediscovered")

- `.StartAsync()` on the Wolverine host builder - removed in `a0d7b1fd`.
- A project now optionally points at the opportunity it was sold as - `f495bb91`.
