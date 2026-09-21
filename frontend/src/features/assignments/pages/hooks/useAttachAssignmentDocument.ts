import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { attachAssignmentDocument } from "@/api/endpoints";
import type { AttachAssignmentDocumentBody } from "@/api/models";
import { assignmentsKeys } from "@/api/query-keys";
import { useProjectionWait } from "@/hooks";

type AttachVariables = {
	assignmentId: string;
	body: AttachAssignmentDocumentBody;
};

/*
 * Straight to the API rather than through a server function, as everywhere a file is uploaded here:
 * the progress bar needs the browser's own upload events, and a server function would only start
 * uploading once the whole file had already reached the server. The request goes to `/api/...`,
 * where the proxy attaches the bearer token, so the browser still never sees a credential.
 */
export function useAttachAssignmentDocument({ onSuccess }: { onSuccess: () => void }) {
	const queryClient = useQueryClient();
	const { wait, waiting } = useProjectionWait();
	const [progress, setProgress] = useState(0);

	const mutation = useMutation({
		mutationFn: ({ assignmentId, body }: AttachVariables) =>
			attachAssignmentDocument(assignmentId, body, {
				onUploadProgress: (event) => {
					if (!event.total) return;

					setProgress(Math.round((event.loaded / event.total) * 100));
				},
			}),

		onSuccess: async () => {
			await wait();

			await queryClient.invalidateQueries({ queryKey: assignmentsKeys.all });

			setProgress(0);
			onSuccess();
		},

		onError: () => {
			setProgress(0);
		},
	});

	return { mutation, waiting, progress };
}
