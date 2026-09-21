import { forwardRef, useImperativeHandle, useState } from "react";
import type { WorkerProjection } from "#/api/models";
import { ConfirmDialog, Dialog, useUnsavedChangesGuard } from "#/components/ui";
import { emptyWorker, type WorkerFormValues } from "./schema";
import { WorkerWizard } from "./WorkerWizard";

export interface WorkerWizardCommand {
	/** Somebody new. `applicationId` preselects the application they applied through. */
	register: (applicationId?: string) => void;
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
				register: (applicationId?: string) => {
					setDirty(false);
					setTarget({
						mode: "register",
						values: { ...emptyWorker, sourceApplicationId: applicationId ?? "" },
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
						key={target.workerId ?? "register"}
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
