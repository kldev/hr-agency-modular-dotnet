import type { AssignmentProjection, ComplianceRequirementView } from "@/api/models";

export interface EditAssignmentFormCommand {
	edit: (assignment: AssignmentProjection) => void;
}

export interface ChangeAssignmentStatusFormCommand {
	changeStatus: (assignment: AssignmentProjection) => void;
}

export interface AttachAssignmentDocumentFormCommand {
	attach: (assignmentId: string) => void;
}

export interface RecordAssignmentComplianceFormCommand {
	record: (assignment: AssignmentProjection, view: ComplianceRequirementView) => void;
}
