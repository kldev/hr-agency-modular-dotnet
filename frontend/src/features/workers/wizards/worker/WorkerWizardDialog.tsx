import { forwardRef, useImperativeHandle, useState } from "react";
import type { WorkerProjection } from "#/api/models";
import { ConfirmDialog, Dialog, useUnsavedChangesGuard } from "#/components/ui";
import { emptyWorker, type WorkerFormValues } from "./schema";
import { WorkerWizard } from "./WorkerWizard";

/**
 * What the register already knows about somebody who applied through us. Passed whole rather than
 * as an id: the caller is holding the application row, and a second lookup to copy four fields we
 * already have on screen would be a round trip for nothing.
 */
export interface WorkerSource {
	applicationId: string;
	candidateId: string;
	firstName: string;
	lastName: string;
	email: string;
	phoneNumber: string;
}

export interface WorkerWizardCommand {
	/** Somebody new. `source` fills in what the application they came through already holds. */
	register: (source?: WorkerSource) => void;
	edit: (worker: WorkerProjection) => void;
}

interface WorkerWizardDialogProps {
	onSuccess: (workerId: string) => void;
}

type Target =
	| { mode: "register"; workerId?: undefined; values: WorkerFormValues }
	| { mode: "edit"; workerId: string; values: WorkerFormValues };

/** The projection is the read model; the wizard works in strings, so nulls flatten to "". */
function toFormValues(worker: WorkerProjection): WorkerFormValues {
	return {
		sourceApplicationId: "",
		sourceCandidateId: worker.sourceCandidateId ?? "",
		firstName: worker.firstName ?? "",
		lastName: worker.lastName ?? "",
		dateOfBirth: worker.dateOfBirth ?? "",
		citizenship: worker.citizenship ?? "",
		identityDocumentKind: worker.identityDocumentKind ?? "",
		identityDocumentNumber: worker.identityDocumentNumber ?? "",
		identityDocumentIssuingCountry: worker.identityDocumentIssuingCountry ?? "",
		identityDocumentValidUntil: worker.identityDocumentValidUntil ?? "",
		email: worker.email ?? "",
		phoneNumber: worker.phoneNumber ?? "",
		street: worker.address?.street ?? "",
		buildingNumber: worker.address?.buildingNumber ?? "",
		unitNumber: worker.address?.unitNumber ?? "",
		postalCode: worker.address?.postalCode ?? "",
		city: worker.address?.city ?? "",
		addressCountryCode: worker.address?.countryCode ?? "",
		note: worker.note ?? "",
	};
}

/**
 * One dialog for registering and for editing. A dialog rather than a route because that is the
 * convention here, and a wizard rather than a drawer because seventeen fields in a slide-over is a
 * scroll bar with a form somewhere inside it.
 */
const WorkerWizardDialog = forwardRef<WorkerWizardCommand, WorkerWizardDialogProps>(
	({ onSuccess }, ref) => {
		const [target, setTarget] = useState<Target | null>(null);

		const { setDirty, confirming, requestClose, discard, keepEditing } = useUnsavedChangesGuard(
			() => setTarget(null),
		);

		useImperativeHandle(
			ref,
			() => ({
				register: (source?: WorkerSource) => {
					setDirty(false);
					setTarget({
						mode: "register",
						/*
						 * Prefilled rather than locked: what somebody typed into a job board form is not
						 * always how their passport spells it, and the passport wins. Every one of these
						 * stays editable on the steps that follow.
						 */
						values: {
							...emptyWorker,
							sourceApplicationId: source?.applicationId ?? "",
							sourceCandidateId: source?.candidateId ?? "",
							firstName: source?.firstName ?? "",
							lastName: source?.lastName ?? "",
							email: source?.email ?? "",
							phoneNumber: source?.phoneNumber ?? "",
						},
					});
				},
				edit: (worker: WorkerProjection) => {
					setDirty(false);
					setTarget({
						mode: "edit",
						workerId: worker.id ?? "",
						values: toFormValues(worker),
					});
				},
			}),
			[setDirty],
		);

		if (!target) {
			return null;
		}

		const editing = target.mode === "edit";

		return (
			<>
				<Dialog
					open={true}
					title={editing ? "Edit worker" : "Register worker"}
					maxWidth="wide"
					onClose={requestClose}
				>
					<WorkerWizard
						key={target.workerId ?? `register-${target.values.sourceApplicationId}`}
						workerId={target.workerId}
						initialValues={target.values}
						onDirtyChange={setDirty}
						onCancel={requestClose}
						onSaved={(workerId) => {
							discard();
							onSuccess(workerId);
						}}
					/>
				</Dialog>

				<ConfirmDialog
					open={confirming}
					title={editing ? "Discard these changes?" : "Discard this worker?"}
					description="Nothing has been saved yet. Closing now throws away everything filled in so far."
					confirmLabel="Discard"
					onConfirm={discard}
					onClose={keepEditing}
				/>
			</>
		);
	},
);

WorkerWizardDialog.displayName = "WorkerWizardDialog";

export { WorkerWizardDialog };
