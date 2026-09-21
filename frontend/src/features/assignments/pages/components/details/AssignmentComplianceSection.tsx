import type { AssignmentProjection, ComplianceRequirementView } from "@/api/models";
import { ComplianceChecklist } from "@/features/compliance";
import { useGetAssignmentComplianceCatalogue } from "../../hooks";

interface AssignmentComplianceSectionProps {
	assignment: AssignmentProjection;
	onRecord: (view: ComplianceRequirementView) => void;
}

/**
 * The register this whole module exists for. What used to be one tick on a project saying that
 * everybody was covered is answered here per named person, per period, per posting company -
 * which is how an A1 is actually issued.
 */
export function AssignmentComplianceSection({
	assignment,
	onRecord,
}: AssignmentComplianceSectionProps) {
	const query = useGetAssignmentComplianceCatalogue(assignment.id);

	const documentName = (documentId: string | null) =>
		assignment.documents.find((document) => document.documentId === documentId)?.fileName ?? "—";

	return (
		<ComplianceChecklist
			description="What this person owes for this posting, in this country, under this engagement type."
			views={query.data}
			isLoading={query.isLoading}
			isError={query.isError}
			emptyTitle="Nothing is required of this person for this posting"
			emptyDescription="Working in the country where the agency is established raises no per person duties. That is an answer, not missing data."
			documentName={documentName}
			onRecord={onRecord}
		/>
	);
}
