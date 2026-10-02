# ADR-0021: Integration tests are split per area, one Testcontainer and one API host per project, on a shared library

- **Status:** Accepted
- **Date:** 2026-10-01
- **Evidence:** `81b8512e` chore(tests): unit tests reference the api directly instead of the integration test project,
  `cce97102` test: move integration test infrastructure and clients into a shared library,
  `89c12953` test: postgres-only repository tests in their own project without the api host,
  `b83b7279` test: suggestion and report integration tests in their own project, drop the all-in-one project,
  `3f18078f` fix(agency): read the org chart with FetchLatest so a lagging snapshot cannot hide a just assigned head,
  `dcfcaf03` test: run the integration host as Testing instead of falling back to Development
- **Specs:** [031-integration-tests-per-area](../../.shipit/specs/done/031-integration-tests-per-area.md),
  [033-quality-gates-and-startup-validation](../../.shipit/specs/done/033-quality-gates-and-startup-validation.md)

## Context
From the first commit (`f1c00ced`) integration tests ran in one project,
`tests/HrAgencySystem.IntegrationTests`: about 430 tests, one PostgreSQL Testcontainer, one API host,
all sequential. Working on one area meant building and running everything.

## Decision
- Seven projects: `IntegrationTests.{Platform, Sales, Recruitment, Delivery, Agency, CrossCutting}`
  (real HTTP against an API host) and `.Persistence` (repositories against a bare container, own
  `PostgresFixture` and a two-method `DatabaseCleaner`).
- Everything shared - `Infrastructure/` (`IntegrationEnvironment`, `ApiApplicationFactory`, fakes,
  `DatabaseCleaner`, `Eventually`) and every `*TestClient` / `*TestData` - is the library
  `HrAgencySystem.IntegrationTests.Shared`; test projects never reference one another. Namespaces
  were kept, so tests did not change.
- Each project declares its own `IntegrationCollection` in `Infrastructure/Collections.cs`, because
  xUnit v2 discovers `[CollectionDefinition]` only in the test assembly.
- Done as one PR per stage (#11-#19), test count equal before and after (433).
- Since `dcfcaf03` the host runs as **`Testing`**, not Development: no seeder, no dev endpoints, every
  needed setting passed with `UseSetting` in `ApiApplicationFactory`.

## Alternatives considered
- Plan 030 (local, not in git) expected `UnitTests` to drop its reference to the integration project
  with nothing in its place; two unit tests use API classes, so it references `HrAgencySystem.Api`
  directly instead (`81b8512e`).
- Referencing test projects from each other was ruled out by the client dependency graph
  (`Suggestion → Company, Project, Team, User, Worker`), hence the shared library.

## Consequences
- A full run costs seven containers and six host starts (each with a Marten schema migration), but
  projects run in parallel and one area costs one of each.
- A fresh host with a cold daemon exposed a race: `SettlementExportTests` failed 2 of 4 runs until
  the chart read used `FetchLatest` (`3f18078f`, PR #20).
- A new test project needs `Collections.cs`, an `InternalsVisibleTo` entry in `.Shared` and in the API,
  and a line in `HrAgencySystem.slnx`.
- A new required option needs a `UseSetting` line, or the `Testing` host fails startup validation.

## Revisit when
- Moving to xUnit v3 (collection discovery rules change).
- An area grows large enough to deserve its own split, or host start time dominates the run.
