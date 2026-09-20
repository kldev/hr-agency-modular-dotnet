import { useState } from "react";
import { toast } from "sonner";
import type { BadRequestDetails, EngagementType } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { DirtyReporter, stepHasErrors } from "#/components/form-wizard/stepValidation";
import { ApiError } from "#/components/ui/ApiError";
import { useAppForm } from "#/forms";
import { useCompanySuggestion } from "#/hooks";
import { useCreateProject } from "../../pages/hooks";
import { toDateOnly } from "../../utils";
import { emptyProject, projectSchema } from "./schema";
import { projectSteps } from "./steps";
import { AssignmentStep } from "./steps/AssignmentStep";
import { BasicsStep } from "./steps/BasicsStep";
import { ClientStep } from "./steps/ClientStep";
import { ReviewStep } from "./steps/ReviewStep";
import { TeamStep } from "./steps/TeamStep";

interface CreateProjectWizardProps {
	onCreated: (projectId: string) => void;
	onCancel: () => void;
	onDirtyChange: (dirty: boolean) => void;
}

export function CreateProjectWizard({
	onCreated,
	onCancel,
	onDirtyChange,
}: CreateProjectWizardProps) {
	const [currentStep, setCurrentStep] = useState(0);

	const { mutation } = useCreateProject({
		onSuccess: () => {
			toast.success("Project created");
		},
	});

	const form = useAppForm({
		defaultValues: emptyProject,

		validators: {
			onChange: projectSchema,
		},

		onSubmit: async ({ value }) => {
			let created: Awaited<ReturnType<typeof mutation.mutateAsync>>;

			try {
				created = await mutation.mutateAsync({
					request: {
						companyId: value.companyId,
						name: value.name.trim(),
						description: value.description.trim(),
						engagementType: value.engagementType as EngagementType,
						street: value.street.trim(),
						buildingNumber: value.buildingNumber.trim(),
						unitNumber: value.unitNumber.trim() || null,
						postalCode: value.postalCode.trim(),
						city: value.city.trim(),
						countryCode: value.countryCode.toUpperCase(),
						startsOn: toDateOnly(value.startsOn),
						endsOn: value.endsOn ? toDateOnly(value.endsOn) : null,
						teamId: value.teamId || null,
					},
				});
			} catch (error) {
				const details = error as BadRequestDetails;

				toast.error(details?.title ?? "Unable to create the project");

				return;
			}

			onCreated(created.projectId);
		},
	});

	const isSubmitting = mutation.isPending;

	/*
	 * The review step names the client while the form only holds its id. The picker's own resolver
	 * answers that, and it is already in the cache from the first step.
	 */
	const clientSuggestion = useCompanySuggestion(form.state.values.companyId);

	const handleNext = async () => {
		const fields = projectSteps[currentStep].fields;

		for (const field of fields) {
			await form.validateField(field, "submit");
		}

		const fieldMeta = form.state.fieldMeta as Record<string, { errors: Array<unknown> }>;

		if (stepHasErrors(fieldMeta, fields)) {
			return;
		}

		setCurrentStep((step) => Math.min(step + 1, projectSteps.length - 1));
	};

	return (
		<FormWizard className="form-wizard--in-dialog">
			<form.Subscribe selector={(state) => state.isDirty}>
				{(isDirty) => <DirtyReporter isDirty={isDirty} onDirtyChange={onDirtyChange} />}
			</form.Subscribe>

			<FormWizard.Header steps={projectSteps} currentStep={currentStep} />

			<FormWizard.Body>
				<FormWizard.Content>
					{currentStep === 0 && <ClientStep form={form} isSubmitting={isSubmitting} />}

					{currentStep === 1 && <BasicsStep form={form} isSubmitting={isSubmitting} />}

					{currentStep === 2 && <AssignmentStep form={form} isSubmitting={isSubmitting} />}

					{currentStep === 3 && <TeamStep form={form} isSubmitting={isSubmitting} />}

					{currentStep === 4 && <ReviewStep form={form} clientName={clientSuggestion.data?.name} />}
				</FormWizard.Content>

				<ApiError error={(mutation.error as unknown as BadRequestDetails) ?? null} />

				<form.Subscribe selector={(state) => state.canSubmit}>
					{(canSubmit) => (
						<FormWizard.Footer
							currentStep={currentStep}
							stepCount={projectSteps.length}
							onBack={() => setCurrentStep((step) => Math.max(step - 1, 0))}
							onNext={handleNext}
							onSubmit={() => {
								void form.handleSubmit();
							}}
							onCancel={onCancel}
							canSubmit={canSubmit}
							isSubmitting={isSubmitting}
							submitLabel="Create project"
						/>
					)}
				</form.Subscribe>
			</FormWizard.Body>
		</FormWizard>
	);
}
