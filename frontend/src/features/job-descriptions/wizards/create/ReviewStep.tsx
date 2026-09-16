import { FormWizard } from "#/components/form-wizard/FormWizard";
import { withForm } from "#/forms";
import type { JobDescriptionFormValues } from "./schema";

export const ReviewStep = withForm({
	defaultValues: {} as JobDescriptionFormValues,

	render: function Render({ form }) {
		const values = form.state.values;
		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					title="Review"
					description="Review the job description before creating it."
				/>

				<div className="form-wizard__summary">
					<div className="form-wizard__summary-section">
						<h3 className="form-wizard__summary-title">Position</h3>

						<div className="form-wizard__summary-grid">
							<SummaryItem label="Job title" value={values.title} />

							<SummaryItem label="Location" value={`${values.location}, ${values.countryCode}`} />
						</div>
					</div>

					<div className="form-wizard__summary-section">
						<h3 className="form-wizard__summary-title">Employment</h3>

						<div className="form-wizard__summary-grid">
							<SummaryItem label="Employment type" value={values.employmentType} />

							<SummaryItem label="Work mode" value={values.workMode} />

							<SummaryItem
								label="Salary"
								value={`${values.salaryMin} – ${values.salaryMax} ${values.currencyCode}`}
							/>
						</div>
					</div>

					<div className="form-wizard__summary-section">
						<h3 className="form-wizard__summary-title">Responsibilities</h3>

						<SummaryList items={values.responsibilities} />
					</div>

					<div className="form-wizard__summary-section">
						<h3 className="form-wizard__summary-title">Requirements</h3>

						<SummaryList items={values.requirements} />
					</div>

					<div className="form-wizard__summary-section">
						<h3 className="form-wizard__summary-title">Skills</h3>

						<SummaryList items={values.skills} />
					</div>
				</div>
			</FormWizard.Section>
		);
	},
});

function SummaryItem({ label, value }: { label: string; value: string }) {
	return (
		<div className="form-wizard__summary-item">
			<span className="form-wizard__summary-label">{label}</span>

			<span className="form-wizard__summary-value">{value || "—"}</span>
		</div>
	);
}

function SummaryList({ items }: { items: readonly string[] }) {
	return (
		<ul className="form-wizard__summary-list">
			{items.filter(Boolean).map((item, index) => (
				<li key={`${item}-${index}`} className="form-wizard__summary-tag">
					{item}
				</li>
			))}
		</ul>
	);
}
