import { type CurrencyCode, Industry } from "@/api/models";

export const industries = Object.fromEntries(
	Object.values(Industry).map((value) => [value, value]),
) as Record<Industry, string>;

export const currencies: Record<CurrencyCode, string> = {
	PLN: "PLN",
	EUR: "EUR",
	USD: "USD",
	GBP: "GBP",
};
