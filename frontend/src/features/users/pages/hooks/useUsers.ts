import { useInfiniteQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getUsers } from "#/api/endpoints";
import type { OrganizationRoleApi } from "#/api/models";
import { getFnOptions } from "#/server/axios";
import { usersKeys } from "@/api/query-keys";

const PAGE_SIZE = 15;

export type UsersFilters = {
	search?: string;
	role?: OrganizationRoleApi;
	page?: number;
	pageSize?: number;
};

const getUsersliceServerFn = createServerFn({
	method: "GET",
})
	.validator((input: UsersFilters) => input)
	.handler(async ({ data }) => {
		return getUsers(
			{
				search: data.search ?? "",
				...(data.role ? { roles: [data.role] } : { roles: [] }),
				page: data.page,
				pageSize: data.pageSize,
			},
			await getFnOptions(),
		);
	});

export function useGetUsersSlice(fillter: UsersFilters) {
	return useInfiniteQuery({
		queryKey: usersKeys.list(fillter),

		initialPageParam: 1,

		queryFn: ({ pageParam }) =>
			getUsersliceServerFn({
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
