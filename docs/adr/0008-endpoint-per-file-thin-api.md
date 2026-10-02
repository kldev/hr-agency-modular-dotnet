# ADR-0008: Minimal API with one file per operation; endpoints only translate HTTP to a command; deny by default; errors as ProblemDetails

- **Status:** Accepted
- **Date:** 2026-08-30
- **Evidence:** `f1c00ced` start project, `a0e5f16c` feat: add company projection and GET endpoint, `99d18478` feat(front): add authentication store, `aea32a5c` feat(api): update Open API schema, `ae9760e0` chore(api): add ApiEndpoints route catalog, `a592e12b` feat(projects): attach, describe and remove project documents through the file service
- **Specs:** [002](../../.shipit/specs/done/002-identity-and-sign-in.md), [011](../../.shipit/specs/done/011-agency-panel-foundation.md), [015](../../.shipit/specs/done/015-file-service.md)

## Context
The API is the only door into the modules and the source of the OpenAPI document from which
the frontend client is generated ([ADR-0022](0022-frontend-tanstack-start-orval.md)). Business
logic in endpoints would bypass the handlers' tests; a missing error description in OpenAPI
produces wrong types on the other side.

## Decision
- `src/HrAgencySystem.Api/Endpoints/<Area>/Endpoint.cs` maps a route group; each operation is a
  file `Maps/Map<Verb>.cs` with a static `Map(RouteGroupBuilder)`, a private `Handler` method and a
  nested `internal record ...Request` with `ToCommand(...)`. Integration tests reuse those records.
- An endpoint reads the caller from the claims, builds the command and calls
  `bus.InvokeAsync<TEvent>(...)`. Nothing else.
- Authorization is fallback-deny: `SetFallbackPolicy(RequireAuthenticatedUser)`; public routes
  call `.AllowAnonymous()`. Role rules are named policies in `Api/Auth` (`AdminPolicy`,
  `PayrollPolicy`, `RatesPolicy`, `FormsDesignPolicy`, `InternalApiPolicy`).
- `GlobalExceptionHandler` maps `SharedKernel` exceptions to ProblemDetails:
  `BusinessRuleException`/`InValidValueException`/`ValidationException` -> 400,
  `NotFoundException` -> 404, `AuthorizationException` -> 401,
  `OrganizationAccessDeniedException` -> 403, Marten `DocumentAlreadyExistsException` -> 409,
  file and reports service failures -> 503. ProblemDetails carry the W3C `traceId`.
- `.ProducesStandardErrors()` declares those responses for OpenAPI. Routes come from one
  catalog, `ApiEndpoints` (`ae9760e0`).
- The one sanctioned exception: an upload endpoint turns the multipart stream into a `FileId`
  through the file service before building the command, because a command goes through the
  outbox and cannot carry a `Stream` (`a592e12b`, [ADR-0012](0012-separate-file-service.md)).

## Alternatives considered
- **Inline lambdas in `MapPost`** - the shape of `f1c00ced`, replaced by a named private
  `Handler` in `a0e5f16c`.
- **Opt-in `RequireAuthorization()` per route group** - the state before `99d18478`
  (2026-09-10), which switched to the fallback policy so a forgotten group is closed, not open.
- **Route strings inline in each group** - replaced by the `ApiEndpoints` catalog on 2026-09-19.

## Consequences
- Endpoints are trivial to review; tests of behaviour belong to handlers and integration tests.
- Every new public route must remember `.AllowAnonymous()`, and every new error path must be
  declared, or the generated client gets a `void` error type (`53446721` fixed one for 503).
- The internal job board routes use a second scheme, `ApiKey`, that the fallback policy never
  sees ([ADR-0017](0017-job-board-over-internal-api.md)).

## Revisit when
1. Endpoints start to need shared pre-processing (tenant checks, idempotency keys) - that belongs
   in Wolverine middleware, not in each `Map*` file.
2. OpenAPI and the real responses drift - consider generating the error declarations.
