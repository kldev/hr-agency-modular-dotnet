import { forwardRef, useImperativeHandle, useState } from "react";
import { z } from "zod";
import type { AssignmentProjection } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { PositionsPicker } from "#/components/ui/pickers";
import { useAppForm } from "#/forms";
import { toDateOnly } from "@/utlis/formatRecord";
import { useUpdateAssignment } from "../pages/hooks";
import type { EditAssignmentFormCommand } from "./AssignmentFormCommand";

interface EditAssignmentDrawerProps {
	onSuccess: () => void;
}

const assignmentSchema = z
	.object({
		positionId: z.string().min(1, "Pick the role this person is taking"),
		startsOn: z.string().min(1, "The start date is required"),
		endsOn: z.string(),
	})
	.superRefine((value, context) => {
		if (value.endsOn && value.startsOn && value.endsOn.slice(0, 10) < value.startsOn.slice(0, 10)) {
			context.addIssue({
				code: "custom",
				path: ["endsOn"],
				message: "An assignment cannot end before the day it started.",
			});
		}
	});

const FormContent: React.FC<{
	assignment: AssignmentProjection;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ assignment, onSuccess, handleClose }) => {
	const [roleInput, setRoleInput] = useState("");

	const { mutation, waiting } = useUpdateAssignment({
		onSuccess: () => {
			mutation.reset();
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: {
			positionId: assignment.positionId,
			startsOn: assignment.startsOn,
			endsOn: assignment.endsOn ?? "",
		},

		validators: { onChange: assignmentSchema },

		onSubmit: async ({ value }) => {
			mutation.mutate({
				assignmentId: assignment.id,
				request: {
					positionId: value.positionId,
					startsOn: toDateOnly(value.startsOn),
					endsOn: value.endsOn ? toDateOnly(value.endsOn) : null,
				},
			});
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title="Edit assignment"
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						{/*
						 * Three fields, and that is the whole command. The person, the project, the posting
						 * company and the engagement type were frozen when this posting was planned: moving
						 * somebody is ending one assignment and opening another, and which of the two applies
						 * decides what they owe - neither is an edit.
						 */}
						<div className="form-hint">
							{assignment.workerFullName} on {assignment.projectName}, posted by{" "}
							{assignment.deliveringEntityName}. Only the role and the period can be corrected here.
						</div>

						{/* Scoped to the posting's own project: the role cannot move to another delivery. */}
						<form.Subscribe selector={(state) => state.values.positionId}>
							{(positionId) => (
								<div className="form-field">
									<label className="form-label" htmlFor="positionId">
										Position
									</label>

									<PositionsPicker
										projectId={assignment.projectId}
										disabled={mutation.isPending}
										value={positionId}
										inputValue={roleInput}
										onInputChange={setRoleInput}
										onChange={(id) => form.setFieldValue("positionId", id ?? "")}
									/>
								</div>
							)}
						</form.Subscribe>

						<form.AppField name="startsOn">
							{(field) => (
								<field.FormDatePicker
									label="Starts on"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<form.AppField name="endsOn">
							{(field) => (
								<field.FormDatePicker
									label="Ends on"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<div className="form-hint">
							An open end is allowed. The period still has to sit inside the project's own, and it
							cannot overlap another posting of the same person - both come back named from the
							server.
						</div>

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

const EditAssignmentDrawer = forwardRef<EditAssignmentFormCommand, EditAssignmentDrawerProps>(
	({ onSuccess }, ref) => {
		const [assignment, setAssignment] = useState<AssignmentProjection | null>(null);

		useImperativeHandle(ref, () => ({ edit: setAssignment }), []);

		if (!assignment) return null;

		return (
			<FormContent
				assignment={assignment}
				onSuccess={onSuccess}
				handleClose={() => setAssignment(null)}
			/>
		);
	},
);

EditAssignmentDrawer.displayName = "EditAssignmentDrawer";

export { EditAssignmentDrawer };
