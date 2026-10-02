# PRODUCT

The single page that says what this product promises, what it will not do, which decisions hold it together and where
it stands. Written on 2026-10-02 by reverse-engineering 603 commits (2026-08-30 → 2026-10-02); from here on it is
kept current by hand, in the spirit of
[spec-driven development for solo developers](https://www.danvega.dev/blog/spec-driven-development-solo-developers).

- Feature specs: [`.shipit/specs/done/`](.shipit/specs/done/README.md) - one per feature, with how each promise is proven
- Decisions: [`docs/adr/`](docs/adr/README.md) - architecture decision records
- Known gaps: [`.shipit/open.md`](.shipit/open.md) · lessons: [`.shipit/retro/findings.md`](.shipit/retro/findings.md)
- Proof: `./verify <feature>` runs the tests behind one feature, `./verify list` names them, `./verify all` runs everything

## What it proves

A multi-tenant SaaS for recruitment agencies hiring for IT roles, built as a learning project where the quality of the
model is the point. It proves eight things:

1. **A modular monolith holds a real domain.** Thirteen business modules, one deployable API, no business module
   references another. *Done when:* the project graph has no module-to-module reference (true on 2026-10-02: modules
   reference only `SharedKernel`, `*.Contracts` and the shared libraries `Compliance`, `Feeds`, `Files`,
   `Reports.ReadModel`) and modules talk only through `*.Contracts` events and `SharedKernel` ports ([ADR-0001](docs/adr/0001-modular-monolith.md), [ADR-0006](docs/adr/0006-contracts-and-shared-kernel-ports.md)).
2. **Event sourcing on Marten + Wolverine carries the whole write side.** *Done when:* every aggregate is a stream,
   every screen reads a projection, and the lag between them is handled explicitly in the API, the tests and the
   front end ([ADR-0002](docs/adr/0002-marten-event-store.md), [ADR-0003](docs/adr/0003-wolverine-static-handlers.md)).
3. **One agency never sees another.** *Done when:* every aggregate, projection, index and file carries
   `OrganizationId`, and a cross-tenant read answers 404 or 403 - `./verify organizations`, `./verify files`
   ([ADR-0004](docs/adr/0004-tenant-on-every-aggregate.md)).
4. **Recruitment for a client works end to end.** Sale → job description → job posts → candidates → applications →
   interviews, with a public job board and job feeds. *Done when:* specs [004](.shipit/specs/done/004-job-descriptions.md)-[010](.shipit/specs/done/010-sales-opportunities.md) verify.
5. **Delivering the sold service is modelled with the law in it.** Projects with a signed contract, people posted
   through assignments, per-country compliance as data. *Done when:* specs [016](.shipit/specs/done/016-projects-and-compliance.md), [019](.shipit/specs/done/019-workers-and-assignments.md), [020](.shipit/specs/done/020-positions.md) verify.
6. **The agency as an employer is a separate axis.** Its own chart, contracts and monthly time sheets, settled to
   payroll. *Done when:* specs [021](.shipit/specs/done/021-agency-chart-and-roles.md), [022](.shipit/specs/done/022-time-sheets-and-settlement.md) verify ([ADR-0015](docs/adr/0015-agency-separate-from-workers.md)).
7. **Side processes leave the API only where they earn it.** Mail, files, feeds and reports run as their own hosts.
   *Done when:* specs [007](.shipit/specs/done/007-job-feeds.md), [012](.shipit/specs/done/012-email-notifications.md), [015](.shipit/specs/done/015-file-service.md), [027](.shipit/specs/done/027-reports.md) verify and `docker compose` starts every host.
8. **It can be operated.** Logs, traces and metrics from every host, options validated at startup. *Done when:* specs [030](.shipit/specs/done/030-observability.md) and [033](.shipit/specs/done/033-quality-gates-and-startup-validation.md) verify.

## Non-goals

**Not ever**

- Production. No data migration, no event versioning, no upcasters ([ADR-0025](docs/adr/0025-learning-project-no-migrations.md)).
- Splitting business modules into services. The file service is the one exception, decided knowingly - not a precedent.
- Replacing Marten or Wolverine. The stack is the subject of the exercise, not a variable.
- Rules enforced only in the UI. A rule lives in the aggregate or nowhere.
- Money in the domain. Hours are agreed and handed to payroll; only the settlement export multiplies them by a rate.

**Not yet**

- Publishing to job boards (`PostToChannel` records a publication, it does not publish).
- Leave requests and absences for the agency's own people.
- A won opportunity creating the project by itself (a project can point at its opportunity, it is created by hand).
- Audit as its own module (`HrAgencySystem.Audit` is an empty project).
- See [`.shipit/open.md`](.shipit/open.md) for the full list.

## Decisions

The ones that shape everything else. Each ADR carries its own "revisit when".

| Decision | Revisit when |
|---|---|
| [Modular monolith](docs/adr/0001-modular-monolith.md) | a module needs to scale or ship on its own schedule |
| [Marten event store](docs/adr/0002-marten-event-store.md) + [Wolverine static handlers](docs/adr/0003-wolverine-static-handlers.md) | never, by the non-goals - the stack is the subject |
| [Tenant on every aggregate](docs/adr/0004-tenant-on-every-aggregate.md) | a tenant needs its own database |
| [Uniqueness through reservations](docs/adr/0005-uniqueness-reservations.md) | a rule cannot be expressed as one key (date overlaps already cannot) |
| [Contracts and SharedKernel ports](docs/adr/0006-contracts-and-shared-kernel-ports.md) | `SharedKernel` grows beyond genuinely shared concepts |
| [Email over RabbitMQ](docs/adr/0010-email-over-rabbitmq.md), [separate file service](docs/adr/0012-separate-file-service.md), [reports service](docs/adr/0018-reports-service-timestamps.md) | the API has to run without a second process |
| [Contract on the project stream](docs/adr/0013-contract-on-project-stream.md), [Worker vs Assignment](docs/adr/0014-workers-one-module-two-aggregates.md) | a contract needs a lifecycle of its own |
| [Integration tests per area](docs/adr/0021-integration-tests-per-area.md), [quality gates](docs/adr/0024-quality-gates.md) | the suite or the inspection gets slow enough to be skipped |

All 25: [docs/adr/README.md](docs/adr/README.md).

## Stops

Points where work halts and waits for the owner of the project:

- Before pushing to the remote, and before anything that changes a remote's visibility.
- Before the first line of a new feature: a spec in `.shipit/specs/` with its acceptance criteria and their `./verify` names.
- When the spec and the code disagree on a business rule - the rule is a business decision, not an implementation detail.
- Before adding a new host or a new project (every Dockerfile has to learn about it; see `CLAUDE.md`).
- Before adding anything to `SharedKernel`.

## Current state

**Works today** - 33 features, each in [`.shipit/specs/done/`](.shipit/specs/done/README.md): tenancy and sign-in,
client companies, job descriptions and posts, candidates and applications, feeds and the public job board, interviews,
sales with a workspace and tasks, e-mail notifications, teams, files, projects with compliance, legal entities,
workers and assignments, positions, the agency chart and time sheets with a settlement export, impersonation, timelines,
reports, dynamic forms, observability, startup validation; the panel on HeroUI v3.

**Next** - the cheapest items in [`.shipit/open.md`](.shipit/open.md): the job description filter and suggestion for
job posts, then one language per post per description.
