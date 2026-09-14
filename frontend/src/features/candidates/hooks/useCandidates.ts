import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { candidatesKeys } from "#/api";
import { getCandidate, getCandidates } from "#/api/endpoints";
import type { CandidateSource } from "#/api/models";
import { getFnOptions } from "#/server/axios";

const PAGE_SIZE = 15;

export interface CandidatesPageFillter {
	search?: string;
	source?: CandidateSource;
	page?: number;
	pageSize?: number;
}

const getSingleServerFn = createServerFn({
	method: "GET",
})
	.validator((input: string) => input)
	.handler(({ data }) => {
		return getCandidate(data, getFnOptions());
	});

const getSliceServerFn = createServerFn({
	method: "GET",
})
	.validator((input: CandidatesPageFillter) => input)
	.handler(({ data }) => {
		return getCandidates(
			{
				page: data.page,
				pageSize: data.pageSize,
				search: data.search,
				...(data.source ? { source: [data.source] } : {}),
			},
			getFnOptions(),
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
					page: pageParam,
					pageSize: PAGE_SIZE,
				},
			}),

		getNextPageParam: (lastPage, _pages, lastPageParam) => {
			return lastPage.hasMore ? lastPageParam + 1 : undefined;
		},
	});
}
