import { z } from "zod";
import type { CreateOpportunityRequest } from "#/api/models";

import { ApiError } from "#/components/ui/ApiError";
import { activityTypeOptions, type LoggableActivityType } from "#/features/sales/types";
import { useAppForm } from "#/forms";

interface ActivityFormProps {
	onSubmit: (value: ActivityLogFormValues) => void;
	error?: Error | null;
	isSubmitting?: boolean;
	formId: string;
	initial?: CreateOpportunityRequest;
	/**
	 * The deals to log against, by id - for a screen about a company rather than one deal. Without
	 * it the form logs against the opportunity its drawer was opened for.
	 */
	opportunities?: Record<string, string>;
}

const activitySchema = z.object({
	type: z.enum(
		Object.keys(activityTypeOptions) as [LoggableActivityType, ...LoggableActivityType[]],
	),
	note: z.string(),
});

export type ActivityLogFormValues = {
	note: string;
	type: LoggableActivityType;
	opportunityId?: string;
};

export const empty: ActivityLogFormValues = {
	note: "",
	type: "Call",
};

export function ActivityForm({
	onSubmit,
	formId,
	error,
	isSubmitting = false,
	opportunities,
}: ActivityFormProps) {
	const form = useAppForm({
		defaultValues: opportunities ? { ...empty, opportunityId: "" } : empty,

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
			{opportunities ? (
				<form.AppField
					name="opportunityId"
					validators={{
						onChange: ({ value }) => (value ? undefined : "Pick the opportunity."),
					}}
				>
					{(field) => (
						<field.FormSelectEnum
							fieldValue={field.state.value ?? ""}
							options={opportunities}
							label="Opportunity"
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(val) => field.handleChange(val)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>
			) : null}

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
