import type { FieldType, FormField, FormPage, SystemField } from "#/api/models";

/*
 * The builder's working copy of a layout, and every change it can make to it. Pure functions over
 * the whole list of pages, because the backend takes the whole layout at once (plan 028 §3.3) and
 * the preview renders whatever is on screen, saved or not. Moving is by a step or to another page;
 * dragging is a later stage, and will call the same `moveField`.
 */

export type Layout = FormPage[];

export type LayoutAction =
	| { type: "addPage"; title: string }
	| { type: "renamePage"; pageId: string; title: string }
	| { type: "movePage"; pageId: string; by: -1 | 1 }
	| { type: "removePage"; pageId: string }
	| { type: "addField"; pageId: string; field: FormField }
	| { type: "updateField"; field: FormField }
	| { type: "moveField"; fieldId: string; by: -1 | 1 }
	| { type: "moveFieldToPage"; fieldId: string; pageId: string }
	| { type: "removeField"; fieldId: string }
	| { type: "reset"; layout: Layout };

function move<T>(items: readonly T[], index: number, by: -1 | 1): T[] {
	const target = index + by;

	if (index < 0 || target < 0 || target >= items.length) {
		return [...items];
	}

	const copy = [...items];
	[copy[index], copy[target]] = [copy[target], copy[index]];

	return copy;
}

export function layoutReducer(layout: Layout, action: LayoutAction): Layout {
	switch (action.type) {
		case "reset":
			return action.layout;

		case "addPage":
			return [
				...layout,
				{ pageId: crypto.randomUUID(), title: action.title, description: null, fields: [] },
			];

		case "renamePage":
			return layout.map((page) =>
				page.pageId === action.pageId ? { ...page, title: action.title } : page,
			);

		case "movePage":
			return move(
				layout,
				layout.findIndex((page) => page.pageId === action.pageId),
				action.by,
			);

		case "removePage":
			return layout.filter((page) => page.pageId !== action.pageId);

		case "addField":
			return layout.map((page) =>
				page.pageId === action.pageId ? { ...page, fields: [...page.fields, action.field] } : page,
			);

		case "updateField":
			return layout.map((page) => ({
				...page,
				fields: page.fields.map((field) =>
					field.fieldId === action.field.fieldId ? action.field : field,
				),
			}));

		case "moveField":
			return layout.map((page) => {
				const index = page.fields.findIndex((field) => field.fieldId === action.fieldId);

				return index < 0 ? page : { ...page, fields: move(page.fields, index, action.by) };
			});

		case "moveFieldToPage": {
			const field = layout.flatMap((page) => page.fields).find((f) => f.fieldId === action.fieldId);

			if (!field) {
				return layout;
			}

			return layout.map((page) => {
				const without = page.fields.filter((f) => f.fieldId !== action.fieldId);

				return page.pageId === action.pageId
					? { ...page, fields: [...without, field] }
					: { ...page, fields: without };
			});
		}

		case "removeField":
			return layout.map((page) => ({
				...page,
				fields: page.fields.filter((field) => field.fieldId !== action.fieldId),
			}));
	}
}

const noRules: FormField["rules"] = {
	required: false,
	minLength: null,
	maxLength: null,
	pattern: null,
	min: null,
	max: null,
	decimals: null,
	minDate: null,
	maxDate: null,
	minSelected: null,
	maxSelected: null,
	message: null,
};

/** A field the author writes; the code starts from the label and stays editable. */
export function newFormField(type: FieldType, label: string, code: string): FormField {
	return {
		fieldId: crypto.randomUUID(),
		source: "Form",
		systemFieldId: null,
		code,
		type,
		label,
		labelOverride: null,
		description: null,
		placeholder: null,
		rules: { ...noRules },
		options:
			type === "SingleChoice" || type === "MultiChoice"
				? [
						{ value: "yes", label: "Yes" },
						{ value: "no", label: "No" },
					]
				: [],
		defaultValue: null,
		visibleWhen: null,
	};
}

/**
 * A system field as the builder places it: a copy of the catalogue for display, although only the
 * id and the label override count - the backend fills the rest from the catalogue on every save.
 */
export function newSystemField(definition: SystemField): FormField {
	return {
		fieldId: crypto.randomUUID(),
		source: "System",
		systemFieldId: definition.systemFieldId,
		code: definition.code,
		type: definition.type,
		label: definition.label,
		labelOverride: null,
		description: definition.description,
		placeholder: null,
		rules: definition.rules,
		options: definition.options,
		defaultValue: null,
		visibleWhen: null,
	};
}

/** `Tax office` -> `taxOffice`, prefixed by the form's area: a starting point, not a rule. */
export function suggestCode(prefix: string, label: string): string {
	const words = label
		.normalize("NFD")
		.replace(/[̀-ͯ]/g, "")
		.replace(/ł/g, "l")
		.replace(/Ł/g, "L")
		.toLowerCase()
		.split(/[^a-z0-9]+/)
		.filter(Boolean);

	const [first = "field", ...rest] = words;
	const name = first.replace(/^[0-9]+/, "") || "field";

	return `${prefix}.${name}${rest.map((word) => word[0].toUpperCase() + word.slice(1)).join("")}`;
}

/** The prefix a form's own codes share: its code in camelCase, `gdpr-consent` -> `gdprConsent`. */
export function codePrefix(formCode: string): string {
	return formCode.replace(/-([a-z0-9])/g, (_, letter: string) => letter.toUpperCase());
}
