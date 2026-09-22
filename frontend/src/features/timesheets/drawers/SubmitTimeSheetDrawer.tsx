import { forwardRef, useImperativeHandle, useState } from "react";
import { toast } from "sonner";
import { z } from "zod";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import { useSubmitTimeSheet } from "../pages/hooks";
import { formatMinutes, monthLabel, submitWarning } from "../types";
import type { SubmitTimeSheetFormCommand } from "./TimeSheetFormCommand";

interface Props {
	onSuccess: () => void;
}

type Target = { year: number; month: number; totalMinutes: number; days: number };

const schema = z.object({
	comment: z.string().max(500, "Note cannot exceed 500 characters."),
});

const FormContent: React.FC<{
	target: Target;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ target, onSuccess, handleClose }) => {
	const { mutation, waiting } = useSubmitTimeSheet({
		onSuccess: () => {
			mutation.reset();
			onSuccess();
			handleClose();
		},

		onError: () => {
			toast.error("The month could not be sent");
		},
	});

	const form = useAppForm({
		defaultValues: { comment: "" },

		validators: { onChange: schema },

		onSubmit: async ({ value }) => {
			mutation.mutate({
				year: target.year,
				month: target.month,
				comment: value.comment.trim() === "" ? null : value.comment.trim(),
			});
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title={`Send ${monthLabel(target)} for approval`}
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<div className="form-hint">
							{formatMinutes(target.totalMinutes)} over {target.days} days. {submitWarning}
						</div>

						{/*
						 * Optional, the same way it is when a supervisor approves: a month that speaks for
						 * itself needs nothing said about it, and the person handing it over is the one who
						 * knows which kind it is. "Three days off sick" belongs next to the hours rather
						 * than in a conversation nobody can find afterwards.
						 */}
						<form.AppField name="comment">
							{(field) => (
								<field.FormTextAreaInput
									label="Note for your supervisor"
									rows={3}
									placeholder="Optional"
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

const SubmitTimeSheetDrawer = forwardRef<SubmitTimeSheetFormCommand, Props>(
	({ onSuccess }, ref) => {
		const [target, setTarget] = useState<Target | null>(null);

		useImperativeHandle(ref, () => ({ submit: setTarget }), []);

		if (!target) return null;

		return (
			<FormContent target={target} onSuccess={onSuccess} handleClose={() => setTarget(null)} />
		);
	},
);

SubmitTimeSheetDrawer.displayName = "SubmitTimeSheetDrawer";

export default SubmitTimeSheetDrawer;
