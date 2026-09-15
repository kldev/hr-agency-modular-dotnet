import type { CreateOpportunityRequest } from "#/api/models";

import { ApiError } from "#/components/ui/ApiError";
import { currenciesOptions } from "#/features/sales/types";
import { useAppForm } from "#/forms";

interface OpportunityFormProps {
	companyId?: string;
	mode: "create" | "edit";
	onSubmit: (value: CreateOpportunityRequest) => void;
	error?: Error | null;
	isSubmitting?: boolean;
	formId: string;
}

const empty: CreateOpportunityRequest = {
	companyId: "",
	currency: "PLN",
	title: "",
	description: "",
	expectedCloseDate: null,
	expectedValue: 0,
	isHotLead: false,
	responsibleId: "",
};

export function CreateOpportunityForm({
	onSubmit,
	formId,
	error,
	isSubmitting = false,
	mode = "create",
	companyId,
}: OpportunityFormProps) {
	const form = useAppForm({
		defaultValues: empty,

		onSubmit: async ({ value }) => {
			onSubmit(value);
		},
	});

	return (
		<form
			id={formId}
			className="drawer-form"
			onSubmit={(event) => {
				event.preventDefault();
				void form.handleSubmit();
			}}
		>
			{mode === "create" && Boolean(companyId) === false ? (
				<form.AppField
					name="companyId"
					validators={{
						onChange: ({ value }) => {
							if (!value.trim()) {
								return "Company is required";
							}

							return undefined;
						},
					}}
				>
					{(field) => (
						<field.FormCompanyPicker
							label="Company"
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(val) => field.handleChange(val.id ?? "")}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>
			) : null}

			<form.AppField
				name="title"
				validators={{
					onChange: ({ value }) => {
						if (!value.trim()) {
							return "Title is required";
						}

						if (value.length < 3) {
							return "Title should be at least 3 characters long";
						}

						if (value.length > 300) {
							return "Title cannot exceed 300 characters.";
						}

						return undefined;
					},
				}}
			>
				{(field) => (
					<field.FormInput
						label="Title"
						errors={field.state.meta.errors}
						fieldName={field.name}
						handleChange={(val) => field.handleChange(val)}
						isSubmitting={isSubmitting}
					/>
				)}
			</form.AppField>

			<form.AppField
				name="description"
				validators={{
					onChange: ({ value }) => {
						if (!value.trim()) {
							return undefined;
						}

						if (value.length < 3) {
							return "Description should be at least 3 characters long";
						}

						if (value.length > 5000) {
							return "Description cannot exceed 5000 characters.";
						}

						return undefined;
					},
				}}
			>
				{(field) => (
					<field.FormTextAreaInput
						label="Description"
						errors={field.state.meta.errors}
						fieldName={field.name}
						handleChange={(val) => field.handleChange(val)}
						isSubmitting={isSubmitting}
					/>
				)}
			</form.AppField>

			<form.AppField
				name="expectedValue"
				validators={{
					onChange: ({ value }) => {
						if (!value) {
							return "Expected value is required";
						}

						if (Number.isNaN(value)) {
							return "Expected value is required";
						}

						if ((value as number) < 0) {
							return "Expected value must be greater then zero";
						}

						if ((value as number) > 1_000_00) {
							return "Max value is 1, 000 00";
						}

						return undefined;
					},
				}}
			>
				{(field) => (
					<field.FormInput
						label="Expected value"
						errors={field.state.meta.errors}
						fieldName={field.name}
						handleChange={(val) => field.handleChange(val)}
						isSubmitting={isSubmitting}
					/>
				)}
			</form.AppField>

			<form.AppField
				name="currency"
				validators={{
					onChange: ({ value }) => {
						if (!value) {
							return "Currency type is required";
						}

						return undefined;
					},
				}}
			>
				{(field) => (
					<field.FormSelectEnum
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
						label="Currency"
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
						minDate={new Date()}
						label="Responsible person"
						errors={field.state.meta.errors}
						fieldName={field.name}
						onChange={() => {}}
						handleChange={() => {}}
						isSubmitting={isSubmitting}
					/>
				)}
			</form.AppField>

			<ApiError error={error as unknown as Parameters<typeof ApiError>[0]["error"]} />
		</form>
	);
}
