import { FormWizard } from "#/components/form-wizard/FormWizard";
import { ReviewErrors, SummaryItem } from "#/components/form-wizard/ReviewSummary";
import { withForm } from "#/forms";
import { getCountryLabel } from "@/components/labels";
import { identityDocumentKinds, requiresLegalisation } from "../../../types";
import { emptyWorker } from "../schema";
import { workerSteps } from "../steps";

export const ReviewStep = withForm({
	defaultValues: emptyWorker,

	render: function Render({ form }) {
		const values = form.state.values;
		const needsLegalisation = requiresLegalisation(values.citizenship);

		const address = [
			`${values.street} ${values.buildingNumber}${values.unitNumber ? `/${values.unitNumber}` : ""}`,
			`${values.postalCode} ${values.city}`,
			values.addressCountryCode.toUpperCase(),
		]
			.filter((part) => part.trim())
			.join(", ");

		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					title="Review"
					description="Check the person before saving. Where they work is not here: that is an assignment, and it is planned separately."
				/>

				<form.Subscribe selector={(state) => state.fieldMeta}>
					{(fieldMeta) => (
						<ReviewErrors fieldMeta={fieldMeta} steps={workerSteps} action="saving" />
					)}
				</form.Subscribe>

				<div className="form-wizard__summary">
					<div className="form-wizard__summary-section">
						<h3 className="form-wizard__summary-title">Person</h3>

						<div className="form-wizard__summary-grid">
							<SummaryItem label="Name" value={`${values.firstName} ${values.lastName}`.trim()} />
							<SummaryItem label="Date of birth" value={values.dateOfBirth.slice(0, 10)} />
							<SummaryItem label="Citizenship" value={getCountryLabel(values.citizenship) ?? ""} />
						</div>
					</div>

					<div className="form-wizard__summary-section">
						<h3 className="form-wizard__summary-title">Identity document</h3>

						<div className="form-wizard__summary-grid">
							<SummaryItem
								label="Kind"
								value={
									values.identityDocumentKind
										? identityDocumentKinds[
												values.identityDocumentKind as keyof typeof identityDocumentKinds
											]
										: ""
								}
							/>
							<SummaryItem label="Number" value={values.identityDocumentNumber} />
							<SummaryItem
								label="Issued by"
								value={getCountryLabel(values.identityDocumentIssuingCountry) ?? ""}
							/>
							<SummaryItem
								label="Valid until"
								value={
									values.identityDocumentValidUntil
										? values.identityDocumentValidUntil.slice(0, 10)
										: "No expiry"
								}
							/>
						</div>
					</div>

					<div className="form-wizard__summary-section">
						<h3 className="form-wizard__summary-title">Contact</h3>

						<div className="form-wizard__summary-grid">
							<SummaryItem label="Email" value={values.email} />
							<SummaryItem label="Phone" value={values.phoneNumber} />
							<SummaryItem label="Address" value={address} />
						</div>
					</div>

					<div className="form-wizard__summary-section">
						<h3 className="form-wizard__summary-title">What happens next</h3>

						<p className="form-wizard__section-description">
							{values.citizenship
								? needsLegalisation
									? "This person starts in Recruitment and will go through Legalisation: their citizenship carries no free movement rights, so a residence title and a permission to work have to be on file before they can be employed."
									: "This person starts in Recruitment and skips Legalisation entirely: their citizenship carries free movement rights, so there is nothing for that stage to do."
								: "Pick a citizenship to see whether this person goes through legalisation."}
						</p>
					</div>
				</div>
			</FormWizard.Section>
		);
	},
});
