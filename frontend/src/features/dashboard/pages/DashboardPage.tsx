import { BarChart3 } from "lucide-react";
import type React from "react";
import { ExportButton } from "#/features/reports/components/ExportButton";
import { ReportRangeFilter } from "#/features/reports/components/ReportRangeFilter";
import {
	exportHref,
	type ReportRange,
	reportPeriod,
	reportRanges,
} from "#/features/reports/period";
import { Page } from "@/components/layout";
import { EmptyState } from "@/components/ui";
import { FunnelChart, MonthlyActivityChart, RecruitmentMetrics, SourcesTable } from "./components";
import { useRecruitmentReport } from "./hooks/useRecruitmentReport";
import "#/features/reports/reports.css";

interface DashboardPageProps {
	range: ReportRange;
	onRangeChange: (range: ReportRange) => void;
}

/**
 * The organization's recruitment over the last few months, from the reports service. The range
 * lives in the url, so a link to "the last year" opens the last year.
 */
const DashboardPage: React.FC<DashboardPageProps> = ({ range, onRangeChange }) => {
	const period = reportPeriod(range);
	const query = useRecruitmentReport(period);
	const report = query.data;

	const nothingHappened =
		report !== undefined &&
		Number(report.totals.applications) === 0 &&
		Number(report.totals.jobPostsPublished) === 0;

	return (
		<Page
			title="Dashboard"
			description={`Recruitment in the last ${reportRanges[range]}, counted in whole calendar months.`}
			onRefresh={() => query.refetch()}
			loading={query.isPending}
			isEmpty={nothingHappened}
			emptyState={
				<EmptyState
					title="Nothing happened in this period"
					description="No job posts went out and no applications came in. Try a longer range."
				>
					<BarChart3 size={24} />
				</EmptyState>
			}
			headerAddon={
				<div className="report-toolbar">
					<ReportRangeFilter value={range} onChange={onRangeChange} />
					<ExportButton href={exportHref("/api/reports/recruitment/export", period)} />
				</div>
			}
		>
			{query.isError ? (
				<p className="form-error">
					The report could not be loaded - the reports service may be down.
				</p>
			) : null}

			{report && !nothingHappened ? (
				<>
					<RecruitmentMetrics report={report} />

					<div className="report-grid">
						<FunnelChart funnel={report.funnel} />
						<MonthlyActivityChart months={report.months} />
					</div>

					<div className="report-grid">
						<SourcesTable sources={report.sources} total={Number(report.totals.applications)} />
					</div>
				</>
			) : null}
		</Page>
	);
};

export default DashboardPage;
