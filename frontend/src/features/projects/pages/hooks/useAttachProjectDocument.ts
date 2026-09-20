import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { attachProjectDocument } from "@/api/endpoints";
import type { AttachProjectDocumentBody } from "@/api/models";
import { projectsKeys } from "@/api/query-keys";
import { useProjectionWait } from "@/hooks";

export const MAX_DOCUMENT_SIZE_BYTES = 25 * 1024 * 1024;

/**
 * Kept next to the `accept` attribute of the file input and checked again before the upload starts.
 * The backend checks the same things; this is here so that a 30 MB file fails in the moment the
 * user picks it instead of after a minute of uploading.
 */
export const ALLOWED_DOCUMENT_TYPES = [
	"application/pdf",
	"image/jpeg",
	"image/png",
	"application/msword",
	"application/vnd.openxmlformats-officedocument.wordprocessingml.document",
	"application/vnd.ms-excel",
	"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
];

export const DOCUMENT_ACCEPT = ".pdf,.jpg,.jpeg,.png,.doc,.docx,.xls,.xlsx";

export function formatFileSize(bytes: number) {
	if (bytes < 1024) return `${bytes} B`;
	if (bytes < 1024 * 1024) return `${Math.round(bytes / 1024)} KB`;

	return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

type AttachVariables = {
	projectId: string;
	body: AttachProjectDocumentBody;
};

/*
 * The one call in this feature that does not go through a server function.
 *
 * A progress bar needs the browser's own upload events, and a server function would only start
 * uploading to the API once the whole file had already reached the server - the user would watch
 * nothing happen twice. The request goes to `/api/...`, where the proxy route attaches the bearer
 * token, so the browser still never sees a credential. The generated `attachProjectDocument` builds
 * the multipart body; `onUploadProgress` rides along in the per-call axios options, which
 * `customInstance` spreads over the request config.
 */
export function useAttachProjectDocument({ onSuccess }: { onSuccess: () => void }) {
	const queryClient = useQueryClient();
	const { wait, waiting } = useProjectionWait();
	const [progress, setProgress] = useState(0);

	const mutation = useMutation({
		mutationFn: ({ projectId, body }: AttachVariables) =>
			attachProjectDocument(projectId, body, {
				onUploadProgress: (event) => {
					if (!event.total) return;

					setProgress(Math.round((event.loaded / event.total) * 100));
				},
			}),

		onSuccess: async () => {
			await wait();

			await queryClient.invalidateQueries({ queryKey: projectsKeys.all });

			setProgress(0);
			onSuccess();
		},

		onError: () => {
			setProgress(0);
		},
	});

	return { mutation, waiting, progress };
}
