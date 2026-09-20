import { useMutation } from "@tanstack/react-query";
import { forwardRef, useImperativeHandle, useState } from "react";
import { toast } from "sonner";
import { createOrganizationUser } from "@/api/endpoints";
import type { CreateUserForOrganizationRequest } from "@/api/models";
import { DetailOverviewHeader } from "@/components/ui";
import { FormDrawer } from "@/components/ui/FormDrawer";
import { useAppForm } from "@/forms";
import { useProjectionWait } from "@/hooks";
import type { CreateOrganizationUserFormCommand } from "./CreateOrganizationUserFormCommand";
import {
	emptyOrganizationUser,
	OrganizationUserForm,
	organizationUserSchema,
} from "./OrganizationUserForm";

interface CreateOrganizationUserDrawerProps {
	onSuccess: () => void;
}

type Target = {
	organizationId: string;
	organizationName: string;
};

const FormContent: React.FC<{
	target: Target;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ target, onSuccess, handleClose }) => {
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn: (request: CreateUserForOrganizationRequest) => createOrganizationUser(request),

		onSuccess: async () => {
			await wait();

			mutation.reset();
			toast.success("User created");
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: emptyOrganizationUser,

		validators: {
			onChange: organizationUserSchema,
		},

		onSubmit: async ({ value }) => {
			mutation.mutate({
				email: value.email.trim(),
				firstName: value.firstName.trim(),
				lastName: value.lastName.trim(),
				role: value.role,
				password: value.password,
				jobTitle: value.jobTitle.trim() || null,
				phone: value.phone.trim() || null,
				organizationId: target.organizationId,
			});
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title="Create organization user"
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<div className="pb-5">
							<DetailOverviewHeader title={target.organizationName} description="" />
						</div>

						<OrganizationUserForm
							form={form}
							error={mutation.error}
							isSubmitting={mutation.isPending}
						/>
					</div>
				</FormDrawer.Content>

				<FormDrawer.Footer>
					<form.FormSaveChangesButton wait={waiting} isPending={mutation.isPending} />
				</FormDrawer.Footer>
			</FormDrawer>
		</form.AppForm>
	);
};

const CreateOrganizationUserDrawer = forwardRef<
	CreateOrganizationUserFormCommand,
	CreateOrganizationUserDrawerProps
>(({ onSuccess }, ref) => {
	const [target, setTarget] = useState<Target | null>(null);

	useImperativeHandle(
		ref,
		() => ({
			create: (id: string, organizationName: string) => {
				setTarget({ organizationId: id, organizationName });
			},
		}),
		[],
	);

	if (!target) return null;

	return <FormContent target={target} onSuccess={onSuccess} handleClose={() => setTarget(null)} />;
});

CreateOrganizationUserDrawer.displayName = "CreateOrganizationUserDrawer";

export default CreateOrganizationUserDrawer;
