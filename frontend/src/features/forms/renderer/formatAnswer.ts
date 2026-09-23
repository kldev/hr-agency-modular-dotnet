import type { FieldValue, FormField } from "#/api/models";
import { getCountryLabel } from "#/components/labels";
import { formatDate } from "#/utlis/dateUtils";

/** An answer as a person reads it on a summary: option labels rather than values, a real date. */
export function formatAnswer(field: FormField, value: FieldValue | null | undefined): string {
	if (!value) {
		return "";
	}

	const label = (option: string) => field.options.find((o) => o.value === option)?.label ?? option;

	switch (field.type) {
		case "Boolean":
			return value.boolean ? "Yes" : "No";
		case "Date":
			return value.date ? formatDate(value.date) : "";
		case "Number":
			return value.number == null ? "" : String(value.number);
		case "SingleChoice":
			return value.text ? label(value.text) : "";
		case "MultiChoice":
			return (value.values ?? []).map(label).join(", ");
		case "Country":
			return value.text ? (getCountryLabel(value.text) ?? value.text) : "";
		default:
			return value.text ?? "";
	}
}
