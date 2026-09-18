import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import type { OpportunityStage } from "#/api/models";
import { getFnOptions } from "#/server/axios";
import {
	getOpportunities,
	getOpportunitiesPipelineTotals,
	getOpportunity,
	getSalesActivities,
} from "@/api/endpoints";
import { salesKeys } from "@/api/query-keys";
import { groupPipelineTotals } from "./pipelineTotals";

const PAGE_SIZE = 15;
const ACTIVITY_PAGE_SIZE = 10;

export interface SalesPageFillters {
	search?: string;
	stage?: OpportunityStage;
	page?: number;
	pageSize?: number;
	responsibleId?: string;
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
				responsibleId: data.responsibleId,
			},
			getFnOptions(),
		);
	});

const getActivitiesSliceServerFn = createServerFn({
	method: "GET",
})
	.validator((input: { opportunityId: string; page: number }) => input)
	.handler(({ data }) => {
		return getSalesActivities(
			{
				opportunityId: data.opportunityId,
				page: data.page,
				pageSize: ACTIVITY_PAGE_SIZE,
			},
			getFnOptions(),
		);
	});

const getPipelineTotalsServerFn = createServerFn({
	method: "GET",
})
	.validator((input: SalesPageFillters) => input)
	.handler(({ data }) => {
		return getOpportunitiesPipelineTotals(
			{
				search: data.search,
				responsibleId: data.responsibleId,
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
					...fillter,
					page: pageParam,
					pageSize: PAGE_SIZE,
				},
			}),

		getNextPageParam: (lastPage, _pages, lastPageParam) => {
			return lastPage.hasMore ? lastPageParam + 1 : undefined;
		},
	});
}

export function useGetPipelineTotals(fillter: SalesPageFillters, enabled = true) {
	const query = useQuery({
		queryKey: salesKeys.totals(fillter),
		enabled,
		queryFn: () => getPipelineTotalsServerFn({ data: fillter }),
	});

	return {
		query,
		totals: groupPipelineTotals(query.data),
	};
}

export function useGetActivitiesSlice(opportunityId: string) {
	return useInfiniteQuery({
		queryKey: salesKeys.activities(opportunityId),

		enabled: Boolean(opportunityId),

		initialPageParam: 1,

		queryFn: ({ pageParam }) =>
			getActivitiesSliceServerFn({
				data: {
					opportunityId,
					page: pageParam,
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
