import { BriefcaseBusiness } from "lucide-react";
import { useState } from "react";
import type { EngagementType } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { ProjectsPicker } from "#/components/ui/pickers";
import { withForm } from "#/forms";
import { engagementTypes } from "@/features/compliance";
import { emptyPosition } from "../schema";

export const RoleStep = withForm({
	defaultValues: emptyPosition,

	props: {
		isSubmitting: false,
		knownProject: false,
	} as { isSubmitting: boolean; knownProject: boolean },

	render: function Render({ form, isSubmitting, knownProject }) {
		const [projectInput, setProjectInput] = useState("");

		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					icon={BriefcaseBusiness}
					title="Role"
					description="A role exists inside one project and dies with it. One client is one project, and inside it painters and bricklayers are two roles with two contracts."
				/>

				{knownProject ? null : (
					<form.Subscribe selector={(state) => state.values.projectId}>
						{(projectId) => (
							<div className="form-field">
								<label className="form-label" htmlFor="projectId">
									Project
								</label>

								<ProjectsPicker
									disabled={isSubmitting}
									value={projectId}
									inputValue={projectInput}
									onInputChange={setProjectInput}
									onChange={(id) => form.setFieldValue("projectId", id ?? "")}
								/>
							</div>
						)}
					</form.Subscribe>
				)}

				<div className="form-wizard__grid">
					<form.AppField name="name">
						{(field) => (
							<field.FormInput
								label="Name"
								fieldName={field.name}
								fieldValue={field.state.value}
								errors={field.state.meta.errors}
								handleChange={(value) => field.handleChange(value)}
								isSubmitting={isSubmitting}
							/>
						)}
					</form.AppField>

					<form.AppField name="contractName">
						{(field) => (
							<field.FormInput
								label="Name on the contract"
								fieldName={field.name}
								fieldValue={field.state.value}
								errors={field.state.meta.errors}
								handleChange={(value) => field.handleChange(value)}
								isSubmitting={isSubmitting}
							/>
						)}
					</form.AppField>
				</div>

				{/*
				 * Two names, because both are real. The internal one tells apart roles that are called
				 * the same thing on paper - "Painter PL contract" and "Painter Belgium" differ by the
				 * contract, the rate and the address, and both documents say "Painter".
				 */}
				<div className="form-hint">
					The internal name is what tells two similar roles apart. Leave the contract name empty to
					use the same wording on documents.
				</div>

				<form.AppField name="plannedHeadcount">
					{(field) => (
						<field.FormInput
							label="Target headcount"
							fieldName={field.name}
							fieldValue={field.state.value}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
							isSubmitting={isSubmitting}
						/>
					)}
				</form.AppField>

				<div className="form-hint">
					Target headcount is how many people this role needs. Left empty it reads as "nobody has
					said", which is a different answer from zero.
				</div>

				{/*
				 * A hint for the assignment wizard and nothing more: the engagement type is stated per
				 * person, because it is what keys their compliance, and one role can hold people posted
				 * abroad next to people employed locally.
				 */}
				<form.AppField name="defaultEngagementType">
					{(field) => (
						<field.FormSelectEnum<EngagementType>
							label="Usual engagement"
							options={engagementTypes}
							fieldName={field.name}
							fieldValue={field.state.value as EngagementType}
							errors={field.state.meta.errors}
							handleChange={(value) => field.handleChange(value)}
						/>
					)}
				</form.AppField>

				<div className="form-hint">
					The usual engagement is only a suggestion for the assignment wizard - each person's is
					still stated on their own posting, because that is what keys their compliance.
				</div>
			</FormWizard.Section>
		);
	},
});
