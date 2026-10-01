/**
 * The money field types with a decimal comma, the way it is written here, and `Number` reads only
 * a dot - so "42,40" parsed directly is `NaN`, which fails every range check with a misleading
 * message. Everything that reads or fills a money field goes through these two.
 */
export function parseMoney(value: string): number {
	return Number(value.replace(",", "."));
}

/** The reverse, for filling the field from the API: 42.4 becomes "42,4", which the field accepts. */
export function moneyInputValue(amount: number | string | null | undefined): string {
	return amount === null || amount === undefined || amount === ""
		? ""
		: String(amount).replace(".", ",");
}

/** Digits, then an optional comma with at most four digits after it - what a money field accepts. */
export function isMoneyInput(value: string): boolean {
	return /^\d*(,\d{0,4})?$/.test(value);
}
