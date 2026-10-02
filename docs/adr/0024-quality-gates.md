# ADR-0024: CSharpier on commit, zero JetBrains inspection findings on push, options validated at startup

- **Status:** Accepted
- **Date:** 2026-09-18 (CSharpier); gates completed 2026-10-02
- **Evidence:** `6eb793f5` feat(api) add csharpier,
  `713d03a8` chore: add husky,
  `bed35fc7` chore: fix ReSharper inspection warnings and add .editorconfig,
  `894c9055` chore(husky): install the hooks on restore and format only the staged files,
  `d5b186f6` feat(config): validate options at startup, before the database is touched,
  `dfa6a3be` chore(husky): run the jetbrains inspection before every push
- **Specs:** [033-quality-gates-and-startup-validation](../../.shipit/specs/done/033-quality-gates-and-startup-validation.md),
  [031-integration-tests-per-area](../../.shipit/specs/done/031-integration-tests-per-area.md)

## Context
The codebase is the point of the exercise (ADR-0025), and most of it is generated with an AI agent.
Formatting drifted between files, inspection warnings accumulated, and a wrong secret or URL only
surfaced on the first request that needed it, sometimes after the seeder had already touched the
database.

## Decision
- **Formatting:** CSharpier (since `6eb793f5`, solution formatted in `b620fb8f`). Husky.Net hooks
  install themselves on the first `dotnet restore` (target in `HrAgencySystem.Api.csproj`, skipped
  without `.git` and with `HUSKY=0`); **pre-commit** formats the staged files and re-stages them.
- **Inspection:** **pre-push** runs `scripts/inspect-code.sh` - `jb inspectcode` at `SUGGESTION`,
  non-zero on any finding. The solution reached zero findings on 2026-10-02.
- False positives are switched off **by convention** in `.editorconfig`, each with its reason
  (Wolverine `*Handler.cs`, request records in `Endpoints/**/Maps`, serialized state in
  `Domain/Projections/Documents/ReadModel/Sagas`, ...). One-off cases use `[UsedImplicitly]` with a
  comment naming who reaches the member.
- **Startup validation:** options are bound with `ValidateOnStart`, and `Program.cs` calls
  `IStartupValidator.Validate()` before seeding. Service secrets must be at least 32 bytes and differ
  from `Jwt:SecretKey` and each other. A new option = binding + validation in its module.

## Alternatives considered
- `713d03a8` first wired the pre-commit hook to a task runner run by hand; `894c9055` made it install
  on restore and format only staged files rather than the whole tree.
- The inspection was first a one-off clean-up (`bed35fc7`, 2026-09-24) without a gate; findings came
  back and were settled again on 2026-10-02 (`cafa5164`, `e9a213c5`, `944d5f61`) before the gate
  (`dfa6a3be`) was added.
No other tools are recorded as considered.

## Consequences
- Never apply "make setter private/init-only" suggestions to serialized state: the value drops out
  of the Marten snapshot. That is why those folders are excluded by convention.
- Running the inspection at push time is slow; it is meant to be run with `--no-build` before
  committing.
- The integration host runs as `Testing` and needs every required option via `UseSetting`
  (ADR-0021).

## Revisit when
- CI exists, so the push gate can move to the pipeline.
- The inspection's false-positive list grows faster than real findings.
