import type { RateBasis, RateUnit, WorkerContractType } from "@/api/models";

/**
 * What we sign with the person, which is not the same question as how we engage them for the
 * client. `EngagementType` keys their compliance; this decides which contract template applies and
 * what has to be in it — one role can have people on an employment contract and on a mandate.
 */
export const workerContractTypes: Record<WorkerContractType, string> = {
	EmploymentContract: "Employment contract",
	TemporaryEmploymentContract: "Temporary employment contract",
	MandateContract: "Mandate contract",
	SelfEmployed: "Self-employed (B2B)",
	Other: "Other",
};

export const rateUnits: Record<RateUnit, string> = {
	Hourly: "per hour",
	Daily: "per day",
	Monthly: "per month",
};

/** Without this "32 PLN" says nothing, which is why the backend refuses to store a rate without it. */
export const rateBases: Record<RateBasis, string> = {
	Gross: "gross",
	Net: "net",
};

export const rateUnitShort: Record<RateUnit, string> = {
	Hourly: "h",
	Daily: "day",
	Monthly: "month",
};
