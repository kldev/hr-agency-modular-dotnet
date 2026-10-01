import { FieldError, type Key, Label, ListBox, Select } from "@heroui/react";
import clsx from "clsx";
import type { ReactNode } from "react";

/*
 * The collection key standing in for "" (an "empty" option such as "No opportunity"). An empty
 * string is a poor collection key, and the select's own "nothing chosen" is null, which shows the
 * placeholder instead.
 */
const EMPTY_KEY = "__empty__";

type SelectFieldProps = {
	id?: string;
	name?: string;
	/** A visible label inside the field, linked to it - what a form field wants. */
	label?: string;
	/** For a select with no visible label of its own, like one per row of a repeatable field. */
	"aria-label"?: string;
	/** The chosen option's id, or "" for none. */
	value: string | null;
	/** Hands over the chosen id, "" for the empty option. */
	onChange: (value: string) => void;
	/** Shown while nothing is chosen. */
	placeholder?: string;
	/** An option that stands for "" - for a choice where "none" is an answer, not a gap. */
	emptyLabel?: string;
	isDisabled?: boolean;
	disabledKeys?: Iterable<Key>;
	/** Marks the field invalid and shows the message under it. */
	errorMessage?: ReactNode;
	onBlur?: () => void;
	className?: string;
	/** `ListBox.Item`s (or sections), each with the option's value as its `id`. */
	children: ReactNode;
};

/**
 * HeroUI's Select for the lists a screen builds itself (deals, units, people of a unit, hours) -
 * enums go through `EnumSelectFilter`. The form holds a string, so this turns the collection's
 * `Key | null` back into one and keeps the empty option off the "" key.
 */
export function SelectField({
	id,
	name,
	label,
	"aria-label": ariaLabel,
	value,
	onChange,
	placeholder = "Select…",
	emptyLabel,
	isDisabled,
	disabledKeys,
	errorMessage,
	onBlur,
	className,
	children,
}: SelectFieldProps) {
	const selectedKey = value ? value : emptyLabel ? EMPTY_KEY : null;

	return (
		<Select
			fullWidth
			id={id}
			name={name}
			aria-label={label ? undefined : (ariaLabel ?? placeholder)}
			className={clsx("select-field", label && "form-field", className)}
			placeholder={placeholder}
			value={selectedKey}
			isDisabled={isDisabled}
			disabledKeys={disabledKeys}
			isInvalid={Boolean(errorMessage)}
			// the form's schema is the authority; native validation would add a second message
			validationBehavior="aria"
			onBlur={onBlur}
			onChange={(key) => onChange(key === null || key === EMPTY_KEY ? "" : String(key))}
		>
			{label ? <Label className="form-label">{label}</Label> : null}

			<Select.Trigger>
				<Select.Value />
				<Select.Indicator />
			</Select.Trigger>

			<Select.Popover>
				<ListBox>
					{emptyLabel ? (
						<ListBox.Item id={EMPTY_KEY} textValue={emptyLabel}>
							{emptyLabel}
							<ListBox.ItemIndicator />
						</ListBox.Item>
					) : null}

					{children}
				</ListBox>
			</Select.Popover>

			{errorMessage ? (
				<FieldError className="form-field-error font-medium">{errorMessage}</FieldError>
			) : null}
		</Select>
	);
}
