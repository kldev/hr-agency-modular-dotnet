# 017 · My profile

**Status:** done · **Built:** 2026-09-20 → 2026-09-22 · **Verify:** `./verify identity` · **Plan:** backend 011 (local, not in git)

## Why
A signed-in member had nowhere to see or correct their own details: the "Profile" entry in the user
menu existed and did nothing, and the password change endpoint had no screen calling it. People also
wanted a picture next to their name instead of initials.

## What it promises
- A member opens `/app/profile` and sees their own details (`GET /api/users/me`).
- They can edit their first name, last name, job title and phone. The e-mail address is not part of
  the command and cannot be changed here; the address the account already has is kept. A profile
  without a name is refused. What they type is trimmed.
- They can change their password on the same page (see [013](013-password-reset-and-sessions.md)).
- They can upload a picture: PNG or JPEG, at most 512 KB; anything else, an empty file or one byte
  over is refused (both faults reported at once). Replacing a picture deletes the old file; removing
  a picture that is not there is refused.
- The picture can be read back immediately (no projection lag) and is served by the HR API, so a
  plain `<img>` authenticates through the panel's proxy; `?v={fileId}` busts the cache after a change.
- The top bar reads the same profile query, so a new name or picture shows without signing in again.
- Another person's or another organization's picture is simply not there for a regular member.

## Surface
- Backend: `src/HrAgencySystem.Identity` (`Documents/UserProfile`, `Application/Users/UpdateOwnProfile`,
  `Application/Users/Avatar/{Change,Remove}`, `Application/Policy/AvatarUploadPolicy`).
- API: `GET|PUT /api/users/me`, `POST|GET|DELETE /api/users/me/avatar`, `PUT /api/users/me/password`.
- Frontend: `frontend/src/features/profile` (`ProfilePage`, `ProfileForm`, `PasswordForm`,
  `AvatarUploader`), route `/app/profile`, top bar in `frontend/src/components/layout/top-bar`.
- Tests: `tests/HrAgencySystem.UnitTests/Identity/{Handlers,Policy}`,
  `tests/HrAgencySystem.IntegrationTests.Platform/Users/{MyProfileTests,AvatarTests}.cs`.

## Out of scope for this feature
- Changing your own e-mail address (not ever on this screen - it is the sign-in identity and the
  claim may be stale).
- Cropping or resizing pictures (not yet).
- Pictures in lists and an administrator setting somebody else's picture came later, in
  [023](023-impersonation-and-avatars.md).

## Acceptance criteria
- [x] Profile answers with the signed-in person; editing changes contact data, keeps the phone and the address — `./verify identity`
- [x] Blank names refused; input trimmed; foreign organization refused — `./verify identity`
- [x] Upload, read back immediately, replace (reports the displaced file), remove; removing nothing refused — `./verify identity`
- [x] Over 512 KB, empty, not PNG/JPEG refused — `./verify identity`
- [x] Another person's / organization's picture is not there — `./verify identity`
- [x] Own password change — `./verify identity`
- [ ] Profile page, uploader and top bar refresh in the panel — unproven (no test found)

## Decisions
- The picture is stored by the file service under a `FileId`:
  [ADR-0012](../../../docs/adr/0012-separate-file-service.md).
- Thin endpoints; deleting the replaced file happens in the endpoint after the command succeeds
  ("first the record, then the bytes"): [ADR-0008](../../../docs/adr/0008-endpoint-per-file-thin-api.md).
- The profile is a plain Marten document (`UserProfile`, id = user id), not an event-sourced stream -
  reads are immediate and the `User` aggregate does not grow fields no rule uses.
- `UpdateOwnProfile` has no `Email` field, so the guarantee is in the type, not in route rules; it
  reuses the existing `UserUpdated` event.
- Picture rules live in the HR API (`AvatarUploadPolicy`), not in the file service, whose limits are
  global for every owner kind; the service's 25 MB stays as a backstop.
- `GET /api/user/me` (session, claims only) and `GET /api/users/me` (display data) are kept apart.

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-19 | `68d5c61d` | Own password change endpoint (no screen yet) |
| 2026-09-20 | `234be976` | Shared contact and password fields across user forms |
| 2026-09-20 | `a873a2f9` | My profile with avatar upload and password change |
| 2026-09-22 | `ae052ada` | `Avatar` primitive that can carry a picture |
| 2026-09-22 | `2587f15c` | Success toasts that repeat the screen dropped |

## Notes from reconstruction
- Plan 011 found that `UserProjection.Apply(UserUpdated)` dropped `Phone`; the profile form would have
  exposed it at once. Fixed in `a873a2f9` (test `Editing_the_profile_keeps_the_phone_it_was_given`).
- A cascading message with a durable queue for deleting the old file was rejected: it would have
  needed the first Wolverine handler inside the API host for a 512 KB clean-up.
