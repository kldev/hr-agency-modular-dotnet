import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getFnOptions } from "#/server/axios";
import { getWorker, getWorkers } from "@/api/endpoints";
import type { WorkerStatus } from "@/api/models";
import { workersKeys } from "@/api/query-keys";

const PAGE_SIZE = 15;

export type WorkersFilters = {
	search?: string;
	status?: WorkerStatus[];
	citizenship?: string;
	/** Where they currently work. The two register views are built from these two. */
	workCountry?: string[];
	excludeWorkCountry?: string[];
	page?: number;
	pageSize?: number;
};

const getWorkersSliceServerFn = createServerFn({
	method: "GET",
})
	.validator((input: WorkersFilters) => input)
	.handler(async ({ data }) => {
		return getWorkers(
			{
				search: data.search ?? "",
				...(data.status?.length ? { status: data.status } : {}),
				...(data.citizenship ? { citizenship: data.citizenship } : {}),
				...(data.workCountry?.length ? { workCountry: data.workCountry } : {}),
				...(data.excludeWorkCountry?.length ? { excludeWorkCountry: data.excludeWorkCountry } : {}),
				page: data.page,
				pageSize: data.pageSize,
			},
			await getFnOptions(),
		);
	});

const getWorkerServerFn = createServerFn({
	method: "GET",
})
	.validator((input: string) => input)
	.handler(async ({ data }) => {
		return getWorker(data, await getFnOptions());
	});

export function useGetWorkersSlice(filter: WorkersFilters) {
	return useInfiniteQuery({
		queryKey: workersKeys.list(filter),

		initialPageParam: 1,

		queryFn: ({ pageParam }) =>
			getWorkersSliceServerFn({
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

/*
 * One `GET` for the whole details page: the projection already carries the documents, the permits
 * and the person's entire posting history, which is the point of it being a MultiStreamProjection.
 */
export function useGetWorker(id: string) {
	return useQuery({
		queryKey: workersKeys.details(id),
		queryFn: () => getWorkerServerFn({ data: id }),
		enabled: Boolean(id),
	});
}
