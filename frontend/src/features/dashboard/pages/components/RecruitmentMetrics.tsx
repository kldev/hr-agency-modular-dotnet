import {
	BriefcaseBusiness,
	CalendarDays,
	CheckCircle2,
	FileText,
	Handshake,
	Percent,
} from "lucide-react";
import type { RecruitmentReport } from "#/api/models";
import { MetricCard, Metrics } from "@/components/ui/MetricCard";

interface RecruitmentMetricsProps {
	report: RecruitmentReport;
}

/**
 * The period at a glance. Every tile is a number the reports service counted - the hire rate is
 * its own figure for the applications received in the period, never a delta made up here.
 */
export function RecruitmentMetrics({ report }: RecruitmentMetricsProps) {
	const { totals, funnel } = report;

	return (
		<Metrics columns={6}>
			<MetricCard
				label="Job posts published"
				value={totals.jobPostsPublished}
				icon={<BriefcaseBusiness />}
			/>
			<MetricCard label="Applications" value={totals.applications} icon={<FileText />} />
			<MetricCard
				label="Interviews scheduled"
				value={totals.interviewsScheduled}
				icon={<CalendarDays />}
			/>
			<MetricCard label="Offers made" value={totals.offers} icon={<Handshake />} />
			<MetricCard label="Hires" value={totals.hires} icon={<CheckCircle2 />} />
			<MetricCard
				label="Hire rate"
				value={funnel.hireRate == null ? "—" : `${(Number(funnel.hireRate) * 100).toFixed(1)}%`}
				icon={<Percent />}
			/>
		</Metrics>
	);
}
