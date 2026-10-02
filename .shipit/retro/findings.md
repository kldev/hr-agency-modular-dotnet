# Retro findings

A ledger of what the history of this repository taught, one finding per row: what happened, the evidence, and the one
change it led to (or should lead to). Reconstructed from 603 commits (2026-08-30 → 2026-10-02); new rows go on top
after each feature.

## Process

| # | Finding | Evidence | The one change |
|---|---|---|---|
| P1 | The project was built plan-first (30 backend plans, 14 frontend plans) but the plans were git-ignored, so the reasoning never travelled with the code | `.plans/` and `.docs/` are in `.gitignore`; this folder had to be reconstructed from commits | Specs and ADRs live in git (`.shipit/`, `docs/adr/`); plans may stay local as drafts |
| P2 | Notes appended after a plan was approved (`uwagi do planu`) repeatedly overrode its decisions | Workers: plan proposed `Workers.Domestic`/`Workers.Foreign`, the code has one module ([ADR-0014](../../docs/adr/0014-workers-one-module-two-aggregates.md)) | A spec's "Decisions" section is updated when the decision changes, not appended to |
| P3 | Big UI migrations went well as a stack of small PRs | HeroUI v3 in PRs #21-#36, integration test split in PRs #11-#19, all merged the same day | Keep "one concern per PR" for any cross-cutting change |
| P4 | Frontend fixes cluster right after a feature lands (the 2026-09-20..22 run) | ~25 `fix(front)` commits in three days, mostly layout, narrow screens and select/date behaviour | Check narrow screens and empty/initial values before calling a screen done; e2e screenshots at the end of each spec |
| P5 | "Done" was never written down per feature, so acceptance had to be inferred from tests | the specs in `specs/done/` mark several criteria `unproven` | Every new spec names `./verify <feature>` or an evidence file for each criterion |

## Stack traps (each cost real time once)

| # | Finding | Evidence | The one change |
|---|---|---|---|
| T1 | Reading state from an async projection right after a write returns stale data | `a37f9c4d` (job post title from the aggregate, not the projection), `3f18078f` (org chart with `FetchLatest`) | Decisions inside a handler read the aggregate/stream; projections serve screens. Tests use `Eventually.AssertAsync` |
| T2 | Concurrent appends to one stream lost data silently | `a780a729` (staffing seats), `b0d70b31` (job post counter) | Append exclusively (`session.Events.AppendExclusive`) wherever a count or a set lives on the stream |
| T3 | Every Dockerfile copies `.csproj` files by hand; a new project breaks `dotnet restore` in the image only | `f51d8a56` | After adding a project, build the image (`docker build -f Dockerfile .`) - nothing else catches it |
| T4 | Postgres identifiers over 63 characters fail at migration | `b8f1eb91` | Name long composite Marten indexes by hand |
| T5 | Wolverine generated code needs `public` types; an `internal` repository injected into a handler fails only at runtime | CLAUDE.md, Workers section | Repositories a handler takes are `public` |
| T6 | A value type without a public constructor did not survive the message wire | `c1803257` | Shared value objects that cross the bus get a public constructor |
| T7 | A file the file service refused surfaced as 503 ("storage down") | `b01e1545` | Map "refused" and "unavailable" to different status codes from the start |
| T8 | Metrics silently empty because of a wrong meter name | `16f4f0e0` (Wolverine's meter is `Wolverine:<service>`) | Check every dashboard `expr` against a running Prometheus |
| T9 | An endpoint under an anonymous prefix never received the bearer | `c110c797` | Authenticated routes never live under `/api/auth` |
| T10 | Exceptions thrown before authentication bypassed ProblemDetails | `55b221c4` | Exception handling is the first middleware |
| T12 | Inserting an enum member in the middle renumbered the ones after it, so every stored event with the old number changed meaning | the sales `Task` activity type went in before `Other` (spec [029](../specs/done/029-sales-workspace-and-tasks.md)); `Other` moved from 5 to 6 | New enum members go at the end, or the enum gets explicit values |
| T11 | Ids in routes accepted any string until constrained | `3c87af33` | Every `{id}` route parameter is `:guid` |

## Open questions for the next retro

- Is the per-handler tenant check worth replacing with one middleware? (see [open.md](../open.md))
- Would typed ids in commands (not on the HTTP boundary) have prevented the swaps described in `open.md`?
