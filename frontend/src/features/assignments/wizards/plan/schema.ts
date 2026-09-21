import { z } from "zod";

/*
 * One posting: one person, one project, one period. Everything here is frozen the moment it is
 * planned - the delivering company, the client and the work country are read from the project and
 * copied onto the assignment, because that is what an A1 is issued against. Only the role and the
 * period can be corrected afterwards.
 *
 * The role is an id, not a name: it is opened on the project first, and picking it is what lets the
 * project say how many people a role still needs.
 */
export const planAssignmentSchema = z
	.object({
		workerId: z.string().min(1, "Pick the person"),
		projectId: z.string().min(1, "Pick the project"),
		engagementType: z.string().min(1, "Pick how this person is engaged"),
		positionId: z.string().min(1, "Pick the role this person is taking"),
		startsOn: z.string().min(1, "The start date is required"),
		endsOn: z.string(),
	})
	.superRefine((value, context) => {
		if (value.endsOn && value.startsOn && value.endsOn.slice(0, 10) < value.startsOn.slice(0, 10)) {
			context.addIssue({
				code: "custom",
				path: ["endsOn"],
				message: "An assignment cannot end before the day it started.",
			});
		}
	});

/*
 * Two rules are deliberately not mirrored here: that the period sits inside the project's own, and
 * that it does not overlap another posting of the same person. The first needs the project's dates
 * at a moment the wizard may not have them, the second needs a query over everybody's postings -
 * both come back from the server as a named 400, which beats a client-side guess that disagrees.
 */
export type PlanAssignmentFormValues = z.infer<typeof planAssignmentSchema>;

export type PlanAssignmentField = keyof PlanAssignmentFormValues;

export const emptyPlanAssignment: PlanAssignmentFormValues = {
	workerId: "",
	projectId: "",
	engagementType: "",
	positionId: "",
	startsOn: "",
	endsOn: "",
};
