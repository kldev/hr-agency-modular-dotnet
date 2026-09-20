import { z } from "zod";
import { OrganizationRoleApi } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import {
	ContactFields,
	contactFieldNames,
	contactFieldsSchema,
	emptyContactFields,
} from "#/features/users/components/form/ContactFields";
import { organizationRoles } from "#/features/users/types";
import { withForm } from "#/forms";

interface OrganizationUserFormProps {
	error: Error | null;
	isSubmitting: boolean;
}

/**
 * The owner-side counterpart of CreateUserForm, deliberately without the team fields: the owner
 * creates a user inside somebody else's organization, and team suggestions are scoped to the
 * caller's own organization, so a picker here would offer the wrong teams.
 */
export const organizationUserSchema = contactFieldsSchema.extend({
	role: z.enum(OrganizationRoleApi),

	password: z
		.string()
		.min(1, "Password is required")
		.min(4, "Password must contain at least 4 characters"),
});

export type OrganizationUserFormValues = z.infer<typeof organizationUserSchema>;

export const emptyOrganizationUser: OrganizationUserFormValues = {
	...emptyContactFields,
	role: OrganizationRoleApi.Recruiter,
	password: "",
};

export const OrganizationUserForm = withForm({
	props: {} as OrganizationUserFormProps,
	defaultValues: emptyOrganizationUser,
	render: function Render({ form, isSubmitting, error }) {
		return (
			<>
				<ContactFields form={form} fields={contactFieldNames} isSubmitting={isSubmitting} />

				<form.AppField name="role">
					{(field) => (
						<field.FormSelectEnum
							label="Organization role"
							options={organizationRoles}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(val) => field.handleChange(val)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="password">
					{(field) => (
						<field.FormPasswordInput
							label="Password"
							hint="The user can reset the password later."
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(val) => field.handleChange(val)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<ApiError error={error as unknown as Parameters<typeof ApiError>[0]["error"]} />
			</>
		);
	},
});
