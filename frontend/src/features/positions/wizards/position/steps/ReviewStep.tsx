import type {
	CurrencyCode,
	EngagementType,
	RateBasis,
	RateUnit,
	WorkerContractType,
} from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { ReviewErrors, SummaryItem } from "#/components/form-wizard/ReviewSummary";
import { withForm } from "#/forms";
import { getCountryLabel } from "@/components/labels";
import { engagementTypes } from "@/features/compliance";
import { rateBases, rateUnits, workerContractTypes } from "../../../types";
import { emptyPosition } from "../schema";
import { positionSteps } from "../steps";

export const ReviewStep = withForm({
	defaultValues: emptyPosition,

	props: {
		projectName: "",
	} as { projectName?: string },

	render: function Render({ form, projectName }) {
		const values = form.state.values;

		/* "32 PLN per hour gross" - the three parts that make the number mean something. */
		const rate = values.rateAmount
			? `${values.rateAmount} ${values.rateCurrency as CurrencyCode} ${
					rateUnits[values.rateUnit as RateUnit]
				} ${rateBases[values.rateBasis as RateBasis]}`
			: "";

		const workplace = values.city
			? `${values.street} ${values.buildingNumber}${
					values.unitNumber ? `/${values.unitNumber}` : ""
				}, ${values.postalCode} ${values.city}${
					values.countryCode ? `, ${getCountryLabel(values.countryCode)}` : ""
				}`
			: "The project's own workplace";

		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					title="Review"
					description="Check the role before saving. Everything here is a proposal: what somebody actually signs is settled on their own posting."
				/>

				<form.Subscribe selector={(state) => state.fieldMeta}>
					{(fieldMeta) => (
						<ReviewErrors fieldMeta={fieldMeta} steps={positionSteps} action="saving the role" />
					)}
				</form.Subscribe>

				<div className="form-wizard__summary">
					<div className="form-wizard__summary-section">
						<h3 className="form-wizard__summary-title">Role</h3>

						<div className="form-wizard__summary-grid">
							<SummaryItem label="Project" value={projectName ?? ""} />
							<SummaryItem label="Name" value={values.name} />
							<SummaryItem label="On the contract" value={values.contractName || values.name} />
							<SummaryItem label="Target headcount" value={values.plannedHeadcount} />
							<SummaryItem
								label="Usual engagement"
								value={
									values.defaultEngagementType
										? engagementTypes[values.defaultEngagementType as EngagementType]
										: ""
								}
							/>
						</div>
					</div>

					<div className="form-wizard__summary-section">
						<h3 className="form-wizard__summary-title">Contract</h3>

						<div className="form-wizard__summary-grid">
							<SummaryItem
								label="We sign"
								value={
									values.contractType
										? workerContractTypes[values.contractType as WorkerContractType]
										: ""
								}
							/>
							<SummaryItem label="Proposed rate" value={rate} />
							<SummaryItem
								label="Paid by"
								value={values.payoutDay ? `day ${values.payoutDay} of the month` : ""}
							/>
							<SummaryItem label="Probation" value={values.probationPeriod} />
							<SummaryItem label="Notice" value={values.noticePeriod} />
						</div>
					</div>

					<div className="form-wizard__summary-section">
						<h3 className="form-wizard__summary-title">Time and place</h3>

						<div className="form-wizard__summary-grid">
							<SummaryItem label="Weekly hours" value={values.weeklyHours} />
							<SummaryItem label="Starts at" value={values.workStartsAt} />
							<SummaryItem label="Workplace" value={workplace} />
						</div>
					</div>
				</div>

				{/* Said here rather than discovered later: a role is where a document gets its wording. */}
				<div className="form-hint">
					Anything left empty stays empty on the documents made from this role. It can be filled in
					later — a role is usually opened before all of its terms are settled.
				</div>
			</FormWizard.Section>
		);
	},
});
