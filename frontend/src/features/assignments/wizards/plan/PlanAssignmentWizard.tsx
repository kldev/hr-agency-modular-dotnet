import { useState } from "react";
import { toast } from "sonner";
import type { BadRequestDetails, EngagementType, PlanAssignmentRequest } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { DirtyReporter, stepHasErrors } from "#/components/form-wizard/stepValidation";
import { ApiError } from "#/components/ui/ApiError";
import { useAppForm } from "#/forms";
import { toDateOnly } from "@/utlis/formatRecord";
import { usePlanAssignment } from "../../pages/hooks";
import { type PlanAssignmentFormValues, planAssignmentSchema } from "./schema";
import { planAssignmentStepsFor } from "./steps";
import { ProjectStep } from "./steps/ProjectStep";
import { ReviewStep } from "./steps/ReviewStep";
import { TermsStep } from "./steps/TermsStep";
import { WorkerStep } from "./steps/WorkerStep";

interface PlanAssignmentWizardProps {
	initialValues: PlanAssignmentFormValues;
	/** Set when the wizard was opened from the person or from the project; that step is dropped. */
	knownWorker: boolean;
	knownProject: boolean;
	onPlanned: (assignmentId: string) => void;
	onCancel: () => void;
	onDirtyChange: (dirty: boolean) => void;
}

/**
 * Planning is the only way a posting comes into being, and there is no editing its way out: the
 * person, the project, the delivering company and the engagement type are frozen here. That is why
 * this is a wizard and not a drawer — the last step says what the choices mean before they are made.
 */
export function PlanAssignmentWizard({
	initialValues,
	knownWorker,
	knownProject,
	onPlanned,
	onCancel,
	onDirtyChange,
}: PlanAssignmentWizardProps) {
	const [currentStep, setCurrentStep] = useState(0);
	const steps = planAssignmentStepsFor({ knownWorker, knownProject });

	const { mutation } = usePlanAssignment({});

	const form = useAppForm({
		defaultValues: initialValues,

		validators: {
			onChange: planAssignmentSchema,
		},

		onSubmit: async ({ value }) => {
			const request: PlanAssignmentRequest = {
				workerId: value.workerId,
				projectId: value.projectId,
				engagementType: value.engagementType as EngagementType,
				positionId: value.positionId,
				startsOn: toDateOnly(value.startsOn),
				endsOn: value.endsOn ? toDateOnly(value.endsOn) : null,
			};

			try {
				const planned = await mutation.mutateAsync({ request });

				onPlanned(planned.assignmentId);
			} catch (error) {
				const details = error as BadRequestDetails;

				toast.error(details?.title ?? "Unable to plan the assignment");
			}
		},
	});

	const isSubmitting = mutation.isPending;

	const handleNext = async () => {
		const fields = steps[currentStep].fields;

		for (const field of fields) {
			await form.validateField(field, "submit");
		}

		const fieldMeta = form.state.fieldMeta as Record<string, { errors: Array<unknown> }>;

		if (stepHasErrors(fieldMeta, fields)) {
			return;
		}

		setCurrentStep((step) => Math.min(step + 1, steps.length - 1));
	};

	return (
		<FormWizard className="form-wizard--in-dialog">
			<form.Subscribe selector={(state) => state.isDirty}>
				{(isDirty) => <DirtyReporter isDirty={isDirty} onDirtyChange={onDirtyChange} />}
			</form.Subscribe>

			<FormWizard.Header steps={steps} currentStep={currentStep} />

			<FormWizard.Body>
				{/* Keyed by step id rather than index, because either of the first two can be missing. */}
				<FormWizard.Content>
					{steps[currentStep].id === "worker" && (
						<WorkerStep form={form} isSubmitting={isSubmitting} />
					)}

					{steps[currentStep].id === "project" && (
						<ProjectStep form={form} isSubmitting={isSubmitting} />
					)}

					{steps[currentStep].id === "terms" && (
						<TermsStep form={form} isSubmitting={isSubmitting} />
					)}

					{steps[currentStep].id === "review" && <ReviewStep form={form} />}
				</FormWizard.Content>

				<ApiError error={(mutation.error as unknown as BadRequestDetails) ?? null} />

				<form.Subscribe selector={(state) => state.canSubmit}>
					{(canSubmit) => (
						<FormWizard.Footer
							currentStep={currentStep}
							stepCount={steps.length}
							onBack={() => setCurrentStep((step) => Math.max(step - 1, 0))}
							onNext={handleNext}
							onSubmit={() => {
								void form.handleSubmit();
							}}
							onCancel={onCancel}
							canSubmit={canSubmit}
							isSubmitting={isSubmitting}
							submitLabel="Plan assignment"
						/>
					)}
				</form.Subscribe>
			</FormWizard.Body>
		</FormWizard>
	);
}
