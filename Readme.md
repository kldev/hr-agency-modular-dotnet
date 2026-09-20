# HR Agency — Modular Monolith with Marten & Wolverine

A **.NET 10 modular monolith** for a multi-tenant recruitment-agency SaaS, built around **Domain-Driven Design, CQRS, event sourcing and strict module boundaries**.

The project explores how far a well-structured monolith can go using **Marten** as the event store and document database and **Wolverine** as the in-process message bus — with a RabbitMQ hop and background workers only where they genuinely earn their place.

> 🚧 **Work in progress**
>
> This is a learning, experimentation and portfolio project. It is never deployed to production, which means there is no data-migration or event-versioning burden — but the quality of the model is the whole point of the exercise.

---

## What the product does

A SaaS for recruitment agencies hiring for IT roles. Agencies post to JustJoinIt, NoFluffJobs and RocketJobs alongside Pracuj.pl and OLX, and `InterviewType` includes `Technical`.

The main flow, including where it currently breaks off:

```text
SalesOpportunity  ──X──►  (no Project aggregate yet)
New→…→Won/Lost                    │
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

**Known gaps, on purpose:** a won opportunity should produce a recruitment project, but no `Project` aggregate exists yet — a job description is created independently and only points at a company. Nothing enforces "every job description has at least one job post". There is no integration with the job boards: `PostToChannel` records that a post was published, it does not publish it.

---

## Architecture

One deployable API composed of business modules, plus three satellite hosts. Modules never reference each other's projects.

### Projects

| Project | Responsibility |
| --- | --- |
| **Identity** | Users, platform owners, credentials, JWT, refresh tokens, password-reset saga |
| **Organization** | Tenants, slugs |
| **Company** | Client companies and their contacts |
| **Sales** | Sales opportunities and follow-up actions |
| **JobDescription** | One job description per position |
| **Recruitment** | Largest module: job posts, channels, candidates, applications, interviews, tags |
| **Teams** | Teams and role-tagged membership |
| **Feeds** | Job-feed read model, XML/JSON serializers, task queue — knows nothing about `Recruitment` |
| **Files** | S3-compatible object storage (RustFS) |
| **SharedKernel** | Deliberately small: exceptions, `OrganizationId`, `IClock`, paging, snapshot ports |
| **Audit** | ⚠️ an empty `.csproj` — a placeholder, not wired into composition |
| **PlatformSeeder** | Demo data, registered only in `Development`/`docker` |
| **Api** | HTTP endpoints, composition root, infrastructure configuration |
| **Web** | Separate public job board (Razor Pages) reusing a reduced subset of the modules |
| **FeedsWorker** | Worker host that generates job feeds — no HTTP surface |
| **NotificationWorker** | Worker host that consumes mail queues, renders and sends |

Anything that crosses a module boundary lives in a dependency-free contracts project:

| Contracts project | Carries |
| --- | --- |
| **Recruitment.Contracts** | Integration events from `Recruitment` to other modules |
| **Teams.Contracts** | `TeamRole`, `TeamInfo`, `TeamMembershipChanged`, `AssignUserToTeam` |
| **EmailTemplates.Contracts** | The mail messages themselves (`IEmailTemplateContract`) |

Mail delivery is split across two more supporting projects: **EmailTemplates** holds the embedded liquid templates, rendering and sending, and **EmailTemplates.Messaging** is the single description of the mail topology — topics, queues and the publish/consume wiring that both hosts read.

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
* **SharedKernel ports** — `IUserSnapshotRepository`, `ICompanySnapshotRepository`, `IJobDescriptionSnapshotRepository`, `ITeamSnapshotRepository`, `IOrganizationChecker`. Each module implements the port for the data it owns; consumers depend only on the interface.

### Multi-tenancy

Every aggregate, projection and query is scoped by `OrganizationId`, carried in a JWT claim. Marten indexes are declared with `OrganizationId` as the leading column.

### Uniqueness invariants

Cross-aggregate uniqueness — company tax id, user email per organization, organization slug, candidate email, one-team-per-person — is enforced by a dedicated **reservation document** with a unique Marten index, written in the same transaction as the event. The handler checks the reservation first for a friendly error and relies on the unique index to defeat concurrent requests.

### Email over RabbitMQ

Mail leaves the API as a message and becomes an actual email in `NotificationWorker`. The exchange `x.emails` is a **topic** exchange, and one durable queue per source domain means a backlog in one domain cannot stall another.

```text
Api handler ──returns OutgoingMessages──► outbox ──► x.emails (topic)
                                                        │ recruitment.#  ──► q.emails.recruitment ─┐
                                                        │ identity.#     ──► q.emails.identity     ├─► NotificationWorker
                                                        │ sales.#        ──► q.emails.sales        │
                                                        └ teams.#        ──► q.emails.teams       ─┘
```

Rendering and sending are separate concerns: liquid templates are rendered through FluentEmail.Liquid, and `ISendEmail` puts the html on the wire via MailKit. A host without SMTP falls back to a logging sender, so it still runs. Locally, mail lands in Mailpit.

### Long-running processes

`PasswordResetSaga` is a Wolverine saga stored as a Marten document: Wolverine loads it, hands it the message, and deletes it when the handler calls `MarkCompleted()`. The window closing is the document ceasing to exist, not a flag anybody has to remember to check.

### Job feeds

`JobFeedSchedulerWorker` queues a generation task per active organization; `JobFeedGenerationWorker` serializes published posts into S3 under `{organizationId}/jobs.{xml,json}`, served anonymously from `GET /p/{slug}/jobs.xml|json`.

Feed content comes from its **own** read model — the relational table `feeds.job_posts`, filled by an EF Core-backed Marten projection that lives in `Recruitment` (the module that owns the events) and read with Dapper. That table is the entire contract between the two projects.

---

## Technology stack

### Backend

* **.NET 10**, ASP.NET Core Minimal APIs
* **Marten 9.37** — event store and document database
* **Wolverine 6.39** — messaging, handler discovery, transactional outbox
* **PostgreSQL 17**
* **RabbitMQ 4** — mail transport
* **EF Core** (feed projection) and **Dapper** (feed reads)
* **MailKit** + **FluentEmail.Liquid**
* **BCrypt.Net**, **JWT bearer**
* **OpenAPI** + **Scalar**
* **CSharpier** (enforced by a Husky pre-commit hook)

### Frontend (`frontend/`)

* **React 19** + **TanStack Start** (Router, Query, Form, Table) on **Vite**
* **Tailwind CSS 4**, **Zod**, **Zustand**, **axios**
* **Yarn 4**, **Biome** for lint and format
* **orval** generates the API client from the running API's OpenAPI document

### Testing

```text
tests/
├── HrAgencySystem.UnitTests/               static handlers + NSubstitute + FixedClock
├── HrAgencySystem.IntegrationTests/        real HTTP against a PostgreSQL Testcontainer
└── HrAgencySystem.EmailTemplates.UnitTests/ renders every liquid template
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
./infrastructure/start.sh --build     # bring everything up
./infrastructure/start.sh --logs      # tail webapi logs;  --stop, --clean
```

`infrastructure/docker-compose.yml` requires the environment variables `SecretKey`, `RustFsAccessKey`, `RustFsSecretKey`, `Cors`, `RabbitMqUser` and `RabbitMqPassword`.

### 3. Run the hosts

```bash
dotnet run --project src/HrAgencySystem.Api        # API           → http://localhost:5000  (Scalar at /docs)
dotnet run --project src/HrAgencySystem.Web        # job board     → http://localhost:5050
dotnet run --project src/HrAgencySystem.FeedsWorker              # feed generation, no HTTP
dotnet run --project src/services/HrAgencySystem.NotificationWorker  # email delivery, no HTTP
```

Start `NotificationWorker` **before** triggering the first email: it declares the queues and bindings, and a topic exchange silently drops a message that matches no binding.

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

---

## Running tests

```bash
dotnet test                                       # everything
dotnet test tests/HrAgencySystem.UnitTests
dotnet test tests/HrAgencySystem.IntegrationTests
dotnet test tests/HrAgencySystem.UnitTests --filter "FullyQualifiedName~CreateCompanyHandlerTests"
```

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
