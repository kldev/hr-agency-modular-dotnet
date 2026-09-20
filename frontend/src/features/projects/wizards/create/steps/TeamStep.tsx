import { UsersRound } from "lucide-react";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { withForm } from "#/forms";
import { emptyProject } from "../schema";

export const TeamStep = withForm({
	defaultValues: emptyProject,

	props: {
		isSubmitting: false,
	} as { isSubmitting: boolean },

	render: function Render({ form, isSubmitting }) {
		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					icon={UsersRound}
					title="Team"
					description="Which of our teams runs the engagement. Optional - it can be assigned later, and changing it later is a separate action."
				/>

				<form.AppField name="teamId">
					{(field) => (
						<field.FormTeamPicker
							label="Team"
							fieldName={field.name}
							fieldValue={{ id: field.state.value || null }}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value.id ?? "")}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>
			</FormWizard.Section>
		);
	},
});
