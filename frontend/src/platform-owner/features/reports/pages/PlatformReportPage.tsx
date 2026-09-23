import { Globe2 } from "lucide-react";
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
import { OrganizationsActivityChart, OrganizationsTable, PlatformMetrics } from "./components";
import { usePlatformReport } from "./hooks/usePlatformReport";
import "#/features/reports/reports.css";

interface PlatformReportPageProps {
	range: ReportRange;
	onRangeChange: (range: ReportRange) => void;
}

/** Every tenant side by side - what the platform owner sees above all organizations. */
export function PlatformReportPage({ range, onRangeChange }: PlatformReportPageProps) {
	const period = reportPeriod(range);
	const query = usePlatformReport(period);
	const report = query.data;

	return (
		<Page
			title="Reports"
			description={`Activity across the platform in the last ${reportRanges[range]}.`}
			onRefresh={() => query.refetch()}
			loading={query.isPending}
			isEmpty={report !== undefined && report.organizations.length === 0}
			emptyState={
				<EmptyState
					title="No organizations yet"
					description="The platform has no tenants to report on."
				>
					<Globe2 size={24} />
				</EmptyState>
			}
			headerAddon={
				<div className="report-toolbar">
					<ReportRangeFilter value={range} onChange={onRangeChange} />
					<ExportButton href={exportHref("/api/owners/reports/platform/export", period)} />
				</div>
			}
		>
			{query.isError ? (
				<p className="form-error">
					The report could not be loaded - the reports service may be down.
				</p>
			) : null}

			{report && report.organizations.length > 0 ? (
				<>
					<PlatformMetrics totals={report.totals} />

					<div className="report-grid">
						<OrganizationsActivityChart organizations={report.organizations} />
					</div>

					<div className="report-grid">
						<OrganizationsTable organizations={report.organizations} />
					</div>
				</>
			) : null}
		</Page>
	);
}
