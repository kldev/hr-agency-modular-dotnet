import type { JobDescriptionField } from "./schema";

export type JobDescriptionStep = {
	id: string;
	title: string;
	description: string;
	fields: readonly JobDescriptionField[];
};

export const postionStepDef: JobDescriptionStep = {
	id: "position",
	title: "Position",
	description: "Basic information about the position",
	fields: ["companyId", "recruiterId", "title", "summary"],
};

export const contentStepDef: JobDescriptionStep = {
	id: "content",
	title: "Content",
	description: "Description and responsibilities",
	fields: ["description", "responsibilities"],
};

export const requirementsStepDef: JobDescriptionStep = {
	id: "requirements",
	title: "Requirements",
	description: "Requirements and skills",
	fields: ["requirements", "skills"],
};

export const employmentStepDef: JobDescriptionStep = {
	id: "employment",
	title: "Employment",
	description: "Employment conditions",
	fields: [
		"countryCode",
		"employmentType",
		"workMode",
		"currencyCode",
		"salaryMin",
		"salaryMax",
		"recruiterId",
	],
};

export const detailsStepDef: JobDescriptionStep = {
	id: "employment",
	title: "Details",
	description: "Location and compensation",
	fields: ["location"],
};

export const reviewStepDef: JobDescriptionStep = {
	id: "review",
	title: "Review",
	description: "Check before creating",
	fields: [],
};

export const jobDescriptionSteps = [
	postionStepDef,
	contentStepDef,
	requirementsStepDef,
	employmentStepDef,
	reviewStepDef,
] satisfies readonly JobDescriptionStep[];
