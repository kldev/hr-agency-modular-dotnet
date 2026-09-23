import { Bar, BarChart, CartesianGrid, Legend, Tooltip, XAxis, YAxis } from "recharts";
import type { OrganizationActivity } from "#/api/models";
import { DetailOverviewHeader } from "@/components/ui";
import { axisTick, ChartFrame, chartColors, tooltipStyle } from "@/components/ui/charts";

/** Ten names fit a chart; the table below has everybody. */
const Shown = 10;

export function OrganizationsActivityChart({
	organizations,
}: {
	organizations: OrganizationActivity[];
}) {
	const data = [...organizations]
		.sort((a, b) => Number(b.applications) - Number(a.applications))
		.slice(0, Shown)
		.map((organization) => ({ ...organization }));

	return (
		<section className="data-details-section">
			<DetailOverviewHeader
				title="Busiest organizations"
				description={`The ${Shown} organizations with the most applications in the period.`}
			/>

			<div className="report-section-body">
				<ChartFrame label="Applications, interviews and hires per organization">
					<BarChart data={data} margin={{ left: -16 }}>
						<CartesianGrid vertical={false} stroke={chartColors.grid} />
						<XAxis dataKey="name" tick={axisTick} interval={0} />
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
						<Bar dataKey="hires" name="Hires" fill={chartColors.success} radius={[3, 3, 0, 0]} />
					</BarChart>
				</ChartFrame>
			</div>
		</section>
	);
}
