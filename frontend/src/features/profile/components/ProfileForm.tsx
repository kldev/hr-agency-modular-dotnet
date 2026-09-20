import { toast } from "sonner";
import { z } from "zod";
import type { MyProfileResponse, UpdateOwnProfileRequest } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { useAppForm } from "#/forms";
import { useUpdateOwnProfile } from "../pages/hooks";

/**
 * The same rules as the administrative edit, minus the e-mail. The address is the login, so it is
 * shown but never submitted - the request record has no field for one and neither does the command
 * behind it.
 */
const profileSchema = z.object({
	firstName: z.string().trim().min(1, "First name is required"),

	lastName: z.string().trim().min(1, "Last name is required"),

	jobTitle: z.string(),

	phone: z.string(),
});

interface ProfileFormProps {
	profile: MyProfileResponse;
}

export function ProfileForm({ profile }: ProfileFormProps) {
	const { mutation, waiting } = useUpdateOwnProfile({
		onSuccess: () => {
			mutation.reset();
			toast.success("Profile updated");
		},
	});

	const form = useAppForm({
		defaultValues: {
			firstName: profile.user.firstName,
			lastName: profile.user.lastName,
			jobTitle: profile.user.jobTitle ?? "",
			phone: profile.user.phone ?? "",
		},

		validators: {
			onChange: profileSchema,
		},

		onSubmit: async ({ value }) => {
			const request: UpdateOwnProfileRequest = {
				firstName: value.firstName.trim(),
				lastName: value.lastName.trim(),
				jobTitle: value.jobTitle.trim() || null,
				phone: value.phone.trim() || null,
			};

			mutation.mutate(request);
		},
	});

	const isSubmitting = mutation.isPending;

	return (
		<form.AppForm>
			<form
				className="drawer-form"
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<div className="form-field">
					<span className="form-label">Email</span>
					<p className="profile-readonly-value">{profile.user.email}</p>
					<div className="form-hint">
						This is also how you sign in. An administrator can change it for you.
					</div>
				</div>

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

				<ApiError error={mutation.error as unknown as Parameters<typeof ApiError>[0]["error"]} />

				<div className="profile-form-actions">
					<form.FormSaveChangesButton wait={waiting} isPending={isSubmitting} />
				</div>
			</form>
		</form.AppForm>
	);
}
