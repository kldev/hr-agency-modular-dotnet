import { Mail } from "lucide-react";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { withForm } from "#/forms";
import { emptyWorker } from "../schema";

export const ContactStep = withForm({
	defaultValues: emptyWorker,

	props: {
		isSubmitting: false,
	} as { isSubmitting: boolean },

	render: function Render({ form, isSubmitting }) {
		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					icon={Mail}
					title="Contact"
					description="All optional. The e-mail address is reserved per organization, so two people cannot share one."
				/>

				<form.AppField name="email">
					{(field) => (
						<field.FormInput
							label="Email"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="phoneNumber">
					{(field) => (
						<field.FormInput
							label="Phone"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="street">
					{(field) => (
						<field.FormInput
							label="Street"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="buildingNumber">
					{(field) => (
						<field.FormInput
							label="Building number"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="unitNumber">
					{(field) => (
						<field.FormInput
							label="Unit number"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="postalCode">
					{(field) => (
						<field.FormInput
							label="Postal code"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="city">
					{(field) => (
						<field.FormInput
							label="City"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="addressCountryCode">
					{(field) => (
						<field.FormCountrySelect
							label="Country"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="note">
					{(field) => (
						<field.FormTextAreaInput
							label="Note"
							rows={3}
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<div className="form-hint">
					The address is all or nothing: filling in any part of it makes street, building number,
					postal code, city and country required. Many people have no work address at all, and
					leaving the whole block empty is fine.
				</div>
			</FormWizard.Section>
		);
	},
});
