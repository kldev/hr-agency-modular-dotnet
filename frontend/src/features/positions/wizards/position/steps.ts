import type { PositionField } from "./schema";

export type PositionStep = {
	id: string;
	title: string;
	description: string;
	fields: readonly PositionField[];
};

/*
 * The steps follow the questions a role answers, not the order the API lists its fields in: who
 * this person is, what they do, what we sign with them, and when and where they work.
 */
export const roleStepDef = {
	id: "role",
	title: "Role",
	description: "Which role, and how many of them",
	fields: ["projectId", "name", "contractName", "defaultEngagementType", "plannedHeadcount"],
} as const satisfies PositionStep;

export const workStepDef = {
	id: "work",
	title: "Work",
	description: "What this person does",
	fields: ["workDescription", "duties", "requiredQualifications"],
} as const satisfies PositionStep;

export const contractStepDef = {
	id: "contract",
	title: "Contract",
	description: "What we sign, and for how much",
	fields: [
		"contractType",
		"rateAmount",
		"rateCurrency",
		"rateUnit",
		"rateBasis",
		"payoutDay",
		"probationPeriod",
		"noticePeriod",
		"allowances",
	],
} as const satisfies PositionStep;

export const scheduleStepDef = {
	id: "schedule",
	title: "Time and place",
	description: "When and where the work happens",
	fields: [
		"weeklyHours",
		"workStartsAt",
		"workSchedule",
		"street",
		"buildingNumber",
		"unitNumber",
		"postalCode",
		"city",
		"countryCode",
	],
} as const satisfies PositionStep;

export const reviewStepDef = {
	id: "review",
	title: "Review",
	description: "Check before saving",
	fields: [],
} as const satisfies PositionStep;

const allSteps = [
	roleStepDef,
	workStepDef,
	contractStepDef,
	scheduleStepDef,
	reviewStepDef,
] as const satisfies readonly PositionStep[];

/**
 * Opened from a project, the project is already known and asking for it again is a step that gets
 * clicked through without being read. The project cannot change on an existing role either — a
 * role belongs to one delivery, and moving it would strand every posting held against it.
 */
export function positionStepsFor({
	knownProject,
}: {
	knownProject: boolean;
}): readonly PositionStep[] {
	if (!knownProject) {
		return allSteps;
	}

	return allSteps.map((step) =>
		step.id === "role"
			? { ...step, fields: step.fields.filter((field) => field !== "projectId") }
			: step,
	);
}

export const positionSteps = allSteps;

/** Every field of the schema belongs to a step, or the wizard would never validate it. */
type GatedField = (typeof positionSteps)[number]["fields"][number];
export type UngatedPositionField = Exclude<PositionField, GatedField>;
export const allSchemaFieldsAreGated = true satisfies UngatedPositionField extends never
	? true
	: false;
