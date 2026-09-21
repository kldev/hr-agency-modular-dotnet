import { z } from "zod";

/*
 * One schema for registering and for editing, because the API takes one shape for both
 * (`WorkerRequest`). The only field that exists on registration alone is `sourceCandidateId`, and
 * nothing fills it in yet.
 */
export const workerSchema = z
	.object({
		/*
		 * Where this person came from. Optional and register-only: the API ignores it on an update,
		 * because somebody's origin does not change. `sourceApplicationId` is what the picker holds;
		 * `sourceCandidateId` is what the command takes, and the application carries it.
		 */
		sourceApplicationId: z.string(),
		sourceCandidateId: z.string(),

		firstName: z
			.string()
			.trim()
			.min(1, "First name is required")
			.max(100, "First name cannot exceed 100 characters."),
		lastName: z
			.string()
			.trim()
			.min(1, "Last name is required")
			.max(100, "Last name cannot exceed 100 characters."),
		dateOfBirth: z.string().min(1, "Date of birth is required"),
		citizenship: z.string().trim().min(2, "Citizenship is required"),

		identityDocumentKind: z.string().min(1, "Pick the kind of document"),
		identityDocumentNumber: z
			.string()
			.trim()
			.min(1, "Document number is required")
			.max(50, "Document number cannot exceed 50 characters."),
		identityDocumentIssuingCountry: z.string().trim().min(2, "Issuing country is required"),
		identityDocumentValidUntil: z.string(),

		email: z.string().trim(),
		phoneNumber: z.string().trim().max(40, "Phone cannot exceed 40 characters."),

		street: z.string().trim(),
		buildingNumber: z.string().trim(),
		unitNumber: z.string().trim(),
		postalCode: z.string().trim(),
		city: z.string().trim(),
		addressCountryCode: z.string().trim(),

		note: z.string().trim().max(5000, "The note cannot exceed 5000 characters."),
	})
	.superRefine((value, context) => {
		if (value.email && !/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(value.email)) {
			context.addIssue({ code: "custom", path: ["email"], message: "Invalid email address" });
		}

		if (
			value.dateOfBirth &&
			value.dateOfBirth.slice(0, 10) >= new Date().toISOString().slice(0, 10)
		) {
			context.addIssue({
				code: "custom",
				path: ["dateOfBirth"],
				message: "The date of birth must be in the past.",
			});
		}

		/*
		 * The address is all or nothing on the backend: touch one part of it and street, building,
		 * postal code, city and country all become required. Saying so here beats five separate
		 * errors coming back from the server.
		 */
		const address = [
			value.street,
			value.buildingNumber,
			value.unitNumber,
			value.postalCode,
			value.city,
			value.addressCountryCode,
		];

		if (address.some(Boolean)) {
			const required = [
				["street", value.street, "Street is required"],
				["buildingNumber", value.buildingNumber, "Building number is required"],
				["postalCode", value.postalCode, "Postal code is required"],
				["city", value.city, "City is required"],
				["addressCountryCode", value.addressCountryCode, "Country is required"],
			] as const;

			for (const [path, given, message] of required) {
				if (!given) {
					context.addIssue({ code: "custom", path: [path], message });
				}
			}
		}
	});

export type WorkerFormValues = z.infer<typeof workerSchema>;

export type WorkerField = keyof WorkerFormValues;

export const emptyWorker: WorkerFormValues = {
	sourceApplicationId: "",
	sourceCandidateId: "",
	firstName: "",
	lastName: "",
	dateOfBirth: "",
	citizenship: "",
	identityDocumentKind: "",
	identityDocumentNumber: "",
	identityDocumentIssuingCountry: "",
	identityDocumentValidUntil: "",
	email: "",
	phoneNumber: "",
	street: "",
	buildingNumber: "",
	unitNumber: "",
	postalCode: "",
	city: "",
	addressCountryCode: "",
	note: "",
};
