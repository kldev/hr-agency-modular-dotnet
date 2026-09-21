import { z } from "zod";
import type { LegalEntityProjection } from "@/api/models";

export const legalEntitySchema = z
	.object({
		name: z.string().trim().min(1, "Name is required"),
		legalName: z.string().trim().min(1, "Registered name is required"),
		taxId: z.string().trim().min(1, "Tax ID is required"),
		vatNumber: z.string(),

		activeFrom: z.string().min(1, "The first day of trading is required"),
		activeTo: z.string(),

		street: z.string().trim().min(1, "Street is required"),
		buildingNumber: z.string().trim().min(1, "Building number is required"),
		unitNumber: z.string(),
		postalCode: z.string().trim().min(1, "Postal code is required"),
		city: z.string().trim().min(1, "City is required"),
		countryCode: z.string().trim().min(1, "Country is required"),

		presidentFirstName: z.string().trim().min(1, "First name is required"),
		presidentLastName: z.string().trim().min(1, "Last name is required"),
		presidentEmail: z.string(),

		description: z.string(),
	})
	.refine((value) => !value.activeTo || value.activeTo >= value.activeFrom, {
		message: "The last day of trading cannot be earlier than the first one",
		path: ["activeTo"],
	});

export type LegalEntityFormValues = z.infer<typeof legalEntitySchema>;

export type LegalEntityField = keyof LegalEntityFormValues;

export const emptyLegalEntity: LegalEntityFormValues = {
	name: "",
	legalName: "",
	taxId: "",
	vatNumber: "",
	activeFrom: "",
	activeTo: "",
	street: "",
	buildingNumber: "",
	unitNumber: "",
	postalCode: "",
	city: "",
	countryCode: "",
	presidentFirstName: "",
	presidentLastName: "",
	presidentEmail: "",
	description: "",
};

export function toLegalEntityValues(entity: LegalEntityProjection): LegalEntityFormValues {
	return {
		name: entity.name,
		legalName: entity.legalName,
		taxId: entity.taxId,
		vatNumber: entity.vatNumber ?? "",
		activeFrom: entity.activeFrom,
		activeTo: entity.activeTo ?? "",
		street: entity.registeredAddress.street,
		buildingNumber: entity.registeredAddress.buildingNumber,
		unitNumber: entity.registeredAddress.unitNumber ?? "",
		postalCode: entity.registeredAddress.postalCode,
		city: entity.registeredAddress.city,
		countryCode: entity.registeredAddress.countryCode,
		presidentFirstName: entity.president.firstName,
		presidentLastName: entity.president.lastName,
		presidentEmail: entity.president.email ?? "",
		description: entity.description ?? "",
	};
}
