import type { BankAccountData } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { ReviewErrors, SummaryItem } from "#/components/form-wizard/ReviewSummary";
import { withForm } from "#/forms";
import { getCountryLabel } from "@/components/labels";
import { emptyLegalEntity } from "../schema";
import { legalEntitySteps } from "../steps";

export const ReviewStep = withForm({
	defaultValues: emptyLegalEntity,

	props: { accounts: [] } as { accounts: BankAccountData[] },

	render: function Render({ form, accounts }) {
		const values = form.state.values;

		const address = [
			`${values.street} ${values.buildingNumber}${values.unitNumber ? `/${values.unitNumber}` : ""}`,
			`${values.postalCode} ${values.city}`,
			getCountryLabel(values.countryCode) ?? values.countryCode,
		]
			.filter((part) => part.trim())
			.join(", ");

		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					title="Review"
					description="Check the company before saving. The tax ID is reserved once, so it cannot collide with another of our entities."
				/>

				<form.Subscribe selector={(state) => state.fieldMeta}>
					{(fieldMeta) => (
						<ReviewErrors fieldMeta={fieldMeta} steps={legalEntitySteps} action="saving" />
					)}
				</form.Subscribe>

				<div className="form-wizard__summary">
					<div className="form-wizard__summary-section">
						<h3 className="form-wizard__summary-title">Company</h3>

						<div className="form-wizard__summary-grid">
							<SummaryItem label="Trading name" value={values.name} />
							<SummaryItem label="Registered name" value={values.legalName} />
							<SummaryItem label="Tax ID" value={values.taxId} />
							<SummaryItem label="VAT number" value={values.vatNumber} />
							<SummaryItem
								label="Trading"
								value={`${values.activeFrom.slice(0, 10)} – ${
									values.activeTo ? values.activeTo.slice(0, 10) : "still active"
								}`}
							/>
						</div>
					</div>

					<div className="form-wizard__summary-section">
						<h3 className="form-wizard__summary-title">Registered address</h3>

						<div className="form-wizard__summary-grid">
							<SummaryItem label="Address" value={address} />
						</div>
					</div>

					<div className="form-wizard__summary-section">
						<h3 className="form-wizard__summary-title">Representation</h3>

						<div className="form-wizard__summary-grid">
							<SummaryItem
								label="President"
								value={`${values.presidentFirstName} ${values.presidentLastName}`.trim()}
							/>
							<SummaryItem label="Email" value={values.presidentEmail} />
						</div>
					</div>

					<div className="form-wizard__summary-section">
						<h3 className="form-wizard__summary-title">Banking</h3>

						{accounts.length === 0 ? (
							<p className="form-wizard__section-description">
								No accounts recorded. That is fine — they can be added whenever they are needed.
							</p>
						) : (
							<ul className="form-wizard__summary-list">
								{accounts.map((account) => (
									<li
										key={`${account.purpose}-${account.iban}`}
										className="form-wizard__summary-tag"
									>
										{account.purpose} · {account.currency} · {account.iban}
									</li>
								))}
							</ul>
						)}
					</div>
				</div>
			</FormWizard.Section>
		);
	},
});
