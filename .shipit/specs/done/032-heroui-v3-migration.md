# 032 · HeroUI v3 migration

**Status:** done · **Built:** 2026-10-01 → 2026-10-01 · **Verify:** frontend only (`yarn build`, `yarn check`, `yarn test`, `yarn e2e:check`) · **Plan:** frontend 014 (local, not in git)

## Why
The agency panel carried about 6,200 lines of hand-made primitives (`components/ui`,
`forms/wrapper.tsx`, the wizard, the table) and about 2,000 lines of CSS. Accessibility depended on
how carefully each one was written. Moving to HeroUI v3 (compound components on React Aria) gives
keyboard and screen-reader behaviour for free, less CSS and one vocabulary for controls, while users
see the same panel.

## What it promises
- Primitives and screens use HeroUI: buttons, form fields, dialogs and drawers, tabs and row menus,
  badges (`Chip`), avatars, metric cards (`Card`), the main table, static tables, pickers, selects,
  enum filters, date and time fields, toasts.
- The look stays as it was: the panel's tokens are mapped onto HeroUI's variables.
- Compositions with behaviour keep their API, so screens move with them: `forms/wrapper.tsx` (TanStack
  Form ↔ HeroUI), `FormDrawer`, `ConfirmDialog` (`role="alertdialog"`), `Dialog` (wizard size 80vw/80vh),
  `FormWizard`, `SuggestionPicker` (`ComboBox`), `EnumFilter` (`ToggleButton`s), `DatePicker`, `MainTable`.
- Behaviours preserved: the unsaved-changes question before closing a dirty drawer or wizard;
  `fieldErrors` from a 400 on the fields; `DynamicForm` control names `f_<fieldId>`; dates typed as
  `dd.MM.yyyy` / `ddMMyyyy` / `dd-MM-yyyy` or pasted, an error without wiping the text, bounds with a
  message, month and year selects; row details still open on double click.
- Toasts are HeroUI `toast`; `sonner` is removed.
- Layout (`components/layout`), kanban (dnd-kit), charts (recharts) and the details page frame stay
  our own.

## Surface
- Frontend: `frontend/package.json` (`@heroui/react`, `@heroui/styles`), `src/styles` (HeroUI theme
  on panel tokens), `src/components/ui/*` (Button, Dialog, Drawer, Badge, Avatar, MetricCard, table,
  date-picker, TimeInput, SuggestionPicker, SelectField, EnumFilter), `src/forms/wrapper.tsx`, the
  React Aria `RouterProvider`, screens under `src/features/**`.
- Tests: Vitest `components/ui/date-picker/datePickerUtils.test.ts`, `components/ui/timeInputUtils.test.ts`;
  e2e selectors in `frontend/e2e/support/ui.ts` (`chooseOption`, `fillDate`) and specs using `grid`/`gridcell`.

## Out of scope for this feature
- Backend - nothing changes.
- Deliberate exceptions that stay hand-made: sign-in, reset and owner login fields; the round checkbox
  in `TaskRow`; the password toggle; form builder page/field lists; time sheet day rows; the interview
  calendar; the org tree; the landing page (navigation, contact form, compliance matrix with
  `caption`/`scope`); `FileDropzone` (native file input); the tag list in `TagForm`.
- Dead CSS from before the migration (`data-contact*`, `job-post-*`, `profile-upload-*`) - not yet.
- A visual redesign - not ever in this feature; the look was to stay the same.

## Acceptance criteria
- [x] Each stage passes `yarn build`, `yarn check`, `yarn test` before merge — evidence: plan 014 stage rule; PR merges #21-#36; not re-run during reconstruction
- [x] Date and time parsing, bounds and formatting keep the old behaviour — `yarn test` (`datePickerUtils.test.ts`, `timeInputUtils.test.ts`)
- [x] E2E selectors updated for `grid`, HeroUI `Select`, `alertdialog` and date segments, and type-check — `yarn e2e:check`
- [ ] The E2E suite passes on HeroUI — unproven (the suite was not run; it needs a seeded API)
- [ ] Tabs, row menus, double-click details, drawer/wizard sizes and the unsaved-changes question work in the live panel — unproven (plan 014 lists them under "to look at with a running backend")
- [ ] Bundle size checked after stage 1 and at the end — unproven (no record found)

## Decisions
- HeroUI v3 compound components on React Aria, migrated in small PRs: [ADR-0023](../../../docs/adr/0023-heroui-v3.md).
- TanStack Start + Form + generated client unchanged underneath: [ADR-0022](../../../docs/adr/0022-frontend-tanstack-start-orval.md).
- `components/ui/Button` stays as a thin composition over HeroUI `Button` (icon slot, spinner, a `title` React Aria drops); call sites moved to HeroUI vocabulary (`onPress`, `isDisabled`, `isPending`) by an AST codemod.
- Badges are `Chip size="sm"` with the panel's own colour classes, because the palette has more colours than `Chip` offers.
- `MainTable` details stay on double click, not `onAction`, which would also fire from menus and links in the row.
- Old class names that collided with HeroUI's BEM roots (`.dropdown`, `.select`, `.spinner`) were renamed or removed stage by stage.

## History
| Date | Commit | Change |
|---|---|---|
| 2026-10-01 | ae042b61 | Foundation: styles on the panel's tokens, React Aria routing (PR #21) |
| 2026-10-01 | 7afefc70 | HeroUI toasts instead of sonner (PR #22) |
| 2026-10-01 | 5891f80f | Buttons (PR #23) |
| 2026-10-01 | a1c8799a | Tabs and row menus (PR #24) |
| 2026-10-01 | 67ac43ad | Badges, avatars, metric cards (PR #25) |
| 2026-10-01 | c4a546f3 | Main table (PR #26) |
| 2026-10-01 | 8e96856b | E2E selectors for the table and toasts (PR #27) |
| 2026-10-01 | d41230cf | Static tables (PR #28) |
| 2026-10-01 | bfaeb57f | Form fields (PR #29, 24ed9738) |
| 2026-10-01 | 0757d169 | Dialogs and drawers (PR #31, 32c9aa9a) |
| 2026-10-01 | 679350dc | Pickers, selects, enum filter (PR #34, 409e9102) |
| 2026-10-01 | 926e7004 | Date and time fields on `DatePicker`/`TimeField` (PR #35, 57e54161) |
| 2026-10-01 | 588c2613 | Dead action-menu CSS removed (PR #36, c934e19c) |

## Notes from reconstruction
- The plan said "no e2e"; the user changed that mid-way: selectors are fixed after each stage that
  changes roles, but the suite itself was still never run.
- Defining the HeroUI variables in stage 1 brought to life CSS declarations that had been dead
  (`--border`, `--muted` ... were never defined), which changed metric cards and dialogs until fixed.
- Typing 31.02 into React Aria segments snaps to the last day of the month; the calendar opens with
  `Alt+↓`. Both are accepted deviations.
- Several stages were done by agents in parallel branches, hence the "merge main into" commits.
