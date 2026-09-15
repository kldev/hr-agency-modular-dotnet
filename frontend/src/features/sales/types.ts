import type { CurrencyCode, OpportunityStage, SalesActivityType } from "@/api/models";

export const salesStage: Record<OpportunityStage, string> = {
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

export const activityTypeOptions: Record<SalesActivityType, string> = {
	Call: "Call",
	Email: "Email",
	Meeting: "Meeting",
	Note: "Note",
	Presentation: "Presentation",
	Other: "Other",
};
