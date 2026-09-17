import { z } from "zod";
import { CurrencyCode, EmploymentType, WorkMode } from "@/api/models";

export const MIN_LIST_ITEM_LENGTH = 2;

/*
 * Issues are reported on the array itself, never on `list[index]`.
 *
 * A zod issue with the path `["responsibilities", 0]` is mapped by TanStack Form onto the field
 * `responsibilities[0]`, which no step validates and no `FieldError` renders - the wizard would let
 * the user walk past a too short item and only fail on submit. The row number goes into the message
 * instead, so the error stays on the field the form actually knows about.
 */
const requiredTextList = (label: string) =>
	z.array(z.string()).superRefine((items, ctx) => {
		const filled = items.filter((item) => item.trim().length > 0);

		if (filled.length === 0) {
			ctx.addIssue({
				code: "custom",
				message: `Add at least one ${label.toLowerCase()}`,
			});

			return;
		}

		items.forEach((item, index) => {
			const value = item.trim();

			if (value.length === 0 || value.length >= MIN_LIST_ITEM_LENGTH) {
				return;
			}

			ctx.addIssue({
				code: "custom",
				message: `${label} ${index + 1} must be at least ${MIN_LIST_ITEM_LENGTH} characters`,
			});
		});
	});

const salaryAmount = (label: string) =>
	z
		.string()
		.min(1, `${label} salary is required`)
		.regex(/^\d+([.,]\d{1,4})?$/, `${label} salary must be a number`);

export const parseSalary = (value: string) => Number(value.replace(",", "."));

/*
 * Deliberately a separate schema, not a variation of `jobDescriptionSchema`.
 *
 * The fields overlap today, but a post and a description are two API contracts that are meant to
 * drift apart - the post is the candidate-facing copy. A shared base would tie together exactly
 * what this module exists to keep separate. What differs: no `companyId` (the post inherits it from
 * the description) and a `languageCode`, because one description can carry several translations.
 */
export const jobPostSchema = z
	.object({
		title: z.string().trim().min(2, "Title is required"),

		summary: z.string().trim().max(1000, "Summary cannot exceed 1000 characters").nullable(),

		description: z.string().trim().min(10, "Description is required"),

		responsibilities: requiredTextList("Responsibility"),
		requirements: requiredTextList("Requirement"),
		skills: requiredTextList("Skill"),

		location: z.string().trim().min(1, "Location is required"),

		countryCode: z.string().length(2, "Select a country"),

		languageCode: z.string().length(2, "Select a language"),

		employmentType: z.enum(Object.values(EmploymentType) as [EmploymentType, ...EmploymentType[]]),

		workMode: z.enum(Object.values(WorkMode) as [WorkMode, ...WorkMode[]]),

		currencyCode: z.enum(CurrencyCode),

		salaryMin: salaryAmount("Minimum"),

		salaryMax: salaryAmount("Maximum"),

		recruiterId: z.string().min(1, "Recruiter is required"),
	})
	.superRefine((values, ctx) => {
		const min = parseSalary(values.salaryMin);
		const max = parseSalary(values.salaryMax);

		if (Number.isNaN(min) || Number.isNaN(max) || max >= min) {
			return;
		}

		ctx.addIssue({
			code: "custom",
			path: ["salaryMax"],
			message: "Maximum salary cannot be lower than the minimum",
		});
	});

export type JobPostFormValues = z.infer<typeof jobPostSchema>;

export type JobPostField = keyof JobPostFormValues;
