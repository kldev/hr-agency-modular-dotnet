import type { EngagementType } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { getErrorMessage } from "#/components/ui";
import { withForm } from "#/forms";
import { previewComplianceRequirements } from "../../../compliancePreview";
import { complianceRequirements, engagementTypes } from "../../../types";
import { emptyProject, type ProjectField } from "../schema";
import { projectSteps } from "../steps";

function findStepForField(field: ProjectField) {
	return projectSteps.find((step) => (step.fields as readonly string[]).includes(field));
}

export const ReviewStep = withForm({
	defaultValues: emptyProject,

	props: {
		clientName: "",
	} as { clientName?: string },

	render: function Render({ form, clientName }) {
		const values = form.state.values;
		const requirements = previewComplianceRequirements(
			values.countryCode,
			values.engagementType as EngagementType,
		);

		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					title="Review"
					description="Check the project before creating it. It starts as a draft: nothing here is final except the client and the engagement type."
				/>

				<form.Subscribe selector={(state) => state.fieldMeta}>
					{(fieldMeta) => <ReviewErrors fieldMeta={fieldMeta} />}
				</form.Subscribe>

				<div className="form-wizard__summary">
					<div className="form-wizard__summary-section">
						<h3 className="form-wizard__summary-title">Project</h3>

						<div className="form-wizard__summary-grid">
							<SummaryItem label="Client" value={clientName ?? ""} />
							<SummaryItem label="Name" value={values.name} />
							<SummaryItem
								label="Engagement"
								value={
									values.engagementType
										? engagementTypes[values.engagementType as EngagementType]
										: ""
								}
							/>
						</div>
					</div>

					<div className="form-wizard__summary-section">
						<h3 className="form-wizard__summary-title">Assignment</h3>

						<div className="form-wizard__summary-grid">
							<SummaryItem
								label="Place of work"
								value={[
									`${values.street} ${values.buildingNumber}${
										values.unitNumber ? `/${values.unitNumber}` : ""
									}`,
									`${values.postalCode} ${values.city}`,
									values.countryCode.toUpperCase(),
								]
									.filter((part) => part.trim())
									.join(", ")}
							/>

							<SummaryItem
								label="Period"
								value={`${values.startsOn.slice(0, 10)} – ${
									values.endsOn ? values.endsOn.slice(0, 10) : "open-ended"
								}`}
							/>
						</div>
					</div>

					<div className="form-wizard__summary-section">
						<h3 className="form-wizard__summary-title">Compliance this brings</h3>

						{requirements.length === 0 ? (
							<p className="form-wizard__section-description">
								{values.countryCode
									? "No country-specific requirements for this country and engagement type. Work in the country where the agency is established does not trigger a host state's duties."
									: "Pick a country and an engagement type to see what the project will have to keep track of."}
							</p>
						) : (
							<>
								<p className="form-wizard__section-description">
									{requirements.length} {requirements.length === 1 ? "item" : "items"} will appear
									on the project's compliance list. Each of them is somebody's job, and this is
									cheaper to learn now than a week from now.
								</p>

								<ul className="form-wizard__summary-list">
									{requirements.map((requirement) => (
										<li key={requirement} className="form-wizard__summary-tag">
											{complianceRequirements[requirement]}
										</li>
									))}
								</ul>
							</>
						)}
					</div>
				</div>
			</FormWizard.Section>
		);
	},
});

type ReviewErrorsProps = {
	fieldMeta: Partial<Record<ProjectField, { errors: Array<unknown> }>>;
};

function ReviewErrors({ fieldMeta }: ReviewErrorsProps) {
	const problems = (
		Object.entries(fieldMeta) as Array<[ProjectField, { errors: unknown[] }]>
	).flatMap(([field, meta]) =>
		(meta?.errors ?? []).map((error) => ({
			field,
			step: findStepForField(field)?.title,
			message: getErrorMessage(error),
		})),
	);

	if (problems.length === 0) {
		return null;
	}

	return (
		<div className="wizard-review-error">
			<div className="form-error" role="alert">
				<strong>Fix the following before saving the project:</strong>

				<ul className="form-wizard__summary-list">
					{problems.map((problem) => (
						<li key={`${problem.field}-${problem.message}`}>
							{problem.step ? `${problem.step}: ` : ""}
							{problem.message}
						</li>
					))}
				</ul>
			</div>
		</div>
	);
}

function SummaryItem({ label, value }: { label: string; value: string }) {
	return (
		<div className="form-wizard__summary-item">
			<span className="form-wizard__summary-label">{label}</span>

			<span className="form-wizard__summary-value">{value || "—"}</span>
		</div>
	);
}
