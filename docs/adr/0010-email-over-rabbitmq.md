# ADR-0010: Mail leaves the API as messages on a RabbitMQ topic exchange and is rendered and sent by `NotificationWorker`, at least once with an idempotency claim

- **Status:** Accepted
- **Date:** 2026-09-19
- **Evidence:** `962bb8e3` feat(email-templates): add liquid email templates, `0666512f` feat(notifications): send job application emails over rabbitmq to NotificationWorker, `73678dec` feat(notifications): share email messaging topology and notify on handovers, `7e182721` feat(email): deliver rendered mails through MailKit behind ISendEmail, `3b3d2e6f` feat(notifications): skip mails for events already processed, `041b7ece` feat(emails): add retry policy
- **Specs:** [012](../../.shipit/specs/done/012-email-notifications.md), [013](../../.shipit/specs/done/013-password-reset-and-sessions.md), [014](../../.shipit/specs/done/014-teams.md), [022](../../.shipit/specs/done/022-time-sheets-and-settlement.md)

## Context
Recruiters, sales people and HR need to hear about handovers, new applications and their
monthly time sheets. Sending SMTP inside a command would make a slow or failing mail server fail
the business operation, and a retried command could send the same mail twice.

## Decision
- Messages are records in `EmailTemplates.Contracts` implementing `IEmailTemplateContract`
  (`EventId`, `Source`). A handler returns them in `OutgoingMessages`
  ([ADR-0003](0003-wolverine-static-handlers.md)); the Marten outbox sends them after commit.
- The whole topology lives in `EmailTemplates.Messaging`: exchange `x.emails` (topic), a routing
  key per message type in `EmailTopics`, one durable queue per source domain in `EmailQueues`
  (`recruitment`, `identity`, `sales`, `teams`, `agency`). The API calls `PublishEmailMessages`
  (declares only the exchange, `UseDurableOutbox()`); the worker calls `ConsumeEmailMessages`
  (declares queues and bindings, `ListenerCount(3)`).
- `NotificationWorker` (`src/services/`) renders embedded liquid through `IEmailTemplateProvider`
  and sends through `ISendEmail` (MailKit when `MailProvider=mailkit`, otherwise a logging sender).
- Each handler wraps render and send in `IProcessedEventStore.SendOnceAsync`: the claim in
  `notifications.processed_events` (`event_id` primary key) is taken before sending and released
  if the send throws.
- `EmailFailurePolicies`: malformed address and SMTP 5xx go straight to the dead letter queue;
  transient failures get cooldown retries of 1 s, 5 s, 15 s.
- A handover mail goes out only when the new assignee is not the person making the change.

## Alternatives considered
- **At-most-once**: the first idempotency guard (`3b3d2e6f`) marked the event before sending and
  kept the mark on failure - recorded in backend plan 004 (local, not in git) as a conscious
  cost. `041b7ece` ("change from at-most-once to at-least-once") added the release on failure
  and the retry policy.
- **Several queues bound with the same key** - an earlier variant; plan 004 records 110
  applications producing 330 handler calls. Replaced by one queue per domain, scaled by listeners.
- **Topic names built with `nameof(...)` at call sites, or `BroadcastToTopicAsync`** - rejected
  in plan 004: renaming a record would silently break the wire contract.
- **FluentEmail sending the mail** - it only renders; MailKit sends (`7e182721`).

## Consequences
- A broken SMTP server delays mail but never fails a command; the outbox keeps messages while
  the broker is down.
- Adding a mail touches five places (contract, topic, publish rule, template, worker handler);
  skipping the handler gives `No known handler` and a dead letter, skipping the template renders
  an empty body. Render tests in `tests/HrAgencySystem.EmailTemplates.UnitTests` guard the latter.
- The consumer must not use `UseListenerConnectionOnly()`; changing the exchange type on an
  existing broker fails with `PRECONDITION_FAILED (406)`.
- Integration tests stub the RabbitMQ transport (`a4ef2e0f`); the transport is checked by hand
  and by the k6 `--emails` script.

## Revisit when
1. Mail goes to candidates (external recipients) - bounces and consent need their own handling.
2. A second kind of outgoing channel (SMS, push) appears - then the topology may need a
   generic notification exchange instead of `x.emails`.
