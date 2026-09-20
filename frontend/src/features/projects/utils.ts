/**
 * `FormDatePicker` keeps a full local date-time (it fixes the time at noon); the API takes plain
 * dates. Cutting the string is enough and, unlike `new Date(...)`, cannot move the day across a
 * timezone boundary.
 */
export function toDateOnly(value: string): string {
	return value.slice(0, 10);
}
