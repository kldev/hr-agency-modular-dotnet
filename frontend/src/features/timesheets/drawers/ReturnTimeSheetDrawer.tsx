import { forwardRef, useImperativeHandle, useState } from "react";
import { z } from "zod";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import type { TimeSheetProjection } from "@/api/models";
import { useReturnTimeSheet } from "../pages/hooks";
import { monthLabel } from "../types";
import type { ReturnTimeSheetFormCommand } from "./TimeSheetFormCommand";

interface Props {
	onSuccess: () => void;
}

/** Mirrors `ReturnTimeSheetForCorrectionHandler.ReasonRequiredMessage`. */
const schema = z.object({
	reason: z
		.string()
		.min(1, "Say what needs correcting. A sheet handed back without a reason comes straight back.")
		.max(500, "Note cannot exceed 500 characters."),
});

const FormContent: React.FC<{
	sheet: TimeSheetProjection;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ sheet, onSuccess, handleClose }) => {
	const { mutation, waiting } = useReturnTimeSheet({
		onSuccess: () => {
			mutation.reset();
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: { reason: "" },

		validators: { onChange: schema },

		onSubmit: async ({ value }) => {
			mutation.mutate({
				sheet: {
					userId: sheet.userId,
					year: Number(sheet.year),
					month: Number(sheet.month),
				},
				request: { reason: value.reason.trim() },
			});
		},
	});

	const person = `${sheet.user.firstName} ${sheet.user.lastName}`;

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title={`Send back to ${person}`}
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<div className="form-hint">
							{monthLabel({ year: Number(sheet.year), month: Number(sheet.month) })} goes back to
							being editable, and comes round for approval again once it is sent.
						</div>

						<form.AppField name="reason">
							{(field) => (
								<field.FormTextAreaInput
									label="What needs correcting"
									rows={4}
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<ApiError
							error={mutation.error as unknown as Parameters<typeof ApiError>[0]["error"]}
						/>
					</div>
				</FormDrawer.Content>

				<FormDrawer.Footer>
					<form.FormSaveChangesButton wait={waiting} isPending={mutation.isPending} />
				</FormDrawer.Footer>
			</FormDrawer>
		</form.AppForm>
	);
};

const ReturnTimeSheetDrawer = forwardRef<ReturnTimeSheetFormCommand, Props>(
	({ onSuccess }, ref) => {
		const [target, setTarget] = useState<TimeSheetProjection | null>(null);

		useImperativeHandle(ref, () => ({ returnForCorrection: setTarget }), []);

		if (!target) return null;

		return <FormContent sheet={target} onSuccess={onSuccess} handleClose={() => setTarget(null)} />;
	},
);

ReturnTimeSheetDrawer.displayName = "ReturnTimeSheetDrawer";

export default ReturnTimeSheetDrawer;
