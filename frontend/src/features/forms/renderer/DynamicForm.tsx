import { useEffect, useMemo, useRef, useState } from "react";
import type { FieldAnswer, FormPage } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { DirtyReporter } from "#/components/form-wizard/stepValidation";
import { useAppForm } from "#/forms";
import { defaultValues, toAnswers } from "../schema/answerValues";
import { type AnswerProblem, answerProblems, buildAnswerSchema } from "../schema/buildAnswerSchema";
import { keyOf } from "../schema/fieldKeys";
import { DynamicField } from "./DynamicField";
import { DynamicReview } from "./DynamicReview";

const REVIEW = "review";

type DynamicFormProps = {
	pages: readonly FormPage[];
	answers?: readonly FieldAnswer[];
	submitLabel: string;
	isSubmitting?: boolean;
	/** What the server named by field code on its last refusal; shown until that field changes. */
	serverFieldErrors?: Record<string, string[]> | null;
	/** Under the fields - the place for an `ApiError`. */
	footer?: React.ReactNode;
	/** On the review step (or under a one page form): a correction's reason, for one. */
	reviewAddon?: React.ReactNode;
	/** Called when somebody leaves a page with it valid - the draft is saved there. */
	onPageLeft?: (answers: FieldAnswer[]) => void;
	onSubmit: (answers: FieldAnswer[]) => void;
	onCancel?: () => void;
	onDirtyChange?: (dirty: boolean) => void;
};

/**
 * Any form, drawn from its layout. One page is a plain form; several are a wizard ending on a review
 * step - the same component either way, so a form never needs a special case for being short.
 * The builder's preview, filling a response in and correcting one are all this component: there is
 * no second renderer to drift away from what people actually see.
 *
 * Pages are held to the full rules only once somebody tries to leave them; before that a page gets
 * the format checks alone, so "required" never greets a page that was just opened.
 */
export function DynamicForm({
	pages,
	answers = [],
	submitLabel,
	isSubmitting = false,
	serverFieldErrors,
	footer,
	reviewAddon,
	onPageLeft,
	onSubmit,
	onCancel,
	onDirtyChange,
}: DynamicFormProps) {
	const wizard = pages.length > 1;
	const [step, setStep] = useState(0);
	const [problems, setProblems] = useState<AnswerProblem[]>([]);

	/* Read by the schema at validation time, so marking a page strict needs no new form. */
	const strictPages = useRef(new Set<string>());

	const schema = useMemo(
		() =>
			buildAnswerSchema(pages, (page) =>
				strictPages.current.has(page.pageId) ? "submit" : "draft",
			),
		[pages],
	);

	const form = useAppForm({
		defaultValues: defaultValues(pages, answers),
		validators: { onChange: schema },
	});

	/* The server names fields by code; the form knows them by key. */
	const [serverErrors, setServerErrors] = useState<Record<string, string[]>>({});

	useEffect(() => {
		const byKey: Record<string, string[]> = {};

		for (const field of pages.flatMap((page) => page.fields)) {
			const messages = serverFieldErrors?.[field.code];

			if (messages?.length) {
				byKey[keyOf(field)] = messages;
			}
		}

		setServerErrors(byKey);
	}, [serverFieldErrors, pages]);

	const steps = [
		...pages.map((page) => ({
			id: page.pageId,
			title: page.title,
			description: page.description ?? "",
			fields: page.fields.map(keyOf),
		})),
		{ id: REVIEW, title: "Review", description: "Check everything before submitting", fields: [] },
	];

	const current = wizard ? steps[step] : steps[0];
	const page = pages.find((candidate) => candidate.pageId === current.id);

	const hold = async (held: readonly FormPage[]) => {
		for (const candidate of held) {
			strictPages.current.add(candidate.pageId);
		}

		// Puts the errors under the controls on screen; the verdict comes from the values themselves.
		await form.validate("change");

		return answerProblems(held, form.state.values, "submit");
	};

	/* A refusal the server gave for a field stands until that field is changed. */
	const refusedOn = (held: FormPage) => held.fields.some((field) => serverErrors[keyOf(field)]);

	const handleNext = async () => {
		if (!page) {
			return;
		}

		const found = await hold([page]);

		if (found.length > 0 || refusedOn(page)) {
			return;
		}

		onPageLeft?.(toAnswers(pages, form.state.values));
		setStep((index) => Math.min(index + 1, steps.length - 1));
	};

	const handleSubmit = async () => {
		const found = await hold(pages);

		setProblems(found);

		if (found.length > 0) {
			/* On a one page form the errors are under the fields already; a wizard shows them on the review. */
			return;
		}

		onSubmit(toAnswers(pages, form.state.values));
	};

	const renderPage = (shown: FormPage) =>
		shown.fields.map((field) => (
			<form.Field key={field.fieldId} name={keyOf(field)}>
				{(control) => (
					<DynamicField
						field={field}
						name={control.name}
						value={control.state.value}
						errors={[...control.state.meta.errors, ...(serverErrors[control.name] ?? [])]}
						disabled={isSubmitting}
						onBlur={control.handleBlur}
						onChange={(value) => {
							control.handleChange(value);

							if (serverErrors[control.name]) {
								setServerErrors(({ [control.name]: _, ...rest }) => rest);
							}
						}}
					/>
				)}
			</form.Field>
		));

	return (
		<FormWizard className="form-wizard--in-dialog">
			<form.Subscribe selector={(state) => state.isDirty}>
				{(isDirty) =>
					onDirtyChange ? <DirtyReporter isDirty={isDirty} onDirtyChange={onDirtyChange} /> : null
				}
			</form.Subscribe>

			{wizard ? <FormWizard.Header steps={steps} currentStep={step} /> : null}

			<FormWizard.Body>
				<FormWizard.Content>
					{page ? (
						<FormWizard.Section>
							<FormWizard.SectionHeader
								title={page.title}
								description={page.description ?? undefined}
							/>
							{renderPage(page)}
							{wizard ? null : reviewAddon}
						</FormWizard.Section>
					) : (
						<form.Subscribe selector={(state) => state.values}>
							{(values) => (
								<>
									{problems.length > 0 ? (
										<div className="wizard-review-error">
											<div className="form-error" role="alert">
												<strong>Fix the following before submitting:</strong>

												<ul className="form-wizard__summary-list">
													{problems.map((problem) => (
														<li key={problem.field.fieldId}>
															<button
																type="button"
																className="underline"
																onClick={() => setStep(pages.indexOf(problem.page))}
															>
																{problem.page.title}
															</button>
															: {problem.field.label} - {problem.message}
														</li>
													))}
												</ul>
											</div>
										</div>
									) : null}

									<DynamicReview pages={pages} answers={toAnswers(pages, values)} />
									{reviewAddon}
								</>
							)}
						</form.Subscribe>
					)}
				</FormWizard.Content>

				{footer}

				<FormWizard.Footer
					currentStep={wizard ? step : 0}
					stepCount={wizard ? steps.length : 1}
					onBack={() => setStep((index) => Math.max(index - 1, 0))}
					onNext={handleNext}
					onSubmit={() => void handleSubmit()}
					onCancel={onCancel}
					isSubmitting={isSubmitting}
					submitLabel={submitLabel}
				/>
			</FormWizard.Body>
		</FormWizard>
	);
}
