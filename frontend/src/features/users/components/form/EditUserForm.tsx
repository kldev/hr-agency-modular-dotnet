import { z } from "zod";
import { ApiError } from "#/components/ui/ApiError";
import { withForm } from "#/forms";

interface EditUserFormProps {
	error: Error | null;
	isSubmitting: boolean;
}

/**
 * Only what UpdateUserRequest carries. The organization role and the team are changed through their
 * own endpoints, so they get their own actions rather than riding along with a contact edit.
 */
export const editUserSchema = z.object({
	email: z
		.string()
		.trim()
		.min(1, "Email is required")
		.regex(/^[^\s@]+@[^\s@]+\.[^\s@]+$/, "Enter a valid email address"),

	firstName: z.string().trim().min(1, "First name is required"),

	lastName: z.string().trim().min(1, "Last name is required"),

	jobTitle: z.string(),

	phone: z.string(),
});

export type EditUserFormValues = z.infer<typeof editUserSchema>;

export const EditUserForm = withForm({
	props: {} as EditUserFormProps,
	defaultValues: {
		email: "",
		firstName: "",
		lastName: "",
		jobTitle: "",
		phone: "",
	} as EditUserFormValues,
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

				<ApiError error={error as unknown as Parameters<typeof ApiError>[0]["error"]} />
			</>
		);
	},
});
