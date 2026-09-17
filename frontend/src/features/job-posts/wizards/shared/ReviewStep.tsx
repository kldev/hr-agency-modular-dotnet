import { FormWizard } from "#/components/form-wizard/FormWizard";
import { getLanguageLabel } from "#/components/types/languages";
import { getErrorMessage } from "#/components/ui";
import { withForm } from "#/forms";
import type { JobPostSource } from "./PostStep";
import type { JobPostField, JobPostFormValues } from "./schema";
import { findStepForField } from "./steps";

const sourceLabels: Record<JobPostSource["kind"], string> = {
	jobDescription: "Job description",
	jobPost: "Source job post",
};

export const ReviewStep = withForm({
	defaultValues: {} as JobPostFormValues,

	props: {
		description: "Review the job post before creating it.",
		source: undefined,
	} as { description?: string; source?: JobPostSource },

	render: function Render({ form, description, source }) {
		const values = form.state.values;

		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader title="Review" description={description} />

				<form.Subscribe selector={(state) => state.fieldMeta}>
					{(fieldMeta) => <ReviewErrors fieldMeta={fieldMeta} />}
				</form.Subscribe>

				<div className="form-wizard__summary">
					{source ? (
						<div className="form-wizard__summary-section">
							<h3 className="form-wizard__summary-title">Source</h3>

							<div className="form-wizard__summary-grid">
								<SummaryItem label={sourceLabels[source.kind]} value={source.label} />
							</div>

							<p className="form-wizard__section-description">
								This post is a copy. Editing the source later will not change anything here.
							</p>
						</div>
					) : null}

					<div className="form-wizard__summary-section">
						<h3 className="form-wizard__summary-title">Post</h3>

						<div className="form-wizard__summary-grid">
							<SummaryItem label="Job title" value={values.title} />

							<SummaryItem
								label="Language"
								value={values.languageCode ? getLanguageLabel(values.languageCode) : ""}
							/>

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
	return field.split(/[[.]/)[0] as JobPostField;
}

type ReviewErrorsProps = {
	fieldMeta: Partial<Record<JobPostField, { errors: Array<unknown> }>>;
};

function ReviewErrors({ fieldMeta }: ReviewErrorsProps) {
	const problems = (
		Object.entries(fieldMeta) as Array<[JobPostField, { errors: unknown[] }]>
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
				<strong>Fix the following before saving the job post:</strong>

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
