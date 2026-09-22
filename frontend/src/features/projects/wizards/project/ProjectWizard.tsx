import { useState } from "react";
import { toast } from "sonner";
import type { BadRequestDetails, EngagementType } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { DirtyReporter, stepHasErrors } from "#/components/form-wizard/stepValidation";
import { ApiError } from "#/components/ui/ApiError";
import { useAppForm } from "#/forms";
import { useCompanySuggestion } from "#/hooks";
import { useCreateProject, useUpdateProject } from "../../pages/hooks";
import { toDateOnly } from "../../utils";
import { type ProjectFormValues, projectSchema } from "./schema";
import { projectStepsFor } from "./steps";
import { AssignmentStep } from "./steps/AssignmentStep";
import { BasicsStep } from "./steps/BasicsStep";
import { ClientStep } from "./steps/ClientStep";
import { ReviewStep } from "./steps/ReviewStep";
import { TeamStep } from "./steps/TeamStep";

interface ProjectWizardProps {
	/** Absent when creating a new project. */
	projectId?: string;
	initialValues: ProjectFormValues;
	onSaved: (projectId: string) => void;
	onCancel: () => void;
	onDirtyChange: (dirty: boolean) => void;
}

/**
 * One wizard for creating and for editing, as with workers and legal entities: the fields are the
 * same fields, and a second form over the same values is a second place to forget a rule. Editing
 * simply asks fewer steps, because the update command takes less than the create one.
 */
export function ProjectWizard({
	projectId,
	initialValues,
	onSaved,
	onCancel,
	onDirtyChange,
}: ProjectWizardProps) {
	const [currentStep, setCurrentStep] = useState(0);
	const editing = Boolean(projectId);
	const steps = projectStepsFor(editing ? "edit" : "create");

	const create = useCreateProject({});

	const update = useUpdateProject({});

	const mutation = editing ? update.mutation : create.mutation;

	const form = useAppForm({
		defaultValues: initialValues,

		validators: {
			onChange: projectSchema,
		},

		onSubmit: async ({ value }) => {
			/* What both commands share; the create one then adds the parties and the team. */
			const common = {
				name: value.name.trim(),
				description: value.description.trim(),
				street: value.street.trim(),
				buildingNumber: value.buildingNumber.trim(),
				unitNumber: value.unitNumber.trim() || null,
				postalCode: value.postalCode.trim(),
				city: value.city.trim(),
				countryCode: value.countryCode.toUpperCase(),
				startsOn: toDateOnly(value.startsOn),
				endsOn: value.endsOn ? toDateOnly(value.endsOn) : null,
			};

			try {
				if (projectId) {
					await update.mutation.mutateAsync({ projectId, request: common });

					onSaved(projectId);

					return;
				}

				const created = await create.mutation.mutateAsync({
					request: {
						...common,
						companyId: value.companyId,
						legalEntityId: value.legalEntityId,
						engagementType: value.engagementType as EngagementType,
						teamId: value.teamId || null,
					},
				});

				onSaved(created.projectId);
			} catch (error) {
				const details = error as BadRequestDetails;

				toast.error(
					details?.title ??
						(editing ? "Unable to update the project" : "Unable to create the project"),
				);
			}
		},
	});

	const isSubmitting = mutation.isPending;

	/*
	 * The review step names the client while the form only holds its id. The picker's own resolver
	 * answers that, and it is already in the cache from the first step.
	 */
	const clientSuggestion = useCompanySuggestion(form.state.values.companyId);

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
				{/* Keyed by step id rather than index, because editing asks two steps fewer. */}
				<FormWizard.Content>
					{steps[currentStep].id === "client" && (
						<ClientStep form={form} isSubmitting={isSubmitting} />
					)}

					{steps[currentStep].id === "basics" && (
						<BasicsStep form={form} isSubmitting={isSubmitting} editing={editing} />
					)}

					{steps[currentStep].id === "assignment" && (
						<AssignmentStep form={form} isSubmitting={isSubmitting} />
					)}

					{steps[currentStep].id === "team" && <TeamStep form={form} isSubmitting={isSubmitting} />}

					{steps[currentStep].id === "review" && (
						<ReviewStep form={form} clientName={clientSuggestion.data?.name} />
					)}
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
							submitLabel={editing ? "Save changes" : "Create project"}
						/>
					)}
				</form.Subscribe>
			</FormWizard.Body>
		</FormWizard>
	);
}
