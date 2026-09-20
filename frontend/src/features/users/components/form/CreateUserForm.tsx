import { z } from "zod";
import { OrganizationRoleApi, TeamRole } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { defaultTeamRole, teamRoles } from "#/features/teams/types";
import { withForm } from "#/forms";
import { organizationRoles } from "../../types";
import {
	ContactFields,
	contactFieldNames,
	contactFieldsSchema,
	emptyContactFields,
} from "./ContactFields";

interface CreateUserFormProps {
	error: Error | null;
	isSubmitting: boolean;
}

export const createUserSchema = contactFieldsSchema.extend({
	role: z.enum(OrganizationRoleApi),

	// The backend's password policy is four characters; anything shorter is refused there.
	password: z
		.string()
		.min(1, "Password is required")
		.min(4, "Password must contain at least 4 characters"),

	teamId: z.string(),

	teamRole: z.enum(TeamRole),
});

export type CreateUserFormValues = z.infer<typeof createUserSchema>;

export const emptyCreateUser: CreateUserFormValues = {
	...emptyContactFields,
	role: OrganizationRoleApi.Recruiter,
	password: "",
	teamId: "",
	teamRole: defaultTeamRole,
};

export const CreateUserForm = withForm({
	props: {} as CreateUserFormProps,
	defaultValues: emptyCreateUser,
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

				<form.AppField name="teamId">
					{(field) => (
						<field.FormTeamPicker
							label="Team (optional)"
							placeholder="Search teams ..."
							hint="A person belongs to at most one team. Leave empty to add them later."
							fieldValue={{ id: field.state.value }}
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(val) => field.handleChange(val.id ?? "")}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.Subscribe selector={(state) => state.values.teamId}>
					{(teamId) =>
						teamId ? (
							<form.AppField name="teamRole">
								{(field) => (
									<field.FormSelectEnum
										label="Team role"
										options={teamRoles}
										fieldValue={field.state.value}
										errors={field.state.meta.errors}
										fieldName={field.name}
										handleChange={(val) => field.handleChange(val)}
										isSubmitting={isSubmitting}
									/>
								)}
							</form.AppField>
						) : null
					}
				</form.Subscribe>

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
