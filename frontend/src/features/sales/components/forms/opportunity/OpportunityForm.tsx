import { z } from "zod";
import { CurrencyCode } from "#/api/models";
import { FieldError } from "#/components/ui";
import { ApiError } from "#/components/ui/ApiError";
import { currenciesOptions } from "#/features/sales/types";
import { withForm } from "#/forms";

interface OpportunityFormProps {
	error: Error | null;
	isSubmitting: boolean;
	companyId?: string;
}

export const opportunitySchema = z.object({
	companyId: z.string().trim().min(1, "Company is required"),

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

	responsibleId: z.string().nullable(),

	expectedCloseDate: z.string().nullable(),
});

export const empty: OpportunityFormValues = {
	companyId: "",
	currency: "PLN",
	title: "",
	description: "",
	expectedCloseDate: null,
	expectedValue: "",
	isHotLead: false,
	responsibleId: "",
};

type OpportunityFormValues = {
	companyId: string;
	title: string;
	description: string;
	expectedValue: string;
	isHotLead: boolean;
	currency: CurrencyCode;
	expectedCloseDate: string | null;
	responsibleId: string | null;
};

export const OpportunityForm = withForm({
	props: {} as OpportunityFormProps,
	defaultValues: empty,
	render: function Render({ form, isSubmitting, error, companyId }) {
		return (
			<>
				{!companyId ? (
					<form.AppField name="companyId">
						{(field) => (
							<field.FormCompanyPicker
								label="Company"
								fieldValue={{ id: field.state.value }}
								errors={field.state.meta.errors}
								fieldName={field.name}
								handleChange={(val) => field.handleChange(val.id ?? "")}
								isSubmitting={isSubmitting}
							/>
						)}
					</form.AppField>
				) : (
					<form.Field name="companyId">
						{(field) => <FieldError errors={field.state.meta.errors} />}
					</form.Field>
				)}

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

				<form.AppField name="responsibleId">
					{(field) => (
						<field.FormUserPicker
							fieldValue={{ id: field.state.value }}
							label="Responsible person"
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(val) => field.handleChange(val.id)}
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
