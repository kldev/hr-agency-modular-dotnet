import { CircleDollarSign, Handshake, Target } from "lucide-react";
import type { OpportunityStage } from "#/api/models";
import { MetricCard, Metrics } from "@/components/ui/MetricCard";
import {
	formatStageValues,
	pipelineStages,
	type StageTotals,
	sumStageCount,
	sumStageValues,
} from "../hooks";

interface SalesMetricsProps {
	totals: Record<OpportunityStage, StageTotals>;
}

// stages still in play - Won and Lost are no longer pipeline
const openStages: OpportunityStage[] = ["New", "Viewed", "Contacted", "Qualified", "Proposal"];

export function SalesMetrics({ totals }: SalesMetricsProps) {
	return (
		<Metrics columns={3}>
			<MetricCard
				icon={<Target size={17} />}
				label="Opportunities"
				value={sumStageCount(totals, pipelineStages)}
			/>

			<MetricCard
				icon={<CircleDollarSign size={17} />}
				label="Pipeline value"
				value={formatStageValues(sumStageValues(totals, openStages))}
			/>

			<MetricCard
				icon={<Handshake size={17} />}
				label="Won value"
				value={formatStageValues(sumStageValues(totals, ["Won"]))}
			/>
		</Metrics>
	);
}
