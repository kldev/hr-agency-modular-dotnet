import type { BankAccountData } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { getErrorMessage } from "#/components/ui";
import { withForm } from "#/forms";
import { getCountryLabel } from "@/components/labels";
import { emptyLegalEntity, type LegalEntityField } from "../schema";
import { legalEntitySteps } from "../steps";

function findStepForField(field: LegalEntityField) {
	return legalEntitySteps.find((step) => (step.fields as readonly string[]).includes(field));
}

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
					{(fieldMeta) => <ReviewErrors fieldMeta={fieldMeta} />}
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

type ReviewErrorsProps = {
	fieldMeta: Partial<Record<LegalEntityField, { errors: Array<unknown> }>>;
};

function ReviewErrors({ fieldMeta }: ReviewErrorsProps) {
	const problems = (
		Object.entries(fieldMeta) as Array<[LegalEntityField, { errors: unknown[] }]>
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
				<strong>Fix the following before saving:</strong>

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
