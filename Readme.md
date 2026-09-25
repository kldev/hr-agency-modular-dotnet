# HR Agency — Modular Monolith with Marten & Wolverine

A **.NET 10 modular monolith** for a multi-tenant recruitment-agency SaaS, built around **Domain-Driven Design, CQRS, event sourcing and strict module boundaries**.

The project explores how far a well-structured monolith can go using **Marten** as the event store and document database and **Wolverine** as the in-process message bus — with a RabbitMQ hop and background workers only where they genuinely earn their place.

> 🚧 **Work in progress**
>
> This is a learning, experimentation and portfolio project. It is never deployed to production, which means there is no data-migration or event-versioning burden — but the quality of the model is the whole point of the exercise.

---

## What the product does

A SaaS for recruitment agencies hiring for IT roles. Agencies post to JustJoinIt, NoFluffJobs and RocketJobs alongside Pracuj.pl and OLX, and `InterviewType` includes `Technical`.

The product has **three axes**, and the easiest mistake in the model is confusing the last two:

1. **recruiting for a client** — sale, job description, posts, applications, interviews;
2. **delivering the sold service** — project, contract, the people we send to the client and the law that follows them (`Projects`, `Workers`, `Compliance`);
3. **the agency as an employer** — its own org chart, its own people's contracts and their hours (`Agency`).

`Workers` is about people at the **client**; `Agency` is about people **here**. Both have hours, contracts and documents, and they are settled with entirely different parties.

The main flow, including where it currently breaks off:

```text
SalesOpportunity  ──X──►  Project (exists, not yet linked to the opportunity)
New→…→Won/Lost            Draft→Active→Suspended→Completed/Cancelled
                                  │
                                  ▼
                         JobDescription  ──1:N──►  JobPost  ──►  Candidate ──► JobApplication ──► Interview
                         one per position          candidate-facing copy,
                         Draft→Open→OnHold         may differ from the description,
                         →Closed/Cancelled         many per description (e.g. per language)
```

Alongside that, people are grouped into **teams**:

```text
Organization (tenant)
   │
   ├── User ── OrganizationRole: Admin/Recruiter/Sales/…     ← permissions
   │      └── Team ── TeamRole: Sales/Recruiter/Operations/Lead
   │                                                          ← division of work
   └── Team "Tiggers" ── members (UserId, TeamRole)
```

A team outlives the people on it: "the Tiggers team handles this" survives recruiter rotation in a way that "Katy handles this" does not.

**Known gaps, on purpose:** a won opportunity should produce a project; the `Projects` module exists, but nothing links it to `SalesOpportunity` yet — a project is created by hand and points at a company. Nothing enforces "every job description has at least one job post". There is no integration with the job boards: `PostToChannel` records that a post was published, it does not publish it.

---

## A look at the panel

Screenshots of the agency panel, taken by the Playwright suite on a freshly seeded platform
(`yarn e2e:screenshots`, see [End-to-end tests](#end-to-end-tests)) - so they show the screens as they
are, with data a real run creates. Every person and company in them is made up.

![Login page](docs/screenshots/login.png)

### Reports

Computed by the separate reports service from its own read model: the agency's recruitment funnel
and monthly activity, and the platform owner's view of every organization - both exportable to Excel.

| Recruitment dashboard | Platform report (owner) | Organizations (owner) |
| --- | --- | --- |
| ![Recruitment dashboard](docs/screenshots/dashboard.png) | ![Platform report](docs/screenshots/platform-reports.png) | ![Owner organizations](docs/screenshots/owner-organizations.png) |

### Recruitment

| Job postings | Candidates | Applications |
| --- | --- | --- |
| ![Job postings](docs/screenshots/job-postings.png) | ![Candidates](docs/screenshots/candidates.png) | ![Applications](docs/screenshots/applications.png) |

Applications as a board - a card dragged onto another stage opens the status drawer with that stage
chosen, so the note still goes with the change; stages it cannot reach are dimmed:

| Applications board | Dropped on a stage |
| --- | --- |
| ![Applications kanban](docs/screenshots/applications-kanban.png) | ![Application dropped on a stage](docs/screenshots/applications-kanban-drop.png) |

The job description wizard - one position, written once, before any post goes out:

| Position | Review | Created |
| --- | --- | --- |
| ![Job description wizard](docs/screenshots/job-description-wizard.png) | ![Job description review](docs/screenshots/job-description-review.png) | ![Job description details](docs/screenshots/job-description-created.png) |

### Sales

| Pipeline board | Opportunity |
| --- | --- |
| ![Sales kanban](docs/screenshots/sales-kanban.png) | ![Sales opportunity](docs/screenshots/sales-opportunity.png) |

| Opportunities | Companies |
| --- | --- |
| ![Sales table](docs/screenshots/sales.png) | ![Companies](docs/screenshots/companies.png) |

The sales workspace - one client company at a time, with its activity, opportunities, projects and
the salesperson's own task list:

| Workspace | Projects | Tasks |
| --- | --- | --- |
| ![Sales workspace](docs/screenshots/sales-workspace.png) | ![Sales workspace projects](docs/screenshots/sales-workspace-projects.png) | ![Sales workspace tasks](docs/screenshots/sales-workspace-tasks.png) |

### Delivery - from a client to people at work

A project for a client, with a signed contract, a responsible contact on the client side, its first
position and the first person posted onto it - everything a project needs to go live, and what
follows.

![Project details](docs/screenshots/project.png)

| Contract & contacts | Positions | People |
| --- | --- | --- |
| ![Project contract](docs/screenshots/project-contract.png) | ![Project positions](docs/screenshots/project-positions.png) | ![Project people](docs/screenshots/project-people.png) |

Documents are uploaded to the separate file service - a domain only ever holds a file id:

| Project documents | Worker documents |
| --- | --- |
| ![Project documents](docs/screenshots/project-documents.png) | ![Worker documents](docs/screenshots/worker-documents.png) |

| New project wizard | Compliance of a hired-out project in Germany |
| --- | --- |
| ![Project wizard](docs/screenshots/project-wizard.png) | ![Project compliance](docs/screenshots/project-compliance.png) |

| Projects | Planning an assignment | Assignment |
| --- | --- | --- |
| ![Projects](docs/screenshots/projects.png) | ![Assignment wizard review](docs/screenshots/assignment-wizard-review.png) | ![Assignment](docs/screenshots/assignment.png) |

![Assignments register](docs/screenshots/assignments.png)

### From an application to the workers' register

An applicant is taken onto the register straight from their application; name and contact details
come along, the passport data is added on the way.

| Identity | Review | Worker file |
| --- | --- | --- |
| ![Register worker wizard](docs/screenshots/worker-wizard.png) | ![Register worker review](docs/screenshots/worker-wizard-review.png) | ![Worker details](docs/screenshots/worker-created.png) |

![Workers register](docs/screenshots/workers.png)

The same register as a board of the pipeline, for both desks; a drop opens the status drawer, and
only the stages the person may go to accept it:

| Workers board | Workers abroad | Dropped on the next stage |
| --- | --- | --- |
| ![Workers kanban](docs/screenshots/workers-kanban.png) | ![Workers abroad kanban](docs/screenshots/workers-abroad-kanban.png) | ![Worker dropped on a stage](docs/screenshots/workers-kanban-drop.png) |

### The agency's own structure and teams

| Org chart | Team |
| --- | --- |
| ![Organization structure](docs/screenshots/organization-structure.png) | ![Team details](docs/screenshots/teams.png) |

---

## Architecture

One deployable API composed of business modules, plus satellite hosts: two workers without HTTP, two internal HTTP services and a public job board. Modules never reference each other's projects.

```text
Frontend (:4300) ─────────────┐
Web job board (:5050) ─X-Api-Key─┤
                                 ▼
                      HR API (:5000) ── Marten: events + documents on PostgreSQL 17
                        │
                        ├─ service token ──► FileService (:5100) ────► S3 bucket "documents" + schema `files`
                        ├─ service token ──► ReportsService (:5200) ─► schema `reports` (Dapper, Excel)
                        ├─ outbox ─► RabbitMQ x.emails ─► NotificationWorker ─► SMTP (Mailpit locally)
                        └─ projection ─► feeds.job_posts ─► FeedsWorker ─► S3 {org}/jobs.{xml,json}
```

### Business modules

| Project | Responsibility |
| --- | --- |
| **Identity** | Users, platform owners, credentials, JWT, refresh tokens, service API keys, password-reset saga |
| **Organization** | Tenants, slugs |
| **Company** | Client companies, their contacts and the client profile |
| **Sales** | Sales opportunities and follow-up actions |
| **JobDescription** | One job description per position |
| **Recruitment** | Largest module: job posts, channels, candidates, applications, interviews, tags |
| **Projects** | Delivery of a sold service: project lifecycle, contract, contacts with roles, documents, per-country compliance |
| **Workers** | People sent to clients: `Worker` (the person, a status pipeline, permits) and `Assignment` (one posting, its documents and per-person compliance — A1, Limosa) |
| **Compliance** | Shared domain library, no persistence: the `(country, EngagementType) → requirements` catalogue read by `Projects` and `Workers` |
| **Agency** | The agency as an employer: org chart, `AgencyEmployment` (contract, hours, rate), monthly time sheets and the settlement export |
| **Forms** | Forms, documents and surveys an administrator builds without a developer: layouts, system field catalogue, frozen published versions, responses per worker |
| **Tasks** | A person's own to-do list for client companies, optionally within a deal; Day/Week/Month board in the caller's time zone |
| **LegalEntities** | The companies the agency trades and posts people through |
| **Teams** | Recruitment teams and role-tagged membership |
| **Feeds** | Job-feed read model, XML/JSON serializers, task queue — knows nothing about `Recruitment` |

### Supporting projects and hosts

| Project | Responsibility |
| --- | --- |
| **SharedKernel** | Deliberately small: exceptions, `OrganizationId`, `IClock`, shared value objects, paging, snapshot ports |
| **Files** | Low-level S3 object storage over RustFS (`IObjectStorage`) — no tenancy, no ownership |
| **Reports.ReadModel** | The `reports` schema — EF Core tables filled by projections in Organization, Recruitment and Projects |
| **EmailTemplates**, **EmailTemplates.Messaging** | Embedded liquid templates, rendering and sending; the single description of the mail topology |
| **Observability**, **Observability.AspNetCore** | Serilog, OpenTelemetry traces/metrics/logs and health endpoints for every host |
| **PlatformSeeder** | Demo data, registered only in `Development`/`docker` |
| **Audit** | ⚠️ an empty `.csproj` — a placeholder, not wired into composition |
| **Api** | HTTP endpoints, composition root, infrastructure configuration |
| **Web** | Public job board (Razor Pages) with no database and no project references — it talks to the API's internal routes with a service API key |
| **FeedsWorker** | Worker host that generates job feeds — no HTTP surface |
| **NotificationWorker** | Worker host that consumes mail queues, renders and sends |
| **FileService** | Separate HTTP host owning private documents: metadata, ownership, tenant isolation; the only process that talks to the document bucket |
| **ReportsService** | Separate HTTP host that aggregates the `reports` tables with SQL (Dapper) and exports Excel; no event store |

Anything that crosses a module or process boundary lives in a dependency-free contracts project:

| Contracts project | Carries |
| --- | --- |
| **Recruitment.Contracts**, **Projects.Contracts**, **Workers.Contracts**, **Tasks.Contracts** | Integration events from the producing module to other modules |
| **Teams.Contracts** | `TeamRole`, `TeamInfo`, `TeamMembershipChanged`, `AssignUserToTeam` |
| **EmailTemplates.Contracts** | The mail messages themselves (`IEmailTemplateContract`) |
| **FileService.Contracts** | `IFileServiceClient`, `FileDescriptor`, `FileOwnerRef`, service-token constants |
| **ReportsService.Contracts** | `IReportsClient`, report DTOs, `ReportPeriod`, service-token constants |

There is **no** `Suggestion` project. Typeahead repositories live inside the module that owns the data and are exposed through `Api/Endpoints/Suggestion`.

### Module anatomy

Every module follows the same internal layout:

```text
<Module>Module.cs              composition root: Add<X>Module() + static ConfigureMarten(StoreOptions)
Domain/                        aggregate (Apply(Event) methods), strongly typed ids, ValueObjects/
Events/                        domain events, stored by Marten
Application/<UseCase>/         command record + static handler class, one folder per use case
Application/Port/              interfaces the module needs
Projections/                   read models (Marten snapshots)
Documents/                     plain Marten documents
Infrastructure/Configuration/  Marten + DI registration
Infrastructure/Persistence/    write-side repos, uniqueness reservation documents
Infrastructure/Query/          read-side query repositories
Integration/                   handlers for other modules' integration events
Services/                      the module's facade over SharedKernel ports
```

Handlers are **static classes with a static `Handle` method**. Wolverine discovers them and injects `IDocumentSession`, repositories and `IClock` as parameters — there is no `IRequestHandler`-style interface anywhere.

### Cross-module communication

Two sanctioned mechanisms, and nothing else:

* **Integration events** via a `*.Contracts` project. The producing handler returns the event in `OutgoingMessages`; the consuming module translates it into its own domain event inside an `Integration/` folder. For example, `Teams` announces `TeamMembershipChanged`, and `Identity` turns it into `UserTeamChanged` so the user read model can show which team somebody is on.
* **SharedKernel ports** — `IUserSnapshotRepository`, `ICompanySnapshotRepository`, `IJobDescriptionSnapshotRepository`, `ITeamSnapshotRepository`, `IProjectSnapshotRepository`, `IWorkerSnapshotRepository`, `IOrganizationChecker`. Each module implements the port for the data it owns; consumers depend only on the interface.

### Multi-tenancy

Every aggregate, projection and query is scoped by `OrganizationId`, carried in a JWT claim. Marten indexes are declared with `OrganizationId` as the leading column.

### Uniqueness invariants

Cross-aggregate uniqueness — company tax id, user email per organization, organization slug, candidate email, one-team-per-person, a worker's identity document and e-mail — is enforced by a dedicated **reservation document** with a unique Marten index, written in the same transaction as the event. The handler checks the reservation first for a friendly error and relies on the unique index to defeat concurrent requests.

### Email over RabbitMQ

Mail leaves the API as a message and becomes an actual email in `NotificationWorker`. The exchange `x.emails` is a **topic** exchange, and one durable queue per source domain means a backlog in one domain cannot stall another.

```text
Api handler ──returns OutgoingMessages──► outbox ──► x.emails (topic)
                                                        │ recruitment.#  ──► q.emails.recruitment ─┐
                                                        │ identity.#     ──► q.emails.identity     │
                                                        │ sales.#        ──► q.emails.sales        ├─► NotificationWorker
                                                        │ teams.#        ──► q.emails.teams        │
                                                        └ agency.#       ──► q.emails.agency      ─┘
```

Delivery is at-least-once without double sends: the worker claims the event id in `notifications.processed_events` before sending and releases the claim if the send throws. Transient SMTP, socket and database failures get three short retries; a malformed address or a 5xx refusal goes straight to the dead letter queue.

Rendering and sending are separate concerns: liquid templates are rendered through FluentEmail.Liquid, and `ISendEmail` puts the html on the wire via MailKit. A host without SMTP falls back to a logging sender, so it still runs. Locally, mail lands in Mailpit.

### Long-running processes

`PasswordResetSaga` is a Wolverine saga stored as a Marten document: Wolverine loads it, hands it the message, and deletes it when the handler calls `MarkCompleted()`. The window closing is the document ceasing to exist, not a flag anybody has to remember to check.

### Job feeds

`JobFeedSchedulerWorker` queues a generation task per active organization; `JobFeedGenerationWorker` serializes published posts into S3 under `{organizationId}/jobs.{xml,json}`, served anonymously from `GET /p/{slug}/jobs.xml|json`.

Feed content comes from its **own** read model — the relational table `feeds.job_posts`, filled by an EF Core-backed Marten projection that lives in `Recruitment` (the module that owns the events) and read with Dapper. That table is the entire contract between the two projects.

### Delivery: projects, workers and compliance

* A **project** goes `Draft → Active → Suspended → Completed/Cancelled`. Going live needs a **signed contract**, a **responsible contact** and a **complete client profile**, each reported separately. The contract lives on the project stream rather than in its own aggregate, so the rule is never checked against a lagging read model.
* **`Worker` is the person, `Assignment` is one posting.** Moving somebody from a Polish project to a German one ends one assignment and opens another on the same file, so their history stays a history. The worker's status is a pipeline with an owner per stage (`Recruitment → ContractPreparation → Legalisation → Onboarding → Employed`), and `Legalisation` exists only for people whose citizenship needs it.
* **Compliance is a catalogue, not control flow.** `ComplianceCatalogue` maps `(country, EngagementType, scope) → requirements`; there is no `if (country == …)` anywhere. Posting an IT specialist to Germany triggers almost nothing, hiring the same person out triggers a licence, a notification and document duties. Project-level requirements sit on the project, per-person ones (A1, Limosa) on the assignment.

### The agency as an employer

* The **org chart** is one document per organization. A supervisor is never stored — it is worked out by walking up the tree at the moment of the question.
* A **time sheet** is one stream per person and month, its id derived from `(organization, user, year, month)`, so "one sheet per month" cannot be broken. `Draft → Submitted → Approved → Settled`, plus `Correction`; the supervisor from the chart approves, payroll settles.
* No money in the domain: the only place a rate becomes an amount is the settlement export (minutes × hourly rate, rounded once per person).

### Forms

Administrators define forms without a developer: the builder saves a whole layout, publishing freezes a version, and a response stays bound to that version for life. System fields (`employee.*`) come from a per-organization catalogue and pre-fill the next form; answers are one typed record, queried through a GIN index. Validation lives twice — C# is the authority, the TypeScript mirror is held to the same JSON fixture of cases.

### Files and the file service

A domain never sees a storage key, only a `FileId`. Uploads go through the owning resource's endpoint into the **FileService**, which builds the key (`{organizationId}/{ownerKind}/{ownerId}/{fileId}{ext}`), records ownership and answers **404** for a file of another organization. Every call carries a short-lived HMAC service token with the organization as a signed claim.

### Reports

The **ReportsService** reads its own `reports` schema — one row per entity with first-reached timestamps, not counters — filled by EF Core-backed Marten projections. It serves the agency's recruitment funnel (a cohort) and monthly activity, the platform owner's cross-organization view, and Excel exports.

### Observability

Every host logs through Serilog and exports logs, traces and metrics over OTLP. `./infrastructure/start.sh --build --observability` adds an OpenTelemetry Collector, **Prometheus** (:9090), **Grafana** (:3000, five provisioned dashboards) and **Rootprint** (:8282) over Quickwit for logs and traces. Domain metrics come from the event store (`marten.event.append{event.type}`), so a new event is counted without code. Without `OTEL_EXPORTER_OTLP_ENDPOINT` nothing is exported.

---

## Technology stack

### Backend

* **.NET 10**, ASP.NET Core Minimal APIs
* **Marten 9.39** — event store and document database
* **Wolverine 6.39** — messaging, handler discovery, transactional outbox, sagas
* **PostgreSQL 17**
* **RabbitMQ 4** — mail transport
* **RustFS** — S3-compatible storage for documents, feeds and observability indexes
* **EF Core** (feed and report projections) and **Dapper** (feed and report reads)
* **DocumentFormat.OpenXml** — Excel exports
* **MailKit** + **FluentEmail.Liquid**
* **BCrypt.Net**, **JWT bearer**, HMAC service tokens, service API keys
* **Serilog** + **OpenTelemetry** (OTLP) — Prometheus, Grafana, Rootprint/Quickwit
* **OpenAPI** + **Scalar**
* **CSharpier** (enforced by a Husky pre-commit hook)

### Frontend (`frontend/`)

* **React 19** + **TanStack Start** (Router, Query, Form, Table) on **Vite**
* **Tailwind CSS 4**, **Zod**, **Zustand**, **axios**, **recharts**, **dnd-kit** (kanban boards)
* **Yarn 4**, **Biome** for lint and format
* **orval** generates the API client from the running API's OpenAPI document
* **Vitest** for pure logic, **Playwright** for flows

### Testing

```text
tests/
├── HrAgencySystem.UnitTests/                      static handlers + NSubstitute + FixedClock
├── HrAgencySystem.IntegrationTests/               real HTTP against a PostgreSQL Testcontainer
├── HrAgencySystem.EmailTemplates.UnitTests/       renders every liquid template
├── HrAgencySystem.FileService.UnitTests/          file service rules
├── HrAgencySystem.ReportsService.UnitTests/       report shaping and export
├── HrAgencySystem.ReportsService.IntegrationTests/ report SQL against its own Testcontainer
└── fixtures/                                      cases shared by the C# and TypeScript form validators

frontend/src/**/*.test.ts                          Vitest: pure logic
frontend/e2e/                                      Playwright: the panel's main flows against the real stack
k6/                                                load scripts: panel, mail, file uploads, public job board
```

Integration tests spin up their **own** PostgreSQL 17 Testcontainer, so Docker must be running but the local compose stack is not required. External Wolverine transports are stubbed, so no broker is needed either. Because projections run in an async daemon, read-model assertions are wrapped in `Eventually.AssertAsync(...)`.

---

## Getting started

### Prerequisites

* [.NET 10 SDK](https://dotnet.microsoft.com/)
* [Docker](https://www.docker.com/) and Docker Compose
* [Node.js](https://nodejs.org/) with Yarn 4 (frontend only)

### 1. Clone

```bash
git clone https://github.com/kldev/hr-agency-modular-dotnet.git
cd hr-agency-modular-dotnet
```

### 2. Start infrastructure

```bash
docker compose up -d
```

| Service | Port | Notes |
| --- | --- | --- |
| PostgreSQL 17 | `5432` | |
| RustFS (S3) | `9000` | console on `9001` |
| RabbitMQ | `5672` | management UI on `15672`, vhost `development` |
| Mailpit | `1025` | web UI on [localhost:8025](http://localhost:8025) |

There is also a full stack including the containerized API:

```bash
./infrastructure/start.sh --build                   # bring everything up
./infrastructure/start.sh --build --observability   # + Grafana :3000, Prometheus :9090, Rootprint :8282
./infrastructure/start.sh --status | --logs [service]   # also --stop, --clean
./infrastructure/start.sh --traffic | --emails | --files | --job-board   # k6 scripts, see k6/README.md
```

`infrastructure/docker-compose.yml` requires the environment variables `SecretKey`, `RustFsAccessKey`, `RustFsSecretKey`, `FileServiceSecret`, `ReportsSecret`, `WebApiKey`, `Cors`, `RabbitMqUser` and `RabbitMqPassword` (see `infrastructure/.env-sample`).

### 3. Run the hosts

```bash
dotnet run --project src/HrAgencySystem.Api        # API           → http://localhost:5000  (Scalar at /docs)
dotnet run --project src/HrAgencySystem.Web        # job board     → http://localhost:5050 (needs a service API key)
dotnet run --project src/HrAgencySystem.FeedsWorker              # feed generation, no HTTP
dotnet run --project src/services/HrAgencySystem.NotificationWorker  # email delivery, no HTTP
dotnet run --project src/services/HrAgencySystem.FileService         # documents → http://localhost:5100
dotnet run --project src/services/HrAgencySystem.ReportsService      # reports   → http://localhost:5200
```

Start `NotificationWorker` **before** triggering the first email: it declares the queues and bindings, and a topic exchange silently drops a message that matches no binding. The API is not self-sufficient for documents: without the file service, document endpoints fail, and `GET /healthz` (or `/health/ready`) shows it.

### 4. Frontend

```bash
cd frontend
yarn dev            # http://localhost:4300
yarn check          # biome lint + format
yarn api:gen        # regenerate the API client — the API must be running
```

### 5. Seed demo data

Available only in the `Development` and `docker` environments:

```text
GET /api/development/seed
GET /api/development/seed/{type}
GET /api/development/seed-sales?count=N
```

Every seeded account shares one password. A seeded platform also has a fixed job-board API key, which is the default the `Web` host uses in development, so the public board works with no manual step; anywhere else, issue a key in the owner panel (**Admin → API keys**).

---

## Running tests

```bash
dotnet test                                       # everything
dotnet test tests/HrAgencySystem.UnitTests
dotnet test tests/HrAgencySystem.IntegrationTests
dotnet test tests/HrAgencySystem.UnitTests --filter "FullyQualifiedName~CreateCompanyHandlerTests"

cd frontend && yarn test                          # Vitest
```

### End-to-end tests

`frontend/e2e/` drives the real panel in Chromium against the real API - a handful of
representative flows, not coverage: sign-in, the job description wizard, a project taken live with a
worker posted onto it, registering an applicant as a worker, documents uploaded to the file service
on both, a reorganisation of the org chart, a new team, the two reports (after moving a few seeded
applications on to offers and hires, so the funnel has an end), and read-only passes over the main
lists and the sales pipeline. The uploaded PDFs are generated in memory (`e2e/support/documents.ts`) and
marked as specimens - no binary fixture lives in the repository.

They need the stack up and **freshly seeded**, sales included. The seed is not repeatable and the
tests create records with fixed names (a person belongs to one team, a sibling unit name is unique),
so every run starts from a clean database:

```bash
./infrastructure/start.sh --clean && ./infrastructure/start.sh   # or: docker compose down -v / up -d + dotnet run
curl http://localhost:5000/api/development/seed                   # ~3 minutes
curl "http://localhost:5000/api/development/seed-sales?count=40&slug=hr-agency"   # the sales board

cd frontend
npx playwright install chromium   # once
yarn e2e                          # headless; starts `yarn dev` on :4300 unless it is already running
yarn e2e:screenshots              # the same run, also rewriting docs/screenshots/*.png
yarn e2e:ui                       # Playwright UI mode
yarn e2e:check                    # type-check the suite (it has its own tsconfig)
```

- Sign-in happens once (`e2e/auth/auth.setup.ts`) as the seeded `j.smith@hr-agency.com`; the session
  cookies are kept in `frontend/playwright/.auth/`, which git ignores. `E2E_EMAIL`, `E2E_PASSWORD`,
  `E2E_BASE_URL` and `E2E_THEME` (`dark` by default, the panel's own default) override the defaults.
- Selectors are roles, labels and visible text - never CSS classes. Where a control had no
  accessible name the component was fixed rather than given a test id.
- Records a flow does not create itself (the demo client, an application to promote, a sales
  opportunity with a history) are set up through the panel's own `/api/*` proxy in
  `e2e/support/api.ts`.
- `seed-sales` is random, so the sales board and table differ between seeds; everything the tests
  create themselves has fixed names and looks the same on every run.
- Screenshots are 2560×1440 and only written with `SCREENSHOTS=1`; a plain run never touches
  `docs/`. Audit timestamps ("created at") come from the server and differ between runs.

---

## HTTP examples

`http/` holds request samples that run directly from JetBrains Rider or VS Code:

```text
auth · candidate · company · company-contacts · interviews · job-applications
job-description · job-post · organization · owner · sales · seed · suggestion · user
```

---

## Architectural principles

### Why a modular monolith

Microservices are not automatically the next step after a monolith. For a platform this size, a modular monolith buys simple local development, simple deployment and transactional consistency where it matters, while explicit boundaries keep any individual module extractable later — if a real business or scaling reason ever appears.

The boundaries are enforced by the project graph, not by convention: a module that wanted to reach into another one would have to add a project reference that does not exist.

### Domain-Driven Design

Business rules live in the domain rather than being scattered across endpoints, persistence code or infrastructure services: aggregates, value objects, domain events, domain-specific exceptions and explicit invariants.

Value objects are records with a private constructor and a `TryCreate` returning `(value, error)`. Handlers accumulate errors and throw a single `ValidationException`, and the messages are `public const string` fields so tests assert on the constant rather than on a string literal.

### CQRS and event sourcing

Commands modify the domain and produce events; queries are optimized around what the API actually returns. Marten stores the events and builds read models as async projections, so a read that follows a write has to wait for the daemon — the API layer, the tests and the frontend each have an explicit way of doing that.

### Endpoints stay thin

`Endpoints/<Area>/Maps/Map<Verb>.cs` maps one operation, translates HTTP into a command and calls `bus.InvokeAsync<TEvent>(...)`. No business logic. Authorization is fallback-deny, so public endpoints must opt out explicitly, and a global exception handler maps domain exceptions onto ProblemDetails.

---

## Design goals

The point is not to maximize the number of technologies used, but to keep the architecture understandable and make business rules explicit:

modular monolith · DDD · CQRS · event sourcing · domain and integration events · Marten projections · Wolverine handlers, sagas and outbox · explicit module boundaries · business invariants · value objects · multi-tenancy · Minimal APIs · integration testing against real PostgreSQL

---

## License

A learning, experimentation and portfolio project.
