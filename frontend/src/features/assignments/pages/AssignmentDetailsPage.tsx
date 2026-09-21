import { useParams } from "@tanstack/react-router";
import type React from "react";
import type { UserSnapshot } from "@/api/models";
import {
	AssignmentStatusBadge,
	AuditInformation,
	DetailsHeader,
	DetailsLoading,
} from "@/components/ui";
import { DataDetails, DataDetailsLayout } from "@/components/ui/details/DataDetails";
import { engagementTypes } from "@/features/compliance";
import {
	AssignmentComplianceSection,
	AssignmentDocumentsSection,
	AssignmentOverviewSection,
	AssignmentStatusSidebar,
} from "./components/details";
import { useGetAssignment } from "./hooks";

const AssignmentDetailsPage: React.FC = () => {
	const { id } = useParams({ from: "/app/assignments/$id" });

	const query = useGetAssignment(id);
	const assignment = query.data;

	if (!id || query.isLoading || query.isError || !assignment) {
		return (
			<DetailsLoading id={id} isLoading={query.isLoading} isError={query.isError || !assignment} />
		);
	}

	return (
		<DataDetails>
			<DetailsHeader
				name={`${assignment.workerFullName} — ${assignment.projectName}`}
				detailsAddons={
					<>
						<AssignmentStatusBadge status={assignment.status} />

						<span className="badge badge-inactive">
							{engagementTypes[assignment.engagementType]}
						</span>
					</>
				}
			/>

			<DataDetailsLayout
				main={
					<>
						<section className="data-details-section">
							<AssignmentOverviewSection assignment={assignment} />
						</section>

						<section className="data-details-section">
							<AssignmentDocumentsSection assignment={assignment} />
						</section>

						<section className="data-details-section">
							<AssignmentComplianceSection
								assignment={assignment}
								onRecord={() => {
									// The recording drawer lands with the next step of the plan.
								}}
							/>
						</section>
					</>
				}
				sidebar={
					<>
						<section className="data-details-section">
							<AssignmentStatusSidebar assignment={assignment} />
						</section>

						<AuditInformation
							createdAt={assignment.createdAt}
							createdBy={assignment.createdBy as UserSnapshot}
							modifiedAt={assignment.modifiedAt}
							modifiedBy={assignment.modifiedBy}
						/>
					</>
				}
			/>
		</DataDetails>
	);
};

export default AssignmentDetailsPage;
