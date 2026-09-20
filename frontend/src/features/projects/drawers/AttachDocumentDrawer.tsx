import axios from "axios";
import { forwardRef, useImperativeHandle, useState } from "react";
import { toast } from "sonner";
import { z } from "zod";
import type { DocumentCategory } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { FileDropzone } from "#/components/ui/FileDropzone";
import { FormDrawer } from "#/components/ui/FormDrawer";
import { useAppForm } from "#/forms";
import {
	ALLOWED_DOCUMENT_TYPES,
	DOCUMENT_ACCEPT,
	formatFileSize,
	MAX_DOCUMENT_SIZE_BYTES,
	useAttachProjectDocument,
} from "../pages/hooks";
import { documentCategories } from "../types";
import { toDateOnly } from "../utils";
import type { AttachDocumentFormCommand } from "./ProjectFormCommand";

interface AttachDocumentDrawerProps {
	onSuccess: () => void;
}

const documentSchema = z.object({
	category: z.string().min(1, "Pick a category"),
	documentDate: z.string().min(1, "The document date is required"),
	validUntil: z.string(),
	note: z.string().trim().max(500, "The note cannot exceed 500 characters."),
});

/** The storage being down is not a broken upload form, and it should not read like one. */
function storageUnavailable(error: unknown) {
	return axios.isAxiosError(error) && error.response?.status === 503;
}

const FormContent: React.FC<{
	projectId: string;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ projectId, onSuccess, handleClose }) => {
	const [file, setFile] = useState<File | null>(null);
	const [fileError, setFileError] = useState<string | null>(null);

	const { mutation, waiting, progress } = useAttachProjectDocument({
		onSuccess: () => {
			mutation.reset();
			toast.success("Document attached");
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

		validators: {
			onChange: documentSchema,
		},

		onSubmit: async ({ value }) => {
			if (!file) {
				setFileError("Pick a file first");
				return;
			}

			mutation.mutate({
				projectId,
				body: {
					file,
					category: value.category as DocumentCategory,
					documentDate: toDateOnly(value.documentDate),
					...(value.validUntil ? { validUntil: toDateOnly(value.validUntil) } : {}),
					...(value.note.trim() ? { note: value.note.trim() } : {}),
				},
			});
		},
	});

	/*
	 * Checked here as well as on the backend. Not as a gate - the client is not one - but because
	 * finding out that a 30 MB file is too large after uploading it for a minute is a waste of
	 * somebody's afternoon.
	 */
	const pickFile = (picked: File | null) => {
		setFileError(null);

		if (!picked) {
			setFile(null);
			return;
		}

		if (picked.size > MAX_DOCUMENT_SIZE_BYTES) {
			setFile(null);
			setFileError(
				`The file is ${formatFileSize(picked.size)}; the limit is ${formatFileSize(
					MAX_DOCUMENT_SIZE_BYTES,
				)}.`,
			);
			return;
		}

		if (picked.type && !ALLOWED_DOCUMENT_TYPES.includes(picked.type)) {
			setFile(null);
			setFileError("That file type is not accepted. PDF, images, Word and Excel files are.");
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
									options={documentCategories}
									fieldValue={field.state.value as DocumentCategory}
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

						{storageUnavailable(mutation.error) ? (
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

const AttachDocumentDrawer = forwardRef<AttachDocumentFormCommand, AttachDocumentDrawerProps>(
	({ onSuccess }, ref) => {
		const [projectId, setProjectId] = useState<string | null>(null);

		useImperativeHandle(
			ref,
			() => ({
				attach: (id: string) => {
					setProjectId(id);
				},
			}),
			[],
		);

		if (!projectId) return null;

		return (
			<FormContent
				projectId={projectId}
				onSuccess={onSuccess}
				handleClose={() => setProjectId(null)}
			/>
		);
	},
);

AttachDocumentDrawer.displayName = "AttachDocumentDrawer";

export default AttachDocumentDrawer;
