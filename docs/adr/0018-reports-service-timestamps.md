# ADR-0018: Reports come from relational rows with first-reached timestamps, aggregated by a separate service

- **Status:** Accepted
- **Date:** 2026-09-23
- **Evidence:** `940e05a2` feat(reports): reports service fed by read model projections, with API client and endpoints,
  `53446721` fix(reports): describe the 503 as problem details so the generated client has no void error,
  `ec772cc4` feat(frontend): recruitment dashboard and platform owner reports with charts and excel export,
  `b83b7279` test: suggestion and report integration tests in their own project, drop the all-in-one project
- **Specs:** [027-reports](../../.shipit/specs/done/027-reports.md),
  [007-job-feeds](../../.shipit/specs/done/007-job-feeds.md)

## Context
The agency dashboard was an empty placeholder and the owner's report page linked to a 404. None of
the facts needed (organization created, post published, application stages, interviews, project
status) left their modules; only the API process ran the event store and the async daemon. The
history timeline work (plan 026) had shown that not every stage change raises
`JobApplicationStatusChanged`, and that counting transitions needs the state before the event.

## Decision
- Follow the **Feeds pattern**: `EfCoreSingleStreamProjection`s living in the owning modules
  (`Organization`, `Recruitment`, `Projects`) write rows into schema `reports`, described by
  `HrAgencySystem.Reports.ReadModel`. The table is the contract; no new integration events.
- **One row per entity with timestamps, not counters.** An application row keeps the first time it
  reached each stage (`screening_at` … `hired_at`, never overwritten). Stages are set by the
  narrative events **and** by `JobApplicationStatusChanged`; first-time columns make the double event
  a no-op. Months come from `date_trunc` at query time.
- The funnel is a **cohort** (applications of the period, "reached at least" via a `coalesce`
  chain); monthly series are **activity**. They are never mixed.
- **One `DbContext` per table** (`ReportsSchema.cs`).
- `ReportsService` (`src/services`) is a separate HTTP host with no event store: Dapper SQL plus
  Excel export. The API calls it through `IReportsClient` with a service token
  (`aud: reports-service`, secret `Reports:Secret`); an organization token carries `org`, the
  owner's carries `scope=platform`. `ReportsServiceException` maps to 503.

## Alternatives considered
From plan 027 (local, not in git), "Dlaczego nie pozostałe warianty":
- A module inside the API process - would work, but the user wanted a separate host.
- A service fed over RabbitMQ - every producer in six modules would publish new messages and the
  service would need its own store and idempotency; kept only as a direction if it ever gets its own
  database.
- A counter row per (organization, month) - rejected: needs a multi-stream projection and the prior
  state, the trap from plan 026; rebuilds could drift sums.
- One `DbContext` for all tables (as planned) - replaced after five projections registered each index
  five times and the Weasel migration failed, taking `feeds.job_posts` down with it.

## Consequences
- A new projection replays all history, so reports cover data from before deployment.
- Rows hold no names of people or companies.
- Service query tests live in `tests/HrAgencySystem.ReportsService.IntegrationTests` with their own
  container; two web hosts cannot share one test project (two `Program` classes).
- Seeded data is created "now", so monthly charts look flat until the seed spreads history.

## Revisit when
- The reports service needs its own database (then the RabbitMQ variant returns).
- Workers/Agency or sales-funnel reports are added (explicitly out of scope).
