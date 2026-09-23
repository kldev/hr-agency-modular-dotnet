import type {
	FieldType,
	FormKind,
	FormResponseStatus,
	FormStatus,
	OrganizationRole,
	ResponseCardinality,
	SystemFieldSource,
} from "#/api/models";

/**
 * Mirrors `Api/Auth/FormsDesignPolicy.cs`: designing forms, the catalogue and correcting a submitted
 * response. Filling one in is open to everybody. The backend stays the authority - hiding a button
 * only saves a click into a 403.
 */
export function isFormsDesigner(role: OrganizationRole | undefined | null): boolean {
	return role === "Admin" || role === "HumanResources";
}

export const fieldTypes: Record<FieldType, string> = {
	Text: "Text",
	TextArea: "Long text",
	Number: "Number",
	Date: "Date",
	Boolean: "Yes / no (tick box)",
	SingleChoice: "Single choice",
	MultiChoice: "Multiple choice",
	Email: "E-mail",
	Phone: "Phone",
	Country: "Country",
};

export const formKinds: Record<FormKind, string> = {
	Document: "Document",
	Survey: "Survey",
};

export const formKindDescriptions: Record<FormKind, string> = {
	Document: "A consent, a statement, a tax form - usually one per person.",
	Survey: "Feedback or a questionnaire - usually asked again and again.",
};

export const cardinalities: Record<ResponseCardinality, string> = {
	OnePerSubject: "One per person",
	Many: "Many per person",
};

export const formStatuses: Record<FormStatus, string> = {
	Draft: "Draft",
	Published: "Published",
	Archived: "Archived",
};

export const responseStatuses: Record<FormResponseStatus, string> = {
	Draft: "In progress",
	Submitted: "Submitted",
};

export const systemFieldSources: Record<SystemFieldSource, string> = {
	None: "None - entered in forms",
	WorkerFirstName: "Worker's first name",
	WorkerLastName: "Worker's last name",
	WorkerDateOfBirth: "Worker's date of birth",
	WorkerCitizenship: "Worker's citizenship",
	WorkerEmail: "Worker's e-mail",
	WorkerPhone: "Worker's phone",
};

/** Mirrors `SystemFieldSources.ValueType`: a source only fits a field of the type it gives. */
export const sourceTypes: Record<SystemFieldSource, FieldType | null> = {
	None: null,
	WorkerFirstName: "Text",
	WorkerLastName: "Text",
	WorkerDateOfBirth: "Date",
	WorkerCitizenship: "Country",
	WorkerEmail: "Email",
	WorkerPhone: "Phone",
};

/** Which rules the builder offers per type - mirrors the applicability checks of `FieldRulesPolicy`. */
export function takesTypedText(type: FieldType) {
	return type === "Text" || type === "TextArea" || type === "Email" || type === "Phone";
}

export function hasOptions(type: FieldType) {
	return type === "SingleChoice" || type === "MultiChoice";
}

/** Mirrors the prefix `FieldCode.SystemPrefix` reserves for the catalogue. */
export const systemCodePrefix = "employee.";

export const formStatusClass: Record<FormStatus, string> = {
	Draft: "badge-draft",
	Published: "badge-active",
	Archived: "badge-closed",
};

export const responseStatusClass: Record<FormResponseStatus, string> = {
	Draft: "badge-viewed",
	Submitted: "badge-won",
};
