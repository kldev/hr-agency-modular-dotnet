import { useMutation } from "@tanstack/react-query";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { createUser } from "@/api/endpoints";
import type { CreateUserRequest } from "@/api/models";
import { SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import { useProjectionWait } from "@/hooks";

import { emptyCreateUser, UserForm } from "./UserForm";
import type { CreateUserFormCommand } from "./UserFormCommand";

interface CreateUserDrawerProps {
	onSuccess: () => void;
}

const CreateUserDrawer = forwardRef<CreateUserFormCommand, CreateUserDrawerProps>(
	({ onSuccess }, ref) => {
		const [isOpen, setIsOpen] = useState(false);
		const [user, setUser] = useState<CreateUserRequest>(emptyCreateUser);

		const { wait, waiting } = useProjectionWait();

		const createMutation = useMutation({
			mutationFn: (request: CreateUserRequest) => createUser(request),

			onSuccess: async () => {
				await wait();

				setIsOpen(false);
				setUser(emptyCreateUser);

				onSuccess();
			},
		});

		useImperativeHandle(
			ref,
			() => ({
				create: () => {
					createMutation.reset();
					setUser(emptyCreateUser);
					setIsOpen(true);
				},
			}),
			[createMutation],
		);

		const handleSave = useCallback(
			(value: CreateUserRequest) => {
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
				title="Create user"
				onClose={handleClose}
				footer={
					<SaveChangesButton form="user-form" isPending={createMutation.isPending} wait={waiting} />
				}
			>
				<UserForm
					initialValue={user}
					onSubmit={handleSave}
					error={createMutation.error}
					isSubmitting={createMutation.isPending}
				/>
			</Drawer>
		);
	},
);

CreateUserDrawer.displayName = "CreateUserDrawer";

export default CreateUserDrawer;
