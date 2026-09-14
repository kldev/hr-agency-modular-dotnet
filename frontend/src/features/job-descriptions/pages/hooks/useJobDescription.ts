import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import type { JobDescriptionStatus } from "#/api/models";
import { getFnOptions } from "#/server/axios";
import { getJobDescription, getJobDescriptionsSlice } from "@/api/endpoints";
import { jobDescriptionKeys } from "@/api/query-keys";
import type { JobsDescriptopnPageFilters } from "../JobsDescriptopnPage";

const PAGE_SIZE = 15;

export const getJobDescriptionServerFn = createServerFn({
	method: "GET",
})
	.validator((input: { id: string }) => input)
	.handler(({ data }) => {
		return getJobDescription(data.id, getFnOptions());
	});

export const getJobDescriptionsSliceServerFn = createServerFn({
	method: "GET",
})
	.validator(
		(input: { search?: string; status?: JobDescriptionStatus; page?: number; pageSize?: number }) =>
			input,
	)
	.handler(({ data }) => {
		return getJobDescriptionsSlice(
			{
				search: data.search,
				...(data.status ? { status: [data.status] } : {}),
				page: data.page,
				pageSize: data.pageSize,
			},
			getFnOptions(),
		);
	});

export function useGetJobDescriptionSlice(fillter: JobsDescriptopnPageFilters) {
	return useInfiniteQuery({
		queryKey: jobDescriptionKeys.list(fillter),

		initialPageParam: 1,

		queryFn: ({ pageParam }) =>
			getJobDescriptionsSliceServerFn({
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

export function useGetJobDescription(id: string) {
	return useQuery({
		queryKey: jobDescriptionKeys.detail(id),
		enabled: Boolean(id),
		queryFn: () =>
			getJobDescriptionServerFn({
				data: {
					id,
				},
			}),
	});
}
