import { KeyRound } from "lucide-react";
import { toast } from "sonner";
import { z } from "zod";
import { OrganizationRoleApi } from "#/api/models";
import { Button, FieldError, Input } from "#/components/ui";
import { ApiError } from "#/components/ui/ApiError";
import { organizationRoles } from "#/features/users/types";
import { withForm } from "#/forms";
import { copyToClipboard, generatePassword } from "#/utlis";

interface OrganizationUserFormProps {
	error: Error | null;
	isSubmitting: boolean;
}

/**
 * The owner-side counterpart of CreateUserForm, deliberately without the team fields: the owner
 * creates a user inside somebody else's organization, and team suggestions are scoped to the
 * caller's own organization, so a picker here would offer the wrong teams.
 */
export const organizationUserSchema = z.object({
	email: z
		.string()
		.trim()
		.min(1, "Email is required")
		.regex(/^[^\s@]+@[^\s@]+\.[^\s@]+$/, "Enter a valid email address"),

	firstName: z.string().trim().min(1, "First name is required"),

	lastName: z.string().trim().min(1, "Last name is required"),

	role: z.enum(OrganizationRoleApi),

	jobTitle: z.string(),

	phone: z.string(),

	password: z
		.string()
		.min(1, "Password is required")
		.min(4, "Password must contain at least 4 characters"),
});

export type OrganizationUserFormValues = z.infer<typeof organizationUserSchema>;

export const emptyOrganizationUser: OrganizationUserFormValues = {
	email: "",
	firstName: "",
	lastName: "",
	role: OrganizationRoleApi.Recruiter,
	jobTitle: "",
	phone: "",
	password: "",
};

export const OrganizationUserForm = withForm({
	props: {} as OrganizationUserFormProps,
	defaultValues: emptyOrganizationUser,
	render: function Render({ form, isSubmitting, error }) {
		return (
			<>
				<form.AppField name="email">
					{(field) => (
						<field.FormInput
							label="Email"
							type="email"
							autoComplete="email"
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(val) => field.handleChange(val)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="firstName">
					{(field) => (
						<field.FormInput
							label="First name"
							autoComplete="given-name"
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(val) => field.handleChange(val)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="lastName">
					{(field) => (
						<field.FormInput
							label="Last name"
							autoComplete="family-name"
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(val) => field.handleChange(val)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="jobTitle">
					{(field) => (
						<field.FormInput
							label="Job title"
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(val) => field.handleChange(val)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="phone">
					{(field) => (
						<field.FormInput
							label="Phone"
							autoComplete="tel"
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(val) => field.handleChange(val)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

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

				<form.Field name="password">
					{(field) => (
						<div className="form-field">
							<label className="form-label" htmlFor={field.name}>
								Password
							</label>

							<div className="form-input-action">
								<Input
									id={field.name}
									name={field.name}
									type="password"
									autoComplete="new-password"
									value={field.state.value}
									disabled={isSubmitting}
									onBlur={field.handleBlur}
									onChange={(event) => field.handleChange(event.target.value)}
								/>

								<Button
									variant="ghost"
									icon={<KeyRound size={16} />}
									onClick={async () => {
										const password = generatePassword();
										await copyToClipboard(`User password: ${password}`);
										field.handleChange(password);
										toast.info("Password copied to clipboard");
									}}
								></Button>
							</div>

							<div className="form-hint">The user can reset the password later.</div>

							<FieldError errors={field.state.meta.errors} />
						</div>
					)}
				</form.Field>

				<ApiError error={error as unknown as Parameters<typeof ApiError>[0]["error"]} />
			</>
		);
	},
});
