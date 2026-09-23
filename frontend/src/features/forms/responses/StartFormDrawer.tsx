import { ClipboardList } from "lucide-react";
import { forwardRef, useImperativeHandle, useState } from "react";
import { z } from "zod";
import type { BadRequestDetails } from "#/api/models";
import { EmptyState } from "#/components/ui";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import { useGetWorkerAvailableForms, useStartFormResponse } from "../hooks";
import { formKinds } from "../types";

export interface StartFormCommand {
	start: (workerId: string) => void;
}

interface StartFormDrawerProps {
	/** The response that was opened - a new one, or for a one-per-person form the one already there. */
	onStarted: (responseId: string) => void;
}

const startSchema = z.object({ formId: z.string().min(1, "Pick a form") });

function FormContent({
	workerId,
	onStarted,
	handleClose,
}: {
	workerId: string;
	onStarted: (responseId: string) => void;
	handleClose: () => void;
}) {
	const available = useGetWorkerAvailableForms(workerId, true);

	const { mutation, waiting } = useStartFormResponse({
		onSuccess: (started) => {
			handleClose();
			onStarted(started.responseId);
		},
	});

	const form = useAppForm({
		defaultValues: { formId: "" },
		validators: { onChange: startSchema },
		onSubmit: ({ value }) =>
			mutation.mutate({ formId: value.formId, subjectKind: "worker", subjectId: workerId }),
	});

	const forms = available.data ?? [];
	const options = Object.fromEntries(forms.map((candidate) => [candidate.id, candidate.name]));
	const descriptions = Object.fromEntries(
		forms.map((candidate) => [
			candidate.id,
			`${formKinds[candidate.kind]} · version ${String(candidate.publishedVersion)}${candidate.description ? ` · ${candidate.description}` : ""}`,
		]),
	);

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title="Fill in a form"
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						{available.isFetched && forms.length === 0 ? (
							<EmptyState
								title="Nothing to fill in"
								description="Every published form this person can have is already started, or none is published yet."
							>
								<ClipboardList size={24} />
							</EmptyState>
						) : (
							<form.AppField name="formId">
								{(field) => (
									<field.FormChoiceGroup
										label="Form"
										columns={1}
										options={options}
										descriptions={descriptions}
										fieldValue={field.state.value}
										errors={field.state.meta.errors}
										fieldName={field.name}
										handleChange={(value) => field.handleChange(value)}
										isSubmitting={mutation.isPending}
									/>
								)}
							</form.AppField>
						)}

						<ApiError error={mutation.error as unknown as BadRequestDetails | null} />
					</div>
				</FormDrawer.Content>

				{forms.length > 0 ? (
					<FormDrawer.Footer>
						<form.FormSaveChangesButton wait={waiting} isPending={mutation.isPending} />
					</FormDrawer.Footer>
				) : null}
			</FormDrawer>
		</form.AppForm>
	);
}

/** Picking a form is one decision, so it is a drawer; filling it in is the dialog that follows. */
const StartFormDrawer = forwardRef<StartFormCommand, StartFormDrawerProps>(({ onStarted }, ref) => {
	const [workerId, setWorkerId] = useState<string | null>(null);

	useImperativeHandle(ref, () => ({ start: (id) => setWorkerId(id) }), []);

	if (!workerId) return null;

	return (
		<FormContent workerId={workerId} onStarted={onStarted} handleClose={() => setWorkerId(null)} />
	);
});

StartFormDrawer.displayName = "StartFormDrawer";

export { StartFormDrawer };
