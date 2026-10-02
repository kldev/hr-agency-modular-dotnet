# 022 · Time sheets and settlement

**Status:** done · **Built:** 2026-09-22 → 2026-09-22 · **Verify:** `./verify agency` (mail bodies: `./verify emails`) · **Plan:** backend 017, 021, 022 / frontend 012 (local, not in git)

## Why
The agency's own people on contracts that carry a legal duty to record hours need a monthly time
sheet: they type their days, their supervisor approves, payroll closes the month. Before this the
system did not even know who works for the agency on which contract, so it could not tell "has not
filled in" from "does not have to". Payroll then copied the hours into a spreadsheet of their own to
work out transfers, and people never learned that their month had been approved or sent back.

## What it promises
- Staff record an agency employment per person: contract type (`WorkerContractType`), period, weekly
  hours, optional rate (`WorkRate`). Terms can change and an engagement can end.
- Whether somebody owes hours is computed (`TimeRecordPolicy`): employment and mandate contracts do,
  B2B and contract for specific work do not. Never stored.
- A person enters a day as a start and a length; minutes in steps of five, more than zero, at most a
  day; a day may run past midnight and still belongs to the day it started on. Saving a day twice
  replaces it. Future months are refused; the current month counts.
- One sheet per person per month, `Draft → Submitted → Approved → Settled`, plus `Correction`; no
  "rejected" state. Only `Draft` and `Correction` can be typed into; an empty month cannot be submitted.
- The owner submits, optionally with a note on the sheet's thread. The supervisor from the chart -
  directly or further up the same line - approves or sends back; a head of another branch and the
  person themselves are refused. Sending back requires a reason.
- Payroll (`HumanResources`, `Admin`) settles approved months and may send back without a place in
  the chart. A month nobody approved cannot be settled.
- A supervisor's team view lists everybody below them who owes hours, including those who have not
  started a sheet; people without the duty are not listed.
- The owner gets a mail when the month is approved (with the note), sent back (with the reason and
  in which capacity) or settled - but not when they decided on their own month.
- Rates are shown only to `RatesPolicy` (`HumanResources`, `Finance`, `Admin`); others read `null`,
  and changing terms without that role keeps the rate in force.
- `GET /api/timesheets/settlement/export` downloads an `.xlsx` of the settlement list: a Summary
  sheet opening with a sentence ("not a payslip"), one row per person and a total, and a Details
  sheet with one row per day, marking days that end after midnight. Amount = minutes × hourly rate,
  rounded once per person, away from zero; a monthly/daily rate or no rate gives an empty amount and
  the row is kept. Downloading changes nothing.

## Surface
- Backend: `src/HrAgencySystem.Agency` (`AgencyEmployment`, `TimeSheet`, `TimeRecordPolicy`,
  `TimeSheetStatusPolicy`, `Application/TimeSheets/Export`), `src/HrAgencySystem.Api/Auth/PayrollPolicy.cs`,
  `RatesPolicy.cs`; mail records in `src/EmailTemplates/HrAgencySystem.EmailTemplates.Contracts/Agency`,
  queue `q.emails.agency`. Endpoints `/api/agency-employments[/{userId}[/terms|/end]]`,
  `/api/timesheets/my[/days[/{date}]|/submit]`, `/api/timesheets/team`, `/api/timesheets/settlement[/export]`,
  `/api/timesheets/{userId}/{year}/{month}[/approve|/return|/settle|/comments]`.
- Frontend: `frontend/src/features/timesheets`, `features/agency-employment`; routes `/app/timesheets`
  (month and tab in the URL; tabs My hours / Team / Approvals / Settlement), `/app/employment`,
  Employment card on `/app/users/$id`.
- Tests: `tests/HrAgencySystem.UnitTests/Agency`, `tests/HrAgencySystem.IntegrationTests.Agency/Agency/SettlementExportTests.cs`,
  `tests/HrAgencySystem.EmailTemplates.UnitTests`.

## Out of scope for this feature
- Leave requests (plan 016) - not yet; drafted, never implemented.
- Entering hours on somebody else's behalf - not yet; `/my/days` always takes the signed-in user.
- Hours of people placed with clients - not ever here; a future separate module.
- Contributions, tax, overtime, night supplements, a payslip - not ever in this module.
- A history of rates per day - not yet; the rate in force at export time is used.
- An amount on `TimeSheetSettled`, a `SettlementRun` aggregate, CSV export - not ever as designed.
- A mail on submission or on a bare comment, in-app notifications - not yet.

## Acceptance criteria
- [x] Day entry rules (five-minute steps, past midnight, future months, replace on save) — `./verify agency`
- [x] Status graph, no settle without approval, settled goes nowhere — `./verify agency`
- [x] Only the chart's supervisor approves; payroll returns without a place in the chart — `./verify agency`
- [x] Team view includes people who have not started and excludes those without the duty — `./verify agency`
- [x] Rates refused or kept for roles not shown pay — `./verify agency`
- [x] Mails go to the owner, not for one's own month — `./verify agency`; bodies render — `./verify emails`
- [x] Export: payroll and finance download it, amounts worked out, supervisor not shown rates, other organization not included — `./verify agency`
- [x] Workbook shape, rounding, totals in one currency only — `./verify agency`
- [ ] The time sheet and employment screens — unproven (no e2e or screenshot found)
- [ ] Mail actually delivered through RabbitMQ and the worker — unproven (no broker test, by plan 021)

## Decisions
- Agency as employer, separate from `Workers`; roles decide only payroll and rates: [ADR-0015](../../../docs/adr/0015-agency-separate-from-workers.md).
- Stream per (organization, user, year, month), no rejected state, money only in the export: [ADR-0016](../../../docs/adr/0016-timesheet-stream-per-person-month.md).
- Mail as returned messages on a fourth topic domain `agency`: [ADR-0010](../../../docs/adr/0010-email-over-rabbitmq.md), [ADR-0003](../../../docs/adr/0003-wolverine-static-handlers.md).
- The supervisor is computed at the moment of the action, not frozen on the sheet: after a
  reorganisation the current supervisor approves.
- `DocumentFormat.OpenXml` for the workbook, generated inside the module - one file on demand does not
  earn its own project.

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-22 | `7e99563e` | Backend: employment, time sheets, approvals, team view |
| 2026-09-22 | `5c0ea092` | A day without a note is not a validation error |
| 2026-09-22 | `feae3fdf` | Never-written sheet answers not found |
| 2026-09-22 | `ef0debde` | Front: vocabulary and widgets |
| 2026-09-22 | `f334df4d` | Front: employment register and monthly hours register |
| 2026-09-22 | `4c1b933e` | Month as a list of days on narrow screens |
| 2026-09-22 | `b90383cf` | A note travels with a submitted month |
| 2026-09-22 | `f9d3dde3` | Mails on approve, send back, settle |
| 2026-09-22 | `59cba475` | Rate on the contract, Excel export |
| 2026-09-22 | `68366c33` | Front: rate on employment, download the settled month |
| 2026-09-22 | `e9175aac` | Decimal comma in money fields |

## Notes from reconstruction
- Plan 017 said "no money ever" and CSV export; plan 022 reversed that for one report only.
- The code computes the amount from minutes, not from the rounded decimal hours the plan described;
  the total is filled only for a single currency; Summary gained e-mail, unit and basis columns.
- `Finance` may download the file (API), but the Settlement tab is gated by the payroll check on the
  front, so Finance has no button.
- The employment endpoints carry no role policy apart from the rate: any member can record or end
  somebody's employment.
