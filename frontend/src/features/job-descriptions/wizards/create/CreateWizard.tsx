import { useState } from "react";
import { toast } from "sonner";

import type { BadRequestDetails } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { ApiError } from "#/components/ui/ApiError";
import { useGetCompany } from "#/features/companies/pages/hooks";
import { useAppForm } from "#/forms";
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

export function CreateJobDescriptionWizard() {
	const navigate = Route.useNavigate();
	const { companyId } = Route.useSearch();

	const [currentStep, setCurrentStep] = useState(0);

	const companyQuery = useGetCompany(companyId ?? "");
	const { mutation } = useCreateJobDescription({
		onSuccess: () => {
			toast.success("Job description created");
		},
	});

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
						responsibilities: value.responsibilities.filter(Boolean),
						requirements: value.requirements.filter(Boolean),
						skills: value.skills.filter(Boolean),
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

		const hasErrors = fields.some((field) => form.getFieldMeta(field)?.errors.length);

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

	const company = companyQuery.data;

	return (
		<FormWizard>
			<FormWizard.Title
				module="Sales"
				title={
					company
						? `Create job description for company: ${company.name} `
						: "Create job description"
				}
				description="  Create a structured job posting ready for your recruitment pipeline."
			/>
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
