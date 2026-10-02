# ADR-0007: Value objects expose `TryCreate`/`Create`; handlers collect every error into one `ValidationException`; messages are constants

- **Status:** Accepted (amended 2026-09-23 by field keyed errors)
- **Date:** 2026-08-30
- **Evidence:** `f1c00ced` start project, `5820e004` test: add organization value object tests, `afbb4781` refactor(shared-kernel): share contact data factory, `302828ec` feat(api): field keyed validation errors next to the flat list
- **Specs:** [001](../../.shipit/specs/done/001-organizations-and-tenancy.md), [003](../../.shipit/specs/done/003-client-companies.md), [004](../../.shipit/specs/done/004-job-descriptions.md), [005](../../.shipit/specs/done/005-job-posts-and-channels.md), [028](../../.shipit/specs/done/028-dynamic-forms.md)

## Context
Forms in the panel send many fields at once. A user who gets one error, fixes it and then gets
the next is badly served, and tests that assert on copied string literals break on every wording
change. Validation also had to stay inside the domain, not in the HTTP layer
([ADR-0008](0008-endpoint-per-file-thin-api.md)).

## Decision
- A value object is a `sealed record` with a private constructor and two factories:
  `static (T? value, string? error) TryCreate(...)` and a throwing `static T Create(...)`.
- Every message is a `public const string` on the type (`TaxId.RequiredMessage`), and tests
  assert on the constant.
- A handler (or a module's `*DataFactory`) calls `TryCreate` for every input, adds each error to a
  list, and throws one `SharedKernel.Exception.ValidationException(errors)` -> 400 with the list.
- `Create` throws `InValidValueException` (since `5820e004`; `f1c00ced` threw `ArgumentException`).
- Value objects used by several modules live in `SharedKernel/ValueObjects` (`Email`,
  `CountryCode`, `SalaryRange`, `PostalAddress`, `WorkRate`, ...).
- **Amendment (`302828ec`):** `FieldValidationException : ValidationException` adds
  `fieldErrors` keyed by field code next to the unchanged flat list, first used by dynamic forms.

## Alternatives considered
- **FluentValidation on commands in Wolverine middleware.** Discussed in `.docs/05` (local) and
  judged a step back: format rules and invariants would split into two places and messages would
  be duplicated.
- **A small error collector in `SharedKernel`** (`ValueCollector`) to remove the repeated
  `if (error != null) errors.Add(error)` lines in the factories. Recommended in `.docs/05` but not
  built; the manual pattern is still in use.
- **Vogen-generated value types** - listed in `.docs/05` as an option for typed ids, not taken.

## Consequences
- Every error of one request comes back at once, and the frontend can show them together.
- The accumulation is boilerplate that grows with the number of fields; forgetting one
  `errors.Add` silently lets a `null!` through. The shared `ContactDataFactory` (`afbb4781`)
  removed one copy of it.
- Two types still throw `ArgumentException` from their throwing factory
  (`TimeSheetPeriod` in `Agency`, `ReportPeriod` in `ReportsService.Contracts`).
- The forms module mirrors its validation in TypeScript and holds both sides to one fixture file
  (`tests/fixtures/forms-validation-cases.json`).

## Revisit when
1. A new module adds another large `*DataFactory` - build the collector first.
2. Field keyed errors are needed outside forms - then every handler should produce them.
