# ADR-0002: Marten on PostgreSQL is both the event store and the document store; projections run in the async daemon

- **Status:** Accepted
- **Date:** 2026-08-30
- **Evidence:** `f1c00ced` start project, `a0e5f16c` feat: add company projection and GET endpoint, `bae5c535` feat(front): add wait for projection hook, `084a47a9` feat(feeds): add relational read model for job feed, `a37f9c4d` fix(recruitment): take the job post title from the aggregate, not the async projection, `3f18078f` fix(agency): read the org chart with FetchLatest
- **Specs:** [001](../../.shipit/specs/done/001-organizations-and-tenancy.md), [004](../../.shipit/specs/done/004-job-descriptions.md), [006](../../.shipit/specs/done/006-candidates-and-applications.md), [010](../../.shipit/specs/done/010-sales-opportunities.md), [011](../../.shipit/specs/done/011-agency-panel-foundation.md), [019](../../.shipit/specs/done/019-workers-and-assignments.md), [028](../../.shipit/specs/done/028-dynamic-forms.md)

## Context
The domain is full of lifecycles (job description, job post, application, opportunity, project,
worker, time sheet) where "what happened and when" is as important as the current state: status
histories, timelines and reports are all read from past events. The project also wanted one
storage engine for event-sourced aggregates and plain documents (tags, notes, reservations).

## Decision
- Marten is configured once in `SetupMartenExtensions.SetupMartenForApplication`: events in schema
  `events`, `StreamIdentity.AsGuid`, `AutoCreate.CreateOrUpdate` (no migrations, see
  [ADR-0025](0025-learning-project-no-migrations.md)), and each module adds its documents,
  events and projections through `<Module>Module.ConfigureMarten(StoreOptions)` into its own schema.
- Aggregates are classes with `Apply(Event)` methods; the decision is made in the handler
  ([ADR-0003](0003-wolverine-static-handlers.md)), the aggregate only rebuilds state.
- Data without history (tags, notes, contacts, reservation documents) is a plain Marten document.
- Read models are projections run by `AddAsyncDaemon(DaemonMode.HotCold)`, registered as async
  since the first projection (`a0e5f16c`: `Snapshot<CompanyProjection>(SnapshotLifecycle.Async)`).
  No projection in the code base is inline.
- Read models that other processes query with SQL are EF Core backed Marten projections into
  relational tables (`feeds.job_posts` from `084a47a9`, the `reports` schema) - see
  [ADR-0009](0009-feeds-own-read-model-and-worker.md) and [ADR-0018](0018-reports-service-timestamps.md).

## Alternatives considered
- For the feed read model, backend plan 002 (local, not in git) compared a JSONB projection, an EF
  Core projection into a table, and a Wolverine handler writing rows by hand. The handler was
  rejected because it loses rebuild and catch-up from the event stream.
- Inline projections were never used; no record of them being weighed.

## Consequences
- A read after a write can see the state before the write. This is handled in three places:
  `Eventually.AssertAsync` in integration tests, `useProjectionWait` in the panel (`bae5c535`), and
  reading the aggregate instead of the projection when a decision depends on fresh data
  (`a37f9c4d`, `3f18078f`, and `CompanySnapshotRepository` falling back to replaying the stream).
- A new projection replays the whole history, so a new read model (reports, feed table) covers
  data from before it existed without a backfill.
- Marten discovers conventional `Create`/`Apply` methods on a document type; an EF row type with
  such names refuses to start (`InvalidProjectionException`), so feed rows use `From`/`Update`.
- Index names over 63 characters fail at migration; long composite indexes are named by hand.
- `TrackEventCounters()` turns every appended event into a metric without extra code.

## Revisit when
1. A screen needs read-your-writes for most of its operations - then an inline projection for
   that read model is cheaper than more waiting code.
2. The product ever ships, which ends `AutoCreate.CreateOrUpdate` and brings event versioning.
