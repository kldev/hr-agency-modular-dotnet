import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getFnOptions } from "#/server/axios";
import { archivePosition, openPosition, restorePosition, updatePosition } from "@/api/endpoints";
import type { PositionRequest } from "@/api/models";
import { positionsKeys, projectsKeys, suggestionKeys } from "@/api/query-keys";
import { useProjectionWait } from "@/hooks";

type MutationOptions = {
	/** Optional: most callers have nothing to add once the screen has already changed. */
	onSuccess?: () => void;
};

const openPositionServerFn = createServerFn({ method: "POST" })
	.validator((input: { projectId: string; req: PositionRequest }) => input)
	.handler(async ({ data }) => {
		return openPosition(data.projectId, data.req, await getFnOptions());
	});

const updatePositionServerFn = createServerFn({ method: "POST" })
	.validator((input: { projectId: string; positionId: string; req: PositionRequest }) => input)
	.handler(async ({ data }) => {
		return updatePosition(data.projectId, data.positionId, data.req, await getFnOptions());
	});

const archivePositionServerFn = createServerFn({ method: "POST" })
	.validator((input: { projectId: string; positionId: string }) => input)
	.handler(async ({ data }) => {
		return archivePosition(data.projectId, data.positionId, await getFnOptions());
	});

const restorePositionServerFn = createServerFn({ method: "POST" })
	.validator((input: { projectId: string; positionId: string }) => input)
	.handler(async ({ data }) => {
		return restorePosition(data.projectId, data.positionId, await getFnOptions());
	});

/*
 * A role lives on the project's stream but is listed from its own projection, and both are fed by
 * the same async daemon - so every one of these waits first and then invalidates both keys. The
 * suggestions go too: the picker in the assignment wizard caches a role by id, and a renamed role
 * served from that cache would show the old name on the next posting planned.
 */
function usePositionMutation<TVariables, TResult>(
	mutationFn: (variables: TVariables) => Promise<TResult>,
	{ onSuccess }: MutationOptions,
) {
	const queryClient = useQueryClient();
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn,

		onSuccess: async () => {
			await wait();

			await queryClient.invalidateQueries({ queryKey: positionsKeys.all });
			await queryClient.invalidateQueries({ queryKey: projectsKeys.all });
			await queryClient.invalidateQueries({ queryKey: suggestionKeys.all });

			onSuccess?.();
		},
	});

	return { mutation, waiting };
}

export function useOpenPosition(options: MutationOptions) {
	return usePositionMutation(
		({ projectId, request }: { projectId: string; request: PositionRequest }) =>
			openPositionServerFn({ data: { projectId, req: request } }),
		options,
	);
}

export function useUpdatePosition(options: MutationOptions) {
	return usePositionMutation(
		({
			projectId,
			positionId,
			request,
		}: {
			projectId: string;
			positionId: string;
			request: PositionRequest;
		}) => updatePositionServerFn({ data: { projectId, positionId, req: request } }),
		options,
	);
}

export function useArchivePosition(options: MutationOptions) {
	return usePositionMutation(
		({ projectId, positionId }: { projectId: string; positionId: string }) =>
			archivePositionServerFn({ data: { projectId, positionId } }),
		options,
	);
}

export function useRestorePosition(options: MutationOptions) {
	return usePositionMutation(
		({ projectId, positionId }: { projectId: string; positionId: string }) =>
			restorePositionServerFn({ data: { projectId, positionId } }),
		options,
	);
}
