import { forwardRef, useImperativeHandle, useState } from "react";
import { ConfirmDialog, Dialog } from "#/components/ui";
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
	const [dirty, setDirty] = useState(false);
	const [confirmingClose, setConfirmingClose] = useState(false);

	useImperativeHandle(
		ref,
		() => ({
			create: () => {
				setDirty(false);
				setOpen(true);
			},
		}),
		[],
	);

	const close = () => {
		setConfirmingClose(false);
		setDirty(false);
		setOpen(false);
	};

	const requestClose = () => {
		if (dirty) {
			setConfirmingClose(true);
			return;
		}

		close();
	};

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
						close();
						onSuccess(projectId);
					}}
				/>
			</Dialog>

			<ConfirmDialog
				open={confirmingClose}
				title="Discard this project?"
				description="Nothing has been saved yet. Closing now throws away everything filled in so far."
				confirmLabel="Discard"
				onConfirm={close}
				onClose={() => setConfirmingClose(false)}
			/>
		</>
	);
});

CreateProjectWizardDialog.displayName = "CreateProjectWizardDialog";

export { CreateProjectWizardDialog };
