import { Landmark } from "lucide-react";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { withForm } from "#/forms";
import { emptyLegalEntity } from "../schema";

export const IdentityStep = withForm({
	defaultValues: emptyLegalEntity,

	props: { isSubmitting: false } as { isSubmitting: boolean },

	render: function Render({ form, isSubmitting }) {
		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					icon={Landmark}
					title="Identity"
					description="One of our own companies: the one that posts people and signs contracts, as opposed to a client."
				/>

				<form.AppField name="name">
					{(field) => (
						<field.FormInput
							label="Trading name"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="legalName">
					{(field) => (
						<field.FormInput
							label="Registered name"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="taxId">
					{(field) => (
						<field.FormInput
							label="Tax ID"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="vatNumber">
					{(field) => (
						<field.FormInput
							label="VAT number"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="activeFrom">
					{(field) => (
						<field.FormDatePicker
							label="Trading since"
							yearSelect
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="activeTo">
					{(field) => (
						<field.FormDatePicker
							label="Trading until"
							yearSelect
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<div className="form-hint">
					Leave the last day of trading empty for a company that is still active. A project can only
					be delivered by an entity that was trading on the day it started.
				</div>
			</FormWizard.Section>
		);
	},
});
