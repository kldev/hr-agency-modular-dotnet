# ADR-0011: A process with a time window is a Wolverine saga whose document is deleted when the window closes; its timeout travels through a durable local queue

- **Status:** Accepted
- **Date:** 2026-09-19
- **Evidence:** `55292e69` feat(identity): add password reset as a Wolverine saga, `c130c01c` feat(identity): add refresh tokens with rotation and reuse detection, `30860843` feat(teams): keep a person on at most one team and announce membership changes, `22abb903` refactor: remove dead members and unused parameters, mark what only reflection reaches
- **Specs:** [013](../../.shipit/specs/done/013-password-reset-and-sessions.md), [012](../../.shipit/specs/done/012-email-notifications.md)

## Context
A password reset link must work for a limited time (default 15 minutes,
`Identity:PasswordResetExpiresInMinutes`) and only once. Storing an expiry column means every
reader has to remember to check it; a forgotten check keeps a link alive forever. The mail
contract `SendPasswordReset` existed since the mail work but nothing produced it (backend plan
004, local, not in git).

## Decision
- `Identity/Sagas/PasswordResetSaga.cs` is a Wolverine `Saga` stored as a Marten document
  (registered in `IdentityDocumentConfiguration`).
- `RequestPasswordResetHandler` answers the same for a known and an unknown e-mail, and for a
  known one sends `StartPasswordReset`. The saga's `Start` stores only the token hash, cascades
  the `SendPasswordReset` mail and a `PasswordResetExpired` `TimeoutMessage` for the window.
- `Handle(PasswordResetExpired)` calls `MarkCompleted()`; Wolverine deletes the document, and the
  link stops working because the saga no longer exists.
- `Handle(CompletePasswordReset)` re-checks `ExpiresAt` (a redelivered timeout can race the
  redemption), compares hashes, sets the password through the e-mail reservation, revokes every
  refresh token of the user, appends `PasswordChanged`, and calls `MarkCompleted()`.
- Every message that must find the saga marks its id with `[property: SagaIdentity]`.
- `IdentityModule.ConfigureWolverine` declares `LocalQueueFor<StartPasswordReset>()` and
  `LocalQueueFor<PasswordResetExpired>()` as `UseDurableInbox()`. It is the fourth wiring place
  next to the three in [ADR-0001](0001-modular-monolith.md).

## Alternatives considered
- **An expiry field checked on redemption** - the saga keeps that check as a second line
  (`ExpiresAt`), but the primary mechanism is deletion. No other variant is recorded.
- **Refresh tokens** (`c130c01c`) are not a saga: they are documents with a hash, rotation and
  reuse detection, because their lifetime is days and nothing has to happen when one expires.

## Consequences
- The saga is the pattern to copy; it is still the only one in the code base.
- Without `[SagaIdentity]` Wolverine throws `IndeterminateSagaStateIdException` at runtime and the
  message dead-letters; a unit test of the saga class cannot catch that.
- Local queues are in memory by default; without the durable inbox a restart drops the timeout
  and the saga stays forever. The same rule was later applied to cross-module messages
  (`TeamMembershipChanged`, `AssignUserToTeam`, `OpportunityTaskCompleted`).
- Scheduled messages live in the shared `messages.wolverine_incoming_envelopes` table, so every
  node connected to the database competes for them; an older API node can claim a timeout it has
  no handler for.
- Unit tests drive `Start` and `Handle` directly with `FixedClock` (`PasswordResetSagaTests`).

## Revisit when
1. A second time-boxed process appears (an invitation link, a form filled through a token -
   plan 028 lists one) - reuse this shape rather than an expiry column.
2. Several API versions must run against one database at the same time.
