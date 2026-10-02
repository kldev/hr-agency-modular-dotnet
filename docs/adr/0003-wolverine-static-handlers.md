# ADR-0003: Wolverine is the in-process bus; handlers are static classes that return events and cascade messages through the Marten outbox

- **Status:** Accepted
- **Date:** 2026-08-30
- **Evidence:** `f1c00ced` start project, `c6f40287` chore: refactor project structure, `2c114b2e` feat(company): add recruitment integrations, `73678dec` feat(notifications): share email messaging topology and notify on handovers, `30860843` feat(teams): keep a person on at most one team and announce membership changes, `a0d7b1fd` fix(api): drop the fire-and-forget StartAsync on the wolverine host builder
- **Specs:** [001](../../.shipit/specs/done/001-organizations-and-tenancy.md), [012](../../.shipit/specs/done/012-email-notifications.md), [013](../../.shipit/specs/done/013-password-reset-and-sessions.md), [014](../../.shipit/specs/done/014-teams.md), [029](../../.shipit/specs/done/029-sales-workspace-and-tasks.md)

## Context
Every write is "validate, decide, append events, maybe tell somebody". The project wanted that
to be one transaction including any message leaving the process, and wanted handlers testable
without a container or a bus.

## Decision
- Wolverine (`UseWolverine`) discovers handlers in each module assembly
  (`options.Discovery.IncludeAssembly(typeof(XModule).Assembly)`), with
  `Policies.AutoApplyTransactions()` and Marten's `IntegrateWithWolverine()` outbox.
- A handler is a `public static class` named `*Handler` with a static `Handle`/`HandleAsync`.
  Dependencies (`IDocumentSession`, repositories, `IClock`) arrive as method parameters. No
  `IRequestHandler`-style interface.
- The handler returns the domain event; endpoints call `bus.InvokeAsync<TEvent>(command)` and use
  it as the response. Messages that must leave the handler (mail, integration events) go into an
  `OutgoingMessages` item of the returned tuple. Handlers never inject `IMessageBus` to publish.
- `[AggregateHandler]` binds a command to an existing stream (used since `f1c00ced`).
- One folder per use case: `Application/<UseCase>/` holds the command record and its handler
  (since `c6f40287`; before it, `Application/Commands` and `Application/Handlers`).
- A module that needs extra Wolverine wiring (sagas, durable local queues) exposes
  `XModule.ConfigureWolverine(options)`.

## Alternatives considered
- **A separate producer class listening to the domain event** and returning the integration
  event. That is how `Recruitment` -> `Company` was built in `2c114b2e`. The first version of
  `TeamMembershipChanged` copied it and never fired: under `InvokeAsync<T>` the returned `T` is
  the reply and is not cascaded. `30860843` moved the message into `OutgoingMessages`, which does
  cascade. The older `Recruitment` -> `Company` pair keeps the old shape; the company job post
  counters it feeds are checked end to end over HTTP by `CompanyJobPostCountTests` (added in `b0d70b31`).
- **`IMessageBus` / `BroadcastToTopicAsync` at the call site** for mail - rejected in backend plan
  004 (local, not in git) so that the topic belongs to the routing rule and the handler stays
  testable without a bus.

## Consequences
- Unit tests call `Handler.Handle(...)` directly with NSubstitute doubles and `FixedClock` and
  assert on the returned event and on `messages.OfType<T>()`.
- Generated handler code lives in another assembly, so anything Wolverine builds must be
  `public`; an `internal` repository makes it fall back to service location and throw
  `InvalidServiceLocationException` at runtime.
- Local queues are in memory by default. Every cross-module message and saga timeout is declared
  `LocalQueueFor<T>().UseDurableInbox()` (Identity, Teams, Sales modules).
- `SetupWolverineForApplication` ended with `UseWolverine(...).StartAsync()` from `f1c00ced`; the
  unobserved task was removed in `a0d7b1fd` after `.docs/05` flagged it.
- Integration tests stub the external transports (`a4ef2e0f`), so they need no broker.

## Revisit when
1. Cross-module integration events need delivery guarantees beyond one process (durable inbox
   on local queues is the current answer).
2. A handler needs to call another module synchronously for a decision - the signal that the
   boundary or a port is wrong.
