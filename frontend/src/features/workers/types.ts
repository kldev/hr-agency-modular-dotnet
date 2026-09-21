import type {
	IdentityDocumentKind,
	ResponsibleDepartment,
	WorkAuthorisationKind,
	WorkerDocumentCategory,
	WorkerStatus,
} from "@/api/models";

/**
 * Which country counts as home. The API deliberately does not know — it takes `workCountry` and
 * `excludeWorkCountry` and leaves the question to whoever is asking — so the answer lives here,
 * in one place, and the two register views are built from it.
 */
export const HOME_WORK_COUNTRY = "PL";

export const workerStatuses: Record<WorkerStatus, string> = {
	Recruitment: "Recruitment",
	ContractPreparation: "Contract preparation",
	Legalisation: "Legalisation",
	Onboarding: "Onboarding",
	Employed: "Employed",
	ProjectChange: "Project change",
	Terminated: "Terminated",
};

export const workerStatusClass: Record<WorkerStatus, string> = {
	Recruitment: "badge-new",
	ContractPreparation: "badge-proposal",
	Legalisation: "badge-qualified",
	Onboarding: "badge-contacted",
	Employed: "badge-active",
	ProjectChange: "badge-suspended",
	Terminated: "badge-inactive",
};

export const workerStatusDescriptions: Record<WorkerStatus, string> = {
	Recruitment: "Being sourced. Nothing has been promised to them yet.",
	ContractPreparation: "Terms agreed; the contract is being drawn up.",
	Legalisation: "Getting the right to work and stay. Only for people who need one.",
	Onboarding: "Paperwork done, getting them ready to start.",
	Employed: "Working, or available to be put on a project.",
	ProjectChange: "Moving between projects. Still ours, still ready to start.",
	Terminated: "Gone. They can be taken back on, but not into recruitment.",
};

export const responsibleDepartments: Record<ResponsibleDepartment, string> = {
	Recruitment: "Recruitment",
	HumanResources: "Human resources",
	Legalisation: "Legalisation",
	Operations: "Operations",
	None: "—",
};

export const identityDocumentKinds: Record<IdentityDocumentKind, string> = {
	IdentityCard: "Identity card",
	Passport: "Passport",
	ResidenceCard: "Residence card",
	Other: "Other",
};

export const workAuthorisationKinds: Record<WorkAuthorisationKind, string> = {
	WorkPermit: "Work permit",
	ResidencePermit: "Residence permit",
	Visa: "Visa",
	WorkStatement: "Statement on entrusting work",
	Other: "Other",
};

export const workerDocumentCategories: Record<WorkerDocumentCategory, string> = {
	Identity: "Identity",
	EmploymentContract: "Employment contract",
	MedicalCertificate: "Medical certificate",
	HealthAndSafety: "Health and safety",
	Qualification: "Qualification",
	Legalisation: "Legalisation",
	Other: "Other",
};

/**
 * Mirrors `LegalisationPolicy.FreeMovement`. Switzerland is on the list although it is not in the
 * EEA: the free movement agreement puts its nationals in the same position for this purpose.
 *
 * The projection already carries `requiresLegalisation`, so nothing on an existing worker needs
 * this. It is here for the registration wizard, which has to answer the question before the person
 * exists.
 */
const freeMovementCountries = new Set([
	"AT",
	"BE",
	"BG",
	"CH",
	"CY",
	"CZ",
	"DE",
	"DK",
	"EE",
	"ES",
	"FI",
	"FR",
	"GR",
	"HR",
	"HU",
	"IE",
	"IS",
	"IT",
	"LI",
	"LT",
	"LU",
	"LV",
	"MT",
	"NL",
	"NO",
	"PL",
	"PT",
	"RO",
	"SE",
	"SI",
	"SK",
]);

export function requiresLegalisation(citizenship: string | undefined | null): boolean {
	return !freeMovementCountries.has((citizenship ?? "").trim().toUpperCase());
}

/**
 * Mirrors `WorkerStatusChangePolicy.Allow`. Offering a transition the domain refuses is an error
 * message where a missing option would do — and `Legalisation` is refused with its own message for
 * anybody holding free movement rights, which is a sentence nobody should have to read twice.
 */
export function allowedWorkerStatusTransitions(
	status: WorkerStatus,
	needsLegalisation: boolean,
): WorkerStatus[] {
	const targets: WorkerStatus[] = [];

	switch (status) {
		case "Recruitment":
			targets.push("ContractPreparation");
			break;
		case "ContractPreparation":
			targets.push(needsLegalisation ? "Legalisation" : "Onboarding");
			break;
		case "Legalisation":
			targets.push("Onboarding");
			break;
		case "Onboarding":
			targets.push("Employed");
			break;
		case "Employed":
			targets.push("ProjectChange", "Onboarding");
			if (needsLegalisation) targets.push("Legalisation");
			break;
		case "ProjectChange":
			targets.push("Employed", "Onboarding");
			if (needsLegalisation) targets.push("Legalisation");
			break;
		case "Terminated":
			// Never back into recruitment: that already happened.
			return ["ContractPreparation"];
	}

	// Somebody can drop out of any stage, and the history keeps which one they were in.
	targets.push("Terminated");

	return targets;
}
