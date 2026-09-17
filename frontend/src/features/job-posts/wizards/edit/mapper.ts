import type { JobPostProjection } from "#/api/models";
import type { JobPostFormValues } from "../shared";

/*
 * `FormArrayField` renders one row per entry, so an empty list has to become a single empty row.
 */
const toListValues = (values: string[]) => (values.length > 0 ? [...values] : [""]);

/* The API types the amounts as `number | string`, the schema as `string`. */
const toAmount = (value: number | string) => String(value ?? "");

export function toFormValues(jobPost: JobPostProjection): JobPostFormValues {
	return {
		title: jobPost.title,
		summary: jobPost.summary ?? "",
		description: jobPost.description,

		responsibilities: toListValues(jobPost.responsibilities),
		requirements: toListValues(jobPost.requirements),
		skills: toListValues(jobPost.skills),

		location: jobPost.location ?? "",

		/* The selects key their options upper case - a value in another case matches none of them. */
		countryCode: jobPost.countryCode.toUpperCase(),
		languageCode: jobPost.languageCode.toUpperCase(),

		employmentType: jobPost.employmentType,
		workMode: jobPost.workMode,

		currencyCode: jobPost.currencyCode,
		salaryMin: toAmount(jobPost.salaryMin),
		salaryMax: toAmount(jobPost.salaryMax),

		recruiterId: jobPost.recruiterId,
	};
}
