import type {
	ComplianceRequirementView,
	ContactRole,
	EmailPurpose,
	ProjectProjection,
} from "@/api/models";

export interface AssignProjectTeamFormCommand {
	assignTeam: (project: ProjectProjection) => void;
}

export interface ChangeProjectStatusFormCommand {
	changeStatus: (project: ProjectProjection) => void;
}

export interface AssignProjectContactFormCommand {
	assign: (project: ProjectProjection, role: ContactRole) => void;
}

export interface SetProjectEmailsFormCommand {
	edit: (project: ProjectProjection, purpose: EmailPurpose) => void;
}

export interface RecordContractFormCommand {
	record: (project: ProjectProjection) => void;
}

export interface AttachDocumentFormCommand {
	attach: (projectId: string) => void;
}

export interface RecordComplianceFormCommand {
	record: (project: ProjectProjection, requirement: ComplianceRequirementView) => void;
}
