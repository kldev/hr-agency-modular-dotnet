import type { ComplianceRequirement, EngagementType } from "#/api/models";

/**
 * A preview of the backend catalogue, and nothing more.
 *
 * `GET /api/projects/{id}/compliance/catalogue` is the authority, but it needs a project - and the
 * one moment this answer is worth most is *before* the project exists, when somebody is picking a
 * country and an engagement type in the wizard. Hence this copy, used only to say "this choice
 * brings N items". The project page never reads it. If the two ever disagree, the API is right.
 */
const preview: Partial<Record<`${string}|${EngagementType}`, ComplianceRequirement[]>> = {
	"BE|PostingOfWorkers": [
		"A1Certificates",
		"BeLimosaDeclaration",
		"BeLiaisonPerson",
		"BeJointCommittee",
		"BeSocialDocumentsExemption",
		"BeMotivatedNotification",
	],
	"BE|TemporaryAgencyWork": [
		"A1Certificates",
		"UserConditionsReceived",
		"BeLimosaDeclaration",
		"BeLiaisonPerson",
		"BeJointCommittee",
		"BeUserJointCommittee",
		"BeTemporaryAgencyRecognition",
		"BeSocialDocumentsExemption",
		"BeMotivatedNotification",
	],
	"DE|PostingOfWorkers": [
		"A1Certificates",
		"DeAentgNotification",
		"DeAuthorisedRecipient",
		"DeDocumentRetention",
		"DeBranchDetermination",
		"DeLongTermPostingNotification",
	],
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
};

export function previewComplianceRequirements(
	countryCode: string,
	engagementType: EngagementType | "",
): ComplianceRequirement[] {
	if (!countryCode || !engagementType) {
		return [];
	}

	return preview[`${countryCode.toUpperCase()}|${engagementType}`] ?? [];
}
