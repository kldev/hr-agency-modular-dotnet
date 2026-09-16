import { z } from "zod";
import { type ChangeOpportunityStageRequest, OpportunityStage } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { salesStageOptions } from "#/features/sales/types";
import { useAppForm } from "#/forms";
import { FormTextAreaInput } from "#/forms/wrapper";
import type { OpportunityInfo } from "../SalesCommand";

interface ChangeStageFormProps {
	onSubmit: (value: ChangeOpportunityStageRequest) => void;
	error?: Error | null;
	isSubmitting?: boolean;
	formId: string;
	info: OpportunityInfo;
}

const schema = z
	.object({
		stage: z.enum(OpportunityStage),
		lostReason: z.string(),
	})
	.superRefine((data, ctx) => {
		if (data.stage === "Lost" && !data.lostReason?.trim()) {
			ctx.addIssue({
				code: "custom",
				path: ["lostReason"],
				message: "Lost reason is required when opportunity is lost.",
			});
		}
	});

type FormValues = {
	stage: OpportunityStage;
	lostReason: string;
};

export function ChangeStageForm({
	onSubmit,
	formId,
	error,
	isSubmitting = false,
	info,
}: ChangeStageFormProps) {
	const initial: FormValues = {
		stage: info.stage,
		lostReason: "",
	};

	const form = useAppForm({
		defaultValues: initial,

		validators: {
			onChange: schema,
		},

		onSubmit: async ({ value }) => {
			onSubmit(value);
		},
	});

	return (
		<form.Subscribe selector={(state) => state.isValid}>
			{(isValid) => (
				<form
					id={formId}
					className={["drawer-form", isValid ? "" : "drawer-form-invalid"].join(" ")}
					onSubmit={(event) => {
						event.preventDefault();
						void form.handleSubmit();
					}}
				>
					<form.AppField name="stage">
						{(field) => (
							<field.FormSelectEnum
								fieldValue={field.state.value}
								options={salesStageOptions}
								label="Type"
								errors={field.state.meta.errors}
								fieldName={field.name}
								handleChange={(val) => field.handleChange(val)}
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
								handleChange={(val) => field.handleChange(val)}
								isSubmitting={isSubmitting}
							/>
						)}
					</form.AppField>

					<FormTextAreaInput
						isSubmitting={true}
						fieldName="title"
						errors={[]}
						label="Title"
						fieldValue={info.title}
						disabled={true}
						handleChange={() => {}}
					></FormTextAreaInput>

					<ApiError error={error as unknown as Parameters<typeof ApiError>[0]["error"]} />
				</form>
			)}
		</form.Subscribe>
	);
}
