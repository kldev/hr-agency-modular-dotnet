import { forwardRef, useImperativeHandle, useState } from "react";
import type { BankAccountData, LegalEntityProjection } from "#/api/models";
import { ConfirmDialog, Dialog, useUnsavedChangesGuard } from "#/components/ui";
import { LegalEntityWizard } from "./LegalEntityWizard";
import { emptyLegalEntity, type LegalEntityFormValues, toLegalEntityValues } from "./schema";

export interface LegalEntityFormCommand {
	create: () => void;
	edit: (entity: LegalEntityProjection) => void;
}

interface LegalEntityWizardDialogProps {
	onSuccess: () => void;
}

type Target = {
	entityId?: string;
	values: LegalEntityFormValues;
	accounts: BankAccountData[];
};

const LegalEntityWizardDialog = forwardRef<LegalEntityFormCommand, LegalEntityWizardDialogProps>(
	({ onSuccess }, ref) => {
		const [target, setTarget] = useState<Target | null>(null);

		const { setDirty, confirming, requestClose, discard, keepEditing } = useUnsavedChangesGuard(
			() => setTarget(null),
		);

		useImperativeHandle(
			ref,
			() => ({
				create: () => {
					setDirty(false);
					setTarget({ values: emptyLegalEntity, accounts: [] });
				},
				edit: (entity: LegalEntityProjection) => {
					setDirty(false);
					setTarget({
						entityId: entity.id,
						values: toLegalEntityValues(entity),
						accounts: entity.bankAccounts ?? [],
					});
				},
			}),
			[setDirty],
		);

		if (!target) {
			return null;
		}

		const editing = Boolean(target.entityId);

		return (
			<>
				<Dialog
					open={true}
					title={editing ? "Edit legal entity" : "New legal entity"}
					maxWidth="wide"
					onClose={requestClose}
				>
					<LegalEntityWizard
						key={target.entityId ?? "create"}
						entityId={target.entityId}
						initialValues={target.values}
						initialAccounts={target.accounts}
						onDirtyChange={setDirty}
						onCancel={requestClose}
						onSaved={() => {
							discard();
							onSuccess();
						}}
					/>
				</Dialog>

				<ConfirmDialog
					open={confirming}
					title={editing ? "Discard these changes?" : "Discard this legal entity?"}
					description="Nothing has been saved yet. Closing now throws away everything filled in so far."
					confirmLabel="Discard"
					onConfirm={discard}
					onClose={keepEditing}
				/>
			</>
		);
	},
);

LegalEntityWizardDialog.displayName = "LegalEntityWizardDialog";

export { LegalEntityWizardDialog };
