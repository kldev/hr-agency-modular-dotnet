import type { ProjectProjection } from "@/api/models";
import { DetailItem, DetailOverviewHeader, ProjectStatusBadge } from "@/components/ui";
import { formatDate } from "@/utlis/dateUtils";
import { ComplianceChip } from "../ComplianceChip";

interface ProjectStatusSidebarProps {
	project: ProjectProjection;
}

export function ProjectStatusSidebar({ project }: ProjectStatusSidebarProps) {
	return (
		<div className="data-overview">
			<DetailOverviewHeader title="Status" description="Where the engagement stands today." />

			<dl className="data-details-list">
				<DetailItem label="Status">
					<ProjectStatusBadge status={project.status} />
				</DetailItem>

				<DetailItem label="Team">{project.teamName ?? "Not assigned"}</DetailItem>

				<DetailItem label="Documents">
					<span className="data-detail-number">{project.documentCount}</span>
				</DetailItem>

				<DetailItem label="Compliance">
					<ComplianceChip
						outstanding={project.complianceOutstandingCount}
						nextExpiryOn={project.nextComplianceExpiryOn}
					/>
				</DetailItem>

				<DetailItem label="Next expiry">
					{project.nextComplianceExpiryOn ? formatDate(project.nextComplianceExpiryOn) : "—"}
				</DetailItem>
			</dl>
		</div>
	);
}
