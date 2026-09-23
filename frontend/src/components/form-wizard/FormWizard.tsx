/** biome-ignore-all lint/a11y/noLabelWithoutControl: s */
import clsx from "clsx";
import { ArrowLeft, ArrowRight, type LucideIcon } from "lucide-react";
import type { ReactNode } from "react";
import { Button } from "@/components/ui/Button";
import "./form-wizard.css";

type WizardStep<TField extends string = string> = {
	id: string;
	title: string;
	description: string;
	fields: readonly TField[];
};
type FormWizardProps = {
	children: ReactNode;
	description?: string;
	className?: string;
};

type WizardTitleProps = {
	module?: string;
	title: string;
	description?: string;
	className?: string;
};

type WizardHeaderProps = {
	steps: readonly WizardStep[];
	currentStep: number;
	className?: string;
};

type WizardBodyProps = {
	children: ReactNode;
	className?: string;
};

type WizardContentProps = {
	children: ReactNode;
	className?: string;
};

type WizardSectionProps = {
	children: ReactNode;
	className?: string;
};

type WizardSectionHeaderProps = {
	title: string;
	description?: string;
	className?: string;
	icon?: LucideIcon;
};

type WizardFooterProps = {
	currentStep: number;
	stepCount: number;

	onBack?: () => void;
	onNext?: () => void;
	onSubmit?: () => void;
	onCancel?: () => void;

	canGoBack?: boolean;
	canGoNext?: boolean;
	canSubmit?: boolean;

	isSubmitting?: boolean;

	nextLabel?: string;
	submitLabel?: string;

	children?: ReactNode;
	className?: string;
};

function FormWizardComponent({ children, className }: FormWizardProps) {
	return <div className={clsx("form-wizard", className)}>{children}</div>;
}

function WizardTitle({ title, module, description, className }: WizardTitleProps) {
	return (
		<div className={clsx("form-title", className)}>
			<div className="form-title__content">
				{module ? (
					<p className="mb-1 text-xs font-semibold uppercase tracking-[0.08em] text-(--color-primary)">
						{module}
					</p>
				) : null}
				<h1 className="text-2xl font-semibold tracking-tight text-(--color-text) sm:text-[28px]">
					{title}
				</h1>
				<p className="from_title__description mt-1 max-w-2xl text-sm text-(--color-text-secondary)">
					{description}
				</p>
			</div>
		</div>
	);
}

function WizardHeader({ steps, currentStep, className }: WizardHeaderProps) {
	const activeStep = steps[currentStep];

	const progress = steps.length > 1 ? ((currentStep + 1) / steps.length) * 100 : 100;

	return (
		<header className={clsx("form-wizard__header", className)}>
			<div className="form-wizard__steps">
				{steps.map((step, index) => {
					const isActive = index === currentStep;
					const isCompleted = index < currentStep;

					return (
						<div
							key={step.id}
							className={clsx(
								"form-wizard__step",
								isActive && "form-wizard__step--active",
								isCompleted && "form-wizard__step--completed",
							)}
						>
							<div className="form-wizard__step-number">{isCompleted ? "✓" : index + 1}</div>

							<div className="form-wizard__step-content">
								<span className="form-wizard__step-title">{step.title}</span>

								<span className="form-wizard__step-description">{step.description}</span>
							</div>
						</div>
					);
				})}
			</div>

			<div className="form-wizard__progress">
				<div className="form-wizard__progress-label">
					<span>{activeStep?.title}</span>

					<span>
						{currentStep + 1} / {steps.length}
					</span>
				</div>

				<div
					className="form-wizard__progress-track"
					role="progressbar"
					aria-valuemin={1}
					aria-valuemax={steps.length}
					aria-valuenow={currentStep + 1}
				>
					<div className="form-wizard__progress-value" style={{ width: `${progress}%` }} />
				</div>
			</div>
		</header>
	);
}

function WizardBody({ children, className }: WizardBodyProps) {
	return <main className={clsx("form-wizard__body", className)}>{children}</main>;
}

function WizardContent({ children, className }: WizardContentProps) {
	return <div className={clsx("form-wizard__content", className)}>{children}</div>;
}

function WizardSection({ children, className }: WizardSectionProps) {
	return <section className={clsx("form-wizard__section", className)}>{children}</section>;
}

function WizardSectionHeader({ title, description, className, icon }: WizardSectionHeaderProps) {
	const Icon = icon as LucideIcon;
	return (
		<header className="mb-7 border-b border-(--color-border-subtle)">
			<div className={clsx("form-wizard__section-header", className)}>
				{icon ? (
					<div className="mb-3 flex h-9 w-9 items-center justify-center rounded-md bg-(--color-primary-soft) text-(--color-primary)">
						<Icon size={17} />
					</div>
				) : null}
				<h2 className="form-wizard__section-title">{title}</h2>

				{description && <p className="form-wizard__section-description">{description}</p>}
			</div>
		</header>
	);
}

function WizardField({
	label,
	hint,
	required,
	htmlFor,
	children,
}: {
	label: string;
	hint?: string;
	required?: boolean;
	/** The id of the control inside - a form field's `fieldName` - so the label names it. */
	htmlFor?: string;
	children: React.ReactNode;
}) {
	return (
		<div className="space-y-2 mb-5">
			<label htmlFor={htmlFor} className="block text-sm font-medium text-(--color-text-secondary)">
				{label}
				{required && (
					<span aria-hidden="true" className="ml-1 text-(--color-danger)">
						*
					</span>
				)}
			</label>
			{children}
			{hint && <p className="text-xs text-(--color-text-muted)">{hint}</p>}
		</div>
	);
}

function WizardFooter({
	currentStep,
	stepCount,
	onBack,
	onNext,
	onSubmit,
	onCancel,

	canGoBack = currentStep > 0,
	canGoNext = currentStep < stepCount - 1,
	canSubmit = true,
	isSubmitting = false,
	nextLabel = "Continue",
	submitLabel = "Create",
	children,
	className,
}: WizardFooterProps) {
	const isFirstStep = currentStep === 0;
	const isLastStep = currentStep === stepCount - 1;

	return (
		<footer className={clsx("form-wizard__footer", className)}>
			<div className="form-wizard__footer-left">
				{!isFirstStep && onBack && (
					<Button
						variant="secondary"
						icon={<ArrowLeft size={14} />}
						onClick={onBack}
						disabled={!canGoBack || isSubmitting}
					>
						<span className="button-label">Back</span>
					</Button>
				)}
			</div>

			<div className="form-wizard__footer-right">
				{children}

				{onCancel && (
					<Button variant="ghost" onClick={onCancel} disabled={isSubmitting}>
						<span className="button-label">Cancel</span>
					</Button>
				)}

				{isLastStep
					? onSubmit && (
							<Button
								variant="primary"
								onClick={onSubmit}
								loading={isSubmitting}
								disabled={!canSubmit}
							>
								{submitLabel}
							</Button>
						)
					: onNext && (
							<Button
								variant="primary"
								icon={<ArrowRight size={14} />}
								onClick={onNext}
								disabled={!canGoNext}
							>
								{nextLabel}
							</Button>
						)}
			</div>
		</footer>
	);
}

export const FormWizard = Object.assign(FormWizardComponent, {
	Header: WizardHeader,
	Title: WizardTitle,
	Body: WizardBody,
	Content: WizardContent,
	Section: WizardSection,
	SectionHeader: WizardSectionHeader,
	Footer: WizardFooter,
	Field: WizardField,
});
