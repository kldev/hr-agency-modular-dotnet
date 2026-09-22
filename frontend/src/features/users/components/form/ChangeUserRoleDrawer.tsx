import { forwardRef, useImperativeHandle, useState } from "react";
import { z } from "zod";
import { OrganizationRoleApi, type UserProjection } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import { useChangeUserRole } from "../../pages/hooks";
import { organizationRoles } from "../../types";
import type { ChangeUserRoleFormCommand } from "./UserFormCommand";

interface ChangeUserRoleDrawerProps {
	onSuccess: () => void;
}

const changeRoleSchema = z.object({
	role: z.enum(OrganizationRoleApi),
});

/*
 * The read model carries OrganizationRole, which also knows the internal System value; the request
 * takes OrganizationRoleApi, which does not. Anything outside the API vocabulary falls back to the
 * first selectable role rather than being sent back as-is.
 */
function toApiRole(role: string): OrganizationRoleApi {
	return role in OrganizationRoleApi
		? (role as OrganizationRoleApi)
		: OrganizationRoleApi.Recruiter;
}

const FormContent: React.FC<{
	user: UserProjection;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ user, onSuccess, handleClose }) => {
	const { mutation, waiting } = useChangeUserRole({
		onSuccess: () => {
			mutation.reset();
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: { role: toApiRole(user.role) },

		validators: {
			onChange: changeRoleSchema,
		},

		onSubmit: async ({ value }) => {
			mutation.mutate({ userId: user.id, request: { role: value.role } });
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title={`Role of ${user.fullName ?? user.email}`}
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<form.AppField name="role">
							{(field) => (
								<field.FormSelectEnum
									label="Organization role"
									options={organizationRoles}
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(val) => field.handleChange(val)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<div className="form-hint">
							This is what the person may do in the organization. Their role on a team is separate.
						</div>

						<ApiError
							error={mutation.error as unknown as Parameters<typeof ApiError>[0]["error"]}
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

const ChangeUserRoleDrawer = forwardRef<ChangeUserRoleFormCommand, ChangeUserRoleDrawerProps>(
	({ onSuccess }, ref) => {
		const [user, setUser] = useState<UserProjection | null>(null);

		useImperativeHandle(
			ref,
			() => ({
				changeRole: (next: UserProjection) => {
					setUser(next);
				},
			}),
			[],
		);

		if (!user) return null;

		return <FormContent user={user} onSuccess={onSuccess} handleClose={() => setUser(null)} />;
	},
);

ChangeUserRoleDrawer.displayName = "ChangeUserRoleDrawer";

export default ChangeUserRoleDrawer;
