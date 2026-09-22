import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { candidatesKeys } from "#/api";
import { getCandidate, getCandidates } from "#/api/endpoints";
import type { CandidateSource } from "#/api/models";
import { getFnOptions } from "#/server/axios";
import { registeredAsWorker, type WorkerFileFilter } from "@/features/applications/types";

const PAGE_SIZE = 15;

export interface CandidatesPageFillter {
	search?: string;
	source?: CandidateSource;
	worker?: WorkerFileFilter;
	page?: number;
	pageSize?: number;
}

const getSingleServerFn = createServerFn({
	method: "GET",
})
	.validator((input: string) => input)
	.handler(async ({ data }) => {
		return getCandidate(data, await getFnOptions());
	});

const getSliceServerFn = createServerFn({
	method: "GET",
})
	.validator((input: CandidatesPageFillter) => input)
	.handler(async ({ data }) => {
		return getCandidates(
			{
				page: data.page,
				pageSize: data.pageSize,
				search: data.search,
				...(data.source ? { source: [data.source] } : {}),
				registeredAsWorker: registeredAsWorker(data.worker),
			},
			await getFnOptions(),
		);
	});

export function useGetCandidate(id: string) {
	return useQuery({
		enabled: Boolean(id),
		queryKey: candidatesKeys.detail(id),
		queryFn: () => getSingleServerFn({ data: id }),
	});
}

export function useGetCandidatesSlice(search: CandidatesPageFillter) {
	return useInfiniteQuery({
		queryKey: candidatesKeys.list(search),

		initialPageParam: 1,

		queryFn: ({ pageParam }) =>
			getSliceServerFn({
				data: {
					search: search.search,
					source: search.source,
					worker: search.worker,
					page: pageParam,
					pageSize: PAGE_SIZE,
				},
			}),

		getNextPageParam: (lastPage, _pages, lastPageParam) => {
			return lastPage.hasMore ? lastPageParam + 1 : undefined;
		},
	});
}
