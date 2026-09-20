import { z } from "zod";

export const projectSchema = z
	.object({
		companyId: z.string().min(1, "Pick a client"),
		name: z
			.string()
			.trim()
			.min(1, "Name is required")
			.max(200, "Name cannot exceed 200 characters."),
		description: z
			.string()
			.trim()
			.min(1, "Description is required")
			.max(2000, "Description cannot exceed 2000 characters."),
		engagementType: z.string().min(1, "Pick the engagement type"),
		street: z.string().trim().min(1, "Street is required"),
		buildingNumber: z.string().trim().min(1, "Building number is required"),
		unitNumber: z.string().trim(),
		postalCode: z.string().trim().min(1, "Postal code is required"),
		city: z.string().trim().min(1, "City is required"),
		countryCode: z.string().trim().min(2, "Country is required"),
		startsOn: z.string().min(1, "Start date is required"),
		endsOn: z.string(),
		teamId: z.string(),
	})
	.superRefine((value, context) => {
		if (value.endsOn && value.endsOn < value.startsOn) {
			context.addIssue({
				code: "custom",
				path: ["endsOn"],
				message: "The end date cannot be earlier than the start date.",
			});
		}
	});

export type ProjectFormValues = z.infer<typeof projectSchema>;

export type ProjectField = keyof ProjectFormValues;

export const emptyProject: ProjectFormValues = {
	companyId: "",
	name: "",
	description: "",
	engagementType: "",
	street: "",
	buildingNumber: "",
	unitNumber: "",
	postalCode: "",
	city: "",
	countryCode: "",
	startsOn: "",
	endsOn: "",
	teamId: "",
};
