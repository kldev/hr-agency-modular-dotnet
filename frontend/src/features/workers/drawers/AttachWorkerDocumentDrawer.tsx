import { forwardRef, useImperativeHandle, useState } from "react";
import { z } from "zod";
import type { WorkerDocumentCategory } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FileDropzone } from "#/components/ui/FileDropzone";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import { formatFileSize } from "@/utlis";
import {
	DOCUMENT_ACCEPT,
	isStorageUnavailable,
	MAX_DOCUMENT_SIZE_BYTES,
	rejectDocument,
} from "@/utlis/documentUpload";
import { toDateOnly } from "@/utlis/formatRecord";
import { useAttachWorkerDocument } from "../pages/hooks";
import { workerDocumentCategories } from "../types";
import type { AttachWorkerDocumentFormCommand } from "./WorkerFormCommand";

interface AttachWorkerDocumentDrawerProps {
	onSuccess: () => void;
}

const documentSchema = z.object({
	category: z.string().min(1, "Pick a category"),
	documentDate: z.string().min(1, "The document date is required"),
	validUntil: z.string(),
	note: z.string().trim().max(500, "The note cannot exceed 500 characters."),
});

const FormContent: React.FC<{
	workerId: string;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ workerId, onSuccess, handleClose }) => {
	const [file, setFile] = useState<File | null>(null);
	const [fileError, setFileError] = useState<string | null>(null);

	const { mutation, waiting, progress } = useAttachWorkerDocument({
		onSuccess: () => {
			mutation.reset();
			onSuccess();
			handleClose();
		},
	});

	const form = useAppForm({
		defaultValues: {
			category: "" as string,
			documentDate: "",
			validUntil: "",
			note: "",
		},

		validators: { onChange: documentSchema },

		onSubmit: async ({ value }) => {
			if (!file) {
				setFileError("Pick a file first");
				return;
			}

			mutation.mutate({
				workerId,
				body: {
					file,
					category: value.category as WorkerDocumentCategory,
					documentDate: toDateOnly(value.documentDate),
					...(value.validUntil ? { validUntil: toDateOnly(value.validUntil) } : {}),
					...(value.note.trim() ? { note: value.note.trim() } : {}),
				},
			});
		},
	});

	/* Checked here as well as on the backend - not as a gate, but so a 30 MB file fails now. */
	const pickFile = (picked: File | null) => {
		setFileError(null);

		if (!picked) {
			setFile(null);
			return;
		}

		const rejection = picked.type ? rejectDocument(picked) : null;

		if (rejection) {
			setFile(null);
			setFileError(rejection);
			return;
		}

		setFile(picked);
	};

	const isUploading = mutation.isPending;

	return (
		<form.AppForm>
			<FormDrawer
				open={true}
				title="Attach document"
				onClose={isUploading ? () => {} : handleClose}
				onSubmit={(event) => {
					event.preventDefault();
					void form.handleSubmit();
				}}
			>
				<FormDrawer.Content>
					<div className="drawer-form">
						<FileDropzone
							accept={DOCUMENT_ACCEPT}
							hint={`PDF, image, Word and Excel files, up to ${formatFileSize(
								MAX_DOCUMENT_SIZE_BYTES,
							)}.`}
							file={file}
							error={fileError}
							progress={isUploading ? progress : null}
							disabled={isUploading}
							onSelect={pickFile}
						/>

						<form.AppField name="category">
							{(field) => (
								<field.FormSelectEnum
									label="Category"
									options={workerDocumentCategories}
									fieldValue={field.state.value as WorkerDocumentCategory}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={isUploading}
								/>
							)}
						</form.AppField>

						<form.AppField name="documentDate">
							{(field) => (
								<field.FormDatePicker
									label="Document date"
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={isUploading}
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
									isSubmitting={isUploading}
								/>
							)}
						</form.AppField>

						<form.AppField name="note">
							{(field) => (
								<field.FormTextAreaInput
									label="Note"
									rows={3}
									fieldValue={field.state.value}
									errors={field.state.meta.errors}
									fieldName={field.name}
									handleChange={(value) => field.handleChange(value)}
									isSubmitting={isUploading}
								/>
							)}
						</form.AppField>

						{isStorageUnavailable(mutation.error) ? (
							<div className="form-error" role="alert">
								Document storage is unavailable, so the file could not be stored. Everything else on
								this page works; only documents need the file service running.
							</div>
						) : (
							<ApiError
								error={mutation.error as unknown as Parameters<typeof ApiError>[0]["error"]}
							/>
						)}
					</div>
				</FormDrawer.Content>

				<FormDrawer.Footer>
					<form.FormSaveChangesButton wait={waiting} isPending={isUploading} />
				</FormDrawer.Footer>
			</FormDrawer>
		</form.AppForm>
	);
};

const AttachWorkerDocumentDrawer = forwardRef<
	AttachWorkerDocumentFormCommand,
	AttachWorkerDocumentDrawerProps
>(({ onSuccess }, ref) => {
	const [workerId, setWorkerId] = useState<string | null>(null);

	useImperativeHandle(ref, () => ({ attach: setWorkerId }), []);

	if (!workerId) return null;

	return (
		<FormContent workerId={workerId} onSuccess={onSuccess} handleClose={() => setWorkerId(null)} />
	);
});

AttachWorkerDocumentDrawer.displayName = "AttachWorkerDocumentDrawer";

export { AttachWorkerDocumentDrawer };
