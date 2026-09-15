import { z } from "zod";
import { CurrencyCode, type UpdateOpportunityRequest } from "#/api/models";

import { ApiError } from "#/components/ui/ApiError";
import { currenciesOptions } from "#/features/sales/types";
import { useAppForm } from "#/forms";

interface OpportunityFormProps {
	onSubmit: (value: UpdateOpportunityRequest) => void;
	error?: Error | null;
	isSubmitting?: boolean;
	formId: string;
	initial: OpportunityEditFormValues;
}

const opportunityEditSchema = z.object({
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

type OpportunityEditFormValues = {
	title: string;
	description: string;
	expectedValue: string;
	isHotLead: boolean;
	currency: CurrencyCode;
	expectedCloseDate: string | null;
};

export function EditOpportunityForm({
	onSubmit,
	formId,
	error,
	isSubmitting = false,

	initial,
}: OpportunityFormProps) {
	const form = useAppForm({
		defaultValues: { ...initial, expectedValue: initial.expectedValue?.toString() || "" },

		validators: {
			onChange: opportunityEditSchema,
		},

		onSubmit: async ({ value }) => {
			console.log(`${JSON.stringify(value)}`);
			onSubmit({
				...value,
				expectedCloseDate: value.expectedCloseDate,
				expectedValue: Number(value.expectedValue.replace(",", ".")),
			});
		},
	});

	console.log(`initial ${initial.expectedCloseDate}`);
	return (
		<form
			id={formId}
			className="drawer-form"
			onSubmit={(event) => {
				event.preventDefault();
				void form.handleSubmit();
			}}
		>
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
		</form>
	);
}
