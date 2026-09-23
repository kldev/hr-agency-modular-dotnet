import type { FormField } from "#/api/models";
import {
	FormCheckbox,
	FormChoiceGroup,
	FormCountrySelect,
	FormDatePicker,
	FormInput,
	FormMultiChoice,
	FormSelectEnum,
	FormTextAreaInput,
} from "#/forms/wrapper";
import type { ControlValue } from "../schema/answerValues";

/*
 * A choice with a handful of answers reads best as cards somebody clicks; a long list does not fit
 * on a screen that way and becomes a select. The limit is a presentation decision, so it lives here
 * rather than in the form definition.
 */
const MAX_CARDS = 6;

type DynamicFieldProps = {
	field: FormField;
	/** The key the control has in the form - never the dotted code, see `fieldKeys.ts`. */
	name: string;
	value: ControlValue;
	errors: Array<unknown>;
	disabled?: boolean;
	onChange: (value: ControlValue) => void;
	onBlur?: () => void;
};

/**
 * One field of a generated form, drawn by the control its type calls for. The only place a
 * `FieldType` becomes a component: adding a type is a case here and a mirror in the validator,
 * never a component per form.
 */
export function DynamicField({
	field,
	name,
	value,
	errors,
	disabled = false,
	onChange,
	onBlur,
}: DynamicFieldProps) {
	const label = field.rules.required ? `${field.label} *` : field.label;
	const text = typeof value === "string" ? value : "";
	const common = { label, fieldName: name, errors, isSubmitting: disabled };
	const options = Object.fromEntries(field.options.map((option) => [option.value, option.label]));

	const input = (() => {
		switch (field.type) {
			case "TextArea":
				return (
					<FormTextAreaInput
						{...common}
						rows={4}
						placeholder={field.placeholder ?? undefined}
						fieldValue={text}
						onBlur={onBlur}
						handleChange={onChange}
					/>
				);
			case "Number":
				return (
					<FormInput
						{...common}
						inputMode="decimal"
						placeholder={field.placeholder ?? undefined}
						fieldValue={text}
						onBlur={onBlur}
						handleChange={onChange}
					/>
				);
			case "Date":
				return <FormDatePicker {...common} fieldValue={text} handleChange={onChange} />;
			case "Boolean":
				return (
					<FormCheckbox
						{...common}
						statement={label}
						fieldValue={value === true}
						handleChange={onChange}
					/>
				);
			case "SingleChoice":
				return field.options.length <= MAX_CARDS ? (
					<FormChoiceGroup
						{...common}
						columns={field.options.length > 3 ? 3 : 1}
						options={options}
						fieldValue={text}
						handleChange={onChange}
					/>
				) : (
					<FormSelectEnum {...common} options={options} fieldValue={text} handleChange={onChange} />
				);
			case "MultiChoice":
				return (
					<FormMultiChoice
						{...common}
						options={field.options}
						fieldValue={Array.isArray(value) ? value : []}
						handleChange={onChange}
					/>
				);
			case "Country":
				return <FormCountrySelect {...common} fieldValue={text} handleChange={onChange} />;
			default:
				return (
					<FormInput
						{...common}
						type={field.type === "Email" ? "email" : field.type === "Phone" ? "tel" : "text"}
						placeholder={field.placeholder ?? undefined}
						fieldValue={text}
						onBlur={onBlur}
						handleChange={onChange}
					/>
				);
		}
	})();

	return (
		<div className="mb-4">
			{input}
			{field.description ? (
				<p className="-mt-2 text-xs text-(--color-text-muted)">{field.description}</p>
			) : null}
		</div>
	);
}
