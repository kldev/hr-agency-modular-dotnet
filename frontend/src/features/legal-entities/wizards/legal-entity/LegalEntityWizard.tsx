import { useState } from "react";
import { toast } from "sonner";
import type { BadRequestDetails, BankAccountData, LegalEntityRequest } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { DirtyReporter, stepHasErrors } from "#/components/form-wizard/stepValidation";
import { ApiError } from "#/components/ui/ApiError";
import { useAppForm } from "#/forms";
import { useCreateLegalEntity, useUpdateLegalEntity } from "../../pages/hooks";
import { type LegalEntityFormValues, legalEntitySchema } from "./schema";
import { legalEntitySteps } from "./steps";
import { AddressStep } from "./steps/AddressStep";
import { BankingStep } from "./steps/BankingStep";
import { IdentityStep } from "./steps/IdentityStep";
import { RepresentationStep } from "./steps/RepresentationStep";
import { ReviewStep } from "./steps/ReviewStep";

interface LegalEntityWizardProps {
	/** Absent when creating a new company. */
	entityId?: string;
	initialValues: LegalEntityFormValues;
	initialAccounts: BankAccountData[];
	onSaved: () => void;
	onCancel: () => void;
	onDirtyChange: (dirty: boolean) => void;
}

/**
 * Sixteen fields plus a repeatable list of bank accounts. That was a slide-over with a scroll bar
 * and a form somewhere inside it; as steps it is four short screens and a summary, and the summary
 * is the first place the whole company can actually be read before it is saved.
 */
export function LegalEntityWizard({
	entityId,
	initialValues,
	initialAccounts,
	onSaved,
	onCancel,
	onDirtyChange,
}: LegalEntityWizardProps) {
	const [currentStep, setCurrentStep] = useState(0);
	const [accounts, setAccounts] = useState<BankAccountData[]>(initialAccounts);

	const editing = Boolean(entityId);

	const create = useCreateLegalEntity({
		onSuccess: () => toast.success("Legal entity created"),
	});

	const update = useUpdateLegalEntity({
		onSuccess: () => toast.success("Legal entity updated"),
	});

	const mutation = editing ? update.mutation : create.mutation;

	const form = useAppForm({
		defaultValues: initialValues,

		validators: { onChange: legalEntitySchema },

		onSubmit: async ({ value }) => {
			const request: LegalEntityRequest = {
				name: value.name.trim(),
				legalName: value.legalName.trim(),
				taxId: value.taxId.trim(),
				vatNumber: value.vatNumber.trim() || null,
				street: value.street.trim(),
				buildingNumber: value.buildingNumber.trim(),
				unitNumber: value.unitNumber.trim() || null,
				postalCode: value.postalCode.trim(),
				city: value.city.trim(),
				countryCode: value.countryCode,
				description: value.description.trim() || null,
				presidentFirstName: value.presidentFirstName.trim(),
				presidentLastName: value.presidentLastName.trim(),
				presidentEmail: value.presidentEmail.trim() || null,
				activeFrom: value.activeFrom,
				activeTo: value.activeTo || null,
				bankAccounts: accounts,
			};

			try {
				if (entityId) {
					await update.mutation.mutateAsync({ id: entityId, request });
				} else {
					await create.mutation.mutateAsync(request);
				}
			} catch (error) {
				const details = error as BadRequestDetails;

				toast.error(
					details?.title ??
						(editing ? "Unable to update the legal entity" : "Unable to create the legal entity"),
				);

				return;
			}

			onSaved();
		},
	});

	const isSubmitting = mutation.isPending;

	const handleNext = async () => {
		const fields = legalEntitySteps[currentStep].fields;

		for (const field of fields) {
			await form.validateField(field, "submit");
		}

		const fieldMeta = form.state.fieldMeta as Record<string, { errors: Array<unknown> }>;

		if (stepHasErrors(fieldMeta, fields)) {
			return;
		}

		setCurrentStep((step) => Math.min(step + 1, legalEntitySteps.length - 1));
	};

	return (
		<FormWizard className="form-wizard--in-dialog">
			<form.Subscribe selector={(state) => state.isDirty}>
				{(isDirty) => <DirtyReporter isDirty={isDirty} onDirtyChange={onDirtyChange} />}
			</form.Subscribe>

			<FormWizard.Header steps={legalEntitySteps} currentStep={currentStep} />

			<FormWizard.Body>
				<FormWizard.Content>
					{currentStep === 0 && <IdentityStep form={form} isSubmitting={isSubmitting} />}

					{currentStep === 1 && <AddressStep form={form} isSubmitting={isSubmitting} />}

					{currentStep === 2 && <RepresentationStep form={form} isSubmitting={isSubmitting} />}

					{currentStep === 3 && (
						<BankingStep accounts={accounts} onChange={setAccounts} isSubmitting={isSubmitting} />
					)}

					{currentStep === 4 && <ReviewStep form={form} accounts={accounts} />}
				</FormWizard.Content>

				<ApiError error={(mutation.error as unknown as BadRequestDetails) ?? null} />

				<form.Subscribe selector={(state) => state.canSubmit}>
					{(canSubmit) => (
						<FormWizard.Footer
							currentStep={currentStep}
							stepCount={legalEntitySteps.length}
							onBack={() => setCurrentStep((step) => Math.max(step - 1, 0))}
							onNext={handleNext}
							onSubmit={() => {
								void form.handleSubmit();
							}}
							onCancel={onCancel}
							canSubmit={canSubmit}
							isSubmitting={isSubmitting}
							submitLabel={editing ? "Save changes" : "Create legal entity"}
						/>
					)}
				</form.Subscribe>
			</FormWizard.Body>
		</FormWizard>
	);
}
