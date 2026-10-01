import { ComboBox, FieldError, Header, Input, Label, ListBox, useFilter } from "@heroui/react";
import clsx from "clsx";
import type { ReactNode } from "react";
import { europeanCountries } from "../types/europeanCountries";
import { agencyFavoriteCountries } from "../types/favoriteCountries";

type CountryCode = keyof typeof europeanCountries;

type CountrySelectProps = {
	id?: string;
	name?: string;
	/** A visible label inside the field, linked to it. */
	label?: string;
	"aria-label"?: string;
	/** A country code, or "" / null for none. */
	value: string | null;
	/** Hands over the chosen code, "" once the field is cleared. */
	onChange: (code: string) => void;
	placeholder?: string;
	isDisabled?: boolean;
	/** Marks the field invalid and shows the message under it. */
	errorMessage?: ReactNode;
	onBlur?: () => void;
	className?: string;
	/**
	 * Codes shown first, in this order, above the rest. Defaults to the agency's own list; pass an
	 * empty array for a plain alphabetical list. A favourite is not repeated below - two options
	 * with one key would be one option shown twice.
	 */
	favorites?: readonly string[];
};

const allCodes = Object.keys(europeanCountries) as CountryCode[];

function isKnown(code: string): code is CountryCode {
	return Object.hasOwn(europeanCountries, code);
}

const optionText = (code: CountryCode) => `${code} — ${europeanCountries[code]}`;

/**
 * Countries as a combo box: the list is long, so typing "pol" or "PL" narrows it instead of a
 * scroll. Filtering is the combo box's own (the list is static), on the "PL — Poland" text.
 * Clearing the text clears the value, as choosing the empty option of a native select did.
 */
export function CountrySelect({
	id,
	name,
	label,
	"aria-label": ariaLabel,
	value,
	onChange,
	placeholder = "Select country",
	isDisabled,
	errorMessage,
	onBlur,
	className,
	favorites = agencyFavoriteCountries,
}: CountrySelectProps) {
	const { contains } = useFilter({ sensitivity: "base" });

	const pinned = favorites.filter(isKnown);
	const rest = allCodes
		.filter((code) => !pinned.includes(code))
		.sort((a, b) => europeanCountries[a].localeCompare(europeanCountries[b]));

	const option = (code: CountryCode) => (
		<ListBox.Item key={code} id={code} textValue={optionText(code)}>
			{optionText(code)}
			<ListBox.ItemIndicator />
		</ListBox.Item>
	);

	return (
		<ComboBox
			fullWidth
			id={id}
			name={name}
			aria-label={label ? undefined : (ariaLabel ?? placeholder)}
			className={clsx("country-select", label && "form-field", className)}
			selectedKey={value || null}
			onSelectionChange={(key) => onChange(key === null ? "" : String(key))}
			defaultFilter={contains}
			isDisabled={isDisabled}
			isInvalid={Boolean(errorMessage)}
			// the form's schema is the authority; native validation would add a second message
			validationBehavior="aria"
			onBlur={onBlur}
		>
			{label ? <Label className="form-label">{label}</Label> : null}

			<ComboBox.InputGroup>
				<Input placeholder={placeholder} />
				<ComboBox.Trigger />
			</ComboBox.InputGroup>

			<ComboBox.Popover>
				<ListBox>
					{pinned.length > 0 ? (
						<>
							<ListBox.Section>
								<Header>Most used</Header>
								{pinned.map(option)}
							</ListBox.Section>
							<ListBox.Section>
								<Header>All countries</Header>
								{rest.map(option)}
							</ListBox.Section>
						</>
					) : (
						rest.map(option)
					)}
				</ListBox>
			</ComboBox.Popover>

			{errorMessage ? (
				<FieldError className="form-field-error font-medium">{errorMessage}</FieldError>
			) : null}
		</ComboBox>
	);
}
