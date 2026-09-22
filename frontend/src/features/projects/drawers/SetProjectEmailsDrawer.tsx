import { forwardRef, useImperativeHandle, useState } from "react";
import { z } from "zod";
import type { EmailPurpose, ProjectProjection } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import { useSetProjectEmailRecipients } from "../pages/hooks";
import { emailPurposeDescriptions, emailPurposes } from "../types";
import type { SetProjectEmailsFormCommand } from "./ProjectFormCommand";

interface SetProjectEmailsDrawerProps {
	onSuccess: () => void;
}

type EmailsTarget = {
	project: ProjectProjection;
	purpose: EmailPurpose;
};

const emailsSchema = z.object({
	emails: z.array(z.string().trim().email("Enter a valid e-mail address")),
});

const FormContent: React.FC<{
	target: EmailsTarget;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ target, onSuccess, handleClose }) => {
	const current = target.project.emailRecipients
		.filter((recipient) => recipient.purpose === target.purpose)
		.map((recipient) => recipient.email);

	const { mutation, waiting } = useSetProjectEmailRecipients({
		onSuccess: () => {
			mutation.reset();
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: {
			emails: current,
		},

		validators: {
			onChange: emailsSchema,
		},

		onSubmit: async ({ value }) => {
			mutation.mutate({
				projectId: target.project.id,
				purpose: target.purpose,
				request: {
					emails: value.emails.map((email) => email.trim()).filter(Boolean),
				},
			});
		},
	});

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title={`${emailPurposes[target.purpose]} e-mails`}
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<div className="form-hint">{emailPurposeDescriptions[target.purpose]}</div>

						<form.AppField name="emails">
							{(field) => (
								<field.FormArrayField
									label="Addresses"
									values={field.state.value}
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									placeholder="name@example.com"
									onChange={(value) => field.handleChange(value)}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<div className="form-hint">
							Saving replaces the whole set for this purpose. An empty list means nobody is
							notified.
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

const SetProjectEmailsDrawer = forwardRef<SetProjectEmailsFormCommand, SetProjectEmailsDrawerProps>(
	({ onSuccess }, ref) => {
		const [target, setTarget] = useState<EmailsTarget | null>(null);

		useImperativeHandle(
			ref,
			() => ({
				edit: (project: ProjectProjection, purpose: EmailPurpose) => {
					setTarget({ project, purpose });
				},
			}),
			[],
		);

		if (!target) return null;

		return (
			<FormContent target={target} onSuccess={onSuccess} handleClose={() => setTarget(null)} />
		);
	},
);

SetProjectEmailsDrawer.displayName = "SetProjectEmailsDrawer";

export default SetProjectEmailsDrawer;
