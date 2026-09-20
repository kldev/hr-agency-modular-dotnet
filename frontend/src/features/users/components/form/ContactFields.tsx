import { z } from "zod";
import { withFieldGroup } from "#/forms";

/**
 * The contact half of every form that describes a person: creating a user, editing one, and the
 * owner-side create in the platform panel. Kept in one place so the email rule and its wording
 * cannot drift apart between the three.
 */
export const contactFieldsSchema = z.object({
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

export type ContactFieldsValues = z.infer<typeof contactFieldsSchema>;

export const emptyContactFields: ContactFieldsValues = {
	email: "",
	firstName: "",
	lastName: "",
	jobTitle: "",
	phone: "",
};

/** Every form using this group names its fields the same way, so the map is the identity. */
export const contactFieldNames = {
	email: "email",
	firstName: "firstName",
	lastName: "lastName",
	jobTitle: "jobTitle",
	phone: "phone",
} as const;

export const ContactFields = withFieldGroup({
	defaultValues: emptyContactFields,
	props: {} as { isSubmitting?: boolean },
	render: function Render({ group, isSubmitting }) {
		return (
			<>
				<group.AppField name="email">
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
				</group.AppField>

				<group.AppField name="firstName">
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
				</group.AppField>

				<group.AppField name="lastName">
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
				</group.AppField>

				<group.AppField name="jobTitle">
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
				</group.AppField>

				<group.AppField name="phone">
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
				</group.AppField>
			</>
		);
	},
});
