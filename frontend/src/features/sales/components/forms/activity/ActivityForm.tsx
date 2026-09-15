import { z } from "zod";
import { type CreateOpportunityRequest, SalesActivityType } from "#/api/models";

import { ApiError } from "#/components/ui/ApiError";
import { activityTypeOptions } from "#/features/sales/types";
import { useAppForm } from "#/forms";

interface ActivityFormProps {
	onSubmit: (value: ActivityLogFormValues) => void;
	error?: Error | null;
	isSubmitting?: boolean;
	formId: string;
	initial?: CreateOpportunityRequest;
}

const activitySchema = z.object({
	type: z.enum(SalesActivityType),
	note: z.string(),
});

export type ActivityLogFormValues = {
	note: string;
	type: SalesActivityType;
};

export const empty: ActivityLogFormValues = {
	note: "",
	type: "Call",
};

export function ActivityForm({ onSubmit, formId, error, isSubmitting = false }: ActivityFormProps) {
	const form = useAppForm({
		defaultValues: empty,

		validators: {
			onChange: activitySchema,
		},

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
			<form.AppField name="type">
				{(field) => (
					<field.FormSelectEnum
						fieldValue={field.state.value}
						options={activityTypeOptions}
						label="Type"
						errors={field.state.meta.errors}
						fieldName={field.name}
						handleChange={(val) => field.handleChange(val)}
						isSubmitting={isSubmitting}
					/>
				)}
			</form.AppField>
			<form.AppField name="note">
				{(field) => (
					<field.FormTextAreaInput
						label="Title"
						fieldValue={field.state.value}
						errors={field.state.meta.errors}
						fieldName={field.name}
						handleChange={(val) => field.handleChange(val)}
						isSubmitting={isSubmitting}
					/>
				)}
			</form.AppField>

			<ApiError error={error as unknown as Parameters<typeof ApiError>[0]["error"]} />
		</form>
	);
}
