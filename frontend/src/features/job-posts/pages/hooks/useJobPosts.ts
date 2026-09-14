import { useInfiniteQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getJobPostsSlice } from "#/api/endpoints";
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

const getJobsSliceServerFn = createServerFn({
	method: "GET",
})
	.validator((input: JobsFilters) => input)
	.handler(({ data }) => {
		return getJobPostsSlice(
			{
				search: data.search ?? "",
				...(data.status ? { status: [data.status] } : { status: [] }),
				page: data.page,
				pageSize: data.pageSize,
			},
			getFnOptions(),
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
