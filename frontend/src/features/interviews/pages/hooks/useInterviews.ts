import { useInfiniteQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import type { InterviewStatus } from "#/api/models";
import { getFnOptions } from "#/server/axios";
import { getInterviews } from "@/api/endpoints";
import { interviewsKeys } from "@/api/query-keys";

const PAGE_SIZE = 15;

export type InterviewsFilters = {
	search?: string;
	status?: InterviewStatus;
	page?: number;
	pageSize?: number;
};

const getInterviewsSliceServerFn = createServerFn({
	method: "GET",
})
	.validator((input: InterviewsFilters) => input)
	.handler(({ data }) => {
		return getInterviews(
			{
				search: data.search ?? "",
				status: data.status,
				page: data.page,
				pageSize: data.pageSize,
			},
			getFnOptions(),
		);
	});

export function useGetInterviewsSlice(fillter: InterviewsFilters) {
	return useInfiniteQuery({
		queryKey: interviewsKeys.list(fillter),

		initialPageParam: 1,

		queryFn: ({ pageParam }) =>
			getInterviewsSliceServerFn({
				data: {
					search: fillter.search,
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
