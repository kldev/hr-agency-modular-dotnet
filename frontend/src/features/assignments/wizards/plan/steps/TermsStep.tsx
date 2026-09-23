import { CalendarRange } from "lucide-react";
import { useState } from "react";
import type { EngagementType } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { PositionsPicker } from "#/components/ui/pickers";
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
		const [roleInput, setRoleInput] = useState("");

		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					icon={CalendarRange}
					title="Terms"
					description="How this person is engaged, which role they take and for how long."
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

				{/*
				 * The roles on offer are the ones opened on the project picked a step earlier, which
				 * is why this reads the form rather than taking a prop: the backend resolves the role
				 * inside that project and refuses anything else.
				 */}
				<form.Subscribe selector={(state) => [state.values.projectId, state.values.positionId]}>
					{([projectId, positionId]) => (
						<div className="form-field">
							<label className="form-label" htmlFor="positionId">
								Position
							</label>

							<PositionsPicker
								id="positionId"
								projectId={projectId}
								disabled={isSubmitting}
								value={positionId}
								inputValue={roleInput}
								onInputChange={setRoleInput}
								onChange={(id) => form.setFieldValue("positionId", id ?? "")}
							/>

							<div className="form-hint">
								Roles are opened on the project. If the one you need is not here, open it there
								first.
							</div>
						</div>
					)}
				</form.Subscribe>

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
