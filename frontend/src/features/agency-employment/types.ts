import type { AgencyEmploymentProjection } from "@/api/models";

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
