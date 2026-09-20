import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getFnOptions } from "#/server/axios";
import { getProject, getProjectComplianceCatalogue, getProjects } from "@/api/endpoints";
import type { ProjectStatus } from "@/api/models";
import { projectsKeys } from "@/api/query-keys";

const PAGE_SIZE = 15;

export type ProjectsFilters = {
	search?: string;
	status?: ProjectStatus[];
	companyId?: string;
	teamId?: string;
	country?: string;
	page?: number;
	pageSize?: number;
};

const getProjectsSliceServerFn = createServerFn({
	method: "GET",
})
	.validator((input: ProjectsFilters) => input)
	.handler(async ({ data }) => {
		return getProjects(
			{
				search: data.search ?? "",
				...(data.status?.length ? { status: data.status } : {}),
				...(data.companyId ? { companyId: data.companyId } : {}),
				...(data.teamId ? { teamId: data.teamId } : {}),
				...(data.country ? { country: data.country } : {}),
				page: data.page,
				pageSize: data.pageSize,
			},
			await getFnOptions(),
		);
	});

const getProjectServerFn = createServerFn({
	method: "GET",
})
	.validator((input: string) => input)
	.handler(async ({ data }) => {
		return getProject(data, await getFnOptions());
	});

const getComplianceCatalogueServerFn = createServerFn({
	method: "GET",
})
	.validator((input: string) => input)
	.handler(async ({ data }) => {
		return getProjectComplianceCatalogue(data, await getFnOptions());
	});

export function useGetProjectsSlice(filter: ProjectsFilters) {
	return useInfiniteQuery({
		queryKey: projectsKeys.list(filter),

		initialPageParam: 1,

		queryFn: ({ pageParam }) =>
			getProjectsSliceServerFn({
				data: {
					...filter,
					page: pageParam,
					pageSize: PAGE_SIZE,
				},
			}),

		getNextPageParam: (lastPage, _pages, lastPageParam) => {
			return lastPage.hasMore ? lastPageParam + 1 : undefined;
		},
	});
}

export function useGetProject(id: string) {
	return useQuery({
		queryKey: projectsKeys.details(id),
		queryFn: () => getProjectServerFn({ data: id }),
		enabled: Boolean(id),
	});
}

/*
 * The catalogue is a second call on a page that otherwise lives off one `GET`. It has to be: the
 * requirements follow from (work country, engagement type) and the project itself only carries the
 * items somebody has already recorded - the ones nobody has touched yet exist only here.
 */
export function useGetComplianceCatalogue(id: string) {
	return useQuery({
		queryKey: projectsKeys.compliance(id),
		queryFn: () => getComplianceCatalogueServerFn({ data: id }),
		enabled: Boolean(id),
	});
}
