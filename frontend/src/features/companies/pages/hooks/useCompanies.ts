import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getFnOptions } from "#/server/axios";
import { getCompanies, getCompany, getCompanyContacts } from "@/api/endpoints";
import { companiesKeys } from "@/api/query-keys";

const PAGE_SIZE = 15;

export type CompanyFilters = {
	search?: string;
	page?: number;
	pageSize?: number;
};

const getCompanyContactsServerFn = createServerFn({
	method: "GET",
})
	.validator((input: string) => input)
	.handler(async ({ data }) => {
		return getCompanyContacts(data, await getFnOptions());
	});

const getCompaniesSliceServerFn = createServerFn({
	method: "GET",
})
	.validator((input: CompanyFilters) => input)
	.handler(async ({ data }) => {
		return getCompanies(
			{
				search: data.search ?? "",
				page: data.page,
				pageSize: data.pageSize,
			},
			await getFnOptions(),
		);
	});

const getCompanyServerFn = createServerFn({
	method: "GET",
})
	.validator((input: string) => input)
	.handler(async ({ data }) => {
		return getCompany(data, await getFnOptions());
	});

export function useGetCompanyContacts(id: string) {
	return useQuery({
		queryKey: companiesKeys.contacts(id),
		queryFn: () => getCompanyContactsServerFn({ data: id }),
		enabled: Boolean(id),
	});
}

export function useGetCompany(id: string) {
	return useQuery({
		queryKey: companiesKeys.details(id),
		queryFn: () => getCompanyServerFn({ data: id }),
		enabled: Boolean(id),
	});
}

export function useGetCompaniesSlice(search: string) {
	return useInfiniteQuery({
		queryKey: companiesKeys.list(search),

		initialPageParam: 1,

		queryFn: ({ pageParam }) =>
			getCompaniesSliceServerFn({
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
