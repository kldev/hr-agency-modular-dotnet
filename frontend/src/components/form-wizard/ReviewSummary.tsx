import { getErrorMessage } from "#/components/ui";

/** The shape every wizard's step list already has; only the title and the fields are read here. */
type ReviewStepDefinition = {
	title: string;
	fields: readonly string[];
};

type ReviewErrorsProps<TField extends string> = {
	fieldMeta: Partial<Record<TField, { errors: Array<unknown> }>>;
	steps: readonly ReviewStepDefinition[];
	/** What the last step is about to do: "saving the project", "planning the assignment". */
	action: string;
};

/**
 * Everything wrong with the form, gathered on the last step and named by the step it belongs to.
 *
 * A wizard hides its earlier steps, so an error left behind on one of them is invisible exactly
 * when it matters - at the point somebody is about to submit and the button will not respond.
 */
export function ReviewErrors<TField extends string>({
	fieldMeta,
	steps,
	action,
}: ReviewErrorsProps<TField>) {
	/* `responsibilities[0]` -> `responsibilities`, so a nested issue still names the step it is on. */
	const findStep = (field: TField) => {
		const base = field.split(/[[.]/)[0];

		return steps.find((step) => step.fields.includes(base));
	};

	const problems = (Object.entries(fieldMeta) as Array<[TField, { errors: unknown[] }]>).flatMap(
		([field, meta]) =>
			(meta?.errors ?? []).map((error) => ({
				field,
				step: findStep(field)?.title,
				message: getErrorMessage(error),
			})),
	);

	if (problems.length === 0) {
		return null;
	}

	return (
		<div className="wizard-review-error">
			<div className="form-error" role="alert">
				<strong>Fix the following before {action}:</strong>

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

/** One line of a review summary; an empty value reads as a dash rather than as a gap. */
export function SummaryItem({ label, value }: { label: string; value: string }) {
	return (
		<div className="form-wizard__summary-item">
			<span className="form-wizard__summary-label">{label}</span>

			<span className="form-wizard__summary-value">{value || "—"}</span>
		</div>
	);
}
