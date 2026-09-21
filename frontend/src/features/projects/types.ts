import type {
	ComplianceRequirement,
	ComplianceStatus,
	ContactRole,
	ContractStatus,
	DocumentCategory,
	EmailPurpose,
	EngagementType,
	ProjectStatus,
} from "@/api/models";

export const projectStatuses: Record<ProjectStatus, string> = {
	Draft: "Draft",
	Active: "Active",
	Suspended: "Suspended",
	Completed: "Completed",
	Cancelled: "Cancelled",
};

export const engagementTypes: Record<EngagementType, string> = {
	PostingOfWorkers: "Posting of workers",
	TemporaryAgencyWork: "Temporary agency work",
	Outsourcing: "Outsourcing",
	LocalEmployment: "Local employment",
};

export const engagementTypeDescriptions: Record<EngagementType, string> = {
	PostingOfWorkers: "Our people do our work at the client's site and stay under our direction.",
	TemporaryAgencyWork: "We hire people out; the client directs their work. Far heavier duties.",
	Outsourcing:
		"We owe a result, not people. Still a posting, plus the duty to show the client is not directing the work.",
	LocalEmployment:
		"A real contract under the law of the country of work. Nobody is posted, so no A1 and no host-state notification.",
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

export const complianceStatuses: Record<ComplianceStatus, string> = {
	NotStarted: "Not started",
	InProgress: "In progress",
	Confirmed: "Confirmed",
	NotApplicable: "Not applicable",
	Expired: "Expired",
};

export const complianceStatusClass: Record<ComplianceStatus, string> = {
	NotStarted: "badge-draft",
	InProgress: "badge-new",
	Confirmed: "badge-active",
	NotApplicable: "badge-inactive",
	Expired: "badge-cancelled",
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

/**
 * The catalogue arrives as enum values; the law behind them does not fit in an enum name. These
 * labels carry the legal handle (§, the local name of the form) so that whoever ticks the item can
 * tell it apart from the neighbouring one without opening the plan.
 */
export const complianceRequirements: Record<ComplianceRequirement, string> = {
	A1Certificates: "A1 certificates for the posted workers",
	UserConditionsReceived: "Client's pay and working conditions received",
	BeLimosaDeclaration: "Limosa declaration",
	BeLiaisonPerson: "Liaison person in Belgium",
	BeJointCommittee: "Joint committee for our activity",
	BeUserJointCommittee: "Client's joint committee",
	BeTemporaryAgencyRecognition: "Temporary agency recognition",
	BeSocialDocumentsExemption: "Exemption from Belgian social documents",
	BeMotivatedNotification: "Motivated notification beyond 12 months",
	DeAentgNotification: "Posting notification (§ 18 AEntG)",
	DeAuegPermit: "Arbeitnehmerüberlassung permit (§ 2 AÜG)",
	DeAuegNotification: "Assignment notification (§ 17b AÜG)",
	DeAuthorisedRecipient: "Authorised recipient in Germany",
	DeDocumentRetention: "Documents kept available in Germany",
	DeBranchDetermination: "Branch and wage floor determined",
	DeUeberlassungAgreement: "Überlassungsvertrag marked as hiring-out",
	DeConstructionSectorRestriction: "Construction sector restriction (§ 1b AÜG)",
	DeLongTermPostingNotification: "Posting beyond 12 months notified (§ 13b AEntG)",
	DeServiceContractDelimitation: "Service contract genuine, not hiring out (§ 1 AÜG)",
	BeProhibitedPlacement: "Not a prohibited placement (art. 31 of 24.07.1987)",
	LocalEmploymentContract: "Local employment contract",
	DeSocialSecurityRegistration: "German social security registration (§ 28a SGB IV)",
	BeDimonaDeclaration: "Dimona declaration",
};

/** Shown in the recording drawer: what the reference number is, and what the dates mean here. */
export const complianceRequirementHints: Record<ComplianceRequirement, string> = {
	A1Certificates:
		"Issued to one named person for one period, so it is recorded on their assignment rather than here.",
	UserConditionsReceived:
		"The client's written statement of the pay and conditions of a comparable worker. Attach the letter.",
	BeLimosaDeclaration:
		"Reference number is the Limosa declaration number; attach the Limosa-1 certificate. A declaration cannot be amended — a change means cancelling and filing a new one.",
	BeLiaisonPerson:
		"Records that the liaison person was declared in Limosa. Their details live under the Authorised recipient contact.",
	BeJointCommittee: "Reference number is the joint committee number, e.g. 322 for agency work.",
	BeUserJointCommittee:
		"The client's joint committee, which as a rule sets the pay. Note any departure from it.",
	BeTemporaryAgencyRecognition:
		"Regional recognition to hire workers out in Belgium. Note the region; the number is required for the Limosa declaration.",
	BeSocialDocumentsExemption:
		"Valid for 12 months from the start. The duty to produce home-country documents on request runs for a year after the posting ends.",
	BeMotivatedNotification:
		"Must be filed before the end of the 12th month — filed later it is void. Valid to marks the filing deadline, not the validity.",
	DeAentgNotification: "For IT work this is usually Not applicable; record the reason in the note.",
	DeAuegPermit:
		"Reference number is the permit number. The first permit always runs for one year; renewal is applied for at least three months before it lapses.",
	DeAuegNotification:
		"Filed per assignment through the Meldeportal before work starts. Reference number is the portal's identifier.",
	DeAuthorisedRecipient:
		"Records that the recipient was named in the notification. Their details live under the Authorised recipient contact.",
	DeDocumentRetention:
		"Contract, working time records and payroll kept in German for inspection. Two years at most, plus eight weeks after the work ends.",
	DeBranchDetermination:
		"Reference number is the branch; the note carries how it was determined. The wage floor follows from branch, group and period.",
	DeUeberlassungAgreement:
		"The hiring-out agreement must exist in text form and name the work as Arbeitnehmerüberlassung before the person starts.",
	DeConstructionSectorRestriction:
		"Not applicable unless the client is a construction business. Confirmed requires proof of at least three years under the same collective agreements.",
	DeLongTermPostingNotification:
		"Filed before the 12th month ends, it extends the posting to 18 months. After that, full German employment conditions apply.",
	DeServiceContractDelimitation:
		"An assessment, not a filing: that we direct the work and owe a result. Getting it wrong makes the whole engagement hiring out without a licence.",
	BeProhibitedPlacement:
		"That the work is not putting personnel at a user's disposal, which Belgium prohibits outside recognised agency work.",
	LocalEmploymentContract:
		"The contract under the law of the country of work. Recorded per person, because that is how it is signed.",
	DeSocialSecurityRegistration:
		"The DEÜV notification, filed before the work starts. Reference number is the registration number.",
	BeDimonaDeclaration:
		"Filed before the first day of work for anybody employed in Belgium. Reference number is the Dimona number.",
};
