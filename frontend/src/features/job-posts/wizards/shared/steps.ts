import type { JobPostField } from "./schema";

export type JobPostStep = {
	id: string;
	title: string;
	description: string;
	fields: readonly JobPostField[];
};

export const postStepDef = {
	id: "post",
	title: "Post",
	description: "Language, recruiter and the headline",
	fields: ["recruiterId", "languageCode", "title", "summary"],
} as const satisfies JobPostStep;

export const contentStepDef = {
	id: "content",
	title: "Content",
	description: "Description and responsibilities",
	fields: ["description", "responsibilities"],
} as const satisfies JobPostStep;

export const requirementsStepDef = {
	id: "requirements",
	title: "Requirements",
	description: "Requirements and skills",
	fields: ["requirements", "skills"],
} as const satisfies JobPostStep;

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
	],
} as const satisfies JobPostStep;

export const reviewStepDef = {
	id: "review",
	title: "Review",
	description: "Check before saving",
	fields: [],
} as const satisfies JobPostStep;

export const jobPostSteps = [
	postStepDef,
	contentStepDef,
	requirementsStepDef,
	employmentStepDef,
	reviewStepDef,
] as const satisfies readonly JobPostStep[];

type GatedField = (typeof jobPostSteps)[number]["fields"][number];

/**
 * Every field of the schema has to be validated by some step, otherwise the wizard lets the user
 * reach the review step with a value that only fails on submit - and nothing visibly happens.
 * This line stops compiling as soon as a new schema field is not covered by any step.
 */
export type UngatedJobPostField = Exclude<JobPostField, GatedField>;

export const allSchemaFieldsAreGated = true satisfies UngatedJobPostField extends never
	? true
	: false;

export function findStepForField(field: JobPostField) {
	return jobPostSteps.find((step) => (step.fields as readonly string[]).includes(field));
}
