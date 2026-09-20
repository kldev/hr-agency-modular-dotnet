import { ClipboardList } from "lucide-react";
import type { EngagementType } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { withForm } from "#/forms";
import { engagementTypeDescriptions, engagementTypes } from "../../../types";
import { emptyProject } from "../schema";

export const BasicsStep = withForm({
	defaultValues: emptyProject,

	props: {
		isSubmitting: false,
	} as { isSubmitting: boolean },

	render: function Render({ form, isSubmitting }) {
		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					icon={ClipboardList}
					title="Basics"
					description="What this engagement is, and on what legal footing it runs."
				/>

				<form.AppField name="name">
					{(field) => (
						<field.FormInput
							label="Name"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="description">
					{(field) => (
						<field.FormTextAreaInput
							label="Description"
							rows={4}
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="engagementType">
					{(field) => (
						<field.FormChoiceGroup
							label="Engagement type"
							options={engagementTypes}
							descriptions={engagementTypeDescriptions}
							columns={2}
							fieldName={field.name}
							fieldValue={(field.state.value as EngagementType) || null}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<div className="form-hint">
					The engagement type decides as much as the country does: posting an IT specialist abroad
					triggers almost nothing, hiring the same person out triggers a licence, a notification and
					document duties. It cannot be changed after the project is created.
				</div>
			</FormWizard.Section>
		);
	},
});
