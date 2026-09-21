import { useParams } from "@tanstack/react-router";
import type React from "react";
import { useRef } from "react";
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
	AttachAssignmentDocumentDrawer,
	type AttachAssignmentDocumentFormCommand,
	ChangeAssignmentStatusDrawer,
	type ChangeAssignmentStatusFormCommand,
	EditAssignmentDrawer,
	type EditAssignmentFormCommand,
	RecordAssignmentComplianceDrawer,
	type RecordAssignmentComplianceFormCommand,
} from "../drawers";
import {
	AssignmentComplianceSection,
	AssignmentDocumentsSection,
	AssignmentOverviewSection,
	AssignmentStatusSidebar,
} from "./components/details";
import { AssignmentActions } from "./components/table";
import { useGetAssignment } from "./hooks";

const AssignmentDetailsPage: React.FC = () => {
	const { id } = useParams({ from: "/app/assignments/$id" });

	const editRef = useRef<EditAssignmentFormCommand>(null);
	const statusRef = useRef<ChangeAssignmentStatusFormCommand>(null);
	const documentRef = useRef<AttachAssignmentDocumentFormCommand>(null);
	const complianceRef = useRef<RecordAssignmentComplianceFormCommand>(null);

	const query = useGetAssignment(id);
	const assignment = query.data;

	const refresh = () => void query.refetch();

	if (!id || query.isLoading || query.isError || !assignment) {
		return (
			<DetailsLoading id={id} isLoading={query.isLoading} isError={query.isError || !assignment} />
		);
	}

	return (
		<>
			<DataDetails>
				<DetailsHeader
					name={`${assignment.workerFullName} — ${assignment.projectName}`}
					onEdit={() => editRef.current?.edit(assignment)}
					detailsAddons={
						<>
							<AssignmentStatusBadge status={assignment.status} />

							<span className="badge badge-inactive">
								{engagementTypes[assignment.engagementType]}
							</span>
						</>
					}
					extraAdd={
						<AssignmentActions
							id={assignment.id}
							mode="details"
							onChangeStatus={() => statusRef.current?.changeStatus(assignment)}
						/>
					}
				/>

				<DataDetailsLayout
					main={
						<>
							<section className="data-details-section">
								<AssignmentOverviewSection assignment={assignment} />
							</section>

							<section className="data-details-section">
								<AssignmentDocumentsSection
									assignment={assignment}
									onAttach={() => documentRef.current?.attach(assignment.id)}
									onRefresh={refresh}
								/>
							</section>

							<section className="data-details-section">
								<AssignmentComplianceSection
									assignment={assignment}
									onRecord={(view) => complianceRef.current?.record(assignment, view)}
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

			<EditAssignmentDrawer ref={editRef} onSuccess={refresh} />
			<ChangeAssignmentStatusDrawer ref={statusRef} onSuccess={refresh} />
			<AttachAssignmentDocumentDrawer ref={documentRef} onSuccess={refresh} />
			<RecordAssignmentComplianceDrawer ref={complianceRef} onSuccess={refresh} />
		</>
	);
};

export default AssignmentDetailsPage;
