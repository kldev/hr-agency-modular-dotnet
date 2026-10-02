# 029 · Sales workspace and tasks

**Status:** done · **Built:** 2026-09-24 → 2026-09-24 · **Verify:** `./verify tasks` · **Plan:** backend 029 (local, not in git)

## Why
A sales person works in the context of one client company, yet its opportunities, projects and
activity history were spread across three lists, and there were no tasks at all. One screen shows the
chosen company on the left, its activities / opportunities / projects in the middle and "my tasks"
across all companies on the right, each ticked off with one click.

## What it promises
- A sales person creates a task for a company (required), optionally within one of that company's
  opportunities, with a title, description, due date and time, priority `Low/Medium/High` and an
  assignee (the creator by default, or a colleague of the same organization).
- The task board is the caller's own: `GET /api/tasks?range=day|week|month&timeZone=...&companyId=`.
  Day, calendar week (Monday-Sunday) and calendar month are cut in the caller's time zone on the
  backend; without a zone it counts in `Europe/Warsaw`; an unknown zone is a 400.
- Active = open tasks due before the end of the range, overdue included, ordered by due date;
  completed = tasks done within the range.
- A task is completed and reopened with one click; completing a done task or reopening an open one is
  refused; only an open task can be edited (and handed over).
- Completing a task that belongs to an opportunity logs a `Task` activity on that opportunity; a
  redelivered message writes nothing twice, a completion after a reopen is a new entry.
- A company or opportunity of another organization, or an opportunity of another company, is refused
  without saying whether the id exists elsewhere; another organization's task is a 404.
- A project can optionally point at the opportunity it was sold as; the project page shows "Sold as"
  and the number of people placed on it.
- The workspace keeps the company in the URL (`?companyId=`), the last choice in `localStorage`.

## Surface
- Backend: `src/HrAgencySystem.Tasks` (schema `tasks`, aggregate `TaskItem`, `TaskRange`,
  `TaskItemProjection`), `src/HrAgencySystem.Tasks.Contracts` (`OpportunityTaskCompleted`),
  `Sales/Integration/OpportunityTaskCompletedHandler`, `IOpportunitySnapshotRepository` in
  `SharedKernel`; API `Endpoints/Tasks`: `GET/POST /api/tasks`, `GET/PUT /api/tasks/{id}`,
  `POST /api/tasks/{id}/complete|reopen` (OpenAPI tag "Sales - Tasks").
- Frontend: `frontend/src/features/tasks`, `frontend/src/features/sales/workspace`,
  `features/sales/components/OpportunitySelect`, route `/app/sales-workspace`.
- Tests: `tests/HrAgencySystem.UnitTests/Tasks`, `UnitTests/Sales/Handlers/OpportunityTaskCompletedHandlerTests.cs`,
  `tests/HrAgencySystem.IntegrationTests.Sales/Tasks`, `frontend/e2e/sales/sales-workspace.spec.ts`,
  Vitest `features/tasks/relativeDay.test.ts`, `features/sales/workspace/search.test.ts`.

## Out of scope for this feature
- A project created automatically from a won opportunity - not yet.
- Mail when a task is assigned to somebody else (the handover rule) - not yet.
- Due-date reminders, a calendar view, tasks for recruiters/HR - not yet.
- A combined `GET /api/sales/workspace` endpoint - not ever (rejected: it would compose three
  modules' reads in the host).
- Company statistics tiles in the left panel - dropped during the work.
- Turning `FollowUpAction` into a task - not ever; they are different things.

## Acceptance criteria
- [x] Day/week/month boundaries, Monday start, a month across the clock change, unknown zone refused, half-open ranges — `./verify tasks` (`TaskRangeTests`)
- [x] Each range holds open tasks due before its end, overdue included — `./verify tasks` (`Each_range_holds_the_open_tasks_due_before_its_end_overdue_included`)
- [x] Done → completed section → back on reopen; a second completion refused — `./verify tasks` (`TasksTests`, `TaskItemHandlerTests`)
- [x] Completing a deal's task logs it on the opportunity — `./verify tasks` (`Completing_a_task_of_a_deal_logs_it_on_the_opportunity`) and `./verify sales` (`OpportunityTaskCompletedHandlerTests`)
- [x] The board is the caller's own and narrows to one company — `./verify tasks`
- [x] Cross-organization company, opportunity and task are refused or not found — `./verify tasks`
- [x] An open task can be changed and handed over — `./verify tasks` (`An_open_task_can_be_changed_and_handed_over`)
- [x] A project links to an opportunity and counts its people — `./verify projects` (`ProjectOpportunityTests`)
- [x] Workspace: company panel, tabs, company switch, range cut, one-click done/undo, add task — e2e: `frontend/e2e/sales/sales-workspace.spec.ts`; evidence: `docs/screenshots/sales-workspace.png`, `sales-workspace-tasks.png`, `sales-workspace-projects.png`

## Decisions
- Separate `Tasks` module talking to `Sales` only through `Tasks.Contracts`: [ADR-0006](../../../docs/adr/0006-contracts-and-shared-kernel-ports.md).
- Cascaded `OutgoingMessages` on a durable local queue declared by the consumer (`SalesModule.ConfigureWolverine`): [ADR-0003](../../../docs/adr/0003-wolverine-static-handlers.md), [ADR-0011](../../../docs/adr/0011-sagas-for-time-windows.md).
- Tenant check in every handler and query, no new role policy: [ADR-0004](../../../docs/adr/0004-tenant-on-every-aggregate.md).
- The aggregate is `TaskItem` because `Task` collides with `System.Threading.Tasks.Task`; its id is a plain `Guid`.
- The activity id is derived from (task, completion number), which makes the integration handler idempotent without a guard table.
- The range is computed on the backend from the browser's zone, so "today" is the same thing for the board and the tests.

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-24 | b2ed201d | Tasks module with day/week/month board, one-click done and reopen |
| 2026-09-24 | cc99a125 | Opportunity snapshot port, `Task` activity from a done task, last activity on an opportunity |
| 2026-09-24 | f495bb91 | Optional opportunity link and people count on a project |
| 2026-09-24 | f23e15aa | Seeded clients, deals, projects and tasks for the workspace |
| 2026-09-24 | de3fa574 | Regenerated API client |
| 2026-09-24 | 7b6bed0b | Sales workspace screen, task list, e2e and screenshots |
| 2026-10-01 | dd423353 | Task integration tests move to `IntegrationTests.Sales` |

## Notes from reconstruction
- The plan placed the route at `/app/sales/workspace` and the API under `/api/sales/tasks`; the code
  uses `/app/sales-workspace` (a child of `/app/sales` would light two sidebar items) and `/api/tasks`.
- The plan's integration message `TaskItemCompletedIntegration` became `OpportunityTaskCompleted`.
- `SalesActivityType.Task` was inserted before `Other`; Marten stores enums as numbers, so `Other`
  moved from 5 to 6 - harmless only because the project never migrates data.
- The opportunity select shows the first 50 opportunities of a company; more are not reachable.
- Went straight to `main` without a PR.
