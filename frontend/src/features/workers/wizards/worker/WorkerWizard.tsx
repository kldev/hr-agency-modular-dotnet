import { useState } from "react";
import { toast } from "sonner";
import type { BadRequestDetails, IdentityDocumentKind, WorkerRequest } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { DirtyReporter, stepHasErrors } from "#/components/form-wizard/stepValidation";
import { ApiError } from "#/components/ui/ApiError";
import { useAppForm } from "#/forms";
import { toDateOnly } from "@/utlis/formatRecord";
import { useRegisterWorker, useUpdateWorker } from "../../pages/hooks";
import { type WorkerFormValues, workerSchema } from "./schema";
import { workerStepsFor } from "./steps";
import { ContactStep } from "./steps/ContactStep";
import { DocumentStep } from "./steps/DocumentStep";
import { IdentityStep } from "./steps/IdentityStep";
import { ReviewStep } from "./steps/ReviewStep";
import { SourceStep } from "./steps/SourceStep";

interface WorkerWizardProps {
	/** Absent when registering somebody new. */
	workerId?: string;
	initialValues: WorkerFormValues;
	onSaved: (workerId: string) => void;
	onCancel: () => void;
	onDirtyChange: (dirty: boolean) => void;
}

/**
 * One wizard for registering and for editing, because the API takes one shape for both. Seventeen
 * fields is too many for a drawer either way, and a second form for the same payload would be a
 * second place to forget the all-or-nothing address rule.
 */
export function WorkerWizard({
	workerId,
	initialValues,
	onSaved,
	onCancel,
	onDirtyChange,
}: WorkerWizardProps) {
	const [currentStep, setCurrentStep] = useState(0);
	const editing = Boolean(workerId);

	/* Seeded from an application: everything the Source step would ask for is already in hand. */
	const knownSource = Boolean(initialValues.sourceApplicationId);

	const steps = workerStepsFor({ mode: editing ? "edit" : "register", knownSource });

	const register = useRegisterWorker({});

	const update = useUpdateWorker({});

	const mutation = editing ? update.mutation : register.mutation;

	const form = useAppForm({
		defaultValues: initialValues,

		validators: {
			onChange: workerSchema,
		},

		onSubmit: async ({ value }) => {
			const request: WorkerRequest = {
				firstName: value.firstName.trim(),
				lastName: value.lastName.trim(),
				dateOfBirth: toDateOnly(value.dateOfBirth),
				citizenship: value.citizenship.toUpperCase(),
				identityDocumentKind: value.identityDocumentKind as IdentityDocumentKind,
				identityDocumentNumber: value.identityDocumentNumber.trim(),
				identityDocumentIssuingCountry: value.identityDocumentIssuingCountry.toUpperCase(),
				identityDocumentValidUntil: value.identityDocumentValidUntil
					? toDateOnly(value.identityDocumentValidUntil)
					: null,
				email: value.email.trim() || null,
				phoneNumber: value.phoneNumber.trim() || null,
				street: value.street.trim() || null,
				buildingNumber: value.buildingNumber.trim() || null,
				unitNumber: value.unitNumber.trim() || null,
				postalCode: value.postalCode.trim() || null,
				city: value.city.trim() || null,
				addressCountryCode: value.addressCountryCode.toUpperCase() || null,
				note: value.note.trim() || null,
				// Register only - the API ignores it on an update, because an origin does not change.
				...(workerId
					? {}
					: {
							sourceCandidateId: value.sourceCandidateId || null,
							// Which application it came out of, so that application can say so.
							sourceApplicationId: value.sourceApplicationId || null,
						}),
			};

			try {
				if (workerId) {
					await update.mutation.mutateAsync({ workerId, request });

					onSaved(workerId);

					return;
				}

				const created = await register.mutation.mutateAsync({ request });

				onSaved(created.workerId);
			} catch (error) {
				const details = error as BadRequestDetails;

				toast.error(
					details?.title ?? (editing ? "Unable to update the worker" : "Unable to register"),
				);
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
				{/* Keyed by step id rather than index, because registering has one step more. */}
				<FormWizard.Content>
					{steps[currentStep].id === "source" && (
						<SourceStep form={form} isSubmitting={isSubmitting} />
					)}

					{steps[currentStep].id === "identity" && (
						<IdentityStep form={form} isSubmitting={isSubmitting} />
					)}

					{steps[currentStep].id === "document" && (
						<DocumentStep form={form} isSubmitting={isSubmitting} />
					)}

					{steps[currentStep].id === "contact" && (
						<ContactStep form={form} isSubmitting={isSubmitting} />
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
							submitLabel={editing ? "Save changes" : "Register worker"}
						/>
					)}
				</form.Subscribe>
			</FormWizard.Body>
		</FormWizard>
	);
}
