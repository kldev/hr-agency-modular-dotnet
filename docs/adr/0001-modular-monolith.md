# ADR-0001: One deployable API built from modules; a separate host only where it earns it

- **Status:** Accepted
- **Date:** 2026-08-30
- **Evidence:** `f1c00ced` start project, `e5f2b830` feat(feeds): add background worker host for feed generation, `0666512f` feat(notifications): send job application emails over rabbitmq to NotificationWorker, `3ff75a9d` feat(files): add the file service with tenant scoped uploads and downloads, `940e05a2` feat(reports): reports service fed by read model projections, `908fb1c7` feat(api): remove unused project
- **Specs:** [001](../../.shipit/specs/done/001-organizations-and-tenancy.md), [007](../../.shipit/specs/done/007-job-feeds.md), [008](../../.shipit/specs/done/008-public-job-board.md), [012](../../.shipit/specs/done/012-email-notifications.md), [015](../../.shipit/specs/done/015-file-service.md), [027](../../.shipit/specs/done/027-reports.md)

## Context
HR Agency is a learning project whose point is the quality of the domain model, not distribution
(see [ADR-0025](0025-learning-project-no-migrations.md)). The business areas (identity, companies,
sales, recruitment, delivery, the agency as employer) share one PostgreSQL database and one event
store, and most use cases touch one area at a time. A network hop between them would add failure
modes without teaching anything about the model.

## Decision
- `HrAgencySystem.Api` is the single deployable that hosts every business module. Each module is
  its own project (`HrAgencySystem.<Module>`) with a `<Module>Module.cs` composition root, its own
  database schema, and is wired in three places in `src/HrAgencySystem.Api/Infrastructure/`
  (`SetupApplicationModulesExtensions`, `SetupMartenForApplication`, `SetupWolverineForApplication`).
- Modules never reference each other's projects ([ADR-0006](0006-contracts-and-shared-kernel-ports.md)).
- A separate process exists only when it has a reason the API cannot satisfy:
  - `FeedsWorker` - background feed generation; scaling the API must not multiply schedulers
    ([ADR-0009](0009-feeds-own-read-model-and-worker.md)).
  - `NotificationWorker` - consumes mail off RabbitMQ ([ADR-0010](0010-email-over-rabbitmq.md)).
  - `FileService` - private documents behind a process boundary ([ADR-0012](0012-separate-file-service.md)).
  - `ReportsService` - SQL aggregation and Excel over the `reports` schema ([ADR-0018](0018-reports-service-timestamps.md)).
  - `Web` - the public job board, with no database access ([ADR-0017](0017-job-board-over-internal-api.md)).
- No business module is ever split into a service of its own.

## Alternatives considered
- **A `Files` module inside the API instead of a file service.** Recommended in backend plan 010
  (local, not in git), which records that the product owner chose a separate process anyway and
  that `.docs/05` ("do not split the monolith") stopped being fully true on that day.
- **Reports as a module in the API process, or a service fed by RabbitMQ.** Both rejected in plan
  027: the first because a separate host was wanted, the second because six producing modules
  would have to start publishing and the service would need its own store and idempotency.
- **Feed workers inside the API.** That was the state from `2cf376aa` until `12b118bc` (extract
  `Feeds` project) and `e5f2b830` (own worker host).
- **A shared `HrAgencySystem.Suggestion` project.** Existed from `f1c00ced`, removed empty in
  `908fb1c7`; typeahead now lives in the module that owns the data.

## Consequences
- One `dotnet run` plus Postgres used to run the whole product. Since `0fd76bd3` documents need
  the file service and reports need the reports service; `/healthz` and `/health/ready` show both.
- One transaction (Marten + Wolverine outbox) covers a command, its events and its outgoing
  messages, because everything lives in one process and one database.
- `Recruitment` grew into the largest module (job posts, candidates, applications, interviews);
  `.docs/05` lists splitting it as low value today.
- `HrAgencySystem.Audit` is still an empty `.csproj` that is not wired anywhere.
- Every new project the hosts reference must be added by hand to each `Dockerfile*`; `f51d8a56`
  is one fix of that kind.

## Revisit when
1. A module needs to scale or deploy independently of the rest for a measured reason.
2. The reports or file service gets its own database - the shared Postgres instance is a
   documented compromise in plans 010 and 027.
