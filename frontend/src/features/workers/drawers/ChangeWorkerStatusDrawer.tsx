import { forwardRef, useImperativeHandle, useState } from "react";
import { z } from "zod";
import type { WorkerProjection, WorkerStatus } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import { useChangeWorkerStatus } from "../pages/hooks";
import { allowedWorkerStatusTransitions, workerStatusDescriptions, workerStatuses } from "../types";
import type { ChangeWorkerStatusFormCommand } from "./WorkerFormCommand";

interface ChangeWorkerStatusDrawerProps {
	onSuccess: () => void;
}

const statusSchema = z.object({
	status: z.string().min(1, "Pick a status"),
	reason: z.string().trim().max(500, "The reason cannot exceed 500 characters."),
});

/** An identity document that has run out is the one precondition worth naming before submitting. */
function documentExpired(worker: WorkerProjection): boolean {
	if (!worker.identityDocumentValidUntil) {
		return false;
	}

	return worker.identityDocumentValidUntil.slice(0, 10) < new Date().toISOString().slice(0, 10);
}

const FormContent: React.FC<{
	worker: WorkerProjection;
	target?: WorkerStatus;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ worker, target, onSuccess, handleClose }) => {
	const { mutation, waiting } = useChangeWorkerStatus({
		onSuccess: () => {
			mutation.reset();
			onSuccess();
			handleClose();
		},
	});

	const targets = allowedWorkerStatusTransitions(
		worker.status ?? "Recruitment",
		Boolean(worker.requiresLegalisation),
	);

	const form = useAppForm({
		defaultValues: {
			status: target && targets.includes(target) ? (target as string) : "",
			reason: "",
		},

		validators: { onChange: statusSchema },

		onSubmit: async ({ value }) => {
			mutation.mutate({
				workerId: worker.id ?? "",
				request: {
					status: value.status as WorkerStatus,
					reason: value.reason.trim() || null,
				},
			});
		},
	});

	/*
	 * Only the transitions the domain allows are offered, and `Legalisation` is filtered out for
	 * anybody with free movement rights - the backend refuses it with a sentence of its own, and a
	 * missing option beats an error message.
	 */
	const options = Object.fromEntries(
		targets.map((status) => [status, workerStatuses[status]]),
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
							Now: <strong>{workerStatuses[worker.status ?? "Recruitment"]}</strong>.{" "}
							{workerStatusDescriptions[worker.status ?? "Recruitment"]}
						</div>

						{targets.length === 0 ? (
							<p className="form-hint">
								There is nowhere to go from here. A terminated file can only be reopened by
								preparing a new contract.
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
									{(status) =>
										status === "Employed" && documentExpired(worker) ? (
											<div className="form-error" role="alert">
												The identity document on file expired on{" "}
												{worker.identityDocumentValidUntil?.slice(0, 10)}. The domain refuses to
												mark somebody employed on a lapsed document — record a current one first.
											</div>
										) : null
									}
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

const ChangeWorkerStatusDrawer = forwardRef<
	ChangeWorkerStatusFormCommand,
	ChangeWorkerStatusDrawerProps
>(({ onSuccess }, ref) => {
	const [change, setChange] = useState<{ worker: WorkerProjection; target?: WorkerStatus } | null>(
		null,
	);

	useImperativeHandle(
		ref,
		() => ({ changeStatus: (worker, target) => setChange({ worker, target }) }),
		[],
	);

	if (!change) return null;

	return (
		<FormContent
			worker={change.worker}
			target={change.target}
			onSuccess={onSuccess}
			handleClose={() => setChange(null)}
		/>
	);
});

ChangeWorkerStatusDrawer.displayName = "ChangeWorkerStatusDrawer";

export { ChangeWorkerStatusDrawer };
