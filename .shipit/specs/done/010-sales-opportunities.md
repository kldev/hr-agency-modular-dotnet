# 010 · Sales opportunities

**Status:** done · **Built:** 2026-09-08 → 2026-09-24 · **Verify:** `./verify sales` · **Plan:** frontend 005, 006 (local, not in git); backend none

## Why
A sales person needs one place to track a deal with a client company from first contact to won or
lost: who owns it, what it is worth, what was said and what has to happen next. Before this module
nothing in the system said which clients were being sold to or how much the pipeline was worth.

## What it promises
- A sales person can create an opportunity for a client company with a title, description,
  expected value, currency, optional expected close date and a hot lead flag. It starts at `New`.
- The responsible person defaults to the creator; another member can be named instead.
- An opportunity moves through `New, Viewed, Contacted, Qualified, Proposal, Won, Lost`. Moving it
  to the stage it already has is refused ("Opportunity is already at this stage"); moving it to
  `Lost` takes a lost reason.
- Handing an opportunity to somebody else (on create or later) sends that person an email; taking it
  over yourself sends nothing.
- Activities (call, email, meeting, note, presentation, other) are logged against an opportunity and
  listed by opportunity or by company, newest first. A task of the deal ticked off in the tasks
  module appears as a `Task` activity, once per completion.
- Follow-up actions (content + date and time) can be added and edited; the opportunity shows the
  one with the latest follow-up date.
- Pipeline totals are kept per stage and per currency (a value in EUR is never added to PLN), and
  per responsible person.
- Everything is scoped to the caller's organization; another organization's opportunities,
  activities and follow-ups are invisible.
- The panel shows the pipeline as a table (search, stage filter, "only mine") or as a kanban with
  one column per stage and per-currency metrics, and an opportunity details page with the pipeline
  stepper, activity timeline, next action panel and company contacts.

## Surface
- Backend: `src/HrAgencySystem.Sales` (aggregates `SalesOpportunity`, `SalesActivity`; document
  `FollowUpAction`; `PipelineProjection`); `Integration/OpportunityTaskCompletedHandler`.
- API: `POST|GET /api/sales/opportunity`, `GET|PUT /api/sales/opportunity/{id}`,
  `PUT .../{id}/stage`, `PUT .../{id}/responsible`, `GET .../totals`, `GET .../totals-responsible`,
  `POST /api/sales/activity`, `GET /api/sales/activities`, `POST|GET /api/sales/follow-up`,
  `GET|PUT /api/sales/follow-up/{id}`. Demo data: `GET /api/development/seed-sales?count=N`.
- Frontend: `frontend/src/features/sales`, routes `/app/sales` (`?view=kanban`) and
  `/app/sales/opportunities/$id`; generic board in `frontend/src/components/kanban`.
- Tests: `tests/HrAgencySystem.UnitTests/Sales`, `tests/HrAgencySystem.IntegrationTests.Sales/Sales*`.

## Out of scope for this feature
- A won opportunity does not create a project (not yet). A project may point back at the deal it was
  sold as (`f495bb91`), but `Sales` publishes no "won" event.
- No stage transition policy: any stage can follow any other except itself (not decided).
- Drag and drop on the sales kanban - plan 006 stage 7 was never started (not yet).
- Deleting an opportunity, activity or follow-up (no endpoint).

## Acceptance criteria
- [x] Create with defaults (responsible = creator), validation of title and value, tenant checks — `./verify sales`
- [x] Update, change responsible (same person refused), change stage — `./verify sales`
- [x] Handover rule: mail only when the responsible person is somebody else — `./verify sales`
- [x] Activities scoped by organization, filtered by opportunity/company, ordered by date — `./verify sales`
- [x] A done task logs exactly one `Task` activity per completion, repeats write nothing — `./verify sales`
- [x] Follow-ups: create, edit, tenant isolation, latest follow-up tracked on the opportunity — `./verify sales`
- [x] Pipeline buckets move with stage and currency changes — `./verify sales`
- [x] Table, kanban view and details page render seeded data — e2e: `frontend/e2e/sales/sales.spec.ts`
- [x] Screens — evidence: `docs/screenshots/sales.png`, `docs/screenshots/sales-kanban.png`, `docs/screenshots/sales-opportunity.png`
- [ ] Moving to the stage it already has is refused — unproven (no test found)
- [ ] Moving to `Lost` without a reason is refused — unproven (no test found)
- [ ] Hot lead flag round-trips — unproven (no test found)

## Decisions
- Opportunity and activity are event-sourced streams: [ADR-0002](../../../docs/adr/0002-marten-event-store.md).
- Static handlers, `[AggregateHandler]` for stage/responsible changes: [ADR-0003](../../../docs/adr/0003-wolverine-static-handlers.md).
- Every read filtered by `OrganizationId`: [ADR-0004](../../../docs/adr/0004-tenant-on-every-aggregate.md).
- `OpportunityTitle` value object with `TryCreate`: [ADR-0007](../../../docs/adr/0007-value-objects-trycreate.md).
- Handover mails over RabbitMQ: [ADR-0010](../../../docs/adr/0010-email-over-rabbitmq.md).
- Follow-up action is a plain Marten document, not an event-sourced aggregate - it has no lifecycle,
  only content and a date.
- Totals are per currency - a single sum would mix PLN and EUR (plan 006 decision 2).
- The kanban base became a domain-free `Kanban` compound component, reused later by workers and
  applications boards (plan 006 notes, `fc4af8d6`).

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-08 | `b0e10afe` | Sales domain |
| 2026-09-08 | `7ee93dae` | Pipeline stage summary projection |
| 2026-09-09 | `912d3224` | Create opportunity handler with tests |
| 2026-09-09 | `a9de170f` | Hot lead flag |
| 2026-09-09 | `9ab6651a` | Change stage, change responsible person |
| 2026-09-11 | `513233b2` | Sales table in the panel |
| 2026-09-15 | `70cd33f5` | Create opportunity drawer |
| 2026-09-18 | `77ece5d5` | Follow-up actions on an opportunity |
| 2026-09-18 | `b4a2b503` | Opportunity details page (plan 005) |
| 2026-09-18 | `ce42f9c7` | Table/kanban switch (plan 006) |
| 2026-09-18 | `ce2a8f12` | Kanban layout and a11y polish |
| 2026-09-19 | `73678dec` | Handover mails for opportunities |
| 2026-09-24 | `cc99a125` | Opportunity snapshot port, task activity, last activity |

## Notes from reconstruction
- Stage changes have no policy object, unlike `ProjectStatusChangePolicy` - `Won` can go back to `New`.
- The details page and kanban were finished in one day (2026-09-18); plan 006 records the kanban as
  "not checked visually" at that time. The e2e suite (`ccd48368`) later covered it.
- CLAUDE.md says nothing links a project to an opportunity; since `f495bb91` a project can carry an
  optional, validated link to a deal of the same company.
