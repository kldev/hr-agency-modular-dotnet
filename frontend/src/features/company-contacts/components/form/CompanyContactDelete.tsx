import { useMutation } from "@tanstack/react-query";
import { forwardRef, useImperativeHandle, useState } from "react";
import { deleteCompanyContact } from "@/api/endpoints";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import { useProjectionWait } from "@/hooks";

export interface CompanyContactDeleteCommand {
	deleteContact: (contactId: string) => void;
}

interface CompanyContactDeleteProps {
	onSuccess: () => void;
}

export const CompanyContactDelete = forwardRef<
	CompanyContactDeleteCommand,
	CompanyContactDeleteProps
>(function CompanyContactDelete({ onSuccess }, ref) {
	const [contactIdToDelete, setContactIdToDelete] = useState<string | null>(null);

	const { wait, waiting } = useProjectionWait();

	const removeMutation = useMutation({
		mutationFn: (contactId: string) => deleteCompanyContact(contactId),

		onSuccess: async () => {
			await wait();
			setContactIdToDelete(null);
			onSuccess();
		},
	});

	useImperativeHandle(
		ref,
		() => ({
			deleteContact: (contactId: string) => {
				setContactIdToDelete(contactId);
			},
		}),
		[],
	);

	const handleConfirm = () => {
		if (!contactIdToDelete) {
			return;
		}

		removeMutation.mutate(contactIdToDelete);
	};

	const handleClose = () => {
		if (removeMutation.isPending || waiting) {
			return;
		}

		setContactIdToDelete(null);
	};

	return (
		<ConfirmDialog
			open={contactIdToDelete !== null}
			title="Delete contact"
			description="Are you sure you want to delete this contact? This action cannot be undone."
			confirmLabel="Delete contact"
			loading={removeMutation.isPending || waiting}
			onConfirm={handleConfirm}
			onClose={handleClose}
		/>
	);
});
