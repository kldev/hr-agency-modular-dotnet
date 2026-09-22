import clsx from "clsx";
import type { SelectHTMLAttributes } from "react";
import { europeanCountries } from "../types/europeanCountries";
import { agencyFavoriteCountries } from "../types/favoriteCountries";
import { Select } from "./Select";

type CountryCode = keyof typeof europeanCountries;

type CountrySelectProps = SelectHTMLAttributes<HTMLSelectElement> & {
	/**
	 * Codes shown first, in this order, above the rest. Defaults to the agency's own list; pass an
	 * empty array for a plain alphabetical list. A favourite is not repeated below - two options
	 * with one value make the browser show whichever comes first as the selected one.
	 */
	favorites?: readonly string[];
};

const allCodes = Object.keys(europeanCountries) as CountryCode[];

function isKnown(code: string): code is CountryCode {
	return Object.hasOwn(europeanCountries, code);
}

export function CountrySelect({
	value,
	onChange,
	className,
	favorites = agencyFavoriteCountries,
	...props
}: CountrySelectProps) {
	const pinned = favorites.filter(isKnown);
	const rest = allCodes
		.filter((code) => !pinned.includes(code))
		.sort((a, b) => europeanCountries[a].localeCompare(europeanCountries[b]));

	const option = (code: CountryCode) => (
		<option key={code} value={code}>
			{code} — {europeanCountries[code]}
		</option>
	);

	return (
		<Select
			{...props}
			value={value}
			onChange={onChange}
			className={clsx("country-select", className)}
		>
			<option value="">Select country</option>

			{pinned.length > 0 ? (
				<>
					<optgroup label="Most used">{pinned.map(option)}</optgroup>
					<optgroup label="All countries">{rest.map(option)}</optgroup>
				</>
			) : (
				rest.map(option)
			)}
		</Select>
	);
}
