import { useMutation } from "@tanstack/react-query";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { createOrganization } from "@/api/endpoints";
import type { OrganizationRequest } from "@/api/models";
import { SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import { useProjectionWait } from "@/hooks";
import type { CreateOrganizationCommand } from "./OrganizationCommand";
import { emptyCreateOrganization, OrganizationForm } from "./OrganizationForm";

interface CreateOrganizationDrawerProps {
	onSuccess: () => void;
}

const CreateOrganizationDrawer = forwardRef<
	CreateOrganizationCommand,
	CreateOrganizationDrawerProps
>(({ onSuccess }, ref) => {
	const [isOpen, setIsOpen] = useState(false);
	const [organization, setOrganization] = useState<OrganizationRequest>(emptyCreateOrganization);

	const { wait, waiting } = useProjectionWait();

	const createMutation = useMutation({
		mutationFn: (request: OrganizationRequest) => createOrganization(request),

		onSuccess: async () => {
			await wait();

			setIsOpen(false);
			setOrganization(emptyCreateOrganization);

			onSuccess();
		},
	});

	useImperativeHandle(
		ref,
		() => ({
			create: () => {
				createMutation.reset();
				setOrganization(emptyCreateOrganization);
				setIsOpen(true);
			},
		}),
		[createMutation],
	);

	const handleSave = useCallback(
		(value: OrganizationRequest) => {
			createMutation.mutate(value);
		},
		[createMutation],
	);

	const handleClose = useCallback(() => {
		if (createMutation.isPending) {
			return;
		}

		createMutation.reset();
		setIsOpen(false);
	}, [createMutation]);

	return (
		<Drawer
			open={isOpen}
			title="Create organization"
			onClose={handleClose}
			footer={
				<SaveChangesButton
					form="organization-form"
					isPending={createMutation.isPending}
					wait={waiting}
				/>
			}
		>
			<OrganizationForm
				initialValue={organization}
				onSubmit={handleSave}
				error={createMutation.error}
				isSubmitting={createMutation.isPending}
			/>
		</Drawer>
	);
});

CreateOrganizationDrawer.displayName = "CreateOrganizationDrawer";

export default CreateOrganizationDrawer;
