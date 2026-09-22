import { forwardRef, useImperativeHandle, useState } from "react";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import type { CreateUserRequest } from "@/api/models";
import { useCreateUser } from "../../pages/hooks";
import { CreateUserForm, createUserSchema, emptyCreateUser } from "./CreateUserForm";
import type { CreateUserFormCommand } from "./UserFormCommand";

interface CreateUserDrawerProps {
	onSuccess: () => void;
}

const FormContent: React.FC<{ onSuccess: () => void; handleClose: () => void }> = ({
	onSuccess,
	handleClose,
}) => {
	const { mutation, waiting } = useCreateUser({
		onSuccess: () => {
			mutation.reset();
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: emptyCreateUser,

		validators: {
			onChange: createUserSchema,
		},

		onSubmit: async ({ value }) => {
			const request: CreateUserRequest = {
				email: value.email.trim(),
				firstName: value.firstName.trim(),
				lastName: value.lastName.trim(),
				role: value.role,
				password: value.password,
				jobTitle: value.jobTitle.trim() || null,
				phone: value.phone.trim() || null,
				teamId: value.teamId || null,
				teamRole: value.teamId ? value.teamRole : null,
			};

			mutation.mutate({ request });
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title="Create user"
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<CreateUserForm form={form} error={mutation.error} isSubmitting={mutation.isPending} />
					</div>
				</FormDrawer.Content>

				<FormDrawer.Footer>
					<form.FormSaveChangesButton wait={waiting} isPending={mutation.isPending} />
				</FormDrawer.Footer>
			</FormDrawer>
		</form.AppForm>
	);
};

const CreateUserDrawer = forwardRef<CreateUserFormCommand, CreateUserDrawerProps>(
	({ onSuccess }, ref) => {
		const [isOpen, setIsOpen] = useState(false);

		useImperativeHandle(
			ref,
			() => ({
				create: () => {
					setIsOpen(true);
				},
			}),
			[],
		);

		if (!isOpen) return null;

		return <FormContent onSuccess={onSuccess} handleClose={() => setIsOpen(false)} />;
	},
);

CreateUserDrawer.displayName = "CreateUserDrawer";

export default CreateUserDrawer;
