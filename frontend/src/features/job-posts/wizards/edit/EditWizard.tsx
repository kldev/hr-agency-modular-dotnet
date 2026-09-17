import { useNavigate } from "@tanstack/react-router";
import { useState } from "react";
import { toast } from "sonner";

import type { BadRequestDetails, JobPostProjection } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { DetailsLoading } from "#/components/ui";
import { ApiError } from "#/components/ui/ApiError";
import { useGetJobDescription } from "#/features/job-descriptions/pages/hooks";
import { useAppForm } from "#/forms";
import { useUpdateJobPost } from "../../hooks";
import { useGetJobPost } from "../../pages/hooks";
import {
	ContentStep,
	EmploymentStep,
	jobPostSchema,
	jobPostSteps,
	PostStep,
	parseSalary,
	RequirementsStep,
	ReviewStep,
} from "../shared";
import { toFormValues } from "./mapper";

const cleanList = (values: string[]) => values.map((value) => value.trim()).filter(Boolean);

export function EditJobPostWizard({ id }: { id: string }) {
	const query = useGetJobPost(id);

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
	return <EditWizardForm jobPost={query.data} />;
}

function EditWizardForm({ jobPost }: { jobPost: JobPostProjection }) {
	const navigate = useNavigate();

	const [currentStep, setCurrentStep] = useState(0);

	const { mutation } = useUpdateJobPost({
		onSuccess: () => {
			toast.success("Changes saved");
		},
	});

	/* The projection carries only the id of the description, and step 0 shows it by name. */
	const jobDescriptionQuery = useGetJobDescription(jobPost.jobDescriptionId);

	const form = useAppForm({
		defaultValues: toFormValues(jobPost),
		validators: {
			onChange: jobPostSchema,
		},

		onSubmit: async ({ value }) => {
			try {
				await mutation.mutateAsync({
					jobPostId: jobPost.id,
					request: {
						title: value.title,
						summary: value.summary || null,
						description: value.description,
						responsibilities: cleanList(value.responsibilities),
						requirements: cleanList(value.requirements),
						skills: cleanList(value.skills),
						location: value.location,
						countryCode: value.countryCode.toUpperCase(),
						languageCode: value.languageCode.toUpperCase(),
						employmentType: value.employmentType,
						workMode: value.workMode,
						currencyCode: value.currencyCode,
						salaryMin: parseSalary(value.salaryMin),
						salaryMax: parseSalary(value.salaryMax),
					},
				});
			} catch (error) {
				const details = error as BadRequestDetails;

				toast.error(details?.title ?? "Unable to save the job post");

				return;
			}

			navigate({
				to: "/app/jobs/$id",
				params: { id: jobPost.id },
				search: { search: undefined, status: undefined },
			});
		},
	});

	const isSubmitting = mutation.isPending;

	/*
	 * `PUT /api/recruitment/job-posting/{id}` takes neither the job description nor the recruiter -
	 * the first is fixed at creation, the second has its own action - so both are shown as text.
	 */
	const source = {
		kind: "jobDescription" as const,
		label: jobDescriptionQuery.data?.title ?? "—",
	};

	const assignment = {
		recruiterName: jobPost.recruiter.fullname ?? jobPost.recruiter.email,
	};

	const handleNext = async () => {
		const fields = jobPostSteps[currentStep].fields;

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

		setCurrentStep((step) => Math.min(step + 1, jobPostSteps.length - 1));
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
				module="Recruitment"
				title={`Edit job post: ${jobPost.title}`}
				description={`${jobPost.company.name} - changes are published as soon as you save.`}
			/>

			<FormWizard.Header steps={jobPostSteps} currentStep={currentStep} />

			<FormWizard.Body>
				<FormWizard.Content>
					{currentStep === 0 && <PostStep form={form} source={source} assignment={assignment} />}

					{currentStep === 1 && <ContentStep form={form} />}

					{currentStep === 2 && <RequirementsStep form={form} />}

					{currentStep === 3 && <EmploymentStep form={form} isSubmitting={isSubmitting} />}

					{currentStep === 4 && (
						<ReviewStep
							form={form}
							source={source}
							description="Review the changes before saving them."
						/>
					)}
				</FormWizard.Content>

				<ApiError error={(mutation.error as unknown as BadRequestDetails) ?? null} />

				<form.Subscribe selector={(state) => state.canSubmit}>
					{(canSubmit) => (
						<FormWizard.Footer
							currentStep={currentStep}
							stepCount={jobPostSteps.length}
							onBack={handleBack}
							onNext={handleNext}
							onSubmit={handleSubmit}
							onCancel={() =>
								navigate({
									to: "/app/jobs/$id",
									params: { id: jobPost.id },
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
