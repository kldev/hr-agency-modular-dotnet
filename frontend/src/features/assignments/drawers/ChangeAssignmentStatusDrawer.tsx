import { forwardRef, useImperativeHandle, useState } from "react";
import { z } from "zod";
import type { AssignmentProjection, AssignmentStatus } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
/*
 * The person, read from the worker side of the same backend module: whether somebody may start work
 * is a fact about them, not about the posting, and the assignment projection deliberately does not
 * copy their status onto every row of theirs.
 */
import { useGetWorker } from "@/features/workers/pages/hooks";
import { workerStatuses } from "@/features/workers/types";
import { toDateOnly } from "@/utlis/formatRecord";
import { useChangeAssignmentStatus } from "../pages/hooks";
import {
	allowedAssignmentStatusTransitions,
	assignmentEndStatuses,
	assignmentStatusDescriptions,
	assignmentStatuses,
} from "../types";
import type { ChangeAssignmentStatusFormCommand } from "./AssignmentFormCommand";

interface ChangeAssignmentStatusDrawerProps {
	onSuccess: () => void;
}

const statusSchema = z.object({
	status: z.string().min(1, "Pick a status"),
	endsOn: z.string(),
	reason: z.string().trim().max(500, "The reason cannot exceed 500 characters."),
});

/** Mirrors `WorkerStatusChangePolicy.MayStartWork`: planning ahead is fine, starting is not. */
const mayStartWork = (status: string | undefined) =>
	status === "Employed" || status === "ProjectChange";

const FormContent: React.FC<{
	assignment: AssignmentProjection;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ assignment, onSuccess, handleClose }) => {
	const worker = useGetWorker(assignment.workerId);

	const { mutation, waiting } = useChangeAssignmentStatus({
		onSuccess: () => {
			mutation.reset();
			onSuccess();
			handleClose();
		},
	});

	const targets = allowedAssignmentStatusTransitions[assignment.status];

	const form = useAppForm({
		defaultValues: {
			status: "" as string,
			endsOn: assignment.endsOn ?? "",
			reason: "",
		},

		validators: { onChange: statusSchema },

		onSubmit: async ({ value }) => {
			const status = value.status as AssignmentStatus;
			const ending = assignmentEndStatuses.includes(status);

			mutation.mutate({
				assignmentId: assignment.id,
				request: {
					status,
					/* The last day is only asked for when the posting ends, and only then is it sent. */
					endsOn: ending && value.endsOn ? toDateOnly(value.endsOn) : null,
					reason: value.reason.trim() || null,
				},
			});
		},
	});

	const options = Object.fromEntries(
		targets.map((status) => [status, assignmentStatuses[status]]),
	) as Record<string, string>;

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title="Change status"
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<div className="form-hint">
							Now: <strong>{assignmentStatuses[assignment.status]}</strong>.{" "}
							{assignmentStatusDescriptions[assignment.status]}
						</div>

						{targets.length === 0 ? (
							<p className="form-hint">
								This posting is over and stays that way. Somebody working for the same client again
								is a new assignment, which is why assignments exist separately from people.
							</p>
						) : (
							<>
								<form.AppField name="status">
									{(field) => (
										<field.FormSelectEnum
											label="New status"
											options={options}
											fieldValue={field.state.value}
											errors={field.state.meta.errors}
											fieldName={field.name}
											handleChange={(value) => field.handleChange(value)}
											isSubmitting={mutation.isPending}
										/>
									)}
								</form.AppField>

								<form.Subscribe selector={(state) => state.values.status}>
									{(status) => (
										<>
											{/*
											 * Said before submitting rather than handled as a 400 afterwards: the
											 * backend refuses to start somebody who is still in the pipeline, and the
											 * answer is to move them along, not to try again.
											 */}
											{status === "Active" && worker.data && !mayStartWork(worker.data.status) ? (
												<div className="form-error" role="alert">
													{assignment.workerFullName} is{" "}
													{workerStatuses[worker.data.status ?? "Recruitment"]} and only somebody
													employed, or moving between projects, may start work. The posting can stay
													planned until they are through the pipeline.
												</div>
											) : null}

											{assignmentEndStatuses.includes(status as AssignmentStatus) ? (
												<form.AppField name="endsOn">
													{(field) => (
														<field.FormDatePicker
															label="Ended on"
															fieldValue={field.state.value}
															errors={field.state.meta.errors}
															fieldName={field.name}
															handleChange={(value) => field.handleChange(value)}
															isSubmitting={mutation.isPending}
														/>
													)}
												</form.AppField>
											) : null}

											{status === "DidNotStart" ? (
												<div className="form-hint">
													Nobody was there, so there is no last day to record. Deliberately not the
													same fact as breaking off early.
												</div>
											) : null}
										</>
									)}
								</form.Subscribe>

								<form.AppField name="reason">
									{(field) => (
										<field.FormTextAreaInput
											label="Reason"
											rows={3}
											fieldValue={field.state.value}
											errors={field.state.meta.errors}
											fieldName={field.name}
											handleChange={(value) => field.handleChange(value)}
											isSubmitting={mutation.isPending}
										/>
									)}
								</form.AppField>
							</>
						)}

						<ApiError
							error={mutation.error as unknown as Parameters<typeof ApiError>[0]["error"]}
						/>
					</div>
				</FormDrawer.Content>

				{targets.length > 0 ? (
					<FormDrawer.Footer>
						<form.FormSaveChangesButton wait={waiting} isPending={mutation.isPending} />
					</FormDrawer.Footer>
				) : null}
			</FormDrawer>
		</form.AppForm>
	);
};

const ChangeAssignmentStatusDrawer = forwardRef<
	ChangeAssignmentStatusFormCommand,
	ChangeAssignmentStatusDrawerProps
>(({ onSuccess }, ref) => {
	const [assignment, setAssignment] = useState<AssignmentProjection | null>(null);

	useImperativeHandle(ref, () => ({ changeStatus: setAssignment }), []);

	if (!assignment) return null;

	return (
		<FormContent
			assignment={assignment}
			onSuccess={onSuccess}
			handleClose={() => setAssignment(null)}
		/>
	);
});

ChangeAssignmentStatusDrawer.displayName = "ChangeAssignmentStatusDrawer";

export { ChangeAssignmentStatusDrawer };
