import type { ComplianceRequirement, EngagementType } from "@/api/models";
import { assignmentScopedRequirements } from "./types";

/**
 * A preview of the backend catalogue, and nothing more.
 *
 * `GET /api/{projects,assignments}/{id}/compliance/catalogue` is the authority, but it needs the
 * record - and the one moment this answer is worth most is *before* it exists, when somebody is
 * picking a country and an engagement type in a wizard. Hence this copy, used only to say "this
 * choice brings N items". No details page reads it. If the two ever disagree, the API is right.
 *
 * It lives here rather than in `features/projects` because both wizards ask the same question now:
 * the project wizard about what the delivery owes, the plan-assignment wizard about what the person
 * owes. Mirrors `ComplianceCatalogue`, including its two derived lists.
 */
const bePosting: ComplianceRequirement[] = [
	"A1Certificates",
	"BeLimosaDeclaration",
	"BeLiaisonPerson",
	"BeJointCommittee",
	"BeSocialDocumentsExemption",
	"BeMotivatedNotification",
];

const dePosting: ComplianceRequirement[] = [
	"A1Certificates",
	"DeAentgNotification",
	"DeAuthorisedRecipient",
	"DeDocumentRetention",
	"DeBranchDetermination",
	"DeLongTermPostingNotification",
];

const preview: Partial<Record<`${string}|${EngagementType}`, ComplianceRequirement[]>> = {
	"BE|PostingOfWorkers": bePosting,
	"BE|Outsourcing": [...bePosting, "BeProhibitedPlacement"],
	"BE|TemporaryAgencyWork": [
		"A1Certificates",
		"UserConditionsReceived",
		"BeTemporaryAgencyRecognition",
		"BeLimosaDeclaration",
		"BeLiaisonPerson",
		"BeJointCommittee",
		"BeUserJointCommittee",
		"BeSocialDocumentsExemption",
		"BeMotivatedNotification",
	],
	"BE|LocalEmployment": ["LocalEmploymentContract", "BeDimonaDeclaration"],
	"DE|PostingOfWorkers": dePosting,
	"DE|Outsourcing": [...dePosting, "DeServiceContractDelimitation"],
	"DE|TemporaryAgencyWork": [
		"A1Certificates",
		"UserConditionsReceived",
		"DeAuegPermit",
		"DeAuegNotification",
		"DeAuthorisedRecipient",
		"DeDocumentRetention",
		"DeBranchDetermination",
		"DeUeberlassungAgreement",
		"DeConstructionSectorRestriction",
		"DeLongTermPostingNotification",
	],
	"DE|LocalEmployment": ["LocalEmploymentContract", "DeSocialSecurityRegistration"],
};

/** Which half of the catalogue is being asked about, mirroring `ComplianceScope`. */
export type CompliancePreviewScope = "project" | "assignment";

/**
 * There is deliberately no scope-less call, exactly as on the backend: asking the catalogue for
 * "everything" is how a per person certificate once ended up as a single tick on a project.
 */
export function previewComplianceRequirements(
	countryCode: string,
	engagementType: EngagementType | "",
	scope: CompliancePreviewScope,
): ComplianceRequirement[] {
	if (!countryCode || !engagementType) {
		return [];
	}

	const requirements = preview[`${countryCode.toUpperCase()}|${engagementType}`] ?? [];

	return requirements.filter(
		(requirement) =>
			assignmentScopedRequirements.includes(requirement) === (scope === "assignment"),
	);
}
