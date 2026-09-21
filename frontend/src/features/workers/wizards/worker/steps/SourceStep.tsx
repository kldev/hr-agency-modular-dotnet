import { FileUser } from "lucide-react";
import { useState } from "react";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import { ApplicationsPicker } from "#/components/ui/pickers";
import { withForm } from "#/forms";
import { emptyWorker } from "../schema";

/**
 * Where this person came from. Optional, because plenty of people are taken on without ever having
 * applied through us — but when they did apply, we already hold their name, e-mail and phone, and
 * making somebody retype all three is how the two records end up disagreeing.
 *
 * The command takes `sourceCandidateId`; the application is what a human recognises, and it carries
 * the candidate id.
 */
export const SourceStep = withForm({
	defaultValues: emptyWorker,

	props: {
		isSubmitting: false,
	} as { isSubmitting: boolean },

	render: function Render({ form, isSubmitting }) {
		const [input, setInput] = useState("");

		return (
			<FormWizard.Section>
				<FormWizard.SectionHeader
					icon={FileUser}
					title="Source"
					description="Which application this person came from, if any. Skip it for somebody taken on directly."
				/>

				<form.Subscribe selector={(state) => state.values.sourceApplicationId}>
					{(sourceApplicationId) => (
						<div className="form-field">
							<label className="form-label" htmlFor="sourceApplicationId">
								Application
							</label>

							<ApplicationsPicker
								disabled={isSubmitting}
								value={sourceApplicationId}
								inputValue={input}
								onInputChange={setInput}
								onChange={(id, application) => {
									form.setFieldValue("sourceApplicationId", id ?? "");
									form.setFieldValue("sourceCandidateId", application?.candidateId ?? "");

									if (!application) {
										return;
									}

									/*
									 * Prefilled rather than locked: what the candidate typed into a job board
									 * is not always how their passport spells it, and the passport wins.
									 */
									form.setFieldValue("firstName", application.applicantFirstName ?? "");
									form.setFieldValue("lastName", application.applicantLastName ?? "");
									form.setFieldValue("email", application.applicantEmail ?? "");
									form.setFieldValue("phoneNumber", application.applicantPhone ?? "");
								}}
							/>

							<div className="form-hint">
								Picking one fills in the name, e-mail and phone from the application. All of it
								stays editable on the next steps — a job board form is not an identity document.
							</div>
						</div>
					)}
				</form.Subscribe>
			</FormWizard.Section>
		);
	},
});
