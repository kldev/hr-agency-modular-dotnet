# 033 · Quality gates and startup validation

**Status:** done · **Built:** 2026-09-18 → 2026-10-02 · **Verify:** `./verify all` · **Plan:** none

## Why
A developer (or an agent) should not be able to push code that is unformatted or that the JetBrains
inspection flags, and an operator should not be able to start the API with a wrong secret or url and
find out at the first request. Before this, formatting depended on discipline, inspection findings
piled up, and a misconfigured host started, touched the database and failed later with an unclear
error. A fresh environment also needed a platform owner without running the development seed.

## What it promises
- Every C# file is formatted by CSharpier with one checked-in configuration (`.csharpierrc.json`,
  tool in `dotnet-tools.json`).
- Husky hooks install themselves on the first `dotnet restore` of a clone (MSBuild target in
  `HrAgencySystem.Api.csproj`; skipped without `.git` and with `HUSKY=0`).
- **pre-commit** formats only the staged `*.cs`, `*.csproj`, `*.props`, `*.targets` files and stages
  them again.
- **pre-push** runs `scripts/inspect-code.sh`: `jb inspectcode` at `SUGGESTION`, non-zero on any
  finding. The solution is at zero findings since 2026-10-02.
- False positives are switched off by convention in `.editorconfig`, each with its reason (Wolverine
  handlers, request records in `Endpoints/**/Maps`, serialized state in `Domain/Projections/Documents/
  ReadModel/Sagas`, ...); one-off cases use `[UsedImplicitly]` with a comment.
- Options are validated at startup (`ValidateOnStart`), and `Program.cs` calls
  `IStartupValidator.Validate()` before seeding, so a bad setting stops the API naming the setting
  before the database is touched.
- Service secrets (`FileService:Secret`, `Reports:Secret`) must be at least 32 bytes and differ from
  `Jwt:SecretKey`; `Reports:Secret` also differs from `FileService:Secret`. Base urls must be absolute
  http(s) urls.
- With `HR_AGENCY_EMAIL` / `HR_AGENCY_PASSWORD` set, the API creates that platform owner at startup in
  any environment; a taken address is left alone so a restart does not fail; the development seed
  reuses an existing owner.
- The SDK is pinned (`global.json`, 10.0.100, `latestFeature`) and packages restore from nuget.org only.

## Surface
- Repo root: `.csharpierrc.json`, `dotnet-tools.json`, `.husky/` (`pre-commit`, `pre-push`,
  `task-runner.json`), `scripts/inspect-code.sh`, `.editorconfig`, `tests/.editorconfig`,
  `global.json`, `Directory.Build.props` (JetBrains.Annotations for every project).
- Backend: `src/HrAgencySystem.Api/Infrastructure/ServiceSecrets.cs`, option classes with validation
  (`JwtConfig`, `S3Config`, `SmtpConfig`, `RabbitMqConfig`, `ApplicationConfig`, `InternalApiConfig`,
  file service and reports client configs), `Identity/Services/SeedStartupAccount.cs`.
- Tests: `tests/HrAgencySystem.IntegrationTests.Platform/Hosting`, `UnitTests/Identity/Infrastructure/JwtConfigValidationTests.cs`,
  `UnitTests/Identity/Services/SeedStartupAccountTests.cs`, `EmailTemplates.UnitTests/EmailSenderRegistrationTests.cs`.

## Out of scope for this feature
- A CI pipeline running the same gates on a server - not yet; the gates are local hooks only.
- Frontend gates in hooks (Biome runs via `yarn check`, not from Husky) - not done.
- A settings page showing effective configuration - not yet.

## Acceptance criteria
- [x] A file service secret equal to the user token key stops the host — `./verify observability` (`StartupConfigurationTests.A_file_service_secret_equal_to_the_user_token_key_stops_the_host`)
- [x] A relative portal url stops the host — `./verify observability` (`A_relative_portal_url_stops_the_host`)
- [x] JWT section: short key, missing issuer, zero lifetime are refused — `./verify identity` (`JwtConfigValidationTests`)
- [x] The MailKit provider without an SMTP host is refused — `./verify emails` (`AddEMailTemplates_WithMailKitProviderAndNoHost_RefusesTheConfiguration`)
- [ ] RabbitMQ, S3 and internal API options stop the host when wrong — unproven (validation exists, no test found)
- [x] Startup platform owner: nothing without both variables, a taken address left alone, a free one becomes owner — `./verify identity` (`SeedStartupAccountTests`)
- [x] Zero inspection findings — evidence: pre-push hook `scripts/inspect-code.sh` (dfa6a3be); not re-run during reconstruction
- [ ] Hooks install on restore and format only staged files — unproven (no test; behaviour lives in the MSBuild target and `task-runner.json`)
- [ ] Docker images restore with the pinned SDK and nuget.org-only source — unproven (no test found)

## Decisions
- CSharpier pre-commit, `jb inspectcode` zero findings pre-push, options validated at startup: [ADR-0024](../../../docs/adr/0024-quality-gates.md).
- The integration host runs as `Testing`, so settings missing in production are missing in tests too: [ADR-0021](../../../docs/adr/0021-integration-tests-per-area.md).
- Never apply inspection suggestions to make setters private or init-only on serialized state - Marten would drop the value from a snapshot; those folders are excluded instead.
- "Absolute" is not enough for a url check: on Linux `/portal` parses as an absolute `file://` uri, so the check requires http or https.

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-18 | 6eb793f5 | CSharpier added (66d8cdca formats the code, 0f7ebb3e explicit config) |
| 2026-09-19 | 713d03a8 | Husky added |
| 2026-09-24 | bed35fc7 | First ReSharper warnings fixed, `.editorconfig` added |
| 2026-09-25 | a2dad2a5 | Platform owner from environment at startup; seed reuses an existing owner |
| 2026-10-02 | 6d1f5498 | Restore from nuget.org only, SDK pinned |
| 2026-10-02 | b620fb8f | Whole solution formatted with CSharpier |
| 2026-10-02 | 894c9055 | Hooks install on restore, only staged files formatted |
| 2026-10-02 | d5b186f6 | Options validated at startup, before the database is touched |
| 2026-10-02 | 55b221c4 | Exceptions handled before authentication runs |
| 2026-10-02 | 3c87af33 | Every id route parameter constrained to `guid` |
| 2026-10-02 | cafa5164 | Inspection taught the handler and namespace conventions |
| 2026-10-02 | ac977d3e | Invalid candidate phone or name is a 400, not a null reference |
| 2026-10-02 | 944d5f61 | Remaining findings settled |
| 2026-10-02 | dfa6a3be | JetBrains inspection before every push |

## Notes from reconstruction
- The inspection sweep found real bugs, not just style: a null reference on an invalid candidate
  phone (ac977d3e), a fire-and-forget `StartAsync` on the Wolverine host builder (a0d7b1fd), a missing
  cancellation token on exclusive appends (7ebd35a9).
- CSharpier existed since 2026-09-18, but the solution was only fully formatted on 2026-10-02; files
  moved in the test split were left unformatted on purpose until then.
- Commit messages carry no bodies; the reasons above come from code comments and `CLAUDE.md`.
- The startup-validation tests sit under the `observability` verify name (`IntegrationTests.Hosting`).
