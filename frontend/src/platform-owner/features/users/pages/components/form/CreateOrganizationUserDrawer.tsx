import { useMutation } from "@tanstack/react-query";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { createOrganizationUser } from "@/api/endpoints";
import type { CreateUserForOrganizationRequest, CreateUserRequest } from "@/api/models";
import { DetailOverviewHeader, SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import { emptyCreateUser, UserForm } from "@/features/users/components";
import { useProjectionWait } from "@/hooks";
import type { CreateOrganizationUserFormCommand } from "./CreateOrganizationUserFormCommand";

interface CreateOrganizationUserDrawerProps {
	onSuccess: () => void;
}

const CreateOrganizationUserDrawer = forwardRef<
	CreateOrganizationUserFormCommand,
	CreateOrganizationUserDrawerProps
>(({ onSuccess }, ref) => {
	const [isOpen, setIsOpen] = useState(false);
	const [organizationId, setOrganizationId] = useState<string | null>(null);
	const [organizationName, setOrganizationName] = useState<string | null>(null);
	const [user, setUser] = useState<CreateUserRequest>(emptyCreateUser);

	const { wait, waiting } = useProjectionWait();

	const createMutation = useMutation({
		mutationFn: (request: CreateUserForOrganizationRequest) => createOrganizationUser(request),

		onSuccess: async () => {
			await wait();

			setIsOpen(false);
			setOrganizationId(null);
			setUser(emptyCreateUser);

			onSuccess();
		},
	});

	useImperativeHandle(
		ref,
		() => ({
			create: (id: string, organizationName: string) => {
				createMutation.reset();

				setOrganizationName(organizationName);
				setOrganizationId(id);
				setUser(emptyCreateUser);
				setIsOpen(true);
			},
		}),
		[createMutation],
	);

	const handleSave = useCallback(
		(value: CreateUserRequest) => {
			if (!organizationId) {
				return;
			}

			createMutation.mutate({
				email: value.email,
				firstName: value.firstName,
				lastName: value.lastName,
				role: value.role,
				password: value.password,
				organizationId,
			});
		},
		[organizationId, createMutation],
	);

	const handleClose = useCallback(() => {
		if (createMutation.isPending) {
			return;
		}

		createMutation.reset();
		setIsOpen(false);
		setOrganizationId(null);
		setOrganizationName(null);
	}, [createMutation]);

	return (
		<Drawer
			open={isOpen}
			title="Create organization user"
			onClose={handleClose}
			footer={
				<SaveChangesButton form="user-form" isPending={createMutation.isPending} wait={waiting} />
			}
		>
			<div className="pb-5">
				<DetailOverviewHeader title={organizationName ?? ""} description="" />
			</div>
			<UserForm
				initialValue={user}
				onSubmit={handleSave}
				error={createMutation.error}
				isSubmitting={createMutation.isPending}
			/>
		</Drawer>
	);
});

CreateOrganizationUserDrawer.displayName = "CreateOrganizationUserDrawer";

export default CreateOrganizationUserDrawer;
