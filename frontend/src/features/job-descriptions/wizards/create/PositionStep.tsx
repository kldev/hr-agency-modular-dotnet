import { FormWizard } from "#/components/form-wizard/FormWizard";
import { withForm } from "#/forms";
import type { JobDescriptionFormValues } from "./schema";

/**
 * Company and recruiter as they are stored on an existing record.
 *
 * Set in edit mode only: neither field is part of `PUT /api/job-description/{id}` - the company is
 * immutable once the record exists and the recruiter is changed through its own action - so an
 * editable picker would promise something the save cannot deliver.
 */
export type PositionAssignment = {
	companyName: string;
	recruiterName: string;
};

export const PositionStep = withForm({
	defaultValues: {} as JobDescriptionFormValues,

	props: {
		assignment: undefined,
	} as { assignment?: PositionAssignment },

	render: function Render({ form, assignment }) {
		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					title="Job basics"
					description="Start with the position title and a concise summary candidates will see first."
				/>
				<FormWizard.Field
					label="Company"
					required
					hint={
						assignment ? "The company cannot be changed after the record is created." : undefined
					}
				>
					{assignment ? (
						<p className="form-wizard__readonly-value">{assignment.companyName}</p>
					) : (
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
					)}
				</FormWizard.Field>

				<FormWizard.Field
					label="Recruiter"
					required
					hint={assignment ? 'Use the "Change recruiter" action to reassign the role.' : undefined}
				>
					{assignment ? (
						<p className="form-wizard__readonly-value">{assignment.recruiterName}</p>
					) : (
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
					)}
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
