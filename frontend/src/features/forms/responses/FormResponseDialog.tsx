import { forwardRef, useImperativeHandle, useState } from "react";
import { toast } from "sonner";
import type { BadRequestDetails, FormResponseView } from "#/api/models";
import { FormWizard } from "#/components/form-wizard/FormWizard";
import {
	Button,
	ConfirmDialog,
	DetailsLoading,
	Dialog,
	FormResponseStatusBadge,
	Textarea,
	useUnsavedChangesGuard,
} from "#/components/ui";
import { ApiError } from "#/components/ui/ApiError";
import { useAuthStore } from "#/stores/authStore";
import { formatDateTime } from "#/utlis/dateUtils";
import {
	useCorrectFormResponse,
	useGetFormResponse,
	useSaveFormResponseDraft,
	useSubmitFormResponse,
} from "../hooks";
import { DynamicForm } from "../renderer/DynamicForm";
import { DynamicReview } from "../renderer/DynamicReview";
import { fieldErrorsOf } from "../schema/fieldErrors";
import { isFormsDesigner } from "../types";

export interface FormResponseDialogCommand {
	open: (responseId: string) => void;
}

interface FormResponseDialogProps {
	onChanged?: () => void;
}

type Mode = "fill" | "view" | "correct";

const historyLabels = {
	Started: "Started",
	DraftSaved: "Saved",
	Submitted: "Submitted",
	Corrected: "Corrected",
} as const;

/**
 * One response, in the one dialog that fills it in, shows it once submitted and corrects it. A
 * draft opens on the form, a submitted response opens on its answers - it is a document now, and
 * changing it is a deliberate second step with a reason, for the people `FormsDesignPolicy` allows.
 *
 * Always the response's own version: a response to v1 is filled, read and corrected against v1
 * whatever version is out today.
 */
function ResponseContent({
	response,
	onClose,
	onDirtyChange,
	onChanged,
}: {
	response: FormResponseView;
	onClose: () => void;
	onDirtyChange: (dirty: boolean) => void;
	onChanged?: () => void;
}) {
	const role = useAuthStore((state) => state.user?.role);
	const [mode, setMode] = useState<Mode>(response.status === "Draft" ? "fill" : "view");
	const [reason, setReason] = useState("");

	const save = useSaveFormResponseDraft();
	const submit = useSubmitFormResponse({
		onSuccess: () => {
			toast.success("Response submitted");
			onChanged?.();
			onClose();
		},
	});
	const correct = useCorrectFormResponse({
		onSuccess: () => {
			toast.success("Response corrected");
			onChanged?.();
			onClose();
		},
	});

	const pages = response.version.pages;

	if (mode === "view") {
		return (
			<FormWizard className="form-wizard--in-dialog">
				<FormWizard.Body>
					<FormWizard.Content>
						<FormWizard.Section>
							<div className="mb-5 flex flex-wrap items-center gap-3 text-sm text-(--color-text-secondary)">
								<FormResponseStatusBadge status={response.status} />
								<span>Version {String(response.formVersion)}</span>
								{Number(response.revision) > 0 ? (
									<span>Revision {String(response.revision)}</span>
								) : null}
							</div>

							<DynamicReview pages={pages} answers={response.answers} />

							<div className="form-wizard__summary-section">
								<h3 className="form-wizard__summary-title">History</h3>
								<ul className="flex flex-col gap-1 text-sm">
									{response.history
										.filter((entry) => entry.kind !== "DraftSaved")
										.map((entry) => (
											<li key={`${entry.kind}-${entry.at}`}>
												{historyLabels[entry.kind]} {formatDateTime(entry.at)} by{" "}
												{entry.by.fullname}
												{entry.reason ? ` - ${entry.reason}` : ""}
											</li>
										))}
								</ul>
							</div>
						</FormWizard.Section>
					</FormWizard.Content>

					<footer className="form-wizard__footer">
						<div className="form-wizard__footer-left" />
						<div className="form-wizard__footer-right">
							<Button variant="ghost" onClick={onClose}>
								Close
							</Button>
							{isFormsDesigner(role) ? (
								<Button variant="primary" onClick={() => setMode("correct")}>
									Correct
								</Button>
							) : null}
						</div>
					</footer>
				</FormWizard.Body>
			</FormWizard>
		);
	}

	const correcting = mode === "correct";
	const active = correcting ? correct.mutation : submit.mutation;

	return (
		<DynamicForm
			pages={pages}
			answers={response.answers}
			submitLabel={correcting ? "Save correction" : "Submit"}
			isSubmitting={active.isPending}
			serverFieldErrors={fieldErrorsOf(active.error ?? save.mutation.error)}
			onDirtyChange={onDirtyChange}
			onCancel={correcting ? () => setMode("view") : onClose}
			/* Each page left is saved, so a long document can be closed half way and picked up again. */
			onPageLeft={
				correcting
					? undefined
					: (answers) => save.mutation.mutate({ responseId: response.id, req: { answers } })
			}
			onSubmit={(answers) =>
				correcting
					? correct.mutation.mutate({ responseId: response.id, req: { answers, reason } })
					: submit.mutation.mutate({ responseId: response.id, req: { answers } })
			}
			reviewAddon={
				correcting ? (
					<FormWizard.Field label="Why is it being corrected?" required htmlFor="correction-reason">
						<Textarea
							id="correction-reason"
							rows={3}
							value={reason}
							onChange={(event) => setReason(event.target.value)}
						/>
					</FormWizard.Field>
				) : null
			}
			footer={
				<ApiError
					error={(active.error ?? save.mutation.error) as unknown as BadRequestDetails | null}
				/>
			}
		/>
	);
}

const FormResponseDialog = forwardRef<FormResponseDialogCommand, FormResponseDialogProps>(
	({ onChanged }, ref) => {
		const [responseId, setResponseId] = useState<string | null>(null);
		const query = useGetFormResponse(responseId);

		const { setDirty, confirming, requestClose, discard, keepEditing } = useUnsavedChangesGuard(
			() => setResponseId(null),
		);

		useImperativeHandle(
			ref,
			() => ({
				open: (id: string) => {
					setDirty(false);
					setResponseId(id);
				},
			}),
			[setDirty],
		);

		if (!responseId) {
			return null;
		}

		const response = query.data;

		return (
			<>
				<Dialog
					open={true}
					title={response?.version.formName ?? "Form"}
					maxWidth="wide"
					onClose={requestClose}
				>
					{/* Not unmounted on a refetch: saving a page refreshes the response, and the wizard must stay
					    on the page it is on. */}
					{response ? (
						<ResponseContent
							key={`${response.id}-${response.revision}-${response.status}`}
							response={response}
							onClose={() => setResponseId(null)}
							onDirtyChange={setDirty}
							onChanged={onChanged}
						/>
					) : (
						<DetailsLoading id={responseId} isLoading={query.isLoading} isError={query.isError} />
					)}
				</Dialog>

				<ConfirmDialog
					open={confirming}
					title="Discard your changes?"
					description="What was typed since the last page will be lost."
					confirmLabel="Discard"
					onConfirm={discard}
					onClose={keepEditing}
				/>
			</>
		);
	},
);

FormResponseDialog.displayName = "FormResponseDialog";

export { FormResponseDialog };
