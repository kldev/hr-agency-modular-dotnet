import { forwardRef, useImperativeHandle, useState } from "react";
import { toast } from "sonner";
import { z } from "zod";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import type { TimeSheetProjection } from "@/api/models";
import { useApproveTimeSheet } from "../pages/hooks";
import { formatMinutes, monthLabel } from "../types";
import type { ApproveTimeSheetFormCommand } from "./TimeSheetFormCommand";

interface Props {
	onSuccess: () => void;
}

const schema = z.object({
	comment: z.string().max(500, "Note cannot exceed 500 characters."),
});

const FormContent: React.FC<{
	sheet: TimeSheetProjection;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ sheet, onSuccess, handleClose }) => {
	const { mutation, waiting } = useApproveTimeSheet({
		onSuccess: () => {
			mutation.reset();
			toast.success("Month approved");
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: { comment: "" },

		validators: { onChange: schema },

		onSubmit: async ({ value }) => {
			mutation.mutate({
				sheet: {
					userId: sheet.userId,
					year: Number(sheet.year),
					month: Number(sheet.month),
				},
				request: { comment: value.comment.trim() === "" ? null : value.comment.trim() },
			});
		},
	});

	const person = `${sheet.user.firstName} ${sheet.user.lastName}`;

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title={`Approve ${person}`}
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<div className="form-hint">
							{monthLabel({ year: Number(sheet.year), month: Number(sheet.month) })} ·{" "}
							{formatMinutes(Number(sheet.totalMinutes ?? 0))} over {Number(sheet.filledDays ?? 0)}{" "}
							days.
						</div>

						{/*
						 * Optional here and required on a return - the asymmetry is the backend's and it
						 * is the point: saying nothing while agreeing is fine, saying nothing while
						 * handing a month back invites a second return.
						 */}
						<form.AppField name="comment">
							{(field) => (
								<field.FormTextAreaInput
									label="Comment"
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

const ApproveTimeSheetDrawer = forwardRef<ApproveTimeSheetFormCommand, Props>(
	({ onSuccess }, ref) => {
		const [target, setTarget] = useState<TimeSheetProjection | null>(null);

		useImperativeHandle(ref, () => ({ approve: setTarget }), []);

		if (!target) return null;

		return <FormContent sheet={target} onSuccess={onSuccess} handleClose={() => setTarget(null)} />;
	},
);

ApproveTimeSheetDrawer.displayName = "ApproveTimeSheetDrawer";

export default ApproveTimeSheetDrawer;
