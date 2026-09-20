import type { CompanyProfileField } from "./schema";

export type CompanyProfileStep = {
	id: string;
	title: string;
	description: string;
	fields: readonly CompanyProfileField[];
};

export const companyProfileSteps = [
	{
		id: "identity",
		title: "Legal identity",
		description: "Who the company legally is",
		fields: ["legalName", "vatNumber"],
	},
	{
		id: "address",
		title: "Registered address",
		description: "Where it legally sits",
		fields: ["street", "buildingNumber", "unitNumber", "postalCode", "city", "countryCode"],
	},
	{
		id: "billing",
		title: "Billing",
		description: "Where the money goes",
		fields: ["iban", "bic"],
	},
	{
		id: "representation",
		title: "Representation",
		description: "Who signs for the company",
		fields: [
			"representativeFirstName",
			"representativeLastName",
			"representativeJobTitle",
			"representativePhone",
			"representativeEmail",
		],
	},
	{
		id: "review",
		title: "Review",
		description: "Check before saving",
		fields: [],
	},
] as const satisfies readonly CompanyProfileStep[];

/** Every field of the schema belongs to a step, or the wizard would never validate it. */
type GatedField = (typeof companyProfileSteps)[number]["fields"][number];
export type UngatedCompanyProfileField = Exclude<CompanyProfileField, GatedField>;
export const allSchemaFieldsAreGated = true satisfies UngatedCompanyProfileField extends never
	? true
	: false;
