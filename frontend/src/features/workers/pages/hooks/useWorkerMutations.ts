import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getFnOptions } from "#/server/axios";
import {
	changeWorkerStatus,
	recordWorkAuthorisation,
	registerWorker,
	removeWorkAuthorisation,
	removeWorkerDocument,
	updateWorker,
	updateWorkerDocument,
} from "@/api/endpoints";
import type {
	ChangeWorkerStatusRequest,
	RecordWorkAuthorisationRequest,
	UpdateWorkerDocumentRequest,
	WorkerRequest,
} from "@/api/models";
import { assignmentsKeys, workersKeys } from "@/api/query-keys";
import { useProjectionWait } from "@/hooks";

type MutationOptions = {
	/** Optional: most callers have nothing to add once the screen has already changed. */
	onSuccess?: () => void;
};

const registerWorkerServerFn = createServerFn({ method: "POST" })
	.validator((input: { req: WorkerRequest }) => input)
	.handler(async ({ data }) => {
		return registerWorker(data.req, await getFnOptions());
	});

const updateWorkerServerFn = createServerFn({ method: "POST" })
	.validator((input: { id: string; req: WorkerRequest }) => input)
	.handler(async ({ data }) => {
		return updateWorker(data.id, data.req, await getFnOptions());
	});

const changeWorkerStatusServerFn = createServerFn({ method: "POST" })
	.validator((input: { id: string; req: ChangeWorkerStatusRequest }) => input)
	.handler(async ({ data }) => {
		return changeWorkerStatus(data.id, data.req, await getFnOptions());
	});

const recordWorkAuthorisationServerFn = createServerFn({ method: "POST" })
	.validator((input: { id: string; req: RecordWorkAuthorisationRequest }) => input)
	.handler(async ({ data }) => {
		return recordWorkAuthorisation(data.id, data.req, await getFnOptions());
	});

const removeWorkAuthorisationServerFn = createServerFn({ method: "POST" })
	.validator((input: { id: string; authorisationId: string }) => input)
	.handler(async ({ data }) => {
		return removeWorkAuthorisation(data.id, data.authorisationId, await getFnOptions());
	});

const updateWorkerDocumentServerFn = createServerFn({ method: "POST" })
	.validator((input: { id: string; documentId: string; req: UpdateWorkerDocumentRequest }) => input)
	.handler(async ({ data }) => {
		return updateWorkerDocument(data.id, data.documentId, data.req, await getFnOptions());
	});

const removeWorkerDocumentServerFn = createServerFn({ method: "POST" })
	.validator((input: { id: string; documentId: string }) => input)
	.handler(async ({ data }) => {
		return removeWorkerDocument(data.id, data.documentId, await getFnOptions());
	});

/*
 * Every worker mutation waits for the projection, then invalidates **both** keys.
 *
 * Both, because `WorkerProjection` is a MultiStreamProjection fed by the person's own stream and by
 * every one of their assignments: changing a status moves rows on the assignment list too, and
 * ending an assignment moves `currentWorkCountry` on the worker row. Invalidating one of the two
 * leaves the other showing yesterday.
 */
export function useWorkerMutation<TVariables, TResult>(
	mutationFn: (variables: TVariables) => Promise<TResult>,
	{ onSuccess }: MutationOptions,
) {
	const queryClient = useQueryClient();
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn,

		onSuccess: async () => {
			await wait();

			await queryClient.invalidateQueries({ queryKey: workersKeys.all });
			await queryClient.invalidateQueries({ queryKey: assignmentsKeys.all });

			onSuccess?.();
		},
	});

	return { mutation, waiting };
}

export function useRegisterWorker(options: MutationOptions) {
	return useWorkerMutation(
		({ request }: { request: WorkerRequest }) => registerWorkerServerFn({ data: { req: request } }),
		options,
	);
}

export function useUpdateWorker(options: MutationOptions) {
	return useWorkerMutation(
		({ workerId, request }: { workerId: string; request: WorkerRequest }) =>
			updateWorkerServerFn({ data: { id: workerId, req: request } }),
		options,
	);
}

export function useChangeWorkerStatus(options: MutationOptions) {
	return useWorkerMutation(
		({ workerId, request }: { workerId: string; request: ChangeWorkerStatusRequest }) =>
			changeWorkerStatusServerFn({ data: { id: workerId, req: request } }),
		options,
	);
}

export function useRecordWorkAuthorisation(options: MutationOptions) {
	return useWorkerMutation(
		({ workerId, request }: { workerId: string; request: RecordWorkAuthorisationRequest }) =>
			recordWorkAuthorisationServerFn({ data: { id: workerId, req: request } }),
		options,
	);
}

export function useRemoveWorkAuthorisation(options: MutationOptions) {
	return useWorkerMutation(
		({ workerId, authorisationId }: { workerId: string; authorisationId: string }) =>
			removeWorkAuthorisationServerFn({ data: { id: workerId, authorisationId } }),
		options,
	);
}

export function useUpdateWorkerDocument(options: MutationOptions) {
	return useWorkerMutation(
		({
			workerId,
			documentId,
			request,
		}: {
			workerId: string;
			documentId: string;
			request: UpdateWorkerDocumentRequest;
		}) => updateWorkerDocumentServerFn({ data: { id: workerId, documentId, req: request } }),
		options,
	);
}

export function useRemoveWorkerDocument(options: MutationOptions) {
	return useWorkerMutation(
		({ workerId, documentId }: { workerId: string; documentId: string }) =>
			removeWorkerDocumentServerFn({ data: { id: workerId, documentId } }),
		options,
	);
}
