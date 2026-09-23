import { createFileRoute } from "@tanstack/react-router";
import { defaultReportRange, parseReportRange, type ReportRange } from "#/features/reports/period";
import { PlatformReportPage } from "#/platform-owner/features/reports/pages/PlatformReportPage";

export const Route = createFileRoute("/admin/reports/")({
	validateSearch: (search): { range?: ReportRange } => ({
		range: parseReportRange(search.range),
	}),
	component: RouteComponent,
});

function RouteComponent() {
	const { range } = Route.useSearch();
	const navigate = Route.useNavigate();

	return (
		<PlatformReportPage
			range={range ?? defaultReportRange}
			onRangeChange={(next) =>
				navigate({ search: { range: next === defaultReportRange ? undefined : next } })
			}
		/>
	);
}
