import { forwardRef, useImperativeHandle, useState } from "react";
import { ConfirmDialog, Dialog, useUnsavedChangesGuard } from "#/components/ui";
import { CreateProjectWizard } from "./CreateProjectWizard";

export interface CreateProjectWizardCommand {
	create: () => void;
}

interface CreateProjectWizardDialogProps {
	onSuccess: (projectId: string) => void;
}

/**
 * The wizard lives in a dialog rather than on its own route, and that is the whole reason this
 * component exists: a dialog closes on a stray Escape or a click on the cross, where a route does
 * not. Anything already typed is therefore worth a question before it disappears.
 */
const CreateProjectWizardDialog = forwardRef<
	CreateProjectWizardCommand,
	CreateProjectWizardDialogProps
>(({ onSuccess }, ref) => {
	const [open, setOpen] = useState(false);

	const { setDirty, confirming, requestClose, discard, keepEditing } = useUnsavedChangesGuard(() =>
		setOpen(false),
	);

	useImperativeHandle(
		ref,
		() => ({
			create: () => {
				setDirty(false);
				setOpen(true);
			},
		}),
		[setDirty],
	);

	if (!open) {
		return null;
	}

	return (
		<>
			<Dialog open={true} title="New project" maxWidth="wide" onClose={requestClose}>
				<CreateProjectWizard
					onDirtyChange={setDirty}
					onCancel={requestClose}
					onCreated={(projectId) => {
						discard();
						onSuccess(projectId);
					}}
				/>
			</Dialog>

			<ConfirmDialog
				open={confirming}
				title="Discard this project?"
				description="Nothing has been saved yet. Closing now throws away everything filled in so far."
				confirmLabel="Discard"
				onConfirm={discard}
				onClose={keepEditing}
			/>
		</>
	);
});

CreateProjectWizardDialog.displayName = "CreateProjectWizardDialog";

export { CreateProjectWizardDialog };
