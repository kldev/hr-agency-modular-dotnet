import { useState } from "react";
import { toast } from "sonner";
import type {
	BadRequestDetails,
	EngagementType,
	PositionRequest,
	RateBasis,
	RateUnit,
	WorkerContractType,
} from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { DirtyReporter, stepHasErrors } from "#/components/form-wizard/stepValidation";
import { ApiError } from "#/components/ui/ApiError";
import { parseMoney } from "#/components/ui/Input";
import { useAppForm } from "#/forms";
import { useProjectSuggestion } from "#/hooks";
import { useOpenPosition, useUpdatePosition } from "../../pages/hooks";
import { type PositionFormValues, positionSchema } from "./schema";
import { positionStepsFor } from "./steps";
import { ContractStep } from "./steps/ContractStep";
import { ReviewStep } from "./steps/ReviewStep";
import { RoleStep } from "./steps/RoleStep";
import { ScheduleStep } from "./steps/ScheduleStep";
import { WorkStep } from "./steps/WorkStep";

interface PositionWizardProps {
	/** Absent when opening a new role. */
	positionId?: string;
	/** Fixed when the wizard is opened from a project; a role never moves between deliveries. */
	knownProjectId?: string;
	initialValues: PositionFormValues;
	onSaved: () => void;
	onCancel: () => void;
	onDirtyChange: (dirty: boolean) => void;
}

/** An empty optional field is null, not "" — the backend treats the two the same but the read model shows what it was given. */
const orNull = (value: string) => {
	const trimmed = value.trim();

	return trimmed.length > 0 ? trimmed : null;
};

const listOrNull = (values: string[]) => {
	const cleaned = values.map((entry) => entry.trim()).filter(Boolean);

	return cleaned.length > 0 ? cleaned : null;
};

/**
 * One wizard for opening a role and for changing its terms: `PositionRequest` is the same shape on
 * both, so a second form over it would be a second place to forget that half an address is worse
 * than none.
 */
export function PositionWizard({
	positionId,
	knownProjectId,
	initialValues,
	onSaved,
	onCancel,
	onDirtyChange,
}: PositionWizardProps) {
	const [currentStep, setCurrentStep] = useState(0);
	const editing = Boolean(positionId);
	const steps = positionStepsFor({ knownProject: Boolean(knownProjectId) });

	const open = useOpenPosition({});
	const update = useUpdatePosition({});

	const mutation = editing ? update.mutation : open.mutation;

	const form = useAppForm({
		defaultValues: initialValues,

		validators: {
			onChange: positionSchema,
		},

		onSubmit: async ({ value }) => {
			const projectId = knownProjectId ?? value.projectId;

			const request: PositionRequest = {
				name: value.name.trim(),
				contractName: orNull(value.contractName),
				workDescription: orNull(value.workDescription),
				duties: listOrNull(value.duties),
				requiredQualifications: listOrNull(value.requiredQualifications),
				contractType: value.contractType as WorkerContractType,
				rateAmount: value.rateAmount ? parseMoney(value.rateAmount) : null,
				rateCurrency: orNull(value.rateCurrency),
				rateUnit: value.rateUnit as RateUnit,
				rateBasis: value.rateBasis as RateBasis,
				street: orNull(value.street),
				buildingNumber: orNull(value.buildingNumber),
				unitNumber: orNull(value.unitNumber),
				postalCode: orNull(value.postalCode),
				city: orNull(value.city),
				countryCode: value.countryCode ? value.countryCode.toUpperCase() : null,
				weeklyHours: value.weeklyHours ? Number(value.weeklyHours) : null,
				workStartsAt: orNull(value.workStartsAt),
				workSchedule: orNull(value.workSchedule),
				payoutDay: value.payoutDay ? Number(value.payoutDay) : null,
				probationPeriod: orNull(value.probationPeriod),
				noticePeriod: orNull(value.noticePeriod),
				allowances: listOrNull(value.allowances),
				plannedHeadcount: value.plannedHeadcount ? Number(value.plannedHeadcount) : null,
				defaultEngagementType: (value.defaultEngagementType as EngagementType) || null,
			};

			try {
				if (positionId) {
					await update.mutation.mutateAsync({ projectId, positionId, request });
				} else {
					await open.mutation.mutateAsync({ projectId, request });
				}

				onSaved();
			} catch (error) {
				const details = error as BadRequestDetails;

				toast.error(
					details?.title ?? (editing ? "Unable to update the role" : "Unable to open the role"),
				);
			}
		},
	});

	/* The review step names the project while the form only holds its id. */
	const project = useProjectSuggestion(knownProjectId ?? form.state.values.projectId);

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

	const isSubmitting = mutation.isPending;

	return (
		<FormWizard className="form-wizard--in-dialog">
			<form.Subscribe selector={(state) => state.isDirty}>
				{(isDirty) => <DirtyReporter isDirty={isDirty} onDirtyChange={onDirtyChange} />}
			</form.Subscribe>

			<FormWizard.Header steps={steps} currentStep={currentStep} />

			<FormWizard.Body>
				{/* Keyed by step id rather than index: opened from a project, the first step asks less. */}
				<FormWizard.Content>
					{steps[currentStep].id === "role" && (
						<RoleStep
							form={form}
							isSubmitting={isSubmitting}
							knownProject={Boolean(knownProjectId)}
						/>
					)}

					{steps[currentStep].id === "work" && <WorkStep form={form} isSubmitting={isSubmitting} />}

					{steps[currentStep].id === "contract" && (
						<ContractStep form={form} isSubmitting={isSubmitting} />
					)}

					{steps[currentStep].id === "schedule" && (
						<ScheduleStep form={form} isSubmitting={isSubmitting} />
					)}

					{steps[currentStep].id === "review" && (
						<ReviewStep form={form} projectName={project.data?.name} />
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
							submitLabel={editing ? "Save changes" : "Open position"}
						/>
					)}
				</form.Subscribe>
			</FormWizard.Body>
		</FormWizard>
	);
}
