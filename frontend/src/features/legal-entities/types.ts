import { BankAccountPurpose, CurrencyCode } from "#/api/models";

/**
 * Money in from clients and money out are separate accounts, and an invoice quotes the first one.
 * Labels rather than raw enum names, same as every other enum map in this codebase.
 */
export const bankAccountPurposes: Record<BankAccountPurpose, string> = {
	[BankAccountPurpose.Incoming]: "Client payments in",
	[BankAccountPurpose.Outgoing]: "Payouts",
};

export const currencyCodes: Record<CurrencyCode, string> = {
	[CurrencyCode.PLN]: "PLN",
	[CurrencyCode.EUR]: "EUR",
	[CurrencyCode.USD]: "USD",
	[CurrencyCode.GBP]: "GBP",
};

/** Whether the entity was trading on the given day - the same rule the backend applies. */
export function isTrading(
	activeFrom: string,
	activeTo: string | null | undefined,
	on = new Date(),
) {
	const day = on.toISOString().slice(0, 10);

	return activeFrom <= day && (!activeTo || activeTo >= day);
}
