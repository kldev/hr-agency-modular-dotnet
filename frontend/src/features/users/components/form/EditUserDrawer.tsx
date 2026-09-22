import { forwardRef, useImperativeHandle, useState } from "react";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import type { UpdateUserRequest, UserProjection } from "@/api/models";
import { useUpdateUser } from "../../pages/hooks";
import { EditUserForm, editUserSchema } from "./EditUserForm";
import type { EditUserFormCommand } from "./UserFormCommand";

interface EditUserDrawerProps {
	onSuccess: () => void;
}

const FormContent: React.FC<{
	user: UserProjection;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ user, onSuccess, handleClose }) => {
	const { mutation, waiting } = useUpdateUser({
		onSuccess: () => {
			mutation.reset();
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: {
			email: user.email,
			firstName: user.firstName,
			lastName: user.lastName,
			jobTitle: user.jobTitle ?? "",
			phone: user.phone ?? "",
		},

		validators: {
			onChange: editUserSchema,
		},

		onSubmit: async ({ value }) => {
			const request: UpdateUserRequest = {
				email: value.email.trim(),
				firstName: value.firstName.trim(),
				lastName: value.lastName.trim(),
				jobTitle: value.jobTitle.trim() || null,
				phone: value.phone.trim() || null,
			};

			mutation.mutate({ userId: user.id, request });
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title="Edit user"
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<EditUserForm form={form} error={mutation.error} isSubmitting={mutation.isPending} />
					</div>
				</FormDrawer.Content>

				<FormDrawer.Footer>
					<form.FormSaveChangesButton wait={waiting} isPending={mutation.isPending} />
				</FormDrawer.Footer>
			</FormDrawer>
		</form.AppForm>
	);
};

const EditUserDrawer = forwardRef<EditUserFormCommand, EditUserDrawerProps>(
	({ onSuccess }, ref) => {
		const [user, setUser] = useState<UserProjection | null>(null);

		useImperativeHandle(
			ref,
			() => ({
				edit: (next: UserProjection) => {
					setUser(next);
				},
			}),
			[],
		);

		if (!user) return null;

		return <FormContent user={user} onSuccess={onSuccess} handleClose={() => setUser(null)} />;
	},
);

EditUserDrawer.displayName = "EditUserDrawer";

export default EditUserDrawer;
