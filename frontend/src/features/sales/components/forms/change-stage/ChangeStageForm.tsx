import { z } from "zod";
import { OpportunityStage } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { salesStageOptions } from "#/features/sales/types";
import { withForm } from "#/forms";
import { FormTextAreaInput } from "#/forms/wrapper";
import type { OpportunityInfo } from "../SalesCommand";

export type ChangeStageFormValues = {
	stage: OpportunityStage;
	lostReason: string;
};

export const changeStageSchema = z
	.object({
		stage: z.enum(OpportunityStage),
		lostReason: z.string(),
	})
	.superRefine((data, ctx) => {
		if (data.stage === "Lost" && !data.lostReason.trim()) {
			ctx.addIssue({
				code: "custom",
				path: ["lostReason"],
				message: "Lost reason is required when opportunity is lost.",
			});
		}
	});
const emptyForm: ChangeStageFormValues = {
	stage: "Proposal",
	lostReason: "",
};

export const ChangeStageForm = withForm({
	defaultValues: emptyForm,

	props: {
		info: {} as OpportunityInfo,
		error: null as Error | null,
		isSubmitting: false,
	},

	render: function Render({ form, info, error, isSubmitting }) {
		return (
			<>
				<form.AppField name="stage">
					{(field) => (
						<field.FormSelectEnum
							fieldValue={field.state.value}
							options={salesStageOptions}
							label="Stage"
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={field.handleChange}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="lostReason">
					{(field) => (
						<field.FormTextAreaInput
							fieldValue={field.state.value}
							label="Lost reason"
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={field.handleChange}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<FormTextAreaInput
					isSubmitting
					fieldName="title"
					errors={[]}
					label="Title"
					fieldValue={info.title}
					disabled
					handleChange={() => {}}
				/>

				<ApiError error={error as unknown as Parameters<typeof ApiError>[0]["error"]} />
			</>
		);
	},
});
