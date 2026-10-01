import {
	Checkbox,
	CheckboxGroup,
	Description,
	FieldError as HeroFieldError,
	Input,
	type InputProps,
	Label,
	Switch,
	TextArea,
	type TextAreaProps,
	TextField,
	toast,
} from "@heroui/react";
import { KeyRound } from "lucide-react";
import { useMemo, useState } from "react";
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
	getErrorMessage,
	isMoneyInput,
	LanguageSelect,
	TeamsPicker,
	TimeInput,
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

/*
 * The bridge between TanStack Form and HeroUI. Every field below takes the same props, whatever
 * control it draws: zod issues and 400 field errors arrive in `errors`, and become `isInvalid` plus
 * the messages in the field's own FieldError. `validationBehavior="aria"` everywhere: the form's
 * schema is the authority, and native validation would add a second, different message.
 *
 * The name and id stay the form key (`fieldName`), so they land on the <input> itself - which is
 * what the dynamic forms' `f_<fieldId>` controls and every label lookup rely on.
 */

/** Every message, in order: TanStack keeps zod issues or plain strings side by side. */
function errorMessages(errors: Array<unknown>): string[] {
	return errors.filter(Boolean).map(getErrorMessage);
}

/** The messages of an invalid field, inside a HeroUI field so it is linked to the control. */
function FieldMessages({ messages }: { messages: string[] }) {
	if (messages.length === 0) {
		return null;
	}

	return (
		<HeroFieldError className="form-field-error font-medium">
			{messages.map((message, index) => (
				<span key={`${message}-${index}`} className="block">
					{message}
				</span>
			))}
		</HeroFieldError>
	);
}

type TextInputProps = Omit<InputProps, "value" | "onChange" | "onBlur" | "name" | "id"> & {
	onBlur?: () => void;
};

type FormInputProps = TextInputProps & AppInputProps<string>;

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
	const messages = errorMessages(errors);

	return (
		<TextField
			fullWidth
			className="form-field"
			id={fieldName}
			name={fieldName}
			value={fieldValue ?? ""}
			onChange={handleChange}
			onBlur={onBlur}
			isDisabled={isSubmitting}
			isInvalid={messages.length > 0}
			validationBehavior="aria"
		>
			<Label className="form-label">{label}</Label>
			<Input {...props} />
			<FieldMessages messages={messages} />
		</TextField>
	);
}

type FormPasswordInputProps = {
	hint?: string;
} & TextInputProps &
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
	const messages = errorMessages(errors);

	return (
		<TextField
			fullWidth
			className="form-field"
			id={fieldName}
			name={fieldName}
			type="password"
			value={fieldValue ?? ""}
			onChange={handleChange}
			onBlur={onBlur}
			isDisabled={isSubmitting}
			isInvalid={messages.length > 0}
			validationBehavior="aria"
		>
			<Label className="form-label">{label}</Label>

			<div className="form-input-action">
				<Input {...props} autoComplete="new-password" />

				<Button
					variant="ghost"
					aria-label="Generate a password"
					icon={<KeyRound size={16} />}
					isDisabled={isSubmitting}
					onPress={async () => {
						const password = generatePassword();
						await copyToClipboard(`User password: ${password}`);
						handleChange(password);
						toast.info("Password copied to clipboard");
					}}
				></Button>
			</div>

			{hint ? <Description className="form-hint">{hint}</Description> : null}

			<FieldMessages messages={messages} />
		</TextField>
	);
}

type FormTextProps = Omit<TextAreaProps, "value" | "onChange" | "onBlur" | "name" | "id"> & {
	onBlur?: () => void;
} & AppInputProps<string>;

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
	const messages = errorMessages(errors);

	return (
		<TextField
			fullWidth
			className="form-field"
			id={fieldName}
			name={fieldName}
			value={fieldValue ?? ""}
			onChange={handleChange}
			onBlur={onBlur}
			isDisabled={isSubmitting}
			isInvalid={messages.length > 0}
			validationBehavior="aria"
		>
			<Label className="form-label">{label}</Label>
			<TextArea {...props} />
			<FieldMessages messages={messages} />
		</TextField>
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
			<UsersPicker
				id={fieldName}
				label={label}
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
			<CompaniesPicker
				id={fieldName}
				label={label}
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
			<TeamsPicker
				id={fieldName}
				label={label}
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
	onBlur?: () => void;
} & AppInputProps<T>;

export function FormSelectEnum<T extends string>({
	label,
	fieldName,
	fieldValue,
	isSubmitting,
	handleChange,
	onBlur,
	options,
	errors,
}: FormSelectEnumProps<T>) {
	const messages = errorMessages(errors);

	return (
		<EnumSelectFilter
			id={fieldName}
			name={fieldName}
			label={label}
			hideAll={true}
			value={fieldValue ? (fieldValue as T) : null}
			options={options}
			isDisabled={isSubmitting}
			errorMessage={messages.length > 0 ? <FieldMessageList messages={messages} /> : undefined}
			onBlur={onBlur}
			// the form holds a string: "nothing chosen" is "", never null
			onChange={(val) => handleChange((val ?? "") as T)}
		/>
	);
}

/** The messages as lines, for a composition that renders its own FieldError. */
function FieldMessageList({ messages }: { messages: string[] }) {
	return messages.map((message, index) => (
		<span key={`${message}-${index}`} className="block">
			{message}
		</span>
	));
}

type FormToggleProps = {
	onBlur?: () => void;
} & AppInputProps<boolean>;

export function FormToggle({
	label,
	fieldName,
	isSubmitting,
	fieldValue,
	onBlur,
	handleChange,
	errors,
}: FormToggleProps) {
	const messages = errorMessages(errors);

	return (
		<div className="form-field">
			{/* above the switch, as everywhere in the panel; the switch's input carries the id */}
			<label className="form-label" htmlFor={fieldName}>
				{label}
			</label>

			<Switch
				id={fieldName}
				name={fieldName}
				isSelected={fieldValue === true}
				onChange={handleChange}
				onBlur={onBlur}
				isDisabled={isSubmitting}
				isInvalid={messages.length > 0}
				validationBehavior="aria"
			>
				<Switch.Content>
					<Switch.Control>
						<Switch.Thumb />
					</Switch.Control>
				</Switch.Content>

				<FieldMessages messages={messages} />
			</Switch>
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

type FormMoneyInputProps = Omit<TextInputProps, "type"> & AppInputProps<string>;

/** An amount typed with a decimal comma; anything else is not let into the field at all. */
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
	const messages = errorMessages(errors);

	return (
		<TextField
			fullWidth
			className="form-field"
			id={fieldName}
			name={fieldName}
			value={fieldValue ?? ""}
			// a refused keystroke never reaches the form, so the controlled field keeps what it had
			onChange={(value) => {
				if (isMoneyInput(value)) {
					handleChange(value);
				}
			}}
			onBlur={onBlur}
			isDisabled={isSubmitting}
			isInvalid={messages.length > 0}
			validationBehavior="aria"
		>
			<Label className="form-label">{label}</Label>
			<Input {...props} type="text" inputMode="decimal" />
			<FieldMessages messages={messages} />
		</TextField>
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
	const messages = errorMessages(errors);

	return (
		<CountrySelect
			id={fieldName}
			name={fieldName}
			label={label}
			value={fieldValue}
			onChange={handleChange}
			isDisabled={isSubmitting}
			favorites={favorites}
			errorMessage={messages.length > 0 ? <FieldMessageList messages={messages} /> : undefined}
		/>
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
	const messages = errorMessages(errors);

	return (
		<div className="form-field">
			<LanguageSelect
				id={fieldName}
				name={fieldName}
				label={label}
				value={fieldValue}
				onChange={handleChange}
				isDisabled={isSubmitting}
				errorMessage={messages.length > 0 ? <FieldMessageList messages={messages} /> : undefined}
			/>

			{hint && <p className="text-xs text-(--color-text-muted)">{hint}</p>}
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
	const messages = errorMessages(errors);

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
				errorMessage={messages.length > 0 ? <FieldMessageList messages={messages} /> : undefined}
				onChange={(value) => handleChange(value as T)}
			/>
		</div>
	);
}

type FormCheckboxProps = AppInputProps<boolean> & {
	/** The sentence the box is ticked against - a consent reads as one, not as a label above a switch. */
	statement?: string;
};

/**
 * A box somebody ticks to agree. Not `FormToggle`: a switch reads as a setting, and a consent or a
 * declaration has to read as something the person states.
 */
export function FormCheckbox({
	label,
	fieldName,
	isSubmitting,
	fieldValue,
	handleChange,
	errors,
	statement,
}: FormCheckboxProps) {
	const messages = errorMessages(errors);

	return (
		<Checkbox
			className="form-field"
			id={fieldName}
			name={fieldName}
			isSelected={fieldValue === true}
			onChange={handleChange}
			isDisabled={isSubmitting}
			isInvalid={messages.length > 0}
			validationBehavior="aria"
		>
			{/* a statement wraps over lines, so the box sits at its first line, not its middle */}
			<Checkbox.Content className="items-start font-normal text-(--color-text)">
				<Checkbox.Control className="mt-0.5">
					<Checkbox.Indicator />
				</Checkbox.Control>

				<span>{statement ?? label}</span>
			</Checkbox.Content>

			<FieldMessages messages={messages} />
		</Checkbox>
	);
}

type FormMultiChoiceProps = AppInputProps<string[]> & {
	options: ReadonlyArray<{ value: string; label: string }>;
};

/** Several answers out of a list, each a box of its own; the order ticked is kept as given. */
export function FormMultiChoice({
	label,
	fieldName,
	isSubmitting,
	fieldValue,
	handleChange,
	errors,
	options,
}: FormMultiChoiceProps) {
	const messages = errorMessages(errors);

	return (
		<CheckboxGroup
			className="form-field"
			name={fieldName}
			// React Aria appends a ticked value and filters an unticked one, so the order is kept
			value={fieldValue ?? []}
			onChange={handleChange}
			isDisabled={isSubmitting}
			isInvalid={messages.length > 0}
			validationBehavior="aria"
		>
			<Label className="form-label">{label}</Label>

			<div className="flex flex-col gap-2">
				{options.map((option) => (
					<Checkbox key={option.value} value={option.value} className="mt-0">
						<Checkbox.Content className="font-normal text-(--color-text)">
							<Checkbox.Control>
								<Checkbox.Indicator />
							</Checkbox.Control>
							{option.label}
						</Checkbox.Content>
					</Checkbox>
				))}
			</div>

			<FieldMessages messages={messages} />
		</CheckboxGroup>
	);
}
