# HR Agency — Modular Monolith with Marten & Wolverine

A **.NET 10 modular monolith** for an HR agency platform, designed around **Domain-Driven Design, CQRS, event-driven architecture, and strong module boundaries**.

The project explores how to build a business-oriented modular monolith using **Marten** as the event store/document database and **Wolverine** for messaging and command handling.

> 🚧 **Work in progress**
>
> This project is actively developed. The domain model, modules, APIs, and infrastructure are evolving as new business capabilities are implemented.

## Architecture

The application is structured as a **modular monolith**: a single deployable application composed of independently organized business modules.

The goal is to keep business capabilities isolated while avoiding the operational complexity of distributed services.

Each module owns its:

* domain model
* application logic
* infrastructure
* events
* persistence configuration
* projections

Modules communicate through explicit contracts and domain events rather than directly depending on each other's internal implementation.

### Modules

| Module             | Responsibility                                                           |
| ------------------ | ------------------------------------------------------------------------ |
| **Identity**       | Users, owners, credentials and identity-related business rules           |
| **Organization**   | Organizations and organization-level business rules                      |
| **Company**        | Company-related functionality                                            |
| **Recruitment**    | Recruitment processes                                                    |
| **JobDescription** | Job descriptions and related functionality                               |
| **Sales**          | Sales-related functionality                                              |
| **Audit**          | Audit-related capabilities                                               |
| **Suggestion**     | Suggestions and related functionality                                    |
| **SharedKernel**   | Small set of genuinely shared abstractions and value objects             |
| **Api**            | HTTP endpoints, application composition and infrastructure configuration |

The current solution structure reflects these module boundaries directly in the source tree.

## Technology Stack

### Backend

* **.NET 10**
* **ASP.NET Core Minimal APIs**
* **C#**
* **Marten 9**
* **Wolverine 6**
* **PostgreSQL 17**
* **BCrypt.Net**
* **OpenAPI**
* **Scalar**

The API project currently targets `net10.0` and uses Marten, Wolverine, Wolverine.Marten, OpenAPI and Scalar.

### Testing

* **xUnit**
* Unit tests
* Integration tests
* PostgreSQL-backed integration testing

Tests are separated into:

```text
tests/
├── HrAgencySystem.UnitTests/
└── HrAgencySystem.IntegrationTests/
```

### Infrastructure

* **PostgreSQL**
* **Docker Compose**
* **RustFS** for S3-compatible object storage

The development infrastructure is defined in `docker-compose.yml`. PostgreSQL and RustFS are provided as local infrastructure dependencies.

---

## Architectural Principles

The project focuses on several architectural principles.

### Modular Monolith

The application is deployed as one application, but the codebase is divided into business modules.

This provides:

* simple local development
* simple deployment
* transactional consistency where appropriate
* clear business boundaries
* lower operational complexity than a distributed microservice architecture

At the same time, explicit module boundaries make individual modules easier to evolve or extract later if there is a genuine business or scaling reason to do so.

### Domain-Driven Design

Business rules belong to the domain rather than being scattered across controllers, persistence code, or infrastructure services.

The domain model uses concepts such as:

* aggregates
* value objects
* domain events
* domain-specific exceptions
* business invariants

### CQRS

Commands and queries are treated as separate use cases.

Commands modify the domain and produce events, while queries are optimized around the data required by the API.

### Event-Driven Architecture

Domain changes are represented as typed events.

For example:

```text
UserCreated
OrganizationCreated
PlatformOwnerCreated
OrganizationSlugUpdated
```

Events are handled and projected independently from the write model.

### Event Sourcing / Marten

Marten is used for event storage and document persistence.

The Identity module, for example, configures event types and projections through Marten and maintains dedicated database schemas and indexes.

This allows the application to model important business changes as events rather than treating the database as the only representation of application state.

### Wolverine

Wolverine is used for message handling and application messaging.

The API configures Wolverine together with Marten during application startup.

This keeps HTTP transport concerns separate from command and message handling.

---

## Example: Identity

The Identity module demonstrates several of the architectural concepts used throughout the project.

A user belongs to an organization, and the user's email must be unique **within that organization**.

This invariant is enforced at the persistence level using a unique Marten index over:

```text
OrganizationId + Email
```

Conceptually:

```text
Organization
     │
     ├── User
     │    ├── Email
     │    ├── FirstName
     │    └── LastName
     │
     └── User email uniqueness
```

This is an example of keeping a business invariant explicit rather than relying only on application-level checks.

---

## Example: Organization

The Organization module owns organization-specific functionality such as slug management.

Organization slugs are protected by a unique persistence constraint:

```text
Slug
```

and organization-related changes are represented using domain events.

This keeps organization-specific rules inside the Organization module instead of exposing persistence details to other modules.

---

## Project Structure

```text
hr-agency-modular-dotnet/
│
├── src/
│   ├── HrAgencySystem.Api/
│   │
│   ├── HrAgencySystem.Identity/
│   ├── HrAgencySystem.Organization/
│   ├── HrAgencySystem.Company/
│   ├── HrAgencySystem.Recruitment/
│   ├── HrAgencySystem.JobDescription/
│   ├── HrAgencySystem.Sales/
│   ├── HrAgencySystem.Audit/
│   ├── HrAgencySystem.Suggestion/
│   │
│   └── HrAgencySystem.SharedKernel/
│
├── tests/
│   ├── HrAgencySystem.UnitTests/
│   └── HrAgencySystem.IntegrationTests/
│
├── http/
│   ├── company.http
│   └── organization.http
│
├── docker-compose.yml
└── HrAgencySystem.slnx
```

The repository currently contains separate source projects for the API and business modules, together with dedicated unit and integration test projects.

---

## Getting Started

### Prerequisites

Make sure you have installed:

* [.NET 10 SDK](https://dotnet.microsoft.com/)
* [Docker](https://www.docker.com/)
* Docker Compose

### 1. Clone the repository

```bash
git clone https://github.com/kldev/hr-agency-modular-dotnet.git

cd hr-agency-modular-dotnet
```

### 2. Start infrastructure

Start PostgreSQL and RustFS:

```bash
docker compose up -d
```

The default development PostgreSQL instance is exposed on:

```text
localhost:5432
```

The S3-compatible RustFS service is exposed on:

```text
localhost:9000
```

and its management console on:

```text
localhost:9001
```

These ports and services are defined in the repository's Docker Compose configuration.

### 3. Run the application

```bash
dotnet run --project src/HrAgencySystem.Api
```

The API will start using the configured development environment.

### 4. API documentation

The application exposes OpenAPI documentation and Scalar during development.

After starting the application, open the Scalar UI provided by the API host.

---

## Running Tests

Run all tests:

```bash
dotnet test
```

Run unit tests:

```bash
dotnet test tests/HrAgencySystem.UnitTests
```

Run integration tests:

```bash
dotnet test tests/HrAgencySystem.IntegrationTests
```

Integration tests use a real PostgreSQL instance rather than replacing persistence with mocks.

---

## HTTP Examples

The repository contains `.http` files with example requests:

```text
http/
├── company.http
└── organization.http
```

These can be executed directly from IDEs such as JetBrains Rider or Visual Studio Code with the appropriate HTTP client support.

---

## Why a Modular Monolith?

Microservices are not automatically the next step after a monolith.

For an HR platform, a modular monolith provides a useful balance:

```text
                    HR Agency Application
                             │
              ┌──────────────┴──────────────┐
              │                             │
        Single Deployment             Independent Modules
              │                             │
              │            ┌────────────────┼────────────────┐
              │            │                │                │
              │         Identity       Organization     Recruitment
              │            │                │                │
              │         Company       JobDescription       Sales
              │            │                │                │
              │          Audit          Suggestion
              │
              └──────────── PostgreSQL ────────────
```

The architecture aims to gain the organizational benefits of modularity without introducing distributed-system complexity before it is actually necessary.

---

## Design Goals

The project is primarily focused on demonstrating and experimenting with:

* Modular Monolith architecture
* Domain-Driven Design
* CQRS
* Event-driven architecture
* Event sourcing
* Domain events
* Marten projections
* Wolverine message handling
* explicit module boundaries
* business invariants
* value objects
* Minimal APIs
* integration testing
* PostgreSQL-backed persistence

The goal is not to maximize the number of technologies used, but to keep the architecture understandable and make business rules explicit.

---

## License

This project is currently intended primarily as a learning, experimentation, and portfolio project.
