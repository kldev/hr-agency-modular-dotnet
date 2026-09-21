import { UserRound } from "lucide-react";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { withForm } from "#/forms";
import { requiresLegalisation } from "../../../types";
import { emptyWorker } from "../schema";

export const IdentityStep = withForm({
	defaultValues: emptyWorker,

	props: {
		isSubmitting: false,
	} as { isSubmitting: boolean },

	render: function Render({ form, isSubmitting }) {
		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					icon={UserRound}
					title="Identity"
					description="Everything here is true of the person, not of where they work. None of it changes when they move to another project."
				/>

				<form.AppField name="firstName">
					{(field) => (
						<field.FormInput
							label="First name"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="lastName">
					{(field) => (
						<field.FormInput
							label="Last name"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="dateOfBirth">
					{(field) => (
						<field.FormDatePicker
							label="Date of birth"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="citizenship">
					{(field) => (
						<field.FormCountrySelect
							label="Citizenship"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				{/* The one field on this step with a consequence, so it says what it is. */}
				<form.Subscribe selector={(state) => state.values.citizenship}>
					{(citizenship) =>
						citizenship ? (
							<div className="form-hint">
								{requiresLegalisation(citizenship)
									? "This citizenship carries no free movement rights, so this person goes through the legalisation stage and needs a permission to work on file."
									: "This citizenship carries free movement rights: no legalisation, and no permission to work to record."}
							</div>
						) : null
					}
				</form.Subscribe>
			</FormWizard.Section>
		);
	},
});
