import { AlertTriangle, Building2 } from "lucide-react";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { useGetCompany } from "#/features/companies/pages/hooks";
import { withForm } from "#/forms";
import { emptyProject } from "../schema";

function ProfileWarning({ companyId }: { companyId: string }) {
	const query = useGetCompany(companyId);

	if (!query.data || query.data.isProfileComplete) {
		return null;
	}

	return (
		<div className="project-inline-warning">
			<AlertTriangle size={15} />

			<span>
				This client has no complete registered profile yet. The project can still be created - it
				starts as a draft - but it cannot go live, and a contract cannot state who signed it and
				under which address, until the profile is filled in. You can do that from the project once
				it exists.
			</span>
		</div>
	);
}

export const ClientStep = withForm({
	defaultValues: emptyProject,

	props: {
		isSubmitting: false,
	} as { isSubmitting: boolean },

	render: function Render({ form, isSubmitting }) {
		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					icon={Building2}
					title="Client"
					description="A project always belongs to a company. Everything the contract says about the other party is read from that company's profile."
				/>

				<form.AppField name="companyId">
					{(field) => (
						<>
							<field.FormCompanyPicker
								label="Client"
								fieldName={field.name}
								fieldValue={{ id: field.state.value || null }}
								errors={field.state.meta.errors}
								handleChange={(value) => field.handleChange(value.id ?? "")}
								isSubmitting={isSubmitting}
							/>

							{field.state.value ? <ProfileWarning companyId={field.state.value} /> : null}
						</>
					)}
				</form.AppField>
			</FormWizard.Section>
		);
	},
});
