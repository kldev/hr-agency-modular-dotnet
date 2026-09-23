import { Bar, BarChart, CartesianGrid, Cell, LabelList, Tooltip, XAxis, YAxis } from "recharts";
import type { RecruitmentFunnel } from "#/api/models";
import { DetailOverviewHeader } from "@/components/ui";
import { axisTick, ChartFrame, chartColors, tooltipStyle } from "@/components/ui/charts";

interface FunnelChartProps {
	funnel: RecruitmentFunnel;
}

/** Deeper stages in stronger colours, so the narrowing reads before the numbers do. */
const stages = [
	{ key: "applied", label: "Applied", color: chartColors.info },
	{ key: "screening", label: "Screening", color: chartColors.cyan },
	{ key: "interview", label: "Interview", color: chartColors.violet },
	{ key: "assessment", label: "Assessment", color: chartColors.orange },
	{ key: "offer", label: "Offer", color: chartColors.warning },
	{ key: "hired", label: "Hired", color: chartColors.success },
] as const;

export function FunnelChart({ funnel }: FunnelChartProps) {
	const data = stages.map((stage) => ({
		stage: stage.label,
		applications: Number(funnel[stage.key]),
		color: stage.color,
	}));

	return (
		<section className="data-details-section">
			<DetailOverviewHeader
				title="Funnel"
				description="Applications received in the period, by the furthest stage each has reached so far."
			/>

			<div className="report-section-body">
				<ChartFrame label="Recruitment funnel by stage">
					<BarChart data={data} layout="vertical" margin={{ left: 8, right: 40 }}>
						<CartesianGrid horizontal={false} stroke={chartColors.grid} />
						<XAxis type="number" allowDecimals={false} tick={axisTick} />
						<YAxis type="category" dataKey="stage" width={90} tick={axisTick} />
						<Tooltip {...tooltipStyle} />
						<Bar dataKey="applications" name="Applications" radius={[0, 4, 4, 0]}>
							{data.map((entry) => (
								<Cell key={entry.stage} fill={entry.color} />
							))}
							<LabelList dataKey="applications" position="right" fill={chartColors.muted} />
						</Bar>
					</BarChart>
				</ChartFrame>

				<p className="report-hint">
					Rejected: {funnel.rejected} · Withdrawn: {funnel.withdrawn}
				</p>
			</div>
		</section>
	);
}
