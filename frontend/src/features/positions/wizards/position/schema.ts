import { z } from "zod";

/*
 * A role inside a project, and with it the terms a worker's contract asks for. Everything is a
 * string here and nulls flatten to "", as in every other wizard: the form is text until it is
 * submitted, and the request builder is the one place that decides what an empty field means.
 *
 * Almost all of it is optional, and deliberately so - a role is usually opened the moment somebody
 * needs to plan a person onto it, with the rate and the notice period settled a week later. Only
 * the name and the contract type are asked for, because those two are what tell two roles apart
 * and which template applies.
 */
export const positionSchema = z
	.object({
		projectId: z.string().min(1, "Pick the project this role belongs to"),
		name: z
			.string()
			.trim()
			.min(1, "The role needs a name")
			.max(200, "The name cannot exceed 200 characters."),
		contractName: z.string().trim().max(200, "The name cannot exceed 200 characters."),
		defaultEngagementType: z.string(),
		plannedHeadcount: z.string(),
		workDescription: z.string().trim().max(4000, "That is longer than 4000 characters."),
		duties: z.array(z.string()),
		requiredQualifications: z.array(z.string()),
		contractType: z.string().min(1, "Pick what we sign with this person"),
		rateAmount: z.string(),
		rateCurrency: z.string(),
		rateUnit: z.string(),
		rateBasis: z.string(),
		payoutDay: z.string(),
		probationPeriod: z.string().trim().max(200, "That is longer than 200 characters."),
		noticePeriod: z.string().trim().max(200, "That is longer than 200 characters."),
		allowances: z.array(z.string()),
		weeklyHours: z.string(),
		workStartsAt: z.string(),
		workSchedule: z.string().trim().max(500, "That is longer than 500 characters."),
		street: z.string().trim(),
		buildingNumber: z.string().trim(),
		unitNumber: z.string().trim(),
		postalCode: z.string().trim(),
		city: z.string().trim(),
		countryCode: z.string().trim(),
	})
	.superRefine((value, context) => {
		/* A rate without a currency is a number nobody can act on, and so is the reverse. */
		if (value.rateAmount && !value.rateCurrency) {
			context.addIssue({
				code: "custom",
				path: ["rateCurrency"],
				message: "A rate needs a currency.",
			});
		}

		if (value.payoutDay) {
			const day = Number(value.payoutDay);

			if (!Number.isInteger(day) || day < 1 || day > 31) {
				context.addIssue({
					code: "custom",
					path: ["payoutDay"],
					message: "The payout day is a day of the month, between 1 and 31.",
				});
			}
		}

		if (value.plannedHeadcount) {
			const headcount = Number(value.plannedHeadcount);

			if (!Number.isInteger(headcount) || headcount < 1) {
				context.addIssue({
					code: "custom",
					path: ["plannedHeadcount"],
					message: "The target headcount is a whole number of people.",
				});
			}
		}

		/*
		 * The workplace is optional - empty means the project's own address - but half an address is
		 * worse than none: it reaches a document and a posting notification looking complete.
		 */
		const address = [value.street, value.buildingNumber, value.postalCode, value.city];

		if (address.some(Boolean) && !address.every(Boolean)) {
			context.addIssue({
				code: "custom",
				path: ["street"],
				message: "Give the whole workplace address, or leave all of it empty for the project's.",
			});
		}
	});

export type PositionFormValues = z.infer<typeof positionSchema>;

export type PositionField = keyof PositionFormValues;

export const emptyPosition: PositionFormValues = {
	projectId: "",
	name: "",
	contractName: "",
	defaultEngagementType: "",
	plannedHeadcount: "",
	workDescription: "",
	duties: [],
	requiredQualifications: [],
	contractType: "",
	rateAmount: "",
	rateCurrency: "",
	rateUnit: "Hourly",
	rateBasis: "Gross",
	payoutDay: "",
	probationPeriod: "",
	noticePeriod: "",
	allowances: [],
	weeklyHours: "",
	workStartsAt: "",
	workSchedule: "",
	street: "",
	buildingNumber: "",
	unitNumber: "",
	postalCode: "",
	city: "",
	countryCode: "",
};
