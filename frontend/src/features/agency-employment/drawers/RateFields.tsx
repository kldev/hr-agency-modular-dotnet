import { z } from "zod";
import { withFieldGroup } from "#/forms";
import type { CurrencyCode, RateBasis, RateInput, RateUnit, WorkRate } from "@/api/models";
import { rateBases, rateUnits } from "@/features/positions/types";
import { currenciesOptions } from "@/features/sales/types";

/**
 * What somebody is paid, as the two employment drawers ask it. Optional in both: an empty amount
 * means "not quoted", which is what the backend stores for most people and what B2B always is.
 *
 * Rendered only for `isRates` - somebody else sends no rate at all, and the backend then keeps the
 * one in force instead of wiping it.
 */
export const rateFieldsSchema = z.object({
	rateAmount: z
		.string()
		.refine((value) => value === "" || Number(value) >= 0, "A rate cannot be negative."),
	rateCurrency: z.string(),
	rateUnit: z.string(),
	rateBasis: z.string(),
});

export type RateFieldsValues = z.infer<typeof rateFieldsSchema>;

/** Hourly and gross, because that is what the settlement file can turn into an amount. */
export const emptyRateFields: RateFieldsValues = {
	rateAmount: "",
	rateCurrency: "PLN",
	rateUnit: "Hourly",
	rateBasis: "Gross",
};

export const rateFieldNames = {
	rateAmount: "rateAmount",
	rateCurrency: "rateCurrency",
	rateUnit: "rateUnit",
	rateBasis: "rateBasis",
} as const;

export function rateFieldsOf(rate: WorkRate | null | undefined): RateFieldsValues {
	return rate
		? {
				rateAmount: String(rate.amount),
				rateCurrency: rate.currency,
				rateUnit: rate.unit,
				rateBasis: rate.basis,
			}
		: emptyRateFields;
}

export function toRateInput(values: RateFieldsValues): RateInput | null {
	if (values.rateAmount === "") return null;

	return {
		amount: Number(values.rateAmount),
		currency: values.rateCurrency,
		unit: values.rateUnit as RateUnit,
		basis: values.rateBasis as RateBasis,
	};
}

export const RateFields = withFieldGroup({
	defaultValues: emptyRateFields,
	props: {} as { isSubmitting?: boolean },
	render: function Render({ group, isSubmitting }) {
		return (
			<>
				<group.AppField name="rateAmount">
					{(field) => (
						<field.FormMoneyInput
							label="Rate"
							placeholder="Optional"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</group.AppField>

				<group.AppField name="rateCurrency">
					{(field) => (
						<field.FormSelectEnum<CurrencyCode>
							label="Currency"
							options={currenciesOptions}
							fieldName={field.name}
							fieldValue={field.state.value as CurrencyCode}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</group.AppField>

				<group.AppField name="rateUnit">
					{(field) => (
						<field.FormSelectEnum<RateUnit>
							label="Per"
							options={rateUnits}
							fieldName={field.name}
							fieldValue={field.state.value as RateUnit}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</group.AppField>

				{/* Without gross or net a rate is a number two people read differently. */}
				<group.AppField name="rateBasis">
					{(field) => (
						<field.FormSelectEnum<RateBasis>
							label="Basis"
							options={rateBases}
							fieldName={field.name}
							fieldValue={field.state.value as RateBasis}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</group.AppField>

				<div className="form-hint">
					Only an hourly rate becomes an amount in the settlement file. A monthly or daily one is
					shown next to the hours and left for payroll to work out.
				</div>
			</>
		);
	},
});
