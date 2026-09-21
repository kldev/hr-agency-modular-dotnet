import { UserRound } from "lucide-react";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { withForm } from "#/forms";
import { emptyLegalEntity } from "../schema";

export const RepresentationStep = withForm({
	defaultValues: emptyLegalEntity,

	props: { isSubmitting: false } as { isSubmitting: boolean },

	render: function Render({ form, isSubmitting }) {
		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					icon={UserRound}
					title="Representation"
					description="Who signs for this company. Their name ends up on the contracts it is party to."
				/>

				<form.AppField name="presidentFirstName">
					{(field) => (
						<field.FormInput
							label="President - first name"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="presidentLastName">
					{(field) => (
						<field.FormInput
							label="President - last name"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="presidentEmail">
					{(field) => (
						<field.FormInput
							label="President - email"
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
							rows={3}
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>
			</FormWizard.Section>
		);
	},
});
