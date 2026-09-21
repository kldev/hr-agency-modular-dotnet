import type { WorkerField } from "./schema";

export type WorkerStep = {
	id: string;
	title: string;
	description: string;
	fields: readonly WorkerField[];
};

export const sourceStepDef = {
	id: "source",
	title: "Source",
	description: "Where they came from",
	fields: ["sourceApplicationId", "sourceCandidateId"],
} as const satisfies WorkerStep;

export const identityStepDef = {
	id: "identity",
	title: "Identity",
	description: "Who this person is",
	fields: ["firstName", "lastName", "dateOfBirth", "citizenship"],
} as const satisfies WorkerStep;

export const documentStepDef = {
	id: "document",
	title: "Document",
	description: "What proves it",
	fields: [
		"identityDocumentKind",
		"identityDocumentNumber",
		"identityDocumentIssuingCountry",
		"identityDocumentValidUntil",
	],
} as const satisfies WorkerStep;

export const contactStepDef = {
	id: "contact",
	title: "Contact",
	description: "How to reach them",
	fields: [
		"email",
		"phoneNumber",
		"street",
		"buildingNumber",
		"unitNumber",
		"postalCode",
		"city",
		"addressCountryCode",
		"note",
	],
} as const satisfies WorkerStep;

export const reviewStepDef = {
	id: "review",
	title: "Review",
	description: "Check before saving",
	fields: [],
} as const satisfies WorkerStep;

const allSteps = [
	sourceStepDef,
	identityStepDef,
	documentStepDef,
	contactStepDef,
	reviewStepDef,
] as const satisfies readonly WorkerStep[];

/**
 * The Source step drops out in two cases, for two different reasons.
 *
 * Editing: the API ignores `sourceCandidateId` on an update, because somebody's origin does not
 * change, so the field would silently do nothing.
 *
 * Registering straight from an application: the caller has just clicked that application and handed
 * us its name, e-mail, phone and candidate id. A step that asks for what was clicked a second ago is
 * a step that gets clicked through without being read.
 */
export function workerStepsFor({
	mode,
	knownSource,
}: {
	mode: "register" | "edit";
	knownSource: boolean;
}): readonly WorkerStep[] {
	return mode === "register" && !knownSource
		? allSteps
		: allSteps.filter((step) => step.id !== "source");
}

export const workerSteps = allSteps;

/** Every field of the schema belongs to a step, or the wizard would never validate it. */
type GatedField = (typeof workerSteps)[number]["fields"][number];
export type UngatedWorkerField = Exclude<WorkerField, GatedField>;
export const allSchemaFieldsAreGated = true satisfies UngatedWorkerField extends never
	? true
	: false;
