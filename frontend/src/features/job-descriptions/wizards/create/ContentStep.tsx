import { FileText } from "lucide-react";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { withForm } from "#/forms";
import type { JobDescriptionFormValues } from "./schema";

export const ContentStep = withForm({
	defaultValues: {} as JobDescriptionFormValues,

	render: function Render({ form }) {
		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					icon={FileText}
					title="Job content"
					description="Describe the role, team, project and the responsibilities of the successful candidate."
				/>

				<form.AppField name="description">
					{(field) => (
						<field.FormTextAreaInput
							label="Description"
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(val) => field.handleChange(val)}
							rows={8}
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
