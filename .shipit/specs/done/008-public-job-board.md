# 008 · Public job board

**Status:** done · **Built:** 2026-09-06 → 2026-09-22 · **Verify:** `./verify job-board` · **Plan:** backend 023 (local, not in git); backend 003 superseded

## Why
A candidate does not have an account and should never see the agency panel. They need a public page
per agency listing open positions, a page per position and a simple form to apply. The agency needs
that public site to be a separate, exposed process that cannot touch the database even if it is
compromised.

## What it promises
- `/{slug}/jobs.html` shows the agency's name and its published posts; the list is loaded by the page
  from the agency's JSON feed (`/{slug}/jobs.json`).
- `/{slug}/{postSlug}` shows one published post with an application form; a post that is not
  published, or an unknown agency, is a 404.
- Submitting the form files an application through the same command the panel uses (candidate
  matched by e-mail, recruiter notified); a rejection from the API (400) is shown on the form, not
  on an error page. `/{slug}/success.html` confirms it.
- The board has no database connection and no project references. It reads and writes through four
  internal API routes, keyed by agency slug, never by organization id:
  `GET /api/internal/boards/{slug}`, `GET .../feed.json|feed.xml`, `GET .../posts/{postSlug}`,
  `POST .../posts/{postSlug}/applications`.
- Internal routes accept only a service API key in `X-Api-Key` (scheme `ApiKey`, policy
  `InternalApiPolicy`); no key, a revoked key, or a user's bearer token gets 401. The routes are not
  in OpenAPI.
- The platform owner issues keys (`sk_` + 64 hex characters, value shown once), lists them by a
  display prefix only, and revokes them (the row stays). Only a SHA-256 hash is stored.
- Reads are retried; the application POST is never retried, because a timed-out one may already be filed.
- When the API is down the board shows an error page - no cache. `GET /healthz` on the board reports
  `api: UP|DOWN`.
- A seeded stack carries a fixed key, so the board works without a manual step; missing or
  malformed key configuration stops the board at startup.

## Surface
- Host: `src/HrAgencySystem.Web` (Razor Pages `Jobs`, `Apply`, `Success`, `Error`;
  `Services/HttpJobBoardClient`, `JobBoardClientRegistration`, `InternalApiConfig`, `ApiHealthProbe`),
  `Dockerfile.web`, service `web` on :5050 in `infrastructure/docker-compose.yml`.
- API: `src/HrAgencySystem.Api/Endpoints/Internal`, `Api/Auth/ApiKeyAuthenticationHandler.cs`,
  `InternalApiPolicy.cs`; owner routes `POST|GET /api/owners/api-keys`, `DELETE /api/owners/api-keys/{keyId}`.
- Identity: `Infrastructure/Persistence/ServiceApiKey.cs`, `Application/ApiKeys/{Issue,Revoke}`.
- Seed: `PlatformSeeder` `ServiceApiKeyScenario`, `Config.JobBoardApiKey`.
- Frontend (owner panel): `frontend/src/platform-owner/features/api-keys`, route `/admin/api-keys`.
- Tests: `tests/HrAgencySystem.IntegrationTests.Recruitment/Internal`,
  `tests/HrAgencySystem.UnitTests/Identity/ApiKeyAuthenticationHandlerTests.cs`.

## Out of scope for this feature
- A key per organization, key rotation or expiry (not decided - plan 023 "what is deliberately absent").
- A cache in the board to survive an API outage (not ever unless decided; confirmed 2026-09-22).
- Candidate accounts, CV upload, application status for the candidate (not yet).
- Rate limiting of the board (plan 023 left it to exposure; sign-in limits are not built either).

## Acceptance criteria
- [x] Without a key every internal route is 401 — `./verify job-board`
- [x] A user's credentials do not open internal routes — `./verify job-board`
- [x] A revoked key is 401 — `./verify job-board`
- [x] With a key the agency and its published post are readable — `./verify job-board`
- [x] Unknown slug → 404 for the board and its posts — `./verify job-board`
- [x] Applying through the board creates the application — `./verify job-board`
- [x] Key scheme: no header, missing `sk_` prefix (no lookup), unknown and revoked keys fail — `./verify identity`
- [x] Issue stores only the hash and returns the value once; nameless key refused; revoke keeps the first revocation — `./verify identity`
- [ ] Unpublished post → 404 on the internal route — unproven (no test found)
- [ ] Razor pages, form error display and `/healthz` of the board — unproven (no test project for `Web`)
- [ ] Owner panel issues, copies once and revokes keys — unproven (no e2e found)

## Decisions
- The board is a separate host with no database, talking to the API over internal routes with service
  keys: [ADR-0017](../../../docs/adr/0017-job-board-over-internal-api.md).
- Only hosts that earn it are separate: [ADR-0001](../../../docs/adr/0001-modular-monolith.md).
- Feeds stay where they are and the board passes them through the internal API (same origin for the
  page's `fetch`, no CORS): [ADR-0009](../../../docs/adr/0009-feeds-own-read-model-and-worker.md).
- Internal routes return their own records, not `JobPostProjection`, so a panel read-model change
  does not change the public contract (plan 023).
- Owner, not agency admin, issues keys: one board serves every agency and picks it by slug (plan 023).
- Option validation at startup for `InternalApi:*`: [ADR-0024](../../../docs/adr/0024-quality-gates.md).

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-06 | `ae1f4542` | Razor job board over the feed, sharing the database |
| 2026-09-06 | `487f4e5d` | Apply url in the feed |
| 2026-09-17 | `12b118bc` | Feeds extracted; the board still registered minimal module variants |
| 2026-09-22 | `e5ee1c7e` | Owner issues and revokes `sk_` keys, own authentication scheme |
| 2026-09-22 | `64d909d5` | Four internal board routes opened to a service key |
| 2026-09-22 | `0e66353d` | Board reads and writes over HTTP; minimal module variants removed |
| 2026-09-22 | `f284b12f` | Owner panel: issue, copy once and revoke API keys |
| 2026-09-22 | `ac348f48` | User secrets id for the board's key |
| 2026-09-22 | `b47c4369` | Board in the docker stack, key from `WebApiKey` |
| 2026-09-22 | `60a85f12` | Seeder writes a fixed key for the board |
| 2026-09-22 | `9afe425f` | Feed apply links point at the board |

## Notes from reconstruction
- Plan 003 (apply asynchronously over RabbitMQ from the board) was never implemented; plan 023 made
  the board a plain HTTP client and applying synchronous through the API instead.
- Plan 023 first chose to redirect feeds to the API's `/p/{slug}/jobs.*`; re-checking the code showed
  the page fetches the feed same-origin, so the board proxies `feed.json|xml` instead.
- Plan 023 said the board had no Dockerfile and skipped the image build; `Dockerfile.web` arrived the
  same day in `b47c4369`.
- No test can prove "a key does not open the panel": in tests the default scheme authenticates every
  request (plan 023 notes). Only the other direction is tested.
