import type {
	ContactRole,
	ContractStatus,
	DocumentCategory,
	EmailPurpose,
	ProjectStatus,
} from "@/api/models";

/*
 * The compliance vocabulary lives in `features/compliance`, because assignments answer the same
 * catalogue. Re-exported here so the project screens keep one import.
 */
export {
	assignmentScopedRequirements,
	complianceRequirementHints,
	complianceRequirements,
	complianceStatusClass,
	complianceStatuses,
	engagementTypeDescriptions,
	engagementTypes,
} from "@/features/compliance/types";

export const projectStatuses: Record<ProjectStatus, string> = {
	Draft: "Draft",
	Active: "Active",
	Suspended: "Suspended",
	Completed: "Completed",
	Cancelled: "Cancelled",
};

export const contactRoles: Record<ContactRole, string> = {
	Responsible: "Responsible",
	ContractSignatory: "Contract signatory",
	Invoicing: "Invoicing",
	OnSite: "On-site",
	AuthorisedRecipient: "Authorised recipient",
};

/** The one role on our side of the table, which is why it is described separately in the UI. */
export const agencySideContactRoles: ContactRole[] = ["AuthorisedRecipient"];

export const clientSideContactRoles: ContactRole[] = [
	"Responsible",
	"ContractSignatory",
	"Invoicing",
	"OnSite",
];

export const contactRoleDescriptions: Record<ContactRole, string> = {
	Responsible: "Runs the engagement on the client side. Required before the project goes live.",
	ContractSignatory: "Signs the contract for the client.",
	Invoicing: "Receives and settles our invoices.",
	OnSite: "Day-to-day contact at the place of work.",
	AuthorisedRecipient:
		"Ours, not the client's: the recipient for service of documents in Germany (§ 18 AEntG) or the liaison person in Belgium.",
};

/**
 * Mirrors `ProjectStatusChangePolicy` on the backend. Offering a transition the domain refuses is
 * an error message where a disabled option would do; a finished project simply has nothing to pick.
 */
export const allowedProjectStatusTransitions: Record<ProjectStatus, ProjectStatus[]> = {
	Draft: ["Active", "Cancelled"],
	Active: ["Suspended", "Completed", "Cancelled"],
	Suspended: ["Active", "Completed", "Cancelled"],
	Completed: [],
	Cancelled: [],
};

export const contractStatuses: Record<ContractStatus, string> = {
	Draft: "Draft",
	Signed: "Signed",
	Terminated: "Terminated",
	Expired: "Expired",
};

export const contractStatusClass: Record<ContractStatus, string> = {
	Draft: "badge-draft",
	Signed: "badge-active",
	Terminated: "badge-cancelled",
	Expired: "badge-suspended",
};

export const documentCategories: Record<DocumentCategory, string> = {
	Contract: "Contract",
	Annex: "Annex",
	Invoice: "Invoice",
	ClientDocument: "Client document",
	Compliance: "Compliance",
	Insurance: "Insurance",
	Other: "Other",
};

export const emailPurposes: Record<EmailPurpose, string> = {
	Invoice: "Invoice",
	Document: "Document",
};

export const emailPurposeDescriptions: Record<EmailPurpose, string> = {
	Invoice: "Where our invoices for this project are sent.",
	Document: "Where documents and formal correspondence are sent.",
};
