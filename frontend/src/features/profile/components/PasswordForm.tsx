import { useNavigate } from "@tanstack/react-router";
import { toast } from "sonner";
import { z } from "zod";
import { ApiError } from "#/components/ui/ApiError";
import { useAppForm } from "#/forms";
import { logout } from "#/server/auth";
import { useAuthStore } from "#/stores/authStore";
import { useChangeOwnPassword } from "../pages/hooks";

const passwordSchema = z
	.object({
		currentPassword: z.string().min(1, "Enter your current password"),

		// The backend is the authority on what a password must look like; this only spares a round
		// trip for the obvious case.
		newPassword: z.string().min(4, "The new password must contain at least 4 characters"),

		confirmation: z.string().min(1, "Repeat the new password"),
	})
	.refine((value) => value.newPassword === value.confirmation, {
		message: "The two passwords do not match",
		path: ["confirmation"],
	});

export function PasswordForm() {
	const navigate = useNavigate();
	const { clearUser } = useAuthStore();

	const mutation = useChangeOwnPassword({
		/*
		 * Changing the password revokes every refresh token this account has, including the one
		 * behind this very session. Signing out here is not extra caution - it is what already
		 * happened, said out loud, instead of letting the session die confusingly a few minutes
		 * later. The same three calls the "Sign out" menu item makes.
		 */
		onSuccess: async () => {
			toast.success("Password changed. Please sign in again.");

			clearUser();
			await logout();

			navigate({ to: "/login" });
		},
	});

	const form = useAppForm({
		defaultValues: {
			currentPassword: "",
			newPassword: "",
			confirmation: "",
		},

		validators: {
			onChange: passwordSchema,
		},

		onSubmit: async ({ value }) => {
			mutation.mutate({
				currentPassword: value.currentPassword,
				newPassword: value.newPassword,
			});
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
				<form.AppField name="currentPassword">
					{(field) => (
						<field.FormInput
							label="Current password"
							type="password"
							autoComplete="current-password"
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(val) => field.handleChange(val)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="newPassword">
					{(field) => (
						<field.FormInput
							label="New password"
							type="password"
							autoComplete="new-password"
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(val) => field.handleChange(val)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="confirmation">
					{(field) => (
						<field.FormInput
							label="Repeat the new password"
							type="password"
							autoComplete="new-password"
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
					<form.FormSaveChangesButton wait={false} isPending={isSubmitting} />
				</div>
			</form>
		</form.AppForm>
	);
}
