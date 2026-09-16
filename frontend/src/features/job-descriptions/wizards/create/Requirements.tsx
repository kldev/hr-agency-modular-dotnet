import { ListChecks } from "lucide-react";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { withForm } from "#/forms";
import type { JobDescriptionFormValues } from "./schema";

export const RequirementsStep = withForm({
	defaultValues: {} as JobDescriptionFormValues,

	render: function Render({ form }) {
		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					icon={ListChecks}
					title="Requirements & skills"
					description="Separate must-have requirements from the technologies and professional skills."
				/>
				<form.AppField name="requirements">
					{(field) => (
						<field.FormArrayField
							fieldValue={field.state.value}
							values={field.state.value}
							fieldName={field.name}
							label="Requirements"
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={false}
							description="Add the requirements candidates should meet."
							placeholder="e.g. 3+ years of experience with .NET"
							onChange={(value) => field.handleChange(value)}
						/>
					)}
				</form.AppField>

				<form.AppField name="skills">
					{(field) => (
						<field.FormArrayField
							fieldValue={field.state.value}
							values={field.state.value}
							fieldName={field.name}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={false}
							label="Skills"
							description="Add the technical or professional skills required for the position."
							placeholder="e.g. C#, TypeScript, SQL"
							onChange={(value) => field.handleChange(value)}
						/>
					)}
				</form.AppField>
			</FormWizard.Section>
		);
	},
});
