import { ClipboardList } from "lucide-react";
import type { EngagementType } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { OpportunitySelect } from "#/features/sales/components/OpportunitySelect";
import { withForm } from "#/forms";
import { engagementTypeDescriptions, engagementTypes } from "../../../types";
import { emptyProject } from "../schema";

export const BasicsStep = withForm({
	defaultValues: emptyProject,

	props: {
		isSubmitting: false,
		editing: false,
	} as { isSubmitting: boolean; editing: boolean },

	render: function Render({ form, isSubmitting, editing }) {
		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					icon={ClipboardList}
					title="Basics"
					description="What this engagement is, and on what legal footing it runs."
				/>

				<form.AppField name="name">
					{(field) => (
						<field.FormInput
							label="Name"
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
							rows={4}
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				{/* The deal it was sold as - optional, and only one of the client's own. */}
				<form.Subscribe selector={(state) => state.values.companyId}>
					{(companyId) => (
						<form.AppField name="salesOpportunityId">
							{(field) => (
								<OpportunitySelect
									label="Sold as (opportunity)"
									companyId={companyId || null}
									fieldName={field.name}
									value={field.state.value}
									onChange={field.handleChange}
									errors={field.state.meta.errors}
									isSubmitting={isSubmitting}
								/>
							)}
						</form.AppField>
					)}
				</form.Subscribe>

				{/*
				 * Offered only while the project is being created. `UpdateProjectRequest` does not carry
				 * it, and a control that silently does nothing is worse than no control: the engagement
				 * type is what the whole compliance catalogue is keyed on, so changing it would change
				 * which duties the delivery owes and which its people already answered.
				 */}
				{editing ? (
					<div className="form-hint">
						Engagement:{" "}
						<strong>
							{engagementTypes[form.state.values.engagementType as EngagementType] ?? "—"}
						</strong>
						. Frozen when the project was created, because the compliance catalogue is keyed on it.
					</div>
				) : (
					<>
						<form.AppField name="engagementType">
							{(field) => (
								<field.FormChoiceGroup
									label="Engagement type"
									options={engagementTypes}
									descriptions={engagementTypeDescriptions}
									columns={2}
									fieldName={field.name}
									fieldValue={(field.state.value as EngagementType) || null}
									errors={field.state.meta.errors}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={isSubmitting}
								/>
							)}
						</form.AppField>

						<div className="form-hint">
							The engagement type decides as much as the country does: posting an IT specialist
							abroad triggers almost nothing, hiring the same person out triggers a licence, a
							notification and document duties. It cannot be changed after the project is created.
						</div>
					</>
				)}
			</FormWizard.Section>
		);
	},
});
