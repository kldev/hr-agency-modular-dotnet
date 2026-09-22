import { forwardRef, useImperativeHandle, useState } from "react";
import { z } from "zod";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import type { TimeSheetProjection } from "@/api/models";
import { useCommentOnTimeSheet } from "../pages/hooks";
import type { CommentOnTimeSheetFormCommand } from "./TimeSheetFormCommand";

interface Props {
	onSuccess: () => void;
}

const schema = z.object({
	content: z.string().min(1, "Write something").max(500, "Note cannot exceed 500 characters."),
});

const FormContent: React.FC<{
	sheet: TimeSheetProjection;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ sheet, onSuccess, handleClose }) => {
	const { mutation, waiting } = useCommentOnTimeSheet({
		onSuccess: () => {
			mutation.reset();
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: { content: "" },

		validators: { onChange: schema },

		onSubmit: async ({ value }) => {
			mutation.mutate({
				sheet: {
					userId: sheet.userId,
					year: Number(sheet.year),
					month: Number(sheet.month),
				},
				request: { content: value.content.trim() },
			});
		},
	});

	const person = `${sheet.user.firstName} ${sheet.user.lastName}`;

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title={`Comment on ${person}'s month`}
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						{/*
						 * The role the comment is written in is worked out by the backend from who is
						 * asking - the author cannot choose to speak as payroll.
						 */}
						<form.AppField name="content">
							{(field) => (
								<field.FormTextAreaInput
									label="Comment"
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

const CommentOnTimeSheetDrawer = forwardRef<CommentOnTimeSheetFormCommand, Props>(
	({ onSuccess }, ref) => {
		const [target, setTarget] = useState<TimeSheetProjection | null>(null);

		useImperativeHandle(ref, () => ({ comment: setTarget }), []);

		if (!target) return null;

		return <FormContent sheet={target} onSuccess={onSuccess} handleClose={() => setTarget(null)} />;
	},
);

CommentOnTimeSheetDrawer.displayName = "CommentOnTimeSheetDrawer";

export default CommentOnTimeSheetDrawer;
