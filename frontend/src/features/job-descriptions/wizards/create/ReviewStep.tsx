import { FormWizard } from "#/components/form-wizard/FormWizard";
import { getErrorMessage } from "#/components/ui";
import { withForm } from "#/forms";
import type { JobDescriptionField, JobDescriptionFormValues } from "./schema";
import { findStepForField } from "./steps";

export const ReviewStep = withForm({
	defaultValues: {} as JobDescriptionFormValues,

	props: {
		description: "Review the job description before creating it.",
	} as { description?: string },

	render: function Render({ form, description }) {
		const values = form.state.values;
		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader title="Review" description={description} />

				<form.Subscribe selector={(state) => state.fieldMeta}>
					{(fieldMeta) => <ReviewErrors fieldMeta={fieldMeta} />}
				</form.Subscribe>

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

/** `responsibilities[0]` -> `responsibilities`, so the error can still be traced back to its step. */
function baseFieldName(field: string) {
	return field.split(/[[.]/)[0] as JobDescriptionField;
}

type ReviewErrorsProps = {
	fieldMeta: Partial<Record<JobDescriptionField, { errors: Array<unknown> }>>;
};

function ReviewErrors({ fieldMeta }: ReviewErrorsProps) {
	const problems = (
		Object.entries(fieldMeta) as Array<[JobDescriptionField, { errors: unknown[] }]>
	).flatMap(([field, meta]) =>
		(meta?.errors ?? []).map((error) => ({
			field,
			step: findStepForField(baseFieldName(field))?.title,
			message: getErrorMessage(error),
		})),
	);

	if (problems.length === 0) {
		return null;
	}

	return (
		<div className="wizard-review-error">
			<div className="form-error" role="alert">
				<strong>Fix the following before creating the job description:</strong>

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
