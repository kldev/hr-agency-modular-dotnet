import { forwardRef, useImperativeHandle, useState } from "react";
import { ConfirmDialog, Dialog, useUnsavedChangesGuard } from "#/components/ui";
import { PlanAssignmentWizard } from "./PlanAssignmentWizard";
import { emptyPlanAssignment, type PlanAssignmentFormValues } from "./schema";

export interface PlanAssignmentWizardCommand {
	/** Opened from a person, from a project, or from neither; whatever is known is filled in. */
	plan: (preset?: { workerId?: string; projectId?: string }) => void;
}

interface PlanAssignmentWizardDialogProps {
	onSuccess: (assignmentId: string) => void;
}

type Target = {
	values: PlanAssignmentFormValues;
	knownWorker: boolean;
	knownProject: boolean;
};

const PlanAssignmentWizardDialog = forwardRef<
	PlanAssignmentWizardCommand,
	PlanAssignmentWizardDialogProps
>(({ onSuccess }, ref) => {
	const [target, setTarget] = useState<Target | null>(null);

	const { setDirty, confirming, requestClose, discard, keepEditing } = useUnsavedChangesGuard(() =>
		setTarget(null),
	);

	useImperativeHandle(
		ref,
		() => ({
			plan: (preset) => {
				setDirty(false);
				setTarget({
					values: {
						...emptyPlanAssignment,
						workerId: preset?.workerId ?? "",
						projectId: preset?.projectId ?? "",
					},
					knownWorker: Boolean(preset?.workerId),
					knownProject: Boolean(preset?.projectId),
				});
			},
		}),
		[setDirty],
	);

	if (!target) {
		return null;
	}

	return (
		<>
			<Dialog open={true} title="Plan assignment" maxWidth="wide" onClose={requestClose}>
				<PlanAssignmentWizard
					key={`${target.values.workerId}-${target.values.projectId}`}
					initialValues={target.values}
					knownWorker={target.knownWorker}
					knownProject={target.knownProject}
					onDirtyChange={setDirty}
					onCancel={requestClose}
					onPlanned={(assignmentId) => {
						discard();
						onSuccess(assignmentId);
					}}
				/>
			</Dialog>

			<ConfirmDialog
				open={confirming}
				title="Discard this assignment?"
				description="Nothing has been saved yet. Closing now throws away everything filled in so far."
				confirmLabel="Discard"
				onConfirm={discard}
				onClose={keepEditing}
			/>
		</>
	);
});

PlanAssignmentWizardDialog.displayName = "PlanAssignmentWizardDialog";

export { PlanAssignmentWizardDialog };
