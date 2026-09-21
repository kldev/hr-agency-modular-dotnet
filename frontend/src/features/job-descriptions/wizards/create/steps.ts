import type { JobDescriptionField } from "./schema";

export type JobDescriptionStep = {
	id: string;
	title: string;
	description: string;
	fields: readonly JobDescriptionField[];
};

export const postionStepDef = {
	id: "position",
	title: "Position",
	description: "Basic information about the position",
	fields: ["companyId", "recruiterId", "title", "summary"],
} as const satisfies JobDescriptionStep;

export const contentStepDef = {
	id: "content",
	title: "Content",
	description: "Description and responsibilities",
	fields: ["description", "responsibilities"],
} as const satisfies JobDescriptionStep;

export const requirementsStepDef = {
	id: "requirements",
	title: "Requirements",
	description: "Requirements and skills",
	fields: ["requirements", "skills"],
} as const satisfies JobDescriptionStep;

export const employmentStepDef = {
	id: "employment",
	title: "Employment",
	description: "Employment conditions",
	fields: [
		"location",
		"countryCode",
		"employmentType",
		"workMode",
		"currencyCode",
		"salaryMin",
		"salaryMax",
		"recruiterId",
	],
} as const satisfies JobDescriptionStep;

export const reviewStepDef = {
	id: "review",
	title: "Review",
	description: "Check before creating",
	fields: [],
} as const satisfies JobDescriptionStep;

export const jobDescriptionSteps = [
	postionStepDef,
	contentStepDef,
	requirementsStepDef,
	employmentStepDef,
	reviewStepDef,
] as const satisfies readonly JobDescriptionStep[];

type GatedField = (typeof jobDescriptionSteps)[number]["fields"][number];

/**
 * Every field of the schema has to be validated by some step, otherwise the wizard lets the user
 * reach the review step with a value that only fails on submit - and nothing visibly happens.
 * This line stops compiling as soon as a new schema field is not covered by any step.
 */
export type UngatedJobDescriptionField = Exclude<JobDescriptionField, GatedField>;

export const allSchemaFieldsAreGated = true satisfies UngatedJobDescriptionField extends never
	? true
	: false;
