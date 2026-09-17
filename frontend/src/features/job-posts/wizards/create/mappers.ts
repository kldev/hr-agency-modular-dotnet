import type { JobDescriptionProjection, JobPostProjection } from "#/api/models";
import type { JobPostFormValues } from "../shared";

/*
 * `FormArrayField` renders one row per entry, so an empty list has to become a single empty row.
 */
const toListValues = (values: string[]) => (values.length > 0 ? [...values] : [""]);

/* The API types the amounts as `number | string`, the schema as `string`. */
const toAmount = (value: number | string) => String(value ?? "");

/*
 * Nothing is copied by the backend: `CreateJobPostHandler` reads the job description only to resolve
 * the company. Seeding the post with the description's content is entirely the job of this mapper -
 * and it stays a copy, the two are free to drift apart afterwards.
 */
export function fromJobDescription(jobDescription: JobDescriptionProjection): JobPostFormValues {
	return {
		title: jobDescription.title,
		summary: jobDescription.summary ?? "",
		description: jobDescription.description,

		responsibilities: toListValues(jobDescription.responsibilities),
		requirements: toListValues(jobDescription.requirements),
		skills: toListValues(jobDescription.skills),

		location: jobDescription.location ?? "",

		/* `CountrySelect`/`LanguageSelect` keys are upper case - another case matches no option. */
		countryCode: jobDescription.countryCode.toUpperCase(),
		languageCode: "PL",

		employmentType: jobDescription.employmentType,
		workMode: jobDescription.workMode,

		currencyCode: jobDescription.currencyCode,
		salaryMin: toAmount(jobDescription.salaryMin),
		salaryMax: toAmount(jobDescription.salaryMax),

		recruiterId: jobDescription.recruiterId,
	};
}

/*
 * The language is deliberately dropped.
 *
 * Copying a post exists to produce another language version, and the backend enforces no uniqueness
 * on `(jobDescriptionId, languageCode)` - prefilling the source language would make a duplicate the
 * default outcome. An empty value fails validation on the first step, so the choice has to be made.
 */
export function fromJobPost(jobPost: JobPostProjection): JobPostFormValues {
	return {
		title: jobPost.title,
		summary: jobPost.summary ?? "",
		description: jobPost.description,

		responsibilities: toListValues(jobPost.responsibilities),
		requirements: toListValues(jobPost.requirements),
		skills: toListValues(jobPost.skills),

		location: jobPost.location ?? "",

		countryCode: jobPost.countryCode.toUpperCase(),
		languageCode: "",

		employmentType: jobPost.employmentType,
		workMode: jobPost.workMode,

		currencyCode: jobPost.currencyCode,
		salaryMin: toAmount(jobPost.salaryMin),
		salaryMax: toAmount(jobPost.salaryMax),

		recruiterId: jobPost.recruiterId,
	};
}
