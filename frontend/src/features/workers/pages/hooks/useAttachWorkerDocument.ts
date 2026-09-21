import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { attachWorkerDocument } from "@/api/endpoints";
import type { AttachWorkerDocumentBody } from "@/api/models";
import { workersKeys } from "@/api/query-keys";
import { useProjectionWait } from "@/hooks";

type AttachVariables = {
	workerId: string;
	body: AttachWorkerDocumentBody;
};

/*
 * The one call in this feature that does not go through a server function, for the same reason as
 * its twin in projects: a progress bar needs the browser's own upload events, and a server function
 * would only start uploading to the API once the whole file had already reached the server. The
 * request goes to `/api/...`, where the proxy attaches the bearer token, so the browser still never
 * sees a credential.
 */
export function useAttachWorkerDocument({ onSuccess }: { onSuccess: () => void }) {
	const queryClient = useQueryClient();
	const { wait, waiting } = useProjectionWait();
	const [progress, setProgress] = useState(0);

	const mutation = useMutation({
		mutationFn: ({ workerId, body }: AttachVariables) =>
			attachWorkerDocument(workerId, body, {
				onUploadProgress: (event) => {
					if (!event.total) return;

					setProgress(Math.round((event.loaded / event.total) * 100));
				},
			}),

		onSuccess: async () => {
			await wait();

			await queryClient.invalidateQueries({ queryKey: workersKeys.all });

			setProgress(0);
			onSuccess();
		},

		onError: () => {
			setProgress(0);
		},
	});

	return { mutation, waiting, progress };
}
