import type { FieldAnswer, FormPage } from "#/api/models";
import { SummaryItem } from "#/components/form-wizard/ReviewSummary";
import { formatAnswer } from "./formatAnswer";

/**
 * Every answer, page by page, as it will be submitted - the review step of a wizard and the read-only
 * view of a submitted response alike. One component for both, so what somebody checked before
 * submitting is what the next person reads.
 */
export function DynamicReview({
	pages,
	answers,
}: {
	pages: readonly FormPage[];
	answers: readonly FieldAnswer[];
}) {
	const byCode = new Map(answers.map((answer) => [answer.fieldCode, answer.value]));

	return (
		<div className="form-wizard__summary">
			{pages.map((page) => (
				<div key={page.pageId} className="form-wizard__summary-section">
					<h3 className="form-wizard__summary-title">{page.title}</h3>
					{page.description ? <p className="form-hint mb-3">{page.description}</p> : null}

					<div className="form-wizard__summary-grid">
						{page.fields.map((field) => (
							<SummaryItem
								key={field.fieldId}
								label={field.label}
								value={formatAnswer(field, byCode.get(field.code))}
							/>
						))}
					</div>
				</div>
			))}
		</div>
	);
}
