import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getFnOptions } from "#/server/axios";
import {
	closeLegalEntity,
	createLegalEntity,
	getLegalEntities,
	getLegalEntity,
	updateLegalEntity,
} from "@/api/endpoints";
import type { CloseLegalEntityRequest, LegalEntityRequest } from "@/api/models";
import { legalEntitiesKeys, projectsKeys, suggestionKeys } from "@/api/query-keys";
import { useProjectionWait } from "@/hooks";

const PAGE_SIZE = 15;

export type LegalEntitiesFilters = {
	search?: string;
	activeOnly?: boolean;
	page?: number;
	pageSize?: number;
};

type MutationOptions = {
	/** Optional: most callers have nothing to add once the screen has already changed. */
	onSuccess?: () => void;
};

const getLegalEntitiesSliceServerFn = createServerFn({ method: "GET" })
	.validator((input: LegalEntitiesFilters) => input)
	.handler(async ({ data }) => {
		return getLegalEntities(
			{
				search: data.search ?? "",
				activeOnly: data.activeOnly ?? false,
				page: data.page,
				pageSize: data.pageSize,
			},
			await getFnOptions(),
		);
	});

const getLegalEntityServerFn = createServerFn({ method: "GET" })
	.validator((input: string) => input)
	.handler(async ({ data }) => {
		return getLegalEntity(data, await getFnOptions());
	});

const createLegalEntityServerFn = createServerFn({ method: "POST" })
	.validator((input: LegalEntityRequest) => input)
	.handler(async ({ data }) => {
		return createLegalEntity(data, await getFnOptions());
	});

const updateLegalEntityServerFn = createServerFn({ method: "POST" })
	.validator((input: { id: string; req: LegalEntityRequest }) => input)
	.handler(async ({ data }) => {
		return updateLegalEntity(data.id, data.req, await getFnOptions());
	});

const closeLegalEntityServerFn = createServerFn({ method: "POST" })
	.validator((input: { id: string; req: CloseLegalEntityRequest }) => input)
	.handler(async ({ data }) => {
		return closeLegalEntity(data.id, data.req, await getFnOptions());
	});

export function useGetLegalEntitiesSlice(filter: LegalEntitiesFilters) {
	return useInfiniteQuery({
		queryKey: legalEntitiesKeys.list(filter),

		initialPageParam: 1,

		queryFn: ({ pageParam }) =>
			getLegalEntitiesSliceServerFn({
				data: { ...filter, page: pageParam, pageSize: PAGE_SIZE },
			}),

		getNextPageParam: (lastPage, _pages, lastPageParam) =>
			lastPage.hasMore ? lastPageParam + 1 : undefined,
	});
}

/**
 * All the companies that could take a project today. A plain query rather than a suggestion
 * endpoint: an agency trades through a handful of entities, so a select beats a typeahead and
 * there is no list long enough to page through.
 */
export function useActiveLegalEntities() {
	return useQuery({
		queryKey: legalEntitiesKeys.list({ activeOnly: true, pageSize: 100 }),
		queryFn: () =>
			getLegalEntitiesSliceServerFn({
				data: { activeOnly: true, page: 1, pageSize: 100 },
			}),
	});
}

export function useGetLegalEntity(id: string) {
	return useQuery({
		queryKey: legalEntitiesKeys.detail(id),
		queryFn: () => getLegalEntityServerFn({ data: id }),
		enabled: Boolean(id),
	});
}

/*
 * A project carries a snapshot of the company that delivers it, so renaming one changes what new
 * projects will record - which is why the projects key is invalidated alongside. Already started
 * projects keep what they kept; nothing here can reach them.
 */
function useLegalEntityMutation<TVariables, TResult>(
	mutationFn: (variables: TVariables) => Promise<TResult>,
	{ onSuccess }: MutationOptions,
) {
	const queryClient = useQueryClient();
	const { wait, waiting } = useProjectionWait();

	const mutation = useMutation({
		mutationFn,

		onSuccess: async () => {
			await wait();

			await queryClient.invalidateQueries({ queryKey: legalEntitiesKeys.all });
			await queryClient.invalidateQueries({ queryKey: projectsKeys.all });
			await queryClient.invalidateQueries({ queryKey: suggestionKeys.all });

			onSuccess?.();
		},
	});

	return { mutation, waiting };
}

export function useCreateLegalEntity(options: MutationOptions) {
	return useLegalEntityMutation(
		(request: LegalEntityRequest) => createLegalEntityServerFn({ data: request }),
		options,
	);
}

export function useUpdateLegalEntity(options: MutationOptions) {
	return useLegalEntityMutation(
		({ id, request }: { id: string; request: LegalEntityRequest }) =>
			updateLegalEntityServerFn({ data: { id, req: request } }),
		options,
	);
}

export function useCloseLegalEntity(options: MutationOptions) {
	return useLegalEntityMutation(
		({ id, activeTo }: { id: string; activeTo: string }) =>
			closeLegalEntityServerFn({ data: { id, req: { activeTo } } }),
		options,
	);
}
