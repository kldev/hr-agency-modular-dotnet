import type { ComplianceRequirementView, ProjectProjection } from "@/api/models";
import { ComplianceChecklist } from "@/features/compliance";
import { useGetComplianceCatalogue } from "../../hooks";

interface ProjectComplianceSectionProps {
	project: ProjectProjection;
	onRecord: (view: ComplianceRequirementView) => void;
}

/**
 * What the delivering entity owes for the engagement as a whole. The per person half of the same
 * catalogue - the A1, the Limosa declaration, the local contract - is answered on each assignment.
 */
export function ProjectComplianceSection({ project, onRecord }: ProjectComplianceSectionProps) {
	const query = useGetComplianceCatalogue(project.id);

	const documentName = (documentId: string | null) =>
		project.documents.find((document) => document.documentId === documentId)?.fileName ?? "—";

	return (
		<ComplianceChecklist
			description="What this country and this engagement type require of us, and where each of them stands."
			views={query.data}
			isLoading={query.isLoading}
			isError={query.isError}
			emptyTitle="No country-specific requirements for this engagement"
			emptyDescription="Work in the country where the agency is established does not trigger a host state's duties. That is an answer, not missing data."
			documentName={documentName}
			onRecord={onRecord}
		/>
	);
}
