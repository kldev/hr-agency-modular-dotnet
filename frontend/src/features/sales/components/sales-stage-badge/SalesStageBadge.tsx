import { CheckCircle2, CircleDot, CircleX, ViewIcon } from "lucide-react";
import type { OpportunityStage } from "#/api/models";
import "./sales-badge.css";

interface SalesStageBadgeProps {
	stage: OpportunityStage;
}

const stageConfig: Record<
	OpportunityStage,
	{
		label: string;
		className: string;
		icon: typeof CircleDot;
	}
> = {
	New: {
		label: "New",
		className: "sales-stage-new",
		icon: CircleDot,
	},
	Viewed: {
		label: "Viewed",
		className: "sales-stage-viewed",
		icon: ViewIcon,
	},
	Contacted: {
		label: "Contacted",
		className: "sales-stage-contacted",
		icon: CircleDot,
	},
	Qualified: {
		label: "Qualified",
		className: "sales-stage-qualified",
		icon: CircleDot,
	},
	Proposal: {
		label: "Proposal",
		className: "sales-stage-proposal",
		icon: CircleDot,
	},
	Won: {
		label: "Won",
		className: "sales-stage-won",
		icon: CheckCircle2,
	},
	Lost: {
		label: "Lost",
		className: "sales-stage-lost",
		icon: CircleX,
	},
};

export function SalesStageBadge({ stage }: SalesStageBadgeProps) {
	const config = stageConfig[stage];
	const Icon = config.icon;

	return (
		<span className={`sales-stage-badge ${config.className}`}>
			<Icon size={13} />
			{config.label}
		</span>
	);
}
