import { ClipboardList } from "lucide-react";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { withForm } from "#/forms";
import { emptyPosition } from "../schema";

/**
 * The part of the role that becomes the scope of work in the contract. Points rather than prose,
 * for the same reason a job description keeps its responsibilities as a list: a document generator
 * has to be able to lay them out, and a paragraph cannot be.
 */
export const WorkStep = withForm({
	defaultValues: emptyPosition,

	props: {
		isSubmitting: false,
	} as { isSubmitting: boolean },

	render: function Render({ form, isSubmitting }) {
		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					icon={ClipboardList}
					title="Work"
					description="What this person actually does, and what they have to hold before they can start."
				/>

				<form.AppField name="workDescription">
					{(field) => (
						<field.FormTextAreaInput
							label="Description"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
							rows={5}
						/>
					)}
				</form.AppField>

				<form.AppField name="duties">
					{(field) => (
						<field.FormArrayField
							label="Duties"
							placeholder="e.g. Prepare the surface"
							fieldName={field.name}
							fieldValue={field.state.value}
							values={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							onChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="requiredQualifications">
					{(field) => (
						<field.FormArrayField
							label="Required qualifications"
							placeholder="e.g. Working at heights certificate"
							fieldName={field.name}
							fieldValue={field.state.value}
							values={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							onChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<div className="form-hint">
					Qualifications are what somebody must hold before they set foot on site - the medicals and
					the certificates a posting is later checked against.
				</div>
			</FormWizard.Section>
		);
	},
});
