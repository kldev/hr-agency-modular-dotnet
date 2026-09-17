import { z } from "zod";
import { CurrencyCode, EmploymentType, WorkMode } from "@/api/models";

const requiredTextList = (label: string) =>
	z
		.array(z.string().trim())
		.transform((items) => items.filter(Boolean))
		.pipe(
			z
				.array(z.string().min(2, `${label} must be at least 2 characters`))
				.min(1, `Add at least one ${label.toLowerCase()}`),
		);

const salaryAmount = (label: string) =>
	z
		.string()
		.min(1, `${label} salary is required`)
		.regex(/^\d+([.,]\d{1,4})?$/, `${label} salary must be a number`);

export const parseSalary = (value: string) => Number(value.replace(",", "."));

export const jobDescriptionSchema = z
	.object({
		companyId: z.string().min(1, "Company is required"),

		title: z.string().trim().min(2, "Title is required"),

		summary: z.string().trim().max(1000, "Summary cannot exceed 1000 characters").nullable(),

		description: z.string().trim().min(10, "Description is required"),

		responsibilities: requiredTextList("Responsibility"),
		requirements: requiredTextList("Requirement"),
		skills: requiredTextList("Skill"),

		location: z.string().trim().min(1, "Location is required"),

		countryCode: z.string().length(2, "Select a country"),

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

export type JobDescriptionFormValues = z.infer<typeof jobDescriptionSchema>;

export type JobDescriptionField = keyof JobDescriptionFormValues;
