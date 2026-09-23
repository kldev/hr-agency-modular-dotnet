import { Bar, BarChart, CartesianGrid, Legend, Tooltip, XAxis, YAxis } from "recharts";
import type { RecruitmentMonth } from "#/api/models";
import { formatMonth } from "#/features/reports/period";
import { DetailOverviewHeader } from "@/components/ui";
import { axisTick, ChartFrame, chartColors, tooltipStyle } from "@/components/ui/charts";

interface MonthlyActivityChartProps {
	months: RecruitmentMonth[];
}

export function MonthlyActivityChart({ months }: MonthlyActivityChartProps) {
	const data = months.map((month) => ({ ...month, label: formatMonth(month.month) }));

	return (
		<section className="data-details-section">
			<DetailOverviewHeader
				title="Month by month"
				description="What happened in each month - an offer made in May is May's, whenever its application came in."
			/>

			<div className="report-section-body">
				<ChartFrame label="Applications, interviews, offers and hires per month">
					<BarChart data={data} margin={{ left: -16 }}>
						<CartesianGrid vertical={false} stroke={chartColors.grid} />
						<XAxis dataKey="label" tick={axisTick} />
						<YAxis allowDecimals={false} tick={axisTick} />
						<Tooltip {...tooltipStyle} />
						<Legend wrapperStyle={{ fontSize: 12 }} />
						<Bar
							dataKey="applications"
							name="Applications"
							fill={chartColors.info}
							radius={[3, 3, 0, 0]}
						/>
						<Bar
							dataKey="interviewsScheduled"
							name="Interviews"
							fill={chartColors.violet}
							radius={[3, 3, 0, 0]}
						/>
						<Bar dataKey="offers" name="Offers" fill={chartColors.warning} radius={[3, 3, 0, 0]} />
						<Bar dataKey="hires" name="Hires" fill={chartColors.success} radius={[3, 3, 0, 0]} />
					</BarChart>
				</ChartFrame>
			</div>
		</section>
	);
}
