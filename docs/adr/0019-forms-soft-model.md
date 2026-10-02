# ADR-0019: Dynamic forms are a soft model - content, frozen versions and copied system fields

- **Status:** Accepted
- **Date:** 2026-09-23
- **Evidence:** `1de47c29` feat(forms): form layout, answer values and validation rules,
  `9cb55ee6` feat(forms): form definition, system field catalogue and response aggregates,
  `e3dba605` feat(forms): handlers, query repositories and module composition,
  `b8f1eb91` fix(forms): index names within the Postgres identifier limit,
  `ea0731a9` test(frontend): vitest for pure logic, with the forms validation mirror held to the shared cases,
  `381fe564` Merge pull request #8 (dynamic-forms)
- **Specs:** [028-dynamic-forms](../../.shipit/specs/done/028-dynamic-forms.md),
  [019-workers-and-assignments](../../.shipit/specs/done/019-workers-and-assignments.md)

## Context
An agency collects many hiring documents and surveys, and they change faster than releases. An
administrator should define them without a developer. The rest of the system is modelled hard
(aggregates with rules); forms have content and validation, but no business rules.

## Decision
- Module `HrAgencySystem.Forms`, schema `forms`. A form never decides anything about a worker, and
  no module decides anything by reading one.
- Three streams: `FormDefinition` (one working draft + last published version number),
  `SystemFieldCatalogue` (one stream per organization, derived id; code uniqueness is a list check),
  `FormResponse`.
- `FormVersion` is a frozen document **inserted in the same transaction as `FormPublished`**, never
  by a projection, so a response started a second after publication finds it.
- The builder saves the whole layout (`PUT /api/forms/{id}/draft` → `FormDraftSaved(Pages)`); a
  deliberate exception to "many small commands".
- System fields: a draft holds the catalogue id and is re-resolved on every save and at publication
  (`SystemFieldResolver`); the published version keeps its own copy. Code and type never change.
- A response is bound to its version for life; submitted = frozen; a correction is
  `FormResponseCorrected` with a reason. `OnePerSubject` responses use a derived id.
- `SubjectRef(Kind, Id)` (like `FileOwnerRef`); only `worker` today. The tenant wall is the port
  lookup with `OrganizationId`.
- Answers are one typed record `FieldValue`; `FormResponseProjection.Answers` has the codebase's
  first GIN index, queried by containment.
- Validation lives twice - `FormAnswersValidator` (C#, the authority) and `validateValue.ts` - and
  both run `tests/fixtures/forms-validation-cases.json`.

## Alternatives considered
Plan 028 (local, not in git), "Odrzucone warianty"; plan 013 is superseded by 028:
- granular builder commands (`AddField`, `MoveField`, ...) - ~10 endpoints, preview only of saved
  state, projection lag on every click;
- `FormVersion` from an async projection - a response could not find its version;
- `Dictionary<string, JsonElement>` for values - `unknown` in the generated client;
- field errors as parsed `"code: message"` strings - replaced by `FieldValidationException` with
  `fieldErrors`;
- an off-the-shelf form engine (SurveyJS, JSON Forms, RJSF, Formily) - a second definition language;
- separate mechanisms for documents and surveys - they differ by configuration only.
Plan 013 named the blocker that forms had no subject; `Workers` (ADR-0014) resolved it.

## Consequences
- Every save stores the full layout in one event; the escape hatch is an inline snapshot.
- A validation rule without a shared case is unchecked on one side.
- Marten index names over 63 characters fail migration; long indexes are named by hand (`b8f1eb91`).
- Publishing must never be "simplified" into copying the stored draft.

## Revisit when
- Forms need conditions, file fields or self-service filling by the worker.
- Someone asks for reports over answers (`reports.form_answers`, deliberately deferred).
