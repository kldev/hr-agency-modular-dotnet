import type { FieldAnswer, FormPage } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
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
		<>
			{pages.map((page) => (
				<FormWizard.Section key={page.pageId}>
					<FormWizard.SectionHeader
						title={page.title}
						description={page.description ?? undefined}
					/>

					<div className="form-wizard__summary-list">
						{page.fields.map((field) => (
							<SummaryItem
								key={field.fieldId}
								label={field.label}
								value={formatAnswer(field, byCode.get(field.code))}
							/>
						))}
					</div>
				</FormWizard.Section>
			))}
		</>
	);
}
