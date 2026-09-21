import type { LegalEntityField } from "./schema";

export type LegalEntityStep = {
	id: string;
	title: string;
	description: string;
	fields: readonly LegalEntityField[];
};

export const identityStepDef = {
	id: "identity",
	title: "Identity",
	description: "Who the company is and since when",
	fields: ["name", "legalName", "taxId", "vatNumber", "activeFrom", "activeTo"],
} as const satisfies LegalEntityStep;

export const addressStepDef = {
	id: "address",
	title: "Address",
	description: "Where it is registered",
	fields: ["street", "buildingNumber", "unitNumber", "postalCode", "city", "countryCode"],
} as const satisfies LegalEntityStep;

export const representationStepDef = {
	id: "representation",
	title: "Representation",
	description: "Who signs for it",
	fields: ["presidentFirstName", "presidentLastName", "presidentEmail", "description"],
} as const satisfies LegalEntityStep;

/** Bank accounts are a repeatable field outside the schema, so this step gates nothing. */
export const bankingStepDef = {
	id: "banking",
	title: "Banking",
	description: "Where the money goes",
	fields: [],
} as const satisfies LegalEntityStep;

export const reviewStepDef = {
	id: "review",
	title: "Review",
	description: "Check before saving",
	fields: [],
} as const satisfies LegalEntityStep;

export const legalEntitySteps = [
	identityStepDef,
	addressStepDef,
	representationStepDef,
	bankingStepDef,
	reviewStepDef,
] as const satisfies readonly LegalEntityStep[];

/** Every field of the schema belongs to a step, or the wizard would never validate it. */
type GatedField = (typeof legalEntitySteps)[number]["fields"][number];
export type UngatedLegalEntityField = Exclude<LegalEntityField, GatedField>;
export const allSchemaFieldsAreGated = true satisfies UngatedLegalEntityField extends never
	? true
	: false;
