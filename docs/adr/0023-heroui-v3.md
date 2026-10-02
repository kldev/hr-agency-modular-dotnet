# ADR-0023: Panel primitives and screens move to HeroUI v3, in small PRs, with the layout kept as our own

- **Status:** Accepted
- **Date:** 2026-10-01
- **Evidence:** `ae042b61` feat(frontend): heroui v3 foundation - styles on the panel's tokens and react aria routing,
  `7afefc70` feat(frontend): heroui toasts instead of sonner,
  `c4a546f3` feat(frontend): main table on heroui,
  `bfaeb57f` feat(frontend): form fields on heroui,
  `926e7004` feat(frontend): date and time fields on heroui DatePicker and TimeField,
  `c934e19c` Merge pull request #36 (dead CSS)
- **Specs:** [032-heroui-v3-migration](../../.shipit/specs/done/032-heroui-v3-migration.md),
  [011-agency-panel-foundation](../../.shipit/specs/done/011-agency-panel-foundation.md)

## Context
By the end of September the panel carried about 6.2 thousand lines of hand-written primitives
(`components/ui`, `forms/wrapper.tsx`, `form-wizard`, `table`) and about 2 thousand lines of CSS in
`src/styles/`, plus `sonner` for toasts. Accessibility was whatever each primitive implemented. A
sibling project of the author had found HeroUI v3 (compound components on React Aria) better than
its own primitives.

## Decision
- Primitives **and** screens use `@heroui/react` directly; thin pass-through wrappers (`Button`,
  `Input`, `Badge`, `Avatar`, `Tabs`, ...) are removed.
- Compositions with behaviour keep their API and change their inside: `forms/wrapper.tsx` (the
  TanStack Form ↔ HeroUI bridge), `FormDrawer`, `ConfirmDialog`, `Dialog` (80vw/80vh wizard size),
  `FormWizard`, `SuggestionPicker`, `EnumFilter`, `DatePicker` (typing by hand, year select, bounds),
  `MainTable` (on HeroUI `Table`), `FileDropzone`.
- Our own: `components/layout/*`, `kanban/` (dnd-kit), `charts/` (recharts), `details/`.
- `sonner` is replaced by HeroUI `toast`.
- The panel's tokens are mapped onto HeroUI variables so the look does not change.
- One stage = one branch → PR → merge (#21-#36), each green on `yarn build`, `yarn check`, `yarn test`.

## Alternatives considered
None recorded beyond keeping the hand-written primitives. Frontend plan 014 (local, not in git)
records the split rule (what stays ours, what becomes HeroUI) rather than competing libraries.

## Consequences
- Accessibility (keyboard, focus, ARIA) comes from React Aria instead of each primitive.
- Playwright selectors had to follow the new DOM (`8e96856b`, `85f67474`).
- Empty options are `""` rather than `null` to satisfy `z.string()` schemas (plan notes).
- The plan required e2e not to gate the stages; the stages were verified by build, lint and unit
  tests, with the look checked separately.

## Revisit when
- HeroUI ships a breaking major version.
- A needed control (kanban, charts) has no HeroUI equivalent and the custom CSS grows again.
