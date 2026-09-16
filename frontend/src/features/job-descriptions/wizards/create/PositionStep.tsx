import { FormWizard } from "#/components/form-wizard/FormWizard";
import { withForm } from "#/forms";
import type { JobDescriptionFormValues } from "./schema";

export const PositionStep = withForm({
	defaultValues: {} as JobDescriptionFormValues,

	render: function Render({ form }) {
		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					title="Job basics"
					description="Start with the position title and a concise summary candidates will see first."
				/>
				<FormWizard.Field label="Company" required>
					<form.AppField name="companyId">
						{(field) => (
							<field.FormCompanyPicker
								fieldValue={{ id: field.state.value }}
								label=""
								fieldName={field.name}
								errors={field.state.meta.errors}
								handleChange={(val) => field.handleChange(val.id ?? "")}
							/>
						)}
					</form.AppField>
				</FormWizard.Field>

				<FormWizard.Field label="Recruiter" required>
					<form.AppField name="recruiterId">
						{(field) => (
							<field.FormUserPicker
								label=""
								fieldName={field.name}
								fieldValue={{ id: field.state.value }}
								errors={field.state.meta.errors}
								handleChange={(value) => field.handleChange(value?.id ?? "")}
							/>
						)}
					</form.AppField>
				</FormWizard.Field>

				<FormWizard.Field label="Job title" required hint="Use a clear, market-facing title.">
					<form.AppField name="title">
						{(field) => (
							<field.FormInput
								label=""
								fieldValue={field.state.value}
								errors={field.state.meta.errors}
								fieldName={field.name}
								handleChange={(val) => field.handleChange(val)}
							/>
						)}
					</form.AppField>
				</FormWizard.Field>

				<FormWizard.Field
					label="Short summary"
					hint="Optional. Keep it concise and focused on the value of the role."
				>
					<form.AppField name="summary">
						{(field) => (
							<field.FormTextAreaInput
								label=""
								rows={3}
								fieldValue={field.state.value}
								errors={field.state.meta.errors}
								fieldName={field.name}
								handleChange={(val) => field.handleChange(val)}
							/>
						)}
					</form.AppField>
				</FormWizard.Field>
			</FormWizard.Section>
		);
	},
});
