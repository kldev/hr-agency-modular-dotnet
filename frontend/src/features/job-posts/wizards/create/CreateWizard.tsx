import { Link } from "@tanstack/react-router";
import { useState } from "react";
import { toast } from "sonner";

import type { BadRequestDetails } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { DetailsLoading } from "#/components/ui";
import { ApiError } from "#/components/ui/ApiError";
import { useGetJobDescription } from "#/features/job-descriptions/pages/hooks";
import { useAppForm } from "#/forms";
import { Route } from "#/routes/app/jobs/add";
import { useCreateJobPost } from "../../hooks";
import { useGetJobPost } from "../../pages/hooks";
import {
	ContentStep,
	EmploymentStep,
	type JobPostFormValues,
	type JobPostSource,
	jobPostSchema,
	jobPostSteps,
	PostStep,
	parseSalary,
	RequirementsStep,
	ReviewStep,
} from "../shared";
import { fromJobDescription, fromJobPost } from "./mappers";

const cleanList = (values: string[]) => values.map((value) => value.trim()).filter(Boolean);

/**
 * One wizard, two seeds - told apart by the search param.
 *
 * The steps, the schema, the validation and the mutation are the same in both cases; what differs
 * is the mapper that fills the form and where "Cancel" goes back to. Two routes would duplicate all
 * of that to express one branch.
 */
export function CreateJobPostWizard() {
	const navigate = Route.useNavigate();
	const { jobDescriptionId, fromJobPostId } = Route.useSearch();

	/*
	 * Both queries are always called (rules of hooks) and disabled by an empty id. Asking for both
	 * sources at once is not a copy of anything in particular, so neither is fetched.
	 */
	const isAmbiguous = Boolean(jobDescriptionId) && Boolean(fromJobPostId);

	const jobDescriptionQuery = useGetJobDescription(
		!isAmbiguous && jobDescriptionId ? jobDescriptionId : "",
	);

	const jobPostQuery = useGetJobPost(!isAmbiguous && fromJobPostId ? fromJobPostId : "");

	if (isAmbiguous || (!jobDescriptionId && !fromJobPostId)) {
		return <MissingSource />;
	}

	if (fromJobPostId) {
		if (jobPostQuery.isLoading || jobPostQuery.isError || !jobPostQuery.data) {
			return (
				<DetailsLoading
					id={fromJobPostId}
					isLoading={jobPostQuery.isLoading}
					isError={jobPostQuery.isError || !jobPostQuery.data}
				/>
			);
		}

		const sourcePost = jobPostQuery.data;

		return (
			<CreateWizardForm
				jobDescriptionId={sourcePost.jobDescriptionId}
				seed={fromJobPost(sourcePost)}
				source={{
					kind: "jobPost",
					label: `${sourcePost.title} (${sourcePost.languageCode})`,
				}}
				title={`Copy job post: ${sourcePost.title}`}
				description="Translate the content into another language. Nothing is translated automatically."
				onCancel={() =>
					navigate({
						to: "/app/jobs/$id",
						params: { id: sourcePost.id },
						search: { search: undefined, status: undefined },
					})
				}
			/>
		);
	}

	if (jobDescriptionQuery.isLoading || jobDescriptionQuery.isError || !jobDescriptionQuery.data) {
		return (
			<DetailsLoading
				id={jobDescriptionId ?? ""}
				isLoading={jobDescriptionQuery.isLoading}
				isError={jobDescriptionQuery.isError || !jobDescriptionQuery.data}
			/>
		);
	}

	const jobDescription = jobDescriptionQuery.data;

	return (
		<CreateWizardForm
			jobDescriptionId={jobDescription.id}
			seed={fromJobDescription(jobDescription)}
			source={{ kind: "jobDescription", label: jobDescription.title }}
			title={`Create job post: ${jobDescription.title}`}
			description={`${jobDescription.company.name} - the content starts as a copy of the job description and is yours to rewrite for candidates.`}
			onCancel={() =>
				navigate({
					to: "/app/job-descriptions/$id",
					params: { id: jobDescription.id },
					search: { search: undefined, status: undefined },
				})
			}
		/>
	);
}

function MissingSource() {
	return (
		<FormWizard>
			<FormWizard.Title
				module="Recruitment"
				title="Create job post"
				description="A job post always belongs to a position."
			/>

			<FormWizard.Body>
				<FormWizard.Content>
					<FormWizard.Section>
						<FormWizard.SectionHeader
							title="Pick a source first"
							description="Start a job post from a job description, or copy an existing post into another language."
						/>

						<Link to="/app/job-descriptions">Go to job descriptions</Link>
					</FormWizard.Section>
				</FormWizard.Content>
			</FormWizard.Body>
		</FormWizard>
	);
}

type CreateWizardFormProps = {
	jobDescriptionId: string;
	seed: JobPostFormValues;
	source: JobPostSource;
	title: string;
	description: string;
	onCancel: () => void;
};

function CreateWizardForm({
	jobDescriptionId,
	seed,
	source,
	title,
	description,
	onCancel,
}: CreateWizardFormProps) {
	const navigate = Route.useNavigate();

	const [currentStep, setCurrentStep] = useState(0);

	const { mutation } = useCreateJobPost({
		onSuccess: () => {
			toast.success("Job post created");
		},
	});

	const form = useAppForm({
		defaultValues: seed,
		validators: {
			onChange: jobPostSchema,
		},

		onSubmit: async ({ value }) => {
			let created: Awaited<ReturnType<typeof mutation.mutateAsync>>;

			try {
				created = await mutation.mutateAsync({
					request: {
						jobDescriptionId,
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
						recruiterId: value.recruiterId,
					},
				});
			} catch (error) {
				const details = error as BadRequestDetails;

				toast.error(details?.title ?? "Unable to create the job post");

				return;
			}

			navigate({
				to: "/app/jobs/$id",
				params: { id: created.jobPostId },
				search: { search: undefined, status: undefined },
			});
		},
	});

	const isSubmitting = mutation.isPending;

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
			<FormWizard.Title module="Recruitment" title={title} description={description} />

			<FormWizard.Header steps={jobPostSteps} currentStep={currentStep} />

			<FormWizard.Body>
				<FormWizard.Content>
					{currentStep === 0 && <PostStep form={form} source={source} />}

					{currentStep === 1 && <ContentStep form={form} />}

					{currentStep === 2 && <RequirementsStep form={form} />}

					{currentStep === 3 && <EmploymentStep form={form} isSubmitting={isSubmitting} />}

					{currentStep === 4 && <ReviewStep form={form} source={source} />}
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
							onCancel={onCancel}
							canSubmit={canSubmit}
							isSubmitting={isSubmitting}
							submitLabel="Create job post"
						/>
					)}
				</form.Subscribe>
			</FormWizard.Body>
		</FormWizard>
	);
}
