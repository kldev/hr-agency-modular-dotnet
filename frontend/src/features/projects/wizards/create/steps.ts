import type { ProjectField } from "./schema";

export type ProjectStep = {
	id: string;
	title: string;
	description: string;
	fields: readonly ProjectField[];
};

export const clientStepDef = {
	id: "client",
	title: "Parties",
	description: "Who the work is for and who delivers it",
	fields: ["companyId", "legalEntityId"],
} as const satisfies ProjectStep;

export const basicsStepDef = {
	id: "basics",
	title: "Basics",
	description: "Name, description and engagement",
	fields: ["name", "description", "engagementType"],
} as const satisfies ProjectStep;

export const assignmentStepDef = {
	id: "assignment",
	title: "Assignment",
	description: "Place of work and period",
	fields: [
		"street",
		"buildingNumber",
		"unitNumber",
		"postalCode",
		"city",
		"countryCode",
		"startsOn",
		"endsOn",
	],
} as const satisfies ProjectStep;

export const teamStepDef = {
	id: "team",
	title: "Team",
	description: "Who runs it on our side",
	fields: ["teamId"],
} as const satisfies ProjectStep;

export const reviewStepDef = {
	id: "review",
	title: "Review",
	description: "Check before saving",
	fields: [],
} as const satisfies ProjectStep;

export const projectSteps = [
	clientStepDef,
	basicsStepDef,
	assignmentStepDef,
	teamStepDef,
	reviewStepDef,
] as const satisfies readonly ProjectStep[];

/** Every field of the schema belongs to a step, or the wizard would never validate it. */
type GatedField = (typeof projectSteps)[number]["fields"][number];
export type UngatedProjectField = Exclude<ProjectField, GatedField>;
export const allSchemaFieldsAreGated = true satisfies UngatedProjectField extends never
	? true
	: false;
