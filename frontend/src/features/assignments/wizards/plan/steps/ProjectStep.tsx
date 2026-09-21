import { FolderKanban } from "lucide-react";
import { useState } from "react";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { ProjectsPicker } from "#/components/ui/pickers";
import { withForm } from "#/forms";
import { getCountryLabel } from "@/components/labels";
import { useGetProject } from "@/features/projects/pages/hooks";
import { formatPeriod } from "@/utlis/formatRecord";
import { emptyPlanAssignment } from "../schema";

/**
 * What follows from the project, said while it is being picked: the country decides which law
 * applies, the delivering entity is the company that will have to issue the A1, and the project's
 * own period is the frame this posting has to sit inside.
 */
function ProjectFacts({ projectId }: { projectId: string }) {
	const { data } = useGetProject(projectId);

	if (!data) {
		return null;
	}

	return (
		<div className="form-hint">
			{data.companyName} · work in {getCountryLabel(data.workCountry)} · posted by{" "}
			{data.deliveringEntity.legalName} · project runs {formatPeriod(data.startsOn, data.endsOn)}.
		</div>
	);
}

export const ProjectStep = withForm({
	defaultValues: emptyPlanAssignment,

	props: {
		isSubmitting: false,
	} as { isSubmitting: boolean },

	render: function Render({ form, isSubmitting }) {
		const [input, setInput] = useState("");

		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					icon={FolderKanban}
					title="Project"
					description="Where they are going. The client, the delivering company and the work country are read from the project and frozen onto this posting."
				/>

				<form.Subscribe selector={(state) => state.values.projectId}>
					{(projectId) => (
						<div className="form-field">
							<label className="form-label" htmlFor="projectId">
								Project
							</label>

							<ProjectsPicker
								disabled={isSubmitting}
								value={projectId}
								inputValue={input}
								onInputChange={setInput}
								onChange={(id) => form.setFieldValue("projectId", id ?? "")}
							/>

							{projectId ? <ProjectFacts projectId={projectId} /> : null}
						</div>
					)}
				</form.Subscribe>
			</FormWizard.Section>
		);
	},
});
