# 027 · Reports

**Status:** done · **Built:** 2026-09-23 → 2026-09-23 · **Verify:** `./verify reports` · **Plan:** backend 027 (local, not in git)

## Why
The agency dashboard at `/app/dashboard` was an empty page with a component showing invented numbers,
and the platform owner's "Reports" menu item led to a 404. A recruitment manager wants to see how
applications move through the funnel month by month; the platform owner wants one table comparing
every organization's activity. Both want the numbers in Excel too.

## What it promises
- A member of an organization sees their own organization's recruitment report: totals for the
  period, a funnel (applications → screening → interview → assessment → offer → hired), a monthly
  series and applications per source. No extra role is needed - it is their own data.
- The organization never comes from a parameter: it is taken from the caller's token.
- The platform owner sees a platform report: one row per organization (posts, published posts,
  applications, interviews, offers, hires, projects, active projects, last activity), most recently
  active first, plus platform totals.
- Both reports export to `.xlsx` (a sheet per section, numbers stored as numbers).
- The period is a month range (`from`/`to`, `yyyy-MM`); without one it is the last six months
  including the current one; more than 24 months or a malformed period is a 400 that never reaches
  the reports service.
- An organization user asking for the platform report gets 403.
- When the reports service is down the API answers 503 (ProblemDetails), not 500; `/health/ready`
  reports it as `Degraded`.
- An empty period returns zeros and empty lists, not an error; the front shows an empty state.

## Surface
- Backend: `src/HrAgencySystem.Reports.ReadModel` (schema `reports`: organizations, job posts,
  applications, interviews, projects; one `DbContext` per table in `ReportsSchema.cs`), EF projections
  in Organization, Recruitment and Projects; `src/services/HrAgencySystem.ReportsService` (Minimal API,
  Dapper queries, `ReportWorkbook` export, `ReportPolicies`); `src/services/HrAgencySystem.ReportsService.Contracts`
  (`IReportsClient`, DTOs, `ReportPeriod`, token constants); API `Endpoints/Reports`:
  `GET /api/reports/recruitment[/export]`, `GET /api/owners/reports/platform[/export]`.
- Infra: `Dockerfile.reports-service`, `reports-service` in `infrastructure/docker-compose.yml`,
  `Reports:BaseUrl` / `Reports:Secret`.
- Frontend: `frontend/src/features/dashboard`, `frontend/src/features/reports`,
  `components/ui/charts` (recharts), routes `/app/dashboard` (`?range=3|6|12`) and `/admin/reports`.
- Tests: `tests/HrAgencySystem.UnitTests/Reports`, `tests/HrAgencySystem.ReportsService.UnitTests`,
  `tests/HrAgencySystem.ReportsService.IntegrationTests`, `tests/HrAgencySystem.IntegrationTests.CrossCutting/Reports`,
  `frontend/e2e/reports/reports.spec.ts`.

## Out of scope for this feature
- Sales pipeline, Workers and Agency reports - not yet (plan §10).
- Seed history spread over past months, so the monthly chart has more than one bar - not yet
  (deferred to a separate plan, still open).
- Form answer reporting (`reports.form_answers`) - not yet, see [028](028-dynamic-forms.md).
- Counters maintained per event - not ever (timestamps instead, see Decisions).

## Acceptance criteria
- [x] An application row keeps the first time it reached each stage; a narrative event plus `JobApplicationStatusChanged` count once; scheduling an interview reaches the stage without a status change — `./verify reports` (`ApplicationReportProjectionTests`)
- [x] A project records when it first went live, through a suspension — `./verify reports` (`ProjectReportProjectionTests`, `ReportProjectionsTests`)
- [x] The funnel follows the applications of the period and never widens; months list every month including empty ones; the first moment of the next month is outside the period — `./verify reports` (`RecruitmentReportQueryTests`)
- [x] Another organization's rows are never counted — `./verify reports` (`AnotherOrganizationsRows_AreNeverCounted`)
- [x] Platform report lists every organization, most recently active first — `./verify reports` (`PlatformReportQueryTests`)
- [x] Service token: wrong secret, a file service token, an expired token are rejected; an organization token opens only the organization report, a platform token only the platform report — `./verify reports` (`ServiceTokenTests`)
- [x] Period defaults to six months, refuses more than two years and malformed input — `./verify reports` (`ReportPeriodTests`, `ReportEndpointsTests`)
- [x] Export has a sheet per section with numeric cells — `./verify reports` (`ReportWorkbookTests`, `RecruitmentExport_ReturnsTheSpreadsheet`)
- [x] A dead reports service is a 503; the platform report is forbidden to an organization user — `./verify reports` (`ReportEndpointsTests`)
- [x] Dashboard and owner report render with charts — e2e: `frontend/e2e/reports/reports.spec.ts`; evidence: `docs/screenshots/dashboard.png`, `docs/screenshots/platform-reports.png`
- [ ] Docker image of the reports service builds and the compose healthcheck passes — unproven (no test found; fixed by hand in 573dabd3)

## Decisions
- Separate read-only service over tables filled by projections: [ADR-0018](../../../docs/adr/0018-reports-service-timestamps.md).
- Same tables-as-contract pattern as feeds: [ADR-0009](../../../docs/adr/0009-feeds-own-read-model-and-worker.md); same service-token pattern as files, but its own audience (`reports-service`) and secret: [ADR-0012](../../../docs/adr/0012-separate-file-service.md).
- One row per entity with first-reached timestamps, months from `date_trunc` - a counter would need the state before the event.
- Funnel is a cohort of the period's applications ("reached at least", a `coalesce` chain); months and totals are activity. They are never mixed.
- Query tests live in their own test project with their own Testcontainer, because two web hosts in one test project means two `Program` classes.

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-23 | 940e05a2 | Reports service, read model projections, API client and endpoints |
| 2026-09-23 | 573dabd3 | `curl` in file and reports service images for the compose healthcheck |
| 2026-09-23 | 53446721 | 503 described as ProblemDetails so the generated client has a typed error |
| 2026-09-23 | ec772cc4 | Recruitment dashboard and owner reports with charts and Excel export |
| 2026-09-23 | 7ebd8490 | Reports service in the readme |
| 2026-09-23 | a2956b1e | Dashboard and platform report screenshots, seeded applications moved to offers and hires |
| 2026-09-25 | aef213dc | Reports service gets observability and a readiness check |
| 2026-10-01 | b83b7279 | Report integration tests move to `IntegrationTests.CrossCutting` |

## Notes from reconstruction
- The plan put five EF projections on one `DbContext`; each index was then registered five times and
  the Weasel migration failed as a whole, taking `feeds.job_posts` down with it. The code uses one
  context per table.
- The plan expected `AddWebObservability` in the new host; it was not on `main` yet and was added two
  days later (aef213dc).
- The seeded data is all "today", so the monthly chart shows one bar; the follow-up plan for seeding
  history was never written.
