import { forwardRef, useImperativeHandle, useState } from "react";
import { ConfirmDialog, DetailsLoading, Dialog, useUnsavedChangesGuard } from "#/components/ui";
import { useGetCompany } from "../../pages/hooks";
import { CompleteCompanyProfileWizard } from "./CompleteCompanyProfileWizard";

export interface CompleteCompanyProfileCommand {
	complete: (companyId: string) => void;
}

interface CompleteCompanyProfileWizardDialogProps {
	onSuccess: () => void;
}

/**
 * Lives in `features/companies` although a project is what usually opens it: these are the
 * company's own details, and the project is only one of the callers. The company details page
 * opens the same component.
 */
const CompleteCompanyProfileWizardDialog = forwardRef<
	CompleteCompanyProfileCommand,
	CompleteCompanyProfileWizardDialogProps
>(({ onSuccess }, ref) => {
	const [companyId, setCompanyId] = useState<string | null>(null);

	const { setDirty, confirming, requestClose, discard, keepEditing } = useUnsavedChangesGuard(() =>
		setCompanyId(null),
	);

	useImperativeHandle(
		ref,
		() => ({
			complete: (id: string) => {
				setDirty(false);
				setCompanyId(id);
			},
		}),
		[setDirty],
	);

	const query = useGetCompany(companyId ?? "");

	if (!companyId) {
		return null;
	}

	return (
		<>
			<Dialog open={true} title="Client data" maxWidth="wide" onClose={requestClose}>
				{query.isLoading || query.isError || !query.data ? (
					<DetailsLoading
						id={companyId}
						isLoading={query.isLoading}
						isError={query.isError || !query.data}
					/>
				) : (
					<CompleteCompanyProfileWizard
						company={query.data}
						onDirtyChange={setDirty}
						onCancel={requestClose}
						onSaved={() => {
							discard();
							onSuccess();
						}}
					/>
				)}
			</Dialog>

			<ConfirmDialog
				open={confirming}
				title="Discard these changes?"
				description="Nothing has been saved yet. Closing now throws away everything filled in so far."
				confirmLabel="Discard"
				onConfirm={discard}
				onClose={keepEditing}
			/>
		</>
	);
});

CompleteCompanyProfileWizardDialog.displayName = "CompleteCompanyProfileWizardDialog";

export { CompleteCompanyProfileWizardDialog };
