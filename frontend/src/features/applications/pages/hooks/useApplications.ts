import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import type { CandidateSource, JobApplicationStatus } from "#/api/models";
import { getFnOptions } from "#/server/axios";
import { getJobApplication, getJobApplicationsSlice } from "@/api/endpoints";
import { applicationKeys } from "@/api/query-keys";
import type { ApplicationFilters } from "../AplicationsPage";

const PAGE_SIZE = 15;

const getApplicationsSliceServerFn = createServerFn({
	method: "GET",
})
	.validator(
		(input: {
			search?: string;
			source?: CandidateSource;
			status?: JobApplicationStatus;
			page?: number;
			pageSize?: number;
		}) => input,
	)
	.handler(({ data }) => {
		return getJobApplicationsSlice(
			{
				search: data.search ?? "",
				...(data.source ? { source: [data.source] } : {}),
				...(data.status ? { status: [data.status] } : {}),
				page: data.page,
				pageSize: data.pageSize,
			},
			getFnOptions(),
		);
	});

const getApplicationDetailsServerFn = createServerFn({
	method: "GET",
})
	.validator((input: { id: string }) => input)
	.handler(({ data }) => {
		return getJobApplication(data.id, getFnOptions());
	});

export function useGetApplicationsSlice(fillter: ApplicationFilters) {
	return useInfiniteQuery({
		queryKey: applicationKeys.list(fillter),

		initialPageParam: 1,

		queryFn: ({ pageParam }) =>
			getApplicationsSliceServerFn({
				data: {
					search: fillter.search,
					source: fillter.source,
					status: fillter.status,
					page: pageParam,
					pageSize: PAGE_SIZE,
				},
			}),

		getNextPageParam: (lastPage, _pages, lastPageParam) => {
			return lastPage.hasMore ? lastPageParam + 1 : undefined;
		},
	});
}

export function useGetApplicationDetails(id: string) {
	return useQuery({
		queryKey: applicationKeys.detail(id),
		queryFn: () =>
			getApplicationDetailsServerFn({
				data: {
					id,
				},
			}),

		enabled: Boolean(id),
	});
}
