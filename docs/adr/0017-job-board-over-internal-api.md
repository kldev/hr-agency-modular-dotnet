# ADR-0017: The public job board has no database; it uses four internal API routes keyed by slug and a service API key

- **Status:** Accepted (supersedes the original design where `Web` registered "minimal" module variants)
- **Date:** 2026-09-22
- **Evidence:** `ae1f4542` feat(web): jobs feed view (razor pages) (the earlier, direct-database design),
  `e5ee1c7e` feat(identity): let the platform owner issue and revoke sk_ keys for programs, with their own auth scheme,
  `64d909d5` feat(api): open four internal board routes to a service key, for the public job board,
  `0e66353d` feat(web): read and write through the API's internal routes, and drop the minimal module variants,
  `60a85f12` feat(seeder): write a fixed service key for the public job board, so a seeded stack needs no manual step
- **Specs:** [008-public-job-board](../../.shipit/specs/done/008-public-job-board.md),
  [007-job-feeds](../../.shipit/specs/done/007-job-feeds.md),
  [002-identity-and-sign-in](../../.shipit/specs/done/002-identity-and-sign-in.md)

## Context
`HrAgencySystem.Web` (Razor Pages) started on 2026-09-06 as a second process on the same database,
registering trimmed module variants (`AddRecruitmentModuleMinimal`, `AddCompanyMinimalModule`,
`ConfigureMartenMinimal`). Every new event or projection in `Recruitment`/`Company` had to be checked
against the minimal variant; the public host held a database connection string with write rights;
one schema change meant two processes to deploy in order.

## Decision
- `Web` has no database access and no project references. It calls the API through
  `HttpJobBoardClient` on four routes:
  `GET /api/internal/boards/{slug}`, `GET …/feed.json|feed.xml`, `GET …/posts/{postSlug}`,
  `POST …/posts/{postSlug}/applications`. Keyed by slug, never by organization id.
- Authentication is a **service API key** (`ServiceApiKey` in `Identity`), header `X-Api-Key`, its own
  scheme `ApiKey`; `InternalApiPolicy` binds the internal routes to that scheme only. Keys belong to no
  organization, are issued and revoked by the platform owner, shown once (`sk_` + 64 hex), stored as
  a SHA-256 hash.
- Reads are retried (`AddStandardResilienceHandler`); the application POST is never retried.
- When the API is down the board shows an error page; there is no cache. `GET /healthz` on the board
  reports `api: UP|DOWN`.
- A seeded platform writes a fixed key so the docker stack serves the board without a manual step.

## Alternatives considered
From plan 023 (local, not in git):
- A second kind of JWT like `FileServiceTokenFactory` - rejected: the key is created by a person,
  lives long and must be revocable.
- Redirecting the feed to the API's `/p/{slug}/jobs.json` - rejected after re-reading the code: it
  would make the page's `fetch` cross-origin and need CORS on an anonymous route; the feed is proxied
  through the internal API instead.
- A response cache in `Web` - rejected; confirmed in `### uwagi do planu` that the public page may be
  temporarily down rather than show a post closed an hour ago.
- A key per organization, key rotation/expiry and rate limiting were listed as out of scope.

## Consequences
- Adding an event or projection never has to consider the job board.
- The career page now depends on API availability.
- An unpublished post answers 404 on the internal route; a 400 from the API is shown on the form.
- No test proves "a key does not open the panel": `TestAuthHandler` authenticates every request.

## Revisit when
- The board must stay up while the API is down (cache with an explicit lifetime).
- Third parties other than the board need keys (per-organization keys, rotation).
