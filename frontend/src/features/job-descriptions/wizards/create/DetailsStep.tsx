import { Globe2 } from "lucide-react";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { withForm } from "#/forms";
import type { JobDescriptionFormValues } from "./schema";

export const DetailsStep = withForm({
	defaultValues: {} as JobDescriptionFormValues,
	props: {
		isSubmitting: false,
	} as { isSubmitting: boolean },
	render: function Render({ form, isSubmitting }) {
		return (
			<FormWizard.Section>
				<div className="mb-3 flex h-9 w-9 items-center justify-center rounded-md bg-(--color-primary-soft) text-(--color-primary)">
					<Globe2 size={18} />
				</div>
				<FormWizard.SectionHeader
					icon={Globe2}
					title="Job content"
					description="Describe the role, team, project and the responsibilities of the successful candidate."
				/>

				<form.AppField name="summary">
					{(field) => (
						<field.FormTextAreaInput
							label="Summary"
							rows={3}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(val) => field.handleChange(val)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>
				<form.AppField name="responsibilities">
					{(field) => (
						<field.FormArrayField
							fieldValue={field.state.value}
							values={field.state.value}
							fieldName={field.name}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={false}
							label="Responsibilities"
							description="Describe the position, team, project and expectations..."
							placeholder="e.g. Design and implement backend services"
							onChange={(value) => field.handleChange(value)}
						/>
					)}
				</form.AppField>
			</FormWizard.Section>
		);
	},
});
