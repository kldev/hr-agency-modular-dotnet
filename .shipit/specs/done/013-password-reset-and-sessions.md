# 013 · Password reset and sessions

**Status:** done · **Built:** 2026-09-19 → 2026-09-22 · **Verify:** `./verify identity` · **Plan:** backend 005; frontend 007, 008 (local, not in git)

## Why
Members of an agency stay signed in for weeks, forget passwords, and sometimes need every session
killed at once. Before this work the panel held one long-lived access token, "forgot password" was a
fake screen, and an expired session left an empty app shell instead of the sign-in page.

## What it promises
- A signed-in member can change their own password by giving the current one; a wrong current
  password, the same password again or one below the policy minimum is refused. A change ends every
  open session of that person.
- Anybody can ask for a reset link by e-mail address. Known and unknown addresses get the same
  answer, and nothing is looked up for a domain that may not sign in.
- The link is valid for `Identity:PasswordResetExpiresInMinutes` (default 15). Only the hash of the
  token is stored. A forged or late token changes nothing; a successful reset closes the window and
  revokes every refresh token of that person.
- Signing in returns a short access token plus a refresh token. Refreshing rotates the refresh token
  without moving its absolute expiry (counted from sign-in).
- A refresh token presented a second time revokes its whole session family; an unknown token is
  refused without revoking anything. Logout revokes the family and accepts a token that is already gone.
- The panel keeps the session in httpOnly cookies, renews it 60 s before the access token dies,
  shares one exchange between parallel server functions, and sends `/app` and `/admin` to `/login`
  when the session is gone.

## Surface
- Backend: `src/HrAgencySystem.Identity/Sagas/PasswordResetSaga.cs` (+ `PasswordResetMessages.cs`,
  durable local queues in `IdentityModule.ConfigureWolverine`),
  `Application/Users/{ChangePassword,RequestPasswordReset,Refresh}`, `Infrastructure/Persistence/RefreshToken*`.
- API: `PUT /api/users/me/password`, `POST /api/auth/password-reset`,
  `POST /api/auth/password-reset/confirm`, `POST /api/auth/refresh`, `POST /api/auth/logout`
  (the last four anonymous).
- Frontend: `frontend/src/server/session.ts`, `frontend/src/routes/{login,forgot-password,reset-password}.tsx`,
  `beforeLoad` guards on `/app` and `/admin`.
- Tests: `tests/HrAgencySystem.UnitTests/Identity/{Sagas,Handlers,Policy}`,
  `tests/HrAgencySystem.IntegrationTests.Platform/{Auth,Users}`.

## Out of scope for this feature
- The platform owner has no refresh token; the owner panel keeps a 6 h token and signs in again (by choice).
- Rotation is not compare-and-swap: two parallel refreshes with the same live token both succeed (accepted risk, plan 005).
- Rate limiting of sign-in and reset requests (not yet).

## Acceptance criteria
- [x] Reset saga: start mails and schedules the timeout; timeout completes; right token changes the password; forged or late token changes nothing — `./verify identity`
- [x] Reset request: unknown address and disallowed domain start nothing; only the token hash is kept — `./verify identity`
- [x] Refresh: rotation keeps expiry, replay and expiry revoke the family, unknown token revokes nothing, revocation is committed before rejecting — `./verify identity`
- [x] Logout ends the session — `./verify identity`
- [x] Own password change: wrong current, same, below policy, other organization refused; sessions ended — `./verify identity`
- [x] Sign-in lands in the panel — e2e: `frontend/e2e/auth/login.spec.ts`
- [ ] Reset end to end through Wolverine (saga identity, durable timeout) — unproven (no test found)
- [ ] Expired session redirects to `/login`; renewal shared across server functions — unproven (no test found; checked live per plan 008)
- [ ] Reset screens (`/forgot-password`, `/reset-password`) — unproven (no test found)

## Decisions
- Reset window as a Wolverine saga; the window closing is the saga document ceasing to exist:
  [ADR-0011](../../../docs/adr/0011-sagas-for-time-windows.md).
- Auth endpoints in the thin endpoint style, `/refresh` and `/logout` anonymous because the refresh
  token is the credential: [ADR-0008](../../../docs/adr/0008-endpoint-per-file-thin-api.md).
- Rotation marks the spent token instead of deleting it - a deleted token cannot tell that it was replayed (plan 005).
- One session family per sign-in, so a replay on one device does not sign out another (plan 005).
- `src/server/session.ts` is the only owner of cookies, because the proxy and server functions are
  two independent paths that would otherwise drift (plan 008).

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-19 | `68d5c61d` | Own password change |
| 2026-09-19 | `55292e69` | Password reset as a Wolverine saga |
| 2026-09-19 | `63665059` | Password reset screens |
| 2026-09-19 | `3e2f19de` | Password policy message aligned with the enforced minimum |
| 2026-09-19 | `c130c01c` | Refresh tokens with rotation and reuse detection |
| 2026-09-19 | `cd45cedd` | Session on a refresh token, guarded app routes |
| 2026-09-22 | `ae052ada` | A session that is one token (for impersonation) |

## Notes from reconstruction
- Plan 008 lists three things that pretended to work: an axios interceptor threw `redirect()` that
  the router never caught, a failed sign-in still navigated to the dashboard, and auth hooks that
  were never called. All three were fixed in `cd45cedd`.
- The guard adds one `/api/user/me` call per navigation inside `/app` (accepted cost, plan 008).
- The saga test covers the class; the `[property: SagaIdentity]` wiring it depends on is only
  exercised when the app runs.
