import { FieldError, Label, ListBox, Select } from "@heroui/react";
import clsx from "clsx";
import type { ReactNode } from "react";

/*
 * The collection key standing in for "" - the "all" entry, or an option a caller supplies with an
 * empty value (the compliance proof picker's "No document"). An empty string is a poor collection
 * key, and the select's own "nothing chosen" is null, which is a different thing: a placeholder.
 */
const EMPTY_KEY = "__empty__";

type EnumSelectFilterProps<T extends string> = {
	id?: string;
	/** For a select with no visible label of its own, like one per row of a repeatable field. */
	"aria-label"?: string;
	value: T | null;
	onChange: (value: T | null | undefined) => void;
	options: Record<T, string>;
	allLabel?: string;
	/** Shown while nothing is picked, when there is no "all" entry to stand in for it. */
	placeholder?: string;
	className?: string;
	hideAll?: boolean;
	/** A visible label inside the field, linked to it - what a form field wants. */
	label?: string;
	name?: string;
	isDisabled?: boolean;
	/** Marks the field invalid and shows the message under it. */
	errorMessage?: ReactNode;
	onBlur?: () => void;
};

export function EnumSelectFilter<T extends string>({
	id,
	"aria-label": ariaLabel,
	value,
	onChange,
	options,
	allLabel = "All",
	placeholder = "Select…",
	className,
	hideAll,
	label,
	name,
	isDisabled,
	errorMessage,
	onBlur,
}: EnumSelectFilterProps<T>) {
	const keys = Object.keys(options) as T[];
	const hasEmptyOption = keys.includes("" as T);

	const entries = [
		...(hideAll ? [] : [{ key: EMPTY_KEY, text: allLabel }]),
		// with an "all" entry present, a caller's own empty option would be a second key for ""
		...keys
			.filter((option) => hideAll || option !== "")
			.map((option) => ({ key: option === "" ? EMPTY_KEY : option, text: options[option] })),
	];

	/*
	 * An empty value selects the "all" entry or the caller's empty option when there is one, and
	 * shows the placeholder otherwise - never the first real option, or a required field would read
	 * as answered while the form holds nothing.
	 */
	const selectedKey = value ? value : !hideAll || hasEmptyOption ? EMPTY_KEY : null;

	return (
		<Select
			fullWidth
			id={id}
			name={name}
			aria-label={label ? undefined : (ariaLabel ?? (hideAll ? placeholder : allLabel))}
			className={clsx("enum-select-filter", label && "form-field", className)}
			placeholder={placeholder}
			value={selectedKey}
			isDisabled={isDisabled}
			isInvalid={Boolean(errorMessage)}
			// the form's schema is the authority; native validation would add a second message
			validationBehavior="aria"
			onBlur={onBlur}
			onChange={(key) => onChange(key === null || key === EMPTY_KEY ? null : (key as T))}
		>
			{label ? <Label className="form-label">{label}</Label> : null}

			<Select.Trigger>
				<Select.Value />
				<Select.Indicator />
			</Select.Trigger>

			<Select.Popover>
				<ListBox>
					{entries.map((entry) => (
						<ListBox.Item key={entry.key} id={entry.key} textValue={entry.text}>
							{entry.text}
							<ListBox.ItemIndicator />
						</ListBox.Item>
					))}
				</ListBox>
			</Select.Popover>

			{errorMessage ? (
				<FieldError className="form-field-error font-medium">{errorMessage}</FieldError>
			) : null}
		</Select>
	);
}
