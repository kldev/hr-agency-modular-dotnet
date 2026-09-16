import { z } from "zod";
import { CurrencyCode, EmploymentType, WorkMode } from "@/api/models";

const requiredTextList = (label: string) =>
	z
		.array(z.string().trim())
		.transform((items) => items.filter(Boolean))
		.pipe(
			z
				.array(z.string().min(5, `${label} must be at least 5 characters`))
				.min(1, `Add at least one ${label.toLowerCase()}`),
		);

export const jobDescriptionSchema = z.object({
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

	salaryMin: z.string().min(1, "Minimum salary is required"),

	salaryMax: z.string().min(1, "Maximum salary is required"),

	recruiterId: z.string().min(1, "Recruiter is required"),
});

export type JobDescriptionFormValues = z.infer<typeof jobDescriptionSchema>;

export type JobDescriptionField = keyof JobDescriptionFormValues;
