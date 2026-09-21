import type { PlanAssignmentField } from "./schema";

export type PlanAssignmentStep = {
	id: string;
	title: string;
	description: string;
	fields: readonly PlanAssignmentField[];
};

export const workerStepDef = {
	id: "worker",
	title: "Worker",
	description: "Who is being posted",
	fields: ["workerId"],
} as const satisfies PlanAssignmentStep;

export const projectStepDef = {
	id: "project",
	title: "Project",
	description: "Where they are going",
	fields: ["projectId"],
} as const satisfies PlanAssignmentStep;

export const termsStepDef = {
	id: "terms",
	title: "Terms",
	description: "Under what arrangement, and for how long",
	fields: ["engagementType", "position", "startsOn", "endsOn"],
} as const satisfies PlanAssignmentStep;

export const reviewStepDef = {
	id: "review",
	title: "Review",
	description: "Check before planning",
	fields: [],
} as const satisfies PlanAssignmentStep;

const allSteps = [
	workerStepDef,
	projectStepDef,
	termsStepDef,
	reviewStepDef,
] as const satisfies readonly PlanAssignmentStep[];

/**
 * The wizard is opened from three places and two of them already know half the answer: from a
 * person the worker is given, from a project the project is. A step that asks for something the
 * caller just clicked on is a step that gets clicked through without being read.
 */
export function planAssignmentStepsFor({
	knownWorker,
	knownProject,
}: {
	knownWorker: boolean;
	knownProject: boolean;
}): readonly PlanAssignmentStep[] {
	return allSteps.filter(
		(step) => !(step.id === "worker" && knownWorker) && !(step.id === "project" && knownProject),
	);
}

export const planAssignmentSteps = allSteps;

/** Every field of the schema belongs to a step, or the wizard would never validate it. */
type GatedField = (typeof planAssignmentSteps)[number]["fields"][number];
export type UngatedPlanAssignmentField = Exclude<PlanAssignmentField, GatedField>;
export const allSchemaFieldsAreGated = true satisfies UngatedPlanAssignmentField extends never
	? true
	: false;
