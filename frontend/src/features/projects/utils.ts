import type { PostalAddress } from "#/api/models";
import { formatDate } from "#/utlis/dateUtils";

/**
 * `FormDatePicker` keeps a full local date-time (it fixes the time at noon); the API takes plain
 * dates. Cutting the string is enough and, unlike `new Date(...)`, cannot move the day across a
 * timezone boundary.
 */
export function toDateOnly(value: string): string {
	return value.slice(0, 10);
}

/** One answer to "how is an address written here", for the three places that show one. */
export function formatAddress(address: PostalAddress): string {
	const building = address.unitNumber
		? `${address.buildingNumber}/${address.unitNumber}`
		: address.buildingNumber;

	return `${address.street} ${building}, ${address.postalCode} ${address.city}, ${address.countryCode}`;
}

/** A project period, with the dash standing in for "open-ended". */
export function formatPeriod(startsOn: string, endsOn: string | null): string {
	return `${formatDate(startsOn)} – ${endsOn ? formatDate(endsOn) : "—"}`;
}
