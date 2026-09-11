import type { OpportunityStage } from "@/api/models";

export const salesStage: Record<OpportunityStage, string> = {
	New: "New",
	Viewed: "Viewed",
	Contacted: "Contacted",
	Qualified: "Qualified",
	Proposal: "Proposal",
	Won: "Won",
	Lost: "Lost",
};
