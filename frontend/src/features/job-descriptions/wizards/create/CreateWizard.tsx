import { useState } from "react";
import { toast } from "sonner";

import type { BadRequestDetails } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { ApiError } from "#/components/ui/ApiError";
import { useAppForm } from "#/forms";
import { useCompanySuggestion } from "#/hooks";
import { Route } from "#/routes/app/job-descriptions/add";
import { useCreateJobDescription } from "../../hooks";
import { ContentStep } from "./ContentStep";
import { EmploymentStep } from "./Employment";
import { PositionStep } from "./PositionStep";
import { RequirementsStep } from "./Requirements";
import { ReviewStep } from "./ReviewStep";
import { type JobDescriptionFormValues, jobDescriptionSchema, parseSalary } from "./schema";
import { jobDescriptionSteps } from "./steps";

export type JobDescriptionField = keyof JobDescriptionFormValues;

const cleanList = (values: string[]) => values.map((value) => value.trim()).filter(Boolean);

export function CreateJobDescriptionWizard() {
	const navigate = Route.useNavigate();
	const { companyId } = Route.useSearch();

	const [currentStep, setCurrentStep] = useState(0);

	const { mutation } = useCreateJobDescription();

	const empty: JobDescriptionFormValues = {
		companyId: companyId ?? "",
		countryCode: "PL",
		currencyCode: "PLN",
		description: "",
		employmentType: "FullTime",
		location: "",
		recruiterId: "",
		requirements: [""],
		responsibilities: [""],
		salaryMax: "",
		salaryMin: "",
		skills: [""],
		summary: "",
		title: "",
		workMode: "OnSite",
	};

	const form = useAppForm({
		defaultValues: empty,
		validators: {
			onChange: jobDescriptionSchema,
		},

		onSubmit: async ({ value }) => {
			try {
				await mutation.mutateAsync({
					request: {
						companyId: value.companyId,
						title: value.title,
						summary: value.summary || null,
						description: value.description,
						responsibilities: cleanList(value.responsibilities),
						requirements: cleanList(value.requirements),
						skills: cleanList(value.skills),
						location: value.location,
						countryCode: value.countryCode,
						employmentType: value.employmentType,
						workMode: value.workMode,
						currencyCode: value.currencyCode,
						salaryMin: parseSalary(value.salaryMin),
						salaryMax: parseSalary(value.salaryMax),
						recruiterId: value.recruiterId,
					},
				});
			} catch (error) {
				const details = error as BadRequestDetails;

				toast.error(details?.title ?? "Unable to create the job description");

				return;
			}

			navigate({
				to: "/app/job-descriptions",
			});
		},
	});

	const isSubmitting = mutation.isPending;

	const handleNext = async () => {
		const fields = jobDescriptionSteps[currentStep].fields;

		for (const field of fields) {
			await form.validateField(field, "submit");
		}

		/*
		 * A schema issue can land on a nested path (`responsibilities[0]`), which `getFieldMeta` of
		 * the array itself does not see - hence the prefix match.
		 */
		const fieldMeta = form.state.fieldMeta as Record<string, { errors: Array<unknown> }>;

		const hasErrors = Object.entries(fieldMeta).some(
			([name, meta]) =>
				meta.errors.length > 0 &&
				fields.some(
					(field) => name === field || name.startsWith(`${field}[`) || name.startsWith(`${field}.`),
				),
		);

		if (hasErrors) {
			return;
		}

		setCurrentStep((step) => Math.min(step + 1, jobDescriptionSteps.length - 1));
	};

	const handleBack = () => {
		setCurrentStep((value) => Math.max(value - 1, 0));
	};

	const handleSubmit = () => {
		void form.handleSubmit();
	};

	return (
		<FormWizard>
			<form.Subscribe selector={(state) => state.values.companyId}>
				{(selectedCompanyId) => <WizardTitle companyId={selectedCompanyId} />}
			</form.Subscribe>
			<FormWizard.Header steps={jobDescriptionSteps} currentStep={currentStep} />

			<FormWizard.Body>
				<FormWizard.Content>
					{currentStep === 0 && <PositionStep form={form} />}
					{currentStep === 1 && <ContentStep form={form} />}

					{currentStep === 2 && <RequirementsStep form={form} />}

					{currentStep === 3 && <EmploymentStep form={form} isSubmitting={isSubmitting} />}

					{currentStep === 4 && <ReviewStep form={form} />}
				</FormWizard.Content>
				<ApiError error={(mutation.error as unknown as BadRequestDetails) ?? null} />

				<form.Subscribe selector={(state) => state.canSubmit}>
					{(canSubmit) => (
						<FormWizard.Footer
							currentStep={currentStep}
							stepCount={jobDescriptionSteps.length}
							onBack={handleBack}
							onNext={handleNext}
							onSubmit={handleSubmit}
							onCancel={() =>
								navigate({
									to: "/app/job-descriptions",
								})
							}
							canSubmit={canSubmit}
							isSubmitting={isSubmitting}
							submitLabel="Create job description"
						/>
					)}
				</form.Subscribe>
			</FormWizard.Body>
		</FormWizard>
	);
}

function WizardTitle({ companyId }: { companyId: string }) {
	const { data: company } = useCompanySuggestion(companyId);

	return (
		<FormWizard.Title
			module="Sales"
			title={
				company ? `Create job description for company: ${company.name}` : "Create job description"
			}
			description="Create a structured job posting ready for your recruitment pipeline."
		/>
	);
}
