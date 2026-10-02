# 012 · Email notifications

**Status:** done · **Built:** 2026-09-19 → 2026-09-22 · **Verify:** `./verify emails` · **Plan:** backend 004 (local, not in git)

## Why
People need to hear about work that lands on them: a recruiter when a candidate applies to their
post, a sales person when a deal is handed to them, a member when they join a team, an employee
when their month of hours is approved. Sending mail inside the API request would tie a user's click
to an SMTP server; mail has to leave reliably even when the mail server or broker is briefly down.

## What it promises
- A handler that decides a mail is due returns it as a message; it leaves through the Marten outbox
  to the RabbitMQ topic exchange `x.emails`, routed by a per-message topic.
- One durable queue per source domain (`recruitment`, `identity`, `sales`, `teams`, `agency`), each
  consumed by `NotificationWorker` with three listeners, so a backlog in one domain does not stall
  another and no mail is sent twice by a second binding.
- Mails sent today: application created (to the post's recruiter), job post recruiter changed,
  opportunity created / responsible changed, password reset link, team member added / role changed,
  time sheet approved / returned for correction / settled.
- Handover rule: a mail about an assignment goes out only when the assignee is somebody other than
  the person who made the change.
- A redelivered message does not send the same mail twice (claim in `notifications.processed_events`
  taken before the send); a failed send releases the claim so the retry really sends.
- Transient failures (SMTP, socket, IO, timeout, Npgsql) are retried after 1 s, 5 s and 15 s; a
  malformed address or a 5xx SMTP refusal goes straight to the dead letter queue.
- Without `MailProvider=mailkit` the worker logs mails instead of sending them; locally Mailpit
  receives them on :1025 (UI :8025).

## Surface
- Backend: `src/EmailTemplates/HrAgencySystem.EmailTemplates.Contracts` (messages),
  `src/EmailTemplates/HrAgencySystem.EmailTemplates` (liquid templates, `IEmailTemplateProvider`,
  `ISendEmail`, MailKit), `src/EmailTemplates/HrAgencySystem.EmailTemplates.Messaging`
  (`EmailTopics`, `EmailQueues`, `PublishEmailMessages`, `ConsumeEmailMessages`),
  `src/services/HrAgencySystem.NotificationWorker` (`Application/*Handler.cs`,
  `Infrastructure/EmailFailurePolicies.cs`, `ProcessedEventStore`). Host image `Dockerfile.notification-worker`.
- Tests: `tests/HrAgencySystem.EmailTemplates.UnitTests`, `tests/HrAgencySystem.UnitTests/Notifications`,
  handover tests in `UnitTests/{Sales,JobPostings,Teams}/Handlers`.

## Out of scope for this feature
- No mail to candidates - every notification is internal (not yet).
- No mail when somebody loses a responsibility (removed from a team, deal taken away) - by design.
- No reminder mails for expiring documents (not yet; plan 009 left it open).
- No monitoring of the dead letter queue (not yet).

## Acceptance criteria
- [x] Every template renders its data with no unresolved liquid and escapes candidate input — `./verify emails`
- [x] `MailProvider` switch picks MailKit or the logging sender; MailKit without a host is refused — `./verify emails`
- [x] Claimed event is not sent again; a failed send releases the claim — `./verify emails`
- [x] Handover rule for opportunities — `./verify sales`
- [x] Handover rule for job post recruiter change — `./verify job-posts`
- [x] Handover rule for team membership and role change — `./verify teams`
- [ ] Retry and dead-letter policy behaves as declared — unproven (no test found)
- [ ] Topology end to end (API → RabbitMQ → worker → SMTP) — unproven (no test found; checked by hand per plan 004)

## Decisions
- Mail as messages over a topic exchange, worker host, at-least-once with an idempotency claim:
  [ADR-0010](../../../docs/adr/0010-email-over-rabbitmq.md).
- Producers return `OutgoingMessages` (cascading), never inject `IMessageBus`:
  [ADR-0003](../../../docs/adr/0003-wolverine-static-handlers.md).
- Mail contracts live in their own `*.Contracts` project with no dependencies:
  [ADR-0006](../../../docs/adr/0006-contracts-and-shared-kernel-ports.md).
- Topics are constants, never `nameof(...)` at a call site - renaming a record would silently cut
  subscribers off (plan 004, decision 2).
- Two contracts for "member added" and "role changed" rather than one with a nullable field - liquid
  renders an unknown variable as empty instead of failing (plan 006).
- FluentEmail only renders; MailKit sends behind `ISendEmail`.

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-19 | `962bb8e3` | Liquid templates for applications and password reset |
| 2026-09-19 | `e58232b1` | RabbitMQ in compose |
| 2026-09-19 | `0666512f` | Application mails over RabbitMQ to `NotificationWorker` |
| 2026-09-19 | `73678dec` | Shared messaging topology, handover mails |
| 2026-09-19 | `7e182721` | MailKit behind `ISendEmail` |
| 2026-09-19 | `3b3d2e6f` | Skip events already processed |
| 2026-09-19 | `041b7ece` | Retry policy |
| 2026-09-19 | `aea298f9` | Team membership mail on its own topic and queue |
| 2026-09-20 | `30860843` | Claim released when the send fails |
| 2026-09-22 | `f9d3dde3` | Time sheet decision mails, `agency` queue |
| 2026-09-25 | `4064aedd` | Mail metrics |

## Notes from reconstruction
- Plan 004 chose at-most-once (claim kept even if SMTP fails). `30860843` reversed it: the claim is
  released on failure, making delivery at-least-once.
- Plan 004 measured the earlier topology (three queues bound to the same key): 110 applications gave
  330 handler calls. That is why a queue is a subscriber, not a unit of scale.
- Plan 004 says the worker was not in any compose file and `SendPasswordReset` had no producer; both
  were closed the same day (`ae1bf180`, `55292e69`).
