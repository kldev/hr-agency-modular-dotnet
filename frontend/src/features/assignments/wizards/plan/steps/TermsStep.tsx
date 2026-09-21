import { CalendarRange } from "lucide-react";
import type { EngagementType } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { withForm } from "#/forms";
import { engagementTypeDescriptions, engagementTypes } from "@/features/compliance";
import { emptyPlanAssignment } from "../schema";

/**
 * The engagement type is stated about this person rather than taken from the project, and the
 * backend takes it the same way: one delivery can post some people and employ others under local
 * law, and which of the two applies decides whether this person needs an A1 or a local contract.
 */
export const TermsStep = withForm({
	defaultValues: emptyPlanAssignment,

	props: {
		isSubmitting: false,
	} as { isSubmitting: boolean },

	render: function Render({ form, isSubmitting }) {
		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					icon={CalendarRange}
					title="Terms"
					description="How this person is engaged, in what position and for how long."
				/>

				<form.AppField name="engagementType">
					{(field) => (
						<field.FormChoiceGroup<EngagementType>
							label="Engagement"
							options={engagementTypes}
							descriptions={engagementTypeDescriptions}
							columns={2}
							fieldValue={field.state.value as EngagementType}
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="position">
					{(field) => (
						<field.FormInput
							label="Position"
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="startsOn">
					{(field) => (
						<field.FormDatePicker
							label="Starts on"
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="endsOn">
					{(field) => (
						<field.FormDatePicker
							label="Ends on"
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							fieldName={field.name}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<div className="form-hint">
					An open end is allowed. The period has to sit inside the project's own and cannot overlap
					another posting of this person; both are checked when it is saved.
				</div>
			</FormWizard.Section>
		);
	},
});
