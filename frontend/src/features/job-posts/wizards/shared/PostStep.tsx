import { FormWizard } from "#/components/form-wizard/FormWizard";
import { withForm } from "#/forms";
import type { JobPostFormValues } from "./schema";

/**
 * What the post was seeded from.
 *
 * Always read-only: `jobDescriptionId` is set once, at creation, and `PUT /api/recruitment/
 * job-posting/{id}` does not accept it - an editable picker would promise something no save can
 * deliver. It is shown because the wizard starts from a *copy* of that source, and the user has to
 * know which one.
 */
export type JobPostSource = {
	kind: "jobDescription" | "jobPost";
	label: string;
};

/** Set in edit mode only - the recruiter is changed through its own action, not through `PUT`. */
export type PostAssignment = {
	recruiterName: string;
};

const sourceLabels: Record<JobPostSource["kind"], string> = {
	jobDescription: "Job description",
	jobPost: "Source job post",
};

export const PostStep = withForm({
	defaultValues: {} as JobPostFormValues,

	props: {
		source: undefined,
		assignment: undefined,
	} as { source?: JobPostSource; assignment?: PostAssignment },

	render: function Render({ form, source, assignment }) {
		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					title="Post basics"
					description="A job post is the candidate-facing copy of a position - it may word things differently than the internal job description."
				/>

				{source ? (
					<FormWizard.Field
						label={sourceLabels[source.kind]}
						hint={
							assignment
								? "The source cannot be changed after the post is created, and its later edits never reach this post."
								: "The content below is a copy. Later changes to the source will not update this post."
						}
					>
						<p className="form-wizard__readonly-value">{source.label}</p>
					</FormWizard.Field>
				) : null}

				<FormWizard.Field
					label="Recruiter"
					required
					hint={assignment ? 'Use the "Change recruiter" action to reassign the post.' : undefined}
				>
					{assignment ? (
						<p className="form-wizard__readonly-value">{assignment.recruiterName}</p>
					) : (
						<form.AppField name="recruiterId">
							{(field) => (
								<field.FormUserPicker
									label=""
									fieldName={field.name}
									fieldValue={{ id: field.state.value }}
									errors={field.state.meta.errors}
									handleChange={(value) => field.handleChange(value?.id ?? "")}
								/>
							)}
						</form.AppField>
					)}
				</FormWizard.Field>

				<FormWizard.Field label="Language" required>
					<form.AppField name="languageCode">
						{(field) => (
							<field.FormLanguageSelect
								label=""
								fieldName={field.name}
								fieldValue={field.state.value}
								errors={field.state.meta.errors}
								handleChange={(value) => field.handleChange(value)}
								isSubmitting={false}
								hint="Pick the language you are translating into."
							/>
						)}
					</form.AppField>
				</FormWizard.Field>

				<FormWizard.Field label="Job title" required hint="Use a clear, market-facing title.">
					<form.AppField name="title">
						{(field) => (
							<field.FormInput
								label=""
								fieldValue={field.state.value}
								errors={field.state.meta.errors}
								fieldName={field.name}
								handleChange={(val) => field.handleChange(val)}
							/>
						)}
					</form.AppField>
				</FormWizard.Field>

				<FormWizard.Field
					label="Short summary"
					hint="Optional. Keep it concise and focused on the value of the role."
				>
					<form.AppField name="summary">
						{(field) => (
							<field.FormTextAreaInput
								label=""
								rows={3}
								fieldValue={field.state.value}
								errors={field.state.meta.errors}
								fieldName={field.name}
								handleChange={(val) => field.handleChange(val)}
							/>
						)}
					</form.AppField>
				</FormWizard.Field>
			</FormWizard.Section>
		);
	},
});
