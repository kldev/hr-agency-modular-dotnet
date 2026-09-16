import { z } from "zod";
import { CurrencyCode } from "#/api/models";

import { ApiError } from "#/components/ui/ApiError";
import { currenciesOptions } from "#/features/sales/types";
import { withForm } from "#/forms";

interface OpportunityFormProps {
	error: Error | null;
	isSubmitting: boolean;
}

export const opportunityEditSchema = z.object({
	currency: z.enum(CurrencyCode),

	title: z
		.string()
		.trim()
		.min(1, "Title is required")
		.min(3, "Title should be at least 3 characters long")
		.max(300, "Title cannot exceed 300 characters."),

	description: z
		.string()
		.trim()
		.refine(
			(value) => value.length === 0 || value.length >= 3,
			"Description should be at least 3 characters long",
		)
		.max(5000, "Description cannot exceed 5000 characters."),

	expectedValue: z
		.string()
		.min(1, "Expected value is required")
		.regex(/^\d+(,\d{1,4})?$/, "Expected value must be a valid amount")
		.refine(
			(value) => Number(value.replace(",", ".")) > 0,
			"Expected value must be greater than zero",
		)
		.refine((value) => Number(value.replace(",", ".")) <= 100_000, "Max value is 100,000"),

	isHotLead: z.boolean(),

	expectedCloseDate: z.string().nullable(),
});

export type OpportunityEditFormValues = {
	title: string;
	description: string;
	expectedValue: string;
	isHotLead: boolean;
	currency: CurrencyCode;
	expectedCloseDate: string | null;
};

const emptyForm: OpportunityEditFormValues = {
	currency: "EUR",
	title: "",
	description: "",
	expectedCloseDate: null,
	isHotLead: false,
	expectedValue: "",
};

export const EditOpportunityForm = withForm({
	defaultValues: emptyForm,
	props: {
		isSubmitting: false,
		error: null,
	} as OpportunityFormProps,
	render: function Render({ form, isSubmitting, error }) {
		return (
			<>
				<form.AppField name="title">
					{(field) => (
						<field.FormInput
							label="Title"
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(val) => field.handleChange(val)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>
				<form.AppField name="description">
					{(field) => (
						<field.FormTextAreaInput
							label="Description"
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(val) => field.handleChange(val)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>
				<form.AppField name="expectedValue">
					{(field) => (
						<field.FormMoneyInput
							label="Expected value"
							fieldValue={Number.isNaN(field.state.value) ? "" : field.state.value.toString()}
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(val) => field.handleChange(val)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>
				<form.AppField name="currency">
					{(field) => (
						<field.FormSelectEnum
							fieldValue={field.state.value}
							options={currenciesOptions}
							label="Currency"
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(val) => field.handleChange(val)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>
				<form.AppField name="isHotLead">
					{(field) => (
						<field.FormToggle
							fieldValue={field.state.value}
							label="Hot lead"
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(val) => field.handleChange(val)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>
				<form.AppField name="expectedCloseDate">
					{(field) => (
						<field.FormDatePicker
							fieldValue={field.state.value}
							minDate={new Date()}
							label="Expected close date"
							errors={field.state.meta.errors}
							fieldName={field.name}
							onChange={() => {}}
							handleChange={(val) => field.handleChange(val)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>
				<ApiError error={error as unknown as Parameters<typeof ApiError>[0]["error"]} />
			</>
		);
	},
});
