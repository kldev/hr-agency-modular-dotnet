import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getFnOptions } from "#/server/axios";
import { getPosition, getPositions } from "@/api/endpoints";
import type { WorkerContractType } from "@/api/models";
import { positionsKeys } from "@/api/query-keys";

const PAGE_SIZE = 15;

export type PositionsFilters = {
	search?: string;
	projectId?: string;
	contractType?: WorkerContractType;
	/** Archived roles are the ones that have run their course; they are off by default. */
	includeArchived?: boolean;
	page?: number;
	pageSize?: number;
};

const getPositionsSliceServerFn = createServerFn({
	method: "GET",
})
	.validator((input: PositionsFilters) => input)
	.handler(async ({ data }) => {
		return getPositions(
			{
				search: data.search ?? "",
				...(data.projectId ? { projectId: data.projectId } : {}),
				...(data.contractType ? { contractType: data.contractType } : {}),
				includeArchived: data.includeArchived ?? false,
				page: data.page,
				pageSize: data.pageSize,
			},
			await getFnOptions(),
		);
	});

const getPositionServerFn = createServerFn({
	method: "GET",
})
	.validator((input: string) => input)
	.handler(async ({ data }) => {
		return getPosition(data, await getFnOptions());
	});

export function useGetPositionsSlice(filter: PositionsFilters) {
	return useInfiniteQuery({
		queryKey: positionsKeys.list(filter),

		initialPageParam: 1,

		queryFn: ({ pageParam }) =>
			getPositionsSliceServerFn({
				data: {
					...filter,
					page: pageParam,
					pageSize: PAGE_SIZE,
				},
			}),

		getNextPageParam: (lastPage, _pages, lastPageParam) => {
			return lastPage.hasMore ? lastPageParam + 1 : undefined;
		},
	});
}

export function useGetPosition(id: string) {
	return useQuery({
		queryKey: positionsKeys.details(id),
		queryFn: () => getPositionServerFn({ data: id }),
		enabled: Boolean(id),
	});
}
