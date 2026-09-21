import type { EngagementType } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { ReviewErrors, SummaryItem } from "#/components/form-wizard/ReviewSummary";
import { withForm } from "#/forms";
import { getCountryLabel } from "@/components/labels";
import {
	complianceRequirements,
	engagementTypes,
	previewComplianceRequirements,
} from "@/features/compliance";
import { useGetProject } from "@/features/projects/pages/hooks";
import { usePositionSuggestion, useWorkerSuggestion } from "@/hooks";
import { formatPeriod } from "@/utlis/formatRecord";
import { emptyPlanAssignment } from "../schema";
import { planAssignmentSteps } from "../steps";

export const ReviewStep = withForm({
	defaultValues: emptyPlanAssignment,

	render: function Render({ form }) {
		const values = form.state.values;

		const worker = useWorkerSuggestion(values.workerId);

		/* The form holds the role's id; its name comes from the same cache the picker filled. */
		const position = usePositionSuggestion(values.positionId);
		const project = useGetProject(values.projectId);

		/*
		 * The per person half of the catalogue, counted before anybody commits to it. What this
		 * posting brings depends on the work country *and* on how the person is engaged: posting an
		 * IT specialist to Germany brings almost nothing, employing them under German law brings a
		 * local contract and a social security registration instead.
		 */
		const requirements = previewComplianceRequirements(
			project.data?.workCountry ?? "",
			values.engagementType as EngagementType,
			"assignment",
		);

		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					title="Review"
					description="Check the posting before planning it. The person, the project and the engagement type are frozen once it exists — a change of any of them is a new posting."
				/>

				<form.Subscribe selector={(state) => state.fieldMeta}>
					{(fieldMeta) => (
						<ReviewErrors
							fieldMeta={fieldMeta}
							steps={planAssignmentSteps}
							action="planning the assignment"
						/>
					)}
				</form.Subscribe>

				<div className="form-wizard__summary">
					<div className="form-wizard__summary-section">
						<h3 className="form-wizard__summary-title">Posting</h3>

						<div className="form-wizard__summary-grid">
							<SummaryItem label="Worker" value={worker.data?.fullName ?? ""} />
							<SummaryItem label="Project" value={project.data?.name ?? ""} />
							<SummaryItem label="Client" value={project.data?.companyName ?? ""} />

							{/* Which of our companies posts them: the one that has to issue the A1. */}
							<SummaryItem
								label="Posted by"
								value={project.data?.deliveringEntity.legalName ?? ""}
							/>

							<SummaryItem
								label="Work country"
								value={project.data ? (getCountryLabel(project.data.workCountry) ?? "") : ""}
							/>

							<SummaryItem
								label="Engagement"
								value={
									values.engagementType
										? engagementTypes[values.engagementType as EngagementType]
										: ""
								}
							/>

							<SummaryItem label="Position" value={position.data?.name ?? ""} />

							<SummaryItem
								label="Period"
								value={values.startsOn ? formatPeriod(values.startsOn, values.endsOn || null) : ""}
							/>
						</div>
					</div>

					<div className="form-wizard__summary-section">
						<h3 className="form-wizard__summary-title">What this person will owe</h3>

						{requirements.length === 0 ? (
							<p className="form-wizard__section-description">
								{project.data && values.engagementType
									? "Nothing is required of this person for this posting. Working in the country where the agency is established raises no per person duties — that is an answer, not missing data."
									: "Pick a project and an engagement type to see what this posting will require."}
							</p>
						) : (
							<>
								<p className="form-wizard__section-description">
									{requirements.length} {requirements.length === 1 ? "item" : "items"} will appear
									on this assignment's compliance list, each of them issued for this one person and
									this one period.
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
