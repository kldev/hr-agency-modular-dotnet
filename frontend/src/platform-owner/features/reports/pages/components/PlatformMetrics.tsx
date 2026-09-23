import {
	Activity,
	BriefcaseBusiness,
	CheckCircle2,
	FileText,
	FolderKanban,
	Globe2,
} from "lucide-react";
import type { PlatformTotals } from "#/api/models";
import { MetricCard, Metrics } from "@/components/ui/MetricCard";

export function PlatformMetrics({ totals }: { totals: PlatformTotals }) {
	return (
		<Metrics columns={6}>
			<MetricCard label="Organizations" value={totals.organizations} icon={<Globe2 />} />
			<MetricCard
				label="Active in the period"
				value={totals.activeOrganizations}
				icon={<Activity />}
			/>
			<MetricCard
				label="Job posts published"
				value={totals.jobPostsPublished}
				icon={<BriefcaseBusiness />}
			/>
			<MetricCard label="Applications" value={totals.applications} icon={<FileText />} />
			<MetricCard label="Hires" value={totals.hires} icon={<CheckCircle2 />} />
			<MetricCard
				label="Projects went live"
				value={totals.projectsWentLive}
				icon={<FolderKanban />}
			/>
		</Metrics>
	);
}
