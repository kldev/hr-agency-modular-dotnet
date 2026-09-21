import { HardHat } from "lucide-react";
import { useState } from "react";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { WorkersPicker } from "#/components/ui/pickers";
import { withForm } from "#/forms";
import { workerStatuses } from "@/features/workers/types";
import { useWorkerSuggestion } from "@/hooks";
import { emptyPlanAssignment } from "../schema";

/** Mirrors `WorkerStatusChangePolicy.MayBePlanned`: only somebody who has left cannot be planned. */
function PipelineNote({ workerId }: { workerId: string }) {
	const { data } = useWorkerSuggestion(workerId);

	if (!data) {
		return null;
	}

	if (data.status === "Terminated") {
		return (
			<div className="form-error" role="alert">
				{data.fullName} has left. A file that is closed cannot be put on next month's crew.
			</div>
		);
	}

	return (
		<div className="form-hint">
			{data.fullName} is {workerStatuses[data.status]}. Planning ahead while the paperwork runs is
			normal — they just cannot start work until they are through the pipeline.
		</div>
	);
}

export const WorkerStep = withForm({
	defaultValues: emptyPlanAssignment,

	props: {
		isSubmitting: false,
	} as { isSubmitting: boolean },

	render: function Render({ form, isSubmitting }) {
		const [input, setInput] = useState("");

		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					icon={HardHat}
					title="Worker"
					description="Who is being posted. One person holds one position at a time, so an overlapping period comes back refused."
				/>

				<form.Subscribe selector={(state) => state.values.workerId}>
					{(workerId) => (
						<div className="form-field">
							<label className="form-label" htmlFor="workerId">
								Worker
							</label>

							<WorkersPicker
								disabled={isSubmitting}
								value={workerId}
								inputValue={input}
								onInputChange={setInput}
								onChange={(id) => form.setFieldValue("workerId", id ?? "")}
							/>

							{workerId ? <PipelineNote workerId={workerId} /> : null}
						</div>
					)}
				</form.Subscribe>
			</FormWizard.Section>
		);
	},
});
