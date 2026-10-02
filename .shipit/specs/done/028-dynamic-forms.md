# 028 · Dynamic forms

**Status:** done · **Built:** 2026-09-23 → 2026-09-24 · **Verify:** `./verify forms` · **Plan:** backend 028, superseding the sketch 013 (local, not in git)

## Why
Every new worker document, consent, declaration or survey needed a developer: a component, an
endpoint, a DTO, a table. HR wants to define such a form themselves - pages, fields, types,
validation - publish it, and let staff fill it in for a worker straight away. Forms are deliberately
the soft part of the model: content and validation of content, never business rules.

## What it promises
- An administrator or HR (`FormsDesignPolicy` = `Admin` + `HumanResources`) creates a form with a
  name, a unique code, a kind (`Document`/`Survey`) and a cardinality (one per person or many).
- The builder saves the whole layout at once (`PUT /api/forms/{id}/draft`); errors come back keyed
  by page and field id (`fieldErrors` next to the flat list in the 400).
- One page renders as a plain form, several as a wizard with a review step; the preview uses the same
  renderer without publishing.
- Fields are either system fields from the organization's catalogue (`employee.*`, a fact about the
  person) or fields of this form only; types `Text, TextArea, Number, Date, Boolean, SingleChoice,
  MultiChoice, Email, Phone, Country`; rules `required, minLength, maxLength, pattern, min, max,
  decimals, minDate, maxDate, minSelected, maxSelected` with an optional custom message.
- Publishing freezes a numbered version; nothing changed since the last version, or an incomplete
  form, is refused. A catalogue change reaches the draft but never a published version.
- Any member fills a form in for a worker; a draft response is pre-filled from earlier answers
  (subject profile), then from the worker's file, then from the field default.
- A submitted response is frozen; a correction needs a reason, keeps the original version and puts
  only what changed into the profile.
- A second start of a one-per-person form returns the first response.
- An archived form takes no new responses but keeps its drafts.
- Responses can be searched by an answer (`GET /api/forms/{id}/responses?field=&...`).
- Another organization sees and changes nothing; a recruiter fills in but cannot design or correct.

## Surface
- Backend: `src/HrAgencySystem.Forms` (schema `forms`: `FormDefinition`, `SystemFieldCatalogue`,
  `FormResponse` streams; `FormVersion`, `SubjectProfile`, `FormCodeReservation` documents; GIN index
  on answers), `IWorkerSnapshotRepository` in `Workers`; API `Endpoints/Forms`, `FormResponses`,
  `SystemFields` (`/api/forms`, `/api/form-responses`, `/api/system-fields`,
  `/api/subjects/{kind}/{id}/form-responses|available-forms`).
- Frontend: `frontend/src/features/forms` (`builder`, `renderer/DynamicForm`, `responses`, `schema`),
  routes `/app/forms`, `/app/forms/$formId`, `/app/forms/system-fields`, forms tab on the worker page.
- Tests: `tests/HrAgencySystem.UnitTests/Forms`, `tests/HrAgencySystem.IntegrationTests.Delivery/Forms`,
  shared cases `tests/fixtures/forms-validation-cases.json`, Vitest under `features/forms`,
  `frontend/e2e/forms/`.

## Out of scope for this feature
- Conditional visibility (`VisibleWhen`) - not yet; a draft with conditional fields is refused.
- File and signature fields, required-document campaigns, worker self-service by link, PDF,
  drag and drop in the builder, repeatable sections - not yet (plan "next stages").
- `reports.form_answers` and an answer report - not yet.
- A form deciding anything about a worker, or a module deciding by reading a form - not ever.
- A ready-made form engine (SurveyJS, JSON Forms ...) - rejected.

## Acceptance criteria
- [x] Layout policy: system field resolved from the catalogue, duplicate codes, bad patterns, rules a type cannot use, options of a type without options — `./verify forms` (`FormLayoutPolicyTests`)
- [x] Publishing freezes the version with today's catalogue, numbers versions, refuses an unchanged or incomplete form — `./verify forms` (`FormDefinitionHandlerTests`, `A_catalogue_change_reaches_the_draft_but_not_a_published_version`)
- [x] A response stays on the version it was given to — `./verify forms` (`A_response_stays_on_the_version_it_was_given_to`)
- [x] Build → publish → fill → submit → correct over HTTP — `./verify forms` (`A_multi_page_form_is_built_published_filled_submitted_and_corrected`)
- [x] Prefill order profile → worker file → default — `./verify forms` (`FormResponseHandlerTests`, `A_value_given_in_one_form_prefills_the_next`)
- [x] Backend and front validators agree case by case — `./verify forms` (`FormAnswersValidatorCasesTests`) and `yarn test` (`validateValue.test.ts`) on the same JSON
- [x] Isolation and permissions — `./verify forms` (`Another_organization_sees_nothing_and_changes_nothing`, `A_recruiter_fills_forms_in_but_does_not_design_or_correct_them`)
- [x] Search by answer — `./verify forms` (`Responses_can_be_found_by_an_answer`)
- [x] Builder, preview, publish, system field — e2e: `frontend/e2e/forms/form-builder.spec.ts`
- [x] Fill, resume and correct for a worker — e2e: `frontend/e2e/forms/worker-form-response.spec.ts`
- [ ] Screenshots of the builder and renderer — unproven (no screenshot committed)

## Decisions
- Soft model, versions frozen at publish, system fields copied into the version: [ADR-0019](../../../docs/adr/0019-forms-soft-model.md).
- Validation messages as constants, errors accumulated: [ADR-0007](../../../docs/adr/0007-value-objects-trycreate.md).
- Streams as the audit trail: [ADR-0002](../../../docs/adr/0002-marten-event-store.md); the form code reservation: [ADR-0005](../../../docs/adr/0005-uniqueness-reservations.md).
- Whole-layout save instead of one command per builder click: fewer endpoints, the preview is not limited to the saved state, no projection lag per click.
- `FormVersion` is inserted in the same transaction as `FormPublished`, not by a projection: a response started right after publishing must find its version.
- Answers are one typed record (`FieldValue`), not `Dictionary<string, JsonElement>`: an explicit contract for the generated client, same JSONB.
- Front stays on TanStack Form; Vitest added for pure logic. Control names are `f_<fieldId>`, because TanStack Form reads a dot as a path.

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-23 | fd4006a8 | Worker snapshot port for modules that fill forms for a worker |
| 2026-09-23 | 302828ec | Field-keyed validation errors next to the flat list |
| 2026-09-23 | 1de47c29 | Layout, answer values and validation rules |
| 2026-09-23 | 9cb55ee6 | Definition, catalogue and response aggregates |
| 2026-09-23 | 71641b31 | Endpoints behind `FormsDesignPolicy` |
| 2026-09-23 | b8f1eb91 | Index names within the 63-character Postgres limit |
| 2026-09-23 | 38e09294 | HTTP tests: lifecycle, versioning, prefill, search, isolation, permissions |
| 2026-09-23 | c1fc5cb2 | Computed `IsEmpty` kept out of stored and served answers |
| 2026-09-23 | ea0731a9 | Vitest, with the validation mirror held to the shared cases |
| 2026-09-23 | 5432a91b | Dynamic form renderer: one page as a form, several as a wizard |
| 2026-09-23 | 3a16bea0 | Builder on its own route with build, preview and versions tabs |
| 2026-09-23 | d664ef79 | E2E for building and filling in |
| 2026-09-24 | e4a504fd | Review and summary stack pages vertically |
| 2026-09-24 | 381fe564 | Merge PR #8 (dynamic-forms) |

## Notes from reconstruction
- Unique indexes on `FormVersion` and `SubjectProfile` were dropped: their ids are derived from the
  same keys, and the generated index names broke the Weasel migration.
- The backend supports a field default value and renaming a form (`PUT /api/forms/{id}`), but the
  builder UI exposes neither.
- The plan named the error format "parsed `code: message` text"; it was rejected for a typed
  `fieldErrors` dictionary on `FieldValidationException`.
- The E2E specs change seeded responses, so a second run needs a reset like the rest of the suite.
