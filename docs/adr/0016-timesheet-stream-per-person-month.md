# ADR-0016: One time sheet stream per person and month, no "rejected" state, money only in the export

- **Status:** Accepted (amended 2026-09-22: the hourly rate and the Excel export added the only money)
- **Date:** 2026-09-22
- **Evidence:** `7e99563e` feat(agency): rcp backend,
  `feae3fdf` fix(agency): a time sheet that was never written on answers not found, and the seeder only closes months it filled,
  `f9d3dde3` feat(agency): tell somebody when their month is approved, sent back or settled,
  `59cba475` feat(agency): put a rate on the contract and export the settled month to Excel
- **Specs:** [022-time-sheets-and-settlement](../../.shipit/specs/done/022-time-sheets-and-settlement.md),
  [021-agency-chart-and-roles](../../.shipit/specs/done/021-agency-chart-and-roles.md)

## Context
Agency staff on an employment contract or a mandate must record hours; B2B and contracts for
specific work do not. A month of hours is entered by the person, approved by their supervisor and
handed to payroll. "One sheet per person per month" must hold, and the system must know who owes
hours at all.

## Decision
- `TimeSheet` is an aggregate whose stream id is **derived** from
  `(organization, user, year, month)` (`AgencyStreamId.ForTimeSheet`, SHA-256 over a fixed
  namespace). A second sheet for the same month has nowhere to go; no reservation document, no read
  before write. `AgencyEmployment` uses the same trick per `(organization, user)`.
- Statuses `Draft → Submitted → Approved → Settled`, plus `Correction`; graph in
  `TimeSheetStatusChangePolicy`. There is **no rejected state**: hours are corrected, not refused.
- A day is a start plus a length in minutes (multiples of five); a shift may cross midnight and
  belongs to the day it started.
- `TimeRecordPolicy` computes who owes hours from `WorkerContractType`; it is law, never stored.
- The supervisor is resolved at the moment of approval, not frozen on the sheet.
- No money in the domain: `TimeSheetSettled` carries no amount. The rate (`WorkRate?`) is a term of
  `AgencyEmployment`; only the settlement export multiplies minutes by an **hourly** rate, rounded
  once per person (`AwayFromZero`). A daily or monthly rate leaves the amount empty.

## Alternatives considered
- Plan 017 (local, not in git) rejected a `SettlementRun` aggregate (a batch only pays off when it
  must be reverted or summed, and without amounts there is nothing to sum) and rejected amounts
  altogether.
- Plan 022 explicitly reverses part of that: the rate enters as a contract term and money enters the
  **report**, not the domain. It chose the rate in force at export time over a history of terms,
  printing the rate used in its own column.
- A plain `decimal` rate was rejected in favour of the existing `WorkRate` (currency, unit,
  gross/net).
- Entering hours on someone else's behalf was left open in plan 017 and is not built.

## Consequences
- A sheet that was never written answers 404 (`feae3fdf`), not an empty draft.
- A mid-month rate change is exported with the rate current at export time.
- Rates are hidden (`null`) from anyone outside `RatesPolicy`; changing terms without that role keeps
  the stored rate.

## Revisit when
- Someone asks to undo a settled month (the trigger plan 017 named for `SettlementRun`).
- Rates change mid-month often enough that a history of terms is needed.
- Payroll needs contributions, taxes or night/overtime premiums - that is a payroll system.
