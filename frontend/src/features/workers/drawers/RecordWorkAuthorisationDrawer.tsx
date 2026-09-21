import { forwardRef, useImperativeHandle, useState } from "react";
import { toast } from "sonner";
import { z } from "zod";
import type { WorkAuthorisationKind, WorkerProjection } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import { toDateOnly } from "@/utlis/formatRecord";
import { useRecordWorkAuthorisation } from "../pages/hooks";
import { workAuthorisationKinds } from "../types";
import type { RecordWorkAuthorisationFormCommand } from "./WorkerFormCommand";

interface RecordWorkAuthorisationDrawerProps {
	onSuccess: () => void;
}

const authorisationSchema = z
	.object({
		kind: z.string().min(1, "Pick the kind of permission"),
		country: z.string().trim().min(2, "Country is required"),
		number: z
			.string()
			.trim()
			.min(1, "Number is required")
			.max(50, "Number cannot exceed 50 characters."),
		validFrom: z.string().min(1, "Valid from is required"),
		validUntil: z.string().min(1, "Valid until is required"),
		documentId: z.string(),
		note: z.string().trim().max(500, "The note cannot exceed 500 characters."),
	})
	.superRefine((value, context) => {
		if (value.validUntil && value.validFrom && value.validUntil < value.validFrom) {
			context.addIssue({
				code: "custom",
				path: ["validUntil"],
				message: "The permission cannot expire before it starts.",
			});
		}
	});

const FormContent: React.FC<{
	worker: WorkerProjection;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ worker, onSuccess, handleClose }) => {
	const { mutation, waiting } = useRecordWorkAuthorisation({
		onSuccess: () => {
			mutation.reset();
			toast.success("Permission recorded");
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: {
			kind: "" as string,
			country: worker.address?.countryCode ?? "PL",
			number: "",
			validFrom: "",
			validUntil: "",
			documentId: "",
			note: "",
		},

		validators: { onChange: authorisationSchema },

		onSubmit: async ({ value }) => {
			mutation.mutate({
				workerId: worker.id ?? "",
				request: {
					kind: value.kind as WorkAuthorisationKind,
					country: value.country.toUpperCase(),
					number: value.number.trim(),
					validFrom: toDateOnly(value.validFrom),
					validUntil: toDateOnly(value.validUntil),
					documentId: value.documentId || null,
					note: value.note.trim() || null,
				},
			});
		},
	});

	/*
	 * Proof is picked from this person's own documents, never typed: the backend refuses a document
	 * id that is not on their file, and a select cannot produce one that is not.
	 */
	const documentOptions = Object.fromEntries(
		(worker.documents ?? []).map((document) => [document.documentId, document.fileName]),
	) as Record<string, string>;

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title="Record permission to work"
				onClose={handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<form.AppField name="kind">
							{(field) => (
								<field.FormSelectEnum
									label="Kind"
									options={workAuthorisationKinds}
									fieldValue={field.state.value as WorkAuthorisationKind}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<form.AppField name="country">
							{(field) => (
								<field.FormCountrySelect
									label="Country"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<form.AppField name="number">
							{(field) => (
								<field.FormInput
									label="Number"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<form.AppField name="validFrom">
							{(field) => (
								<field.FormDatePicker
									label="Valid from"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<form.AppField name="validUntil">
							{(field) => (
								<field.FormDatePicker
									label="Valid until"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						{Object.keys(documentOptions).length > 0 ? (
							<form.AppField name="documentId">
								{(field) => (
									<field.FormSelectEnum
										label="Proof document"
										options={documentOptions}
										fieldValue={field.state.value}
										errors={field.state.meta.errors}
										fieldName={field.name}
										handleChange={(value) => field.handleChange(value)}
										isSubmitting={mutation.isPending}
									/>
								)}
							</form.AppField>
						) : (
							<div className="form-hint">
								Attach the scan as a document first if you want to point at it from here.
							</div>
						)}

						<form.AppField name="note">
							{(field) => (
								<field.FormTextAreaInput
									label="Note"
									rows={3}
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={mutation.isPending}
								/>
							)}
						</form.AppField>

						<ApiError
							error={mutation.error as unknown as Parameters<typeof ApiError>[0]["error"]}
						/>
					</div>
				</FormDrawer.Content>

				<FormDrawer.Footer>
					<form.FormSaveChangesButton wait={waiting} isPending={mutation.isPending} />
				</FormDrawer.Footer>
			</FormDrawer>
		</form.AppForm>
	);
};

const RecordWorkAuthorisationDrawer = forwardRef<
	RecordWorkAuthorisationFormCommand,
	RecordWorkAuthorisationDrawerProps
>(({ onSuccess }, ref) => {
	const [worker, setWorker] = useState<WorkerProjection | null>(null);

	useImperativeHandle(ref, () => ({ record: setWorker }), []);

	if (!worker) return null;

	return <FormContent worker={worker} onSuccess={onSuccess} handleClose={() => setWorker(null)} />;
});

RecordWorkAuthorisationDrawer.displayName = "RecordWorkAuthorisationDrawer";

export { RecordWorkAuthorisationDrawer };
