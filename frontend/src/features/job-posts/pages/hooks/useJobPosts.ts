import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getJobPost, getJobPostsSlice } from "#/api/endpoints";
import type { JobPostStatus } from "#/api/models";
import { getFnOptions } from "#/server/axios";
import { jobPostsKeys } from "@/api/query-keys";

const PAGE_SIZE = 15;

export type JobsFilters = {
	search?: string;
	status?: JobPostStatus;
	page?: number;
	pageSize?: number;
};

const getJobPostServerFn = createServerFn({
	method: "GET",
})
	.validator((input: { id: string }) => input)
	.handler(async ({ data }) => {
		return getJobPost(data.id, await getFnOptions());
	});

const getJobsSliceServerFn = createServerFn({
	method: "GET",
})
	.validator((input: JobsFilters) => input)
	.handler(async ({ data }) => {
		return getJobPostsSlice(
			{
				search: data.search ?? "",
				...(data.status ? { status: [data.status] } : { status: [] }),
				page: data.page,
				pageSize: data.pageSize,
			},
			await getFnOptions(),
		);
	});

export function useGetJobsSlice(fillter: JobsFilters) {
	return useInfiniteQuery({
		queryKey: jobPostsKeys.list(fillter),

		initialPageParam: 1,

		queryFn: ({ pageParam }) =>
			getJobsSliceServerFn({
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

/*
 * The details screen, the edit wizard and every mutation share `jobPostsKeys.details(id)` - without
 * one key an invalidation after saving leaves the details page on stale data.
 */
export function useGetJobPost(id: string) {
	return useQuery({
		queryKey: jobPostsKeys.details(id),
		enabled: Boolean(id),
		queryFn: () =>
			getJobPostServerFn({
				data: {
					id,
				},
			}),
	});
}
