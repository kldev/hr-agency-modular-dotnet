import { KeyRound } from "lucide-react";
import { useMemo, useState } from "react";
import { toast } from "sonner";
import type {
	CompanySuggestion,
	OrganizationRole,
	TeamSuggestion,
	UserSuggestion,
} from "#/api/models";
import {
	ArrayField,
	type ArrayFieldProps,
	Button,
	ChoiceGroup,
	CompaniesPicker,
	CountrySelect,
	DatePicker,
	type DatePickerProps,
	EnumSelectFilter,
	FieldError,
	Input,
	type InputProps,
	LanguageSelect,
	MoneyInput,
	type MoneyInputProps,
	TeamsPicker,
	Textarea,
	type TextareaProps,
	TimeInput,
	Toggle,
	type ToggleProps,
	UsersPicker,
} from "#/components/ui";
import { parseScheduledAt } from "#/features/interviews/utils";
import { copyToClipboard, generatePassword } from "#/utlis";
import { formatLocalDateTime } from "#/utlis/formatLocalDateTime";
import type { FormDateTimeValue } from ".";

type AppInputProps<T> = {
	label: string;
	fieldName: string;
	fieldValue: T | null;
	isSubmitting?: boolean;
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

type FormPasswordInputProps = {
	hint?: string;
} & InputProps &
	AppInputProps<string>;

/**
 * A password field with the generate-and-copy affordance the user-creation forms need. The generated
 * value goes to the clipboard because whoever creates the account has to pass it on - it is never
 * shown again.
 */
export function FormPasswordInput({
	label,
	fieldName,
	isSubmitting,
	fieldValue,
	onBlur,
	handleChange,
	errors,
	hint,
	...props
}: FormPasswordInputProps) {
	return (
		<div className="form-field">
			<label className="form-label" htmlFor={fieldName}>
				{label}
			</label>

			<div className="form-input-action">
				<Input
					{...props}
					id={fieldName}
					name={fieldName}
					type="password"
					autoComplete="new-password"
					value={fieldValue ?? ""}
					disabled={isSubmitting}
					onBlur={onBlur}
					onChange={(event) => handleChange(event.target.value)}
				/>

				<Button
					variant="ghost"
					icon={<KeyRound size={16} />}
					onClick={async () => {
						const password = generatePassword();
						await copyToClipboard(`User password: ${password}`);
						handleChange(password);
						toast.info("Password copied to clipboard");
					}}
				></Button>
			</div>

			{hint ? <div className="form-hint">{hint}</div> : null}

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
	placeholder?: string;
} & AppInputProps<{ id: string | null; user?: UserSuggestion | null }>;

export function FormUserPicker({
	label,
	fieldName,
	isSubmitting,
	fieldValue,
	handleChange,
	role,
	errors,
	placeholder,
}: FormUserPickerProps) {
	const [input, setInput] = useState("");

	return (
		<div className="form-field">
			<label className="form-label" htmlFor={fieldName}>
				{label}
			</label>
			<UsersPicker
				id={fieldName}
				placeholder={placeholder}
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
				id={fieldName}
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

type FormTeamPickerProps = {
	placeholder?: string;
	hint?: string;
} & AppInputProps<{ id: string | null; team?: TeamSuggestion | null }>;

export function FormTeamPicker({
	label,
	fieldName,
	isSubmitting,
	fieldValue,
	handleChange,
	errors,
	placeholder,
	hint,
}: FormTeamPickerProps) {
	const [input, setInput] = useState("");

	return (
		<div className="form-field">
			<label className="form-label" htmlFor={fieldName}>
				{label}
			</label>
			<TeamsPicker
				id={fieldName}
				placeholder={placeholder}
				disabled={isSubmitting}
				value={fieldValue?.id ?? ""}
				inputValue={input}
				onChange={(id, item) => handleChange({ id: id, team: item })}
				onInputChange={(v) => {
					setInput(v);
				}}
			/>
			{hint ? <div className="form-hint">{hint}</div> : null}
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
				id={fieldName}
				hideAll={true}
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
				checked={fieldValue == null ? undefined : fieldValue}
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
				id={fieldName}
				value={initialDate.date}
				onChange={(val) => handleChange(val ? formatLocalDateTime(val, "12:00") : "")}
				disabled={isSubmitting}
				clearable
			/>

			<FieldError errors={errors} />
		</div>
	);
}

type FormMoneyInputProps = MoneyInputProps & AppInputProps<string>;
export function FormMoneyInput({
	label,
	fieldName,
	isSubmitting,
	fieldValue,
	onBlur,
	handleChange,
	errors,
	...props
}: FormMoneyInputProps) {
	return (
		<div className="form-field">
			<label className="form-label" htmlFor={fieldName}>
				{" "}
				{label}
			</label>
			<MoneyInput
				{...props}
				id={fieldName}
				name={fieldName}
				value={fieldValue ?? ""}
				disabled={isSubmitting}
				onBlur={onBlur}
				onChange={(event) => handleChange(event.target.value)}
			/>{" "}
			<FieldError errors={errors} />{" "}
		</div>
	);
}

type FormDateTimeProps = DatePickerProps & AppInputProps<FormDateTimeValue>;

export function FormDateTime({
	label,
	fieldName,
	isSubmitting,
	fieldValue,
	handleChange,
	errors,
}: FormDateTimeProps) {
	const datePart = fieldValue ? fieldValue.date : null;
	const timePart = fieldValue ? fieldValue.time : null;

	return (
		<div className="form-field">
			<label className="form-label" htmlFor={fieldName}>
				{label}
			</label>

			<div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
				<div className="form-field">
					<span className="form-label">Date</span>

					<DatePicker
						value={datePart}
						onChange={(v) => {
							handleChange({ date: v, time: timePart ?? "" });
						}}
						disabled={isSubmitting}
						clearable
					/>
				</div>

				<div className="form-field">
					<span className="form-label">Time</span>

					<TimeInput
						value={timePart ?? undefined}
						onChange={(v) => handleChange({ date: datePart, time: v })}
						disabled={isSubmitting}
					/>
				</div>
			</div>

			<FieldError errors={errors} />
		</div>
	);
}

type FormTimeInputProps = {
	/** Widened where a night shift has to be enterable - see `TimeInput`. */
	fromMinutes?: number;
	toMinutes?: number;
} & AppInputProps<string>;

/**
 * `TimeInput` on its own has been in the tree since the interview forms, but only ever inside
 * `FormDateTime`, where a time is half of a moment. A work day is entered as a start and a length,
 * so the time stands alone here.
 */
export function FormTimeInput({
	label,
	fieldName,
	isSubmitting,
	fieldValue,
	handleChange,
	errors,
	fromMinutes,
	toMinutes,
}: FormTimeInputProps) {
	return (
		<div className="form-field">
			<label className="form-label" htmlFor={fieldName}>
				{label}
			</label>

			<TimeInput
				id={fieldName}
				name={fieldName}
				value={fieldValue ?? ""}
				disabled={isSubmitting}
				fromMinutes={fromMinutes}
				toMinutes={toMinutes}
				onChange={(value) => handleChange(value)}
			/>

			<FieldError errors={errors} />
		</div>
	);
}

type FormCountrySelectProps = {
	label: string;
	fieldName: string;
	fieldValue: string | null;
	isSubmitting: boolean;
	errors: Array<unknown>;
	handleChange: (value: string) => void;
	/** See `CountrySelect`: defaults to the agency's favourites, `[]` for a plain list. */
	favorites?: readonly string[];
};

export function FormCountrySelect({
	label,
	fieldName,
	fieldValue,
	isSubmitting,
	errors,
	handleChange,
	favorites,
}: FormCountrySelectProps) {
	return (
		<div className="form-field">
			<label className="form-label" htmlFor={fieldName}>
				{label}
			</label>

			<CountrySelect
				id={fieldName}
				value={fieldValue ?? ""}
				onChange={(event) => handleChange(event.target.value)}
				disabled={isSubmitting}
				favorites={favorites}
			/>

			<FieldError errors={errors} />
		</div>
	);
}

type FormLanguageSelectProps = {
	label: string;
	fieldName: string;
	fieldValue: string | null;
	isSubmitting: boolean;
	errors: Array<unknown>;
	handleChange: (value: string) => void;
	hint?: string;
};

export function FormLanguageSelect({
	label,
	fieldName,
	fieldValue,
	isSubmitting,
	errors,
	handleChange,
	hint,
}: FormLanguageSelectProps) {
	return (
		<div className="form-field">
			<label className="form-label" htmlFor={fieldName}>
				{label}
			</label>

			<LanguageSelect
				id={fieldName}
				value={fieldValue ?? ""}
				onChange={(event) => handleChange(event.target.value)}
				disabled={isSubmitting}
			/>

			{hint && <p className="text-xs text-(--color-text-muted)">{hint}</p>}

			<FieldError errors={errors} />
		</div>
	);
}

type FormArrayFieldProps = ArrayFieldProps & AppInputProps<string[]>;
export function FormArrayField({
	label,
	fieldName,
	isSubmitting,
	fieldValue,
	handleChange,
	errors,
	values = [],
	...props
}: FormArrayFieldProps) {
	return (
		<div className="form-field">
			<label className="form-label" htmlFor={fieldName}>
				{label}
			</label>
			<ArrayField
				{...props}
				values={fieldValue ?? []}
				label={undefined}
				itemLabel={label}
				disabled={isSubmitting}
				onChange={(values) => handleChange(values)}
			/>
			<FieldError errors={errors} />{" "}
		</div>
	);
}

type FormChoiceGroupProps<T extends string> = {
	options: Record<T, string>;
	descriptions?: Partial<Record<T, string>>;
	columns?: 1 | 2 | 3;
} & AppInputProps<T>;

export function FormChoiceGroup<T extends string>({
	label,
	fieldName,
	isSubmitting,
	fieldValue,
	handleChange,
	options,
	descriptions,
	columns,
	errors,
}: FormChoiceGroupProps<T>) {
	return (
		<div className="form-field">
			<ChoiceGroup
				label={label}
				name={fieldName}
				value={fieldValue ? (fieldValue as T) : null}
				options={options}
				descriptions={descriptions}
				columns={columns}
				disabled={isSubmitting}
				onChange={(value) => handleChange(value as T)}
			/>

			<FieldError errors={errors} />
		</div>
	);
}
