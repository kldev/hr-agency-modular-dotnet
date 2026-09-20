import { z } from "zod";
import type { CompanyProjection } from "#/api/models";

const optionalEmail = z
	.string()
	.trim()
	.refine((value) => value === "" || /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(value), {
		message: "Enter a valid e-mail address",
	});

/**
 * Everything is optional, exactly as on the backend: a company enters the system as a name on a
 * business card and the paperwork is filled in over time. The two rules that do apply are about
 * halves of things - half an address or half a representative is worse than none.
 */
export const companyProfileSchema = z
	.object({
		legalName: z.string().trim().max(200, "The legal name cannot exceed 200 characters."),
		vatNumber: z.string().trim(),
		street: z.string().trim(),
		buildingNumber: z.string().trim(),
		unitNumber: z.string().trim(),
		postalCode: z.string().trim(),
		city: z.string().trim(),
		countryCode: z.string().trim(),
		iban: z.string().trim(),
		bic: z.string().trim(),
		representativeFirstName: z.string().trim(),
		representativeLastName: z.string().trim(),
		representativeJobTitle: z.string().trim(),
		representativePhone: z.string().trim(),
		representativeEmail: optionalEmail,
	})
	.superRefine((value, context) => {
		const addressParts = [
			["street", value.street],
			["buildingNumber", value.buildingNumber],
			["postalCode", value.postalCode],
			["city", value.city],
			["countryCode", value.countryCode],
		] as const;

		const anyAddress = addressParts.some(([, part]) => part !== "");

		if (anyAddress) {
			for (const [field, part] of addressParts) {
				if (part === "") {
					context.addIssue({
						code: "custom",
						path: [field],
						message: "A registered address needs all of its parts.",
					});
				}
			}
		}

		const representativeParts = [
			["representativeFirstName", value.representativeFirstName],
			["representativeLastName", value.representativeLastName],
			["representativeJobTitle", value.representativeJobTitle],
			["representativePhone", value.representativePhone],
			["representativeEmail", value.representativeEmail],
		] as const;

		const anyRepresentative = representativeParts.some(([, part]) => part !== "");

		if (anyRepresentative) {
			for (const [field, part] of representativeParts) {
				if (part === "") {
					context.addIssue({
						code: "custom",
						path: [field],
						message: "A representative needs all of their details.",
					});
				}
			}
		}

		if ((value.iban === "") !== (value.bic === "")) {
			context.addIssue({
				code: "custom",
				path: [value.iban === "" ? "iban" : "bic"],
				message: "An account number and a BIC go together.",
			});
		}
	});

export type CompanyProfileFormValues = z.infer<typeof companyProfileSchema>;

export type CompanyProfileField = keyof CompanyProfileFormValues;

export function toFormValues(company: CompanyProjection): CompanyProfileFormValues {
	const profile = company.profile;
	const address = profile?.registeredAddress;
	const representative = profile?.legalRepresentative;

	return {
		legalName: profile?.legalName ?? company.name,
		vatNumber: profile?.vatNumber ?? "",
		street: address?.street ?? "",
		buildingNumber: address?.buildingNumber ?? "",
		unitNumber: address?.unitNumber ?? "",
		postalCode: address?.postalCode ?? "",
		city: address?.city ?? "",
		countryCode: address?.countryCode ?? company.countryCode ?? "",
		iban: profile?.iban ?? "",
		bic: profile?.bic ?? "",
		representativeFirstName: representative?.firstName ?? "",
		representativeLastName: representative?.lastName ?? "",
		representativeJobTitle: representative?.jobTitle ?? "",
		representativePhone: representative?.phone ?? "",
		representativeEmail: representative?.email ?? "",
	};
}
