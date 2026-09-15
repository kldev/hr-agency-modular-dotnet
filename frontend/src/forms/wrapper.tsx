import { useMemo, useState } from "react";
import type { CompanySuggestion, OrganizationRole, UserSuggestion } from "#/api/models";
import {
	CompaniesPicker,
	DatePicker,
	type DatePickerProps,
	EnumSelectFilter,
	FieldError,
	Input,
	type InputProps,
	Textarea,
	type TextareaProps,
	Toggle,
	type ToggleProps,
	UsersPicker,
} from "#/components/ui";
import { parseScheduledAt } from "#/features/interviews/utils";
import { formatLocalDateTime } from "#/utlis/formatLocalDateTime";

type AppInputProps<T> = {
	label: string;
	fieldName: string;
	fieldValue?: T | null;
	isSubmitting: boolean;
	errors: Array<unknown>;
	handleChange: (val: T) => void;
};

type FormInputProps = InputProps & AppInputProps<string>;

export function FormInput({
	label,
	fieldName,
	isSubmitting,
	fieldValue,
	onBlur,
	handleChange,
	errors,
	...props
}: FormInputProps) {
	return (
		<div className="form-field">
			<label className="form-label" htmlFor={fieldName}>
				{label}
			</label>

			<Input
				{...props}
				id={fieldName}
				name={fieldName}
				value={fieldValue ?? ""}
				disabled={isSubmitting}
				onBlur={onBlur}
				onChange={(event) => handleChange(event.target.value)}
			/>

			<FieldError errors={errors} />
		</div>
	);
}

type FormTextProps = TextareaProps & AppInputProps<string>;

export function FormTextAreaInput({
	label,
	fieldName,
	isSubmitting,
	fieldValue,
	onBlur,
	handleChange,
	errors,
	...props
}: FormTextProps) {
	return (
		<div className="form-field">
			<label className="form-label" htmlFor={fieldName}>
				{label}
			</label>

			<Textarea
				{...props}
				id={fieldName}
				name={fieldName}
				value={fieldValue ?? ""}
				disabled={isSubmitting}
				onBlur={onBlur}
				onChange={(event) => handleChange(event.target.value)}
			/>

			<FieldError errors={errors} />
		</div>
	);
}

type FormUserPickerProps = {
	role?: OrganizationRole;
} & AppInputProps<{ id: string | null; user?: UserSuggestion | null }>;

export function FormUserPicker({
	label,
	fieldName,
	isSubmitting,
	fieldValue,
	handleChange,
	role,
	errors,
}: FormUserPickerProps) {
	const [input, setInput] = useState("");

	return (
		<div className="form-field">
			<label className="form-label" htmlFor={fieldName}>
				{label}
			</label>
			<UsersPicker
				disabled={isSubmitting}
				value={fieldValue?.id ?? ""}
				inputValue={input}
				onChange={(id, item) => handleChange({ id: id, user: item })}
				role={role}
				onInputChange={(v) => {
					setInput(v);
				}}
			/>
			<FieldError errors={errors} />
		</div>
	);
}

type FormCompanyPickerProps = {} & AppInputProps<{
	id: string | null;
	company?: CompanySuggestion | null;
}>;

export function FormCompanyPicker({
	label,
	fieldName,
	isSubmitting,
	fieldValue,
	handleChange,

	errors,
}: FormCompanyPickerProps) {
	const [input, setInput] = useState("");

	return (
		<div className="form-field">
			<label className="form-label" htmlFor={fieldName}>
				{label}
			</label>
			<CompaniesPicker
				disabled={isSubmitting}
				value={fieldValue?.id ?? ""}
				inputValue={input}
				onChange={(id, item) => handleChange({ id: id, company: item })}
				onInputChange={(v) => {
					setInput(v);
				}}
			/>
			<FieldError errors={errors} />
		</div>
	);
}

type FormSelectEnumProps<T extends string> = {
	options: Record<T, string>;
} & AppInputProps<T>;

export function FormSelectEnum<T extends string>({
	label,
	fieldName,
	fieldValue,
	handleChange,
	options,

	errors,
}: FormSelectEnumProps<T>) {
	return (
		<div className="form-field">
			<label className="form-label" htmlFor={fieldName}>
				{label}
			</label>
			<EnumSelectFilter
				value={fieldValue ? (fieldValue as T) : null}
				options={options}
				onChange={(val) => handleChange(val as T)}
			/>
			<FieldError errors={errors} />
		</div>
	);
}

type FormToggleProps = ToggleProps & AppInputProps<boolean>;

export function FormToggle({
	label,
	fieldName,
	isSubmitting,
	fieldValue,
	onBlur,
	handleChange,
	errors,
	...props
}: FormToggleProps) {
	return (
		<div className="form-field">
			<label className="form-label" htmlFor={fieldName}>
				{label}
			</label>

			<Toggle
				{...props}
				id={fieldName}
				name={fieldName}
				checked={fieldValue || undefined}
				disabled={isSubmitting}
				onBlur={onBlur}
				onChange={(event) => handleChange(event.target.checked)}
			/>

			<FieldError errors={errors} />
		</div>
	);
}

type FormDatePickerProps = DatePickerProps & AppInputProps<string>;

export function FormDatePicker({
	label,
	fieldName,
	isSubmitting,
	fieldValue,
	handleChange,
	onChange,
	errors,
	...props
}: FormDatePickerProps) {
	const initialDate = useMemo(() => parseScheduledAt(fieldValue ?? ""), [fieldValue]);
	return (
		<div className="form-field">
			<label className="form-label" htmlFor={fieldName}>
				{label}
			</label>

			<DatePicker
				{...props}
				value={initialDate.date}
				onChange={(val) => handleChange(val ? formatLocalDateTime(val, "12:00") : "")}
				disabled={isSubmitting}
				clearable
			/>

			<FieldError errors={errors} />
		</div>
	);
}
