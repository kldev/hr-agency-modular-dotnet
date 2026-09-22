import type { AgencyEmploymentProjection, OrganizationRole, WorkRate } from "@/api/models";
import { rateBases, rateUnitShort } from "@/features/positions/types";

export { contractRequiresTimeRecord, workerContractTypes } from "#/features/contracts/types";

/**
 * Mirrors `AgencyEmployment.IsEnded`. An engagement with an end date behind it is history: terms
 * cannot be changed on it and it cannot be ended twice - both refusals share one message on the
 * backend (`AlreadyEndedMessage`), so both actions disappear together here.
 */
export function isEnded(employment: AgencyEmploymentProjection): boolean {
	return Boolean(employment.endsOn);
}

/** Mirrors `StartAgencyEmploymentHandler.WeeklyHoursRangeMessage`. */
export const maxWeeklyHours = 168;

export const weeklyHoursRangeMessage = "Weekly hours must be between 0 and 168.";

export const alreadyEmployedMessage =
	"This person already has an employment record. Change its terms instead of adding a second one.";

/**
 * Why the register exists at all, said once so the empty state and the page description agree:
 * without it the monitoring screen cannot tell somebody who has not filled their hours in from
 * somebody who never has to.
 */
export const registerPurpose =
	"What each person works for us on. The contract decides who owes hours, so the time sheet monitoring reads this register.";

/**
 * Mirrors `Api/Auth/RatesPolicy.cs`: who is shown what somebody is paid. Wider than `isPayroll`,
 * because finance handles money without closing anybody's month; a supervisor approving hours is
 * deliberately not here. The backend answers `null` to everybody else, so this only decides whether
 * to offer fields and a column that would stay empty.
 */
export function isRates(role: OrganizationRole | undefined | null): boolean {
	return role === "HumanResources" || role === "Finance" || role === "Admin";
}

/** "45 PLN/h gross" - the unit and the basis are part of the number, not a footnote to it. */
export function formatRate(rate: WorkRate): string {
	return `${rate.amount} ${rate.currency}/${rateUnitShort[rate.unit]} ${rateBases[rate.basis]}`;
}
