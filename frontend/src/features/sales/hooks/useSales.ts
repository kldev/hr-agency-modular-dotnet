import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import type { OpportunityStage } from "#/api/models";
import { getFnOptions } from "#/server/axios";
import { getOpportunities, getOpportunity } from "@/api/endpoints";
import { salesKeys } from "@/api/query-keys";

const PAGE_SIZE = 15;

export interface SalesPageFillters {
	search?: string;
	stage?: OpportunityStage;
	page?: number;
	pageSize?: number;
}
const getSingleServerFn = createServerFn({
	method: "GET",
})
	.validator((input: { id: string }) => input)
	.handler(({ data }) => {
		return getOpportunity(data.id, getFnOptions());
	});

const getSliceServerFn = createServerFn({
	method: "GET",
})
	.validator((input: SalesPageFillters) => input)
	.handler(({ data }) => {
		return getOpportunities(
			{
				search: data.search,
				stage: data.stage,
				page: data.page,
				pageSize: data.pageSize,
			},
			getFnOptions(),
		);
	});

export function useGetOpportunitesSlice(fillter: SalesPageFillters) {
	return useInfiniteQuery({
		queryKey: salesKeys.list(fillter),

		initialPageParam: 1,

		queryFn: ({ pageParam }) =>
			getSliceServerFn({
				data: {
					search: fillter.search,
					stage: fillter.stage,
					page: pageParam,
					pageSize: PAGE_SIZE,
				},
			}),

		getNextPageParam: (lastPage, _pages, lastPageParam) => {
			return lastPage.hasMore ? lastPageParam + 1 : undefined;
		},
	});
}

export function useGetOpportunity(id: string) {
	return useQuery({
		queryKey: salesKeys.opportunity(id),
		enabled: Boolean(id),
		queryFn: () =>
			getSingleServerFn({
				data: {
					id,
				},
			}),
	});
}
