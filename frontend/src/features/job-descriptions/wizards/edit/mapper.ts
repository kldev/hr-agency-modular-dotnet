import type { JobDescriptionProjection } from "#/api/models";
import type { JobDescriptionFormValues } from "../create/schema";

/*
 * `FormArrayField` renders one row per entry, so an empty list has to become a single empty row -
 * the same seed `CreateWizard` starts from.
 */
const toListValues = (values: string[]) => (values.length > 0 ? [...values] : [""]);

/* The API types the amounts as `number | string`, the schema as `string`. */
const toAmount = (value: number | string) => String(value ?? "");

export function toFormValues(jobDescription: JobDescriptionProjection): JobDescriptionFormValues {
	return {
		companyId: jobDescription.companyId,

		title: jobDescription.title,
		summary: jobDescription.summary ?? "",
		description: jobDescription.description,

		responsibilities: toListValues(jobDescription.responsibilities),
		requirements: toListValues(jobDescription.requirements),
		skills: toListValues(jobDescription.skills),

		location: jobDescription.location ?? "",

		/* `CountrySelect` keys are upper case - a value in another case matches no option. */
		countryCode: jobDescription.countryCode.toUpperCase(),

		employmentType: jobDescription.employmentType,
		workMode: jobDescription.workMode,

		currencyCode: jobDescription.currencyCode,
		salaryMin: toAmount(jobDescription.salaryMin),
		salaryMax: toAmount(jobDescription.salaryMax),

		recruiterId: jobDescription.recruiterId,
	};
}
