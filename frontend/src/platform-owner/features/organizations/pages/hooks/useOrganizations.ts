import { useInfiniteQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { organizationKeys } from "#/api";
import { getOrganizations } from "#/api/endpoints";
import { getFnOptions } from "#/server/axios";

const PAGE_SIZE = 15;

interface OrganizationsPageFillter {
	search?: string;
	page?: number;
	pageSize?: number;
}

export const getSliceServerFn = createServerFn({
	method: "GET",
})
	.validator((input: OrganizationsPageFillter) => input)
	.handler(async ({ data }) => {
		return getOrganizations(
			{ search: data.search, page: data.page, pageSize: data.pageSize },
			await getFnOptions(),
		);
	});

export function useGetOrganizationsSlice(search?: string) {
	return useInfiniteQuery({
		queryKey: organizationKeys.list({ search: search }),

		initialPageParam: 1,

		queryFn: ({ pageParam }) =>
			getSliceServerFn({
				data: {
					search: search,
					page: pageParam,
					pageSize: PAGE_SIZE,
				},
			}),

		getNextPageParam: (lastPage, _pages, lastPageParam) => {
			return lastPage.hasMore ? lastPageParam + 1 : undefined;
		},
	});
}
