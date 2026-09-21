import { IdCard } from "lucide-react";
import type { IdentityDocumentKind } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { withForm } from "#/forms";
import { identityDocumentKinds } from "../../../types";
import { emptyWorker } from "../schema";

export const DocumentStep = withForm({
	defaultValues: emptyWorker,

	props: {
		isSubmitting: false,
	} as { isSubmitting: boolean },

	render: function Render({ form, isSubmitting }) {
		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					icon={IdCard}
					title="Identity document"
					description="One person, one file: the document number is what stops the same person being opened twice."
				/>

				<form.AppField name="identityDocumentKind">
					{(field) => (
						<field.FormSelectEnum
							label="Kind"
							options={identityDocumentKinds}
							fieldName={field.name}
							fieldValue={field.state.value as IdentityDocumentKind}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="identityDocumentNumber">
					{(field) => (
						<field.FormInput
							label="Document number"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="identityDocumentIssuingCountry">
					{(field) => (
						<field.FormCountrySelect
							label="Issued by"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<form.AppField name="identityDocumentValidUntil">
					{(field) => (
						<field.FormDatePicker
							label="Valid until"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<div className="form-hint">
					Leave the expiry empty if the document has none. An expired document does not stop
					anything here, but it does stop the person being marked as employed.
				</div>
			</FormWizard.Section>
		);
	},
});
