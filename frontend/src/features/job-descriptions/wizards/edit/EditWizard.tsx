import { useNavigate } from "@tanstack/react-router";
import { useState } from "react";
import { toast } from "sonner";

import type { BadRequestDetails, JobDescriptionProjection } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { DetailsLoading } from "#/components/ui";
import { ApiError } from "#/components/ui/ApiError";
import { useAppForm } from "#/forms";
import { useUpdateJobDescription } from "../../hooks";
import { useGetJobDescription } from "../../pages/hooks";
import { ContentStep } from "../create/ContentStep";
import { EmploymentStep } from "../create/Employment";
import { PositionStep } from "../create/PositionStep";
import { RequirementsStep } from "../create/Requirements";
import { ReviewStep } from "../create/ReviewStep";
import { jobDescriptionSchema, parseSalary } from "../create/schema";
import { jobDescriptionSteps } from "../create/steps";
import { toFormValues } from "./mapper";

const cleanList = (values: string[]) => values.map((value) => value.trim()).filter(Boolean);

export function EditJobDescriptionWizard({ id }: { id: string }) {
	const query = useGetJobDescription(id);

	if (!id || query.isLoading || query.isError || !query.data) {
		return (
			<DetailsLoading id={id} isLoading={query.isLoading} isError={query.isError || !query.data} />
		);
	}

	/*
	 * The form is seeded once, from `defaultValues`, so it may only be mounted with the record in
	 * hand. Resetting it from an effect after the fetch would drop `dirty`/`touched` and could
	 * overwrite what the user has already typed on a refetch.
	 */
	return <EditWizardForm jobDescription={query.data} />;
}

function EditWizardForm({ jobDescription }: { jobDescription: JobDescriptionProjection }) {
	const navigate = useNavigate();

	const [currentStep, setCurrentStep] = useState(0);

	const { mutation } = useUpdateJobDescription({
		onSuccess: () => {
			toast.success("Changes saved");
		},
	});

	const form = useAppForm({
		defaultValues: toFormValues(jobDescription),
		validators: {
			onChange: jobDescriptionSchema,
		},

		onSubmit: async ({ value }) => {
			try {
				await mutation.mutateAsync({
					jobDescriptionId: jobDescription.id,
					request: {
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
					},
				});
			} catch (error) {
				const details = error as BadRequestDetails;

				toast.error(details?.title ?? "Unable to save the job description");

				return;
			}

			navigate({
				to: "/app/job-descriptions/$id",
				params: { id: jobDescription.id },
				search: { search: undefined, status: undefined },
			});
		},
	});

	const isSubmitting = mutation.isPending;

	const assignment = {
		companyName: jobDescription.company.name,
		recruiterName: jobDescription.recruiter.fullname ?? jobDescription.recruiter.email,
	};

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
			<FormWizard.Title
				module="Sales"
				title={`Edit job description: ${jobDescription.title}`}
				description={`${jobDescription.company.name} - changes are published as soon as you save.`}
			/>

			<FormWizard.Header steps={jobDescriptionSteps} currentStep={currentStep} />

			<FormWizard.Body>
				<FormWizard.Content>
					{currentStep === 0 && <PositionStep form={form} assignment={assignment} />}

					{currentStep === 1 && <ContentStep form={form} />}

					{currentStep === 2 && <RequirementsStep form={form} />}

					{currentStep === 3 && <EmploymentStep form={form} isSubmitting={isSubmitting} />}

					{currentStep === 4 && (
						<ReviewStep form={form} description="Review the changes before saving them." />
					)}
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
									to: "/app/job-descriptions/$id",
									params: { id: jobDescription.id },
									search: { search: undefined, status: undefined },
								})
							}
							canSubmit={canSubmit}
							isSubmitting={isSubmitting}
							submitLabel="Save changes"
						/>
					)}
				</form.Subscribe>
			</FormWizard.Body>
		</FormWizard>
	);
}
