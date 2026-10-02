# 031 · Integration tests per area

**Status:** done · **Built:** 2026-10-01 → 2026-10-02 · **Verify:** `./verify all` · **Plan:** backend 030 (local, not in git)

## Why
All ~430 integration tests lived in one project with one PostgreSQL container and one API host, run
sequentially. A developer working on time sheets had to build and run sales, recruitment and forms
tests too. Splitting by area lets one area be built and run on its own, and lets the solution run
the areas in parallel.

## What it promises
- Integration tests live in one project per area, each with its own Testcontainer and API host:
  `IntegrationTests.Platform` (auth, users, organizations, owner, teams, observability, hosting,
  security), `.Sales` (companies, sales, tasks), `.Recruitment` (job descriptions, posts, candidates,
  interviews, timeline, feeds, internal board API), `.Delivery` (projects, workers, legal entities,
  forms), `.Agency`, `.CrossCutting` (suggestions, reports).
- `IntegrationTests.Persistence` tests repositories against a bare container with no API host, its
  own `PostgresFixture` and a two-method `DatabaseCleaner`.
- Infrastructure (`IntegrationEnvironment`, `ApiApplicationFactory`, fakes, `DatabaseCleaner`,
  `Eventually`) and every `*TestClient`/`*TestData` live in the library
  `IntegrationTests.Shared`; test projects never reference one another.
- Namespaces stayed (`HrAgencySystem.IntegrationTests.<Area>`), so no test's `using` changed.
- The test count is the same before and after the split (433).
- `HrAgencySystem.UnitTests` no longer references an integration test project.
- The host under test runs as `Testing`, not Development: no seeder, no development endpoints, no
  `appsettings.Development.json`; every needed setting is passed with `UseSetting`.

## Surface
- Tests: `tests/HrAgencySystem.IntegrationTests.{Shared,Platform,Sales,Recruitment,Delivery,Agency,CrossCutting,Persistence}`,
  each with `Infrastructure/Collections.cs`; `tests/.editorconfig`; `HrAgencySystem.slnx`.
- Backend: `InternalsVisibleTo` for the test projects in `HrAgencySystem.Api` and `.Shared`
  (and `Feeds` for `.Persistence`).
- Runner: `./verify <feature>` maps each feature to its project and namespace filter.

## Out of scope for this feature
- Splitting `HrAgencySystem.UnitTests` - not done; it stays one project.
- Running the API host once for several areas - not ever under this design (one host per project
  is the cost accepted: 7 containers and 6 host starts for a full run).
- Moving to xUnit v3 - not done; the xUnit v2 collection rule below still applies.

## Acceptance criteria
- [x] Each area project builds and runs on its own — `./verify all` (and per area, e.g. `./verify agency`)
- [x] Same number of tests before and after (433) — evidence: counts table in plan 030, "after" section; not re-counted during reconstruction
- [x] Development endpoints are not mapped in the test host — `./verify observability` (`TestingEnvironmentTests.Development_endpoints_are_not_mapped`)
- [x] A flaky agency test exposed by the split is fixed by reading the org chart with `FetchLatest` — `./verify agency` (`SettlementExportTests`)
- [ ] Projects run in parallel and a single area costs one container and one host start — unproven (no measurement recorded)

## Decisions
- One Testcontainer and host per area, shared library, `Testing` environment: [ADR-0021](../../../docs/adr/0021-integration-tests-per-area.md).
- External Wolverine transports are stubbed so integration tests need no broker (a4ef2e0f, earlier groundwork).
- xUnit v2 discovers `[CollectionDefinition]` only in the test assembly, so each project declares a
  one-line `IntegrationCollection` with the same `Name` constant; the `[Collection]` attributes did not change.
- Clients and test data go to the library because the fixture graph crosses areas (suggestions build
  companies, projects, teams, users, workers), which rules out references between test projects.
- `.Persistence` has its own small cleaner: the full cleaner knows every module's types and would pull in the whole solution.
- Each stage was a branch, a PR and a merge from a fresh `main`.

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-19 | a4ef2e0f | Stub external Wolverine transports in integration tests |
| 2026-10-01 | 81b8512e | Unit tests reference the API directly (PR #11, e5c9fe4d) |
| 2026-10-01 | cce97102 | Infrastructure and clients into the shared library (PR #12, 7a41fda1) |
| 2026-10-01 | 89c12953 | Postgres-only repository tests without the API host (PR #13, ad1cb478) |
| 2026-10-01 | 4fc02921 | Platform project (PR #14, 4fedac7f) |
| 2026-10-01 | dd423353 | Sales project (PR #15, a8efc6b5) |
| 2026-10-01 | 37bb346d | Recruitment project (PR #16, 837f39ed) |
| 2026-10-01 | 3181f5e5 | Delivery project (PR #17, d3a896c4) |
| 2026-10-01 | 62b3b77d | Agency project (PR #18, fcfe040a) |
| 2026-10-01 | b83b7279 | CrossCutting project, all-in-one project removed (PR #19, 36a23806) |
| 2026-10-01 | 3f18078f | Org chart read with `FetchLatest` - flaky test fixed (PR #20, ceb288f3) |
| 2026-10-02 | dcfcaf03 | Integration host runs as `Testing` instead of falling back to Development |

## Notes from reconstruction
- The plan said unit tests would lose their reference and get nothing in exchange; two unit tests use
  API classes, so `UnitTests` now references `HrAgencySystem.Api` directly and still builds the host.
- The split surfaced a race: `GetStructureAsync` replayed the stream only when the projection was
  missing, so a lagging snapshot hid a just-assigned head and approval returned 400 in 2 of 4 runs.
- Files not yet formatted with CSharpier were moved unchanged so git recorded clean renames.
- Until dcfcaf03 the test host silently ran as Development.
