import type { AssignmentProjection } from "@/api/models";
import { AssignmentStatusBadge, DetailItem, DetailOverviewHeader } from "@/components/ui";
import { ComplianceChip } from "@/features/compliance";
import { formatDate } from "@/utlis/dateUtils";
import { assignmentStatusDescriptions } from "../../../types";

interface AssignmentStatusSidebarProps {
	assignment: AssignmentProjection;
}

export function AssignmentStatusSidebar({ assignment }: AssignmentStatusSidebarProps) {
	return (
		<div className="data-overview">
			<DetailOverviewHeader
				title="Status"
				description={assignmentStatusDescriptions[assignment.status]}
			/>

			<dl className="data-details-list">
				<DetailItem label="Status">
					<AssignmentStatusBadge status={assignment.status} />
				</DetailItem>

				<DetailItem label="Documents">
					<span className="data-detail-number">{Number(assignment.documentCount)}</span>
				</DetailItem>

				<DetailItem label="Required">
					<span className="data-detail-number">{Number(assignment.complianceRequiredCount)}</span>
				</DetailItem>

				<DetailItem label="Compliance">
					<ComplianceChip
						outstanding={assignment.complianceOutstandingCount}
						nextExpiryOn={assignment.nextComplianceExpiryOn}
					/>
				</DetailItem>

				<DetailItem label="Next expiry">
					{assignment.nextComplianceExpiryOn ? formatDate(assignment.nextComplianceExpiryOn) : "—"}
				</DetailItem>
			</dl>
		</div>
	);
}
