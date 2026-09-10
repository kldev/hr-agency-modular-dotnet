import clsx from "clsx";
import type { OpportunityStage } from "@/api/models";

const statusClass: Record<OpportunityStage, string> = {
	New: "badge-new",
	Viewed: "badge-viewed",
	Contacted: "badge-contacted",
	Qualified: "badge-qualified",
	Proposal: "badge-proposal",
	Won: "badge-won",
	Lost: "badge-lost",
};

export function OpportunityStageBadge({ status }: { status: OpportunityStage }) {
	return <span className={clsx("badge", statusClass[status])}>{status}</span>;
}
