import type { CurrencyCode, OpportunityStage, SalesActivityType } from "@/api/models";

export type { ViewMode as SalesView } from "@/components/ui";

export const salesStageOptions: Record<OpportunityStage, string> = {
	New: "New",
	Viewed: "Viewed",
	Contacted: "Contacted",
	Qualified: "Qualified",
	Proposal: "Proposal",
	Won: "Won",
	Lost: "Lost",
};

export const currenciesOptions: Record<CurrencyCode, string> = {
	PLN: "PLN",
	EUR: "EUR",
	USD: "USD",
	GBP: "GBP",
};

/** An activity somebody logs by hand - a completed task is written by the tasks module instead. */
export type LoggableActivityType = Exclude<SalesActivityType, "Task">;

export const activityTypeOptions: Record<LoggableActivityType, string> = {
	Call: "Call",
	Email: "Email",
	Meeting: "Meeting",
	Note: "Note",
	Presentation: "Presentation",
	Other: "Other",
};

export const activityTypeLabels: Record<SalesActivityType, string> = {
	...activityTypeOptions,
	Task: "Task",
};
