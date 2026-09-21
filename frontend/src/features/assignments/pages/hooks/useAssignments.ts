import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getFnOptions } from "#/server/axios";
import { getAssignment, getAssignmentComplianceCatalogue, getAssignments } from "@/api/endpoints";
import type { AssignmentStatus } from "@/api/models";
import { assignmentsKeys } from "@/api/query-keys";

const PAGE_SIZE = 15;

export type AssignmentsFilters = {
	search?: string;
	status?: AssignmentStatus[];
	workerId?: string;
	projectId?: string;
	workCountry?: string;
	page?: number;
	pageSize?: number;
};

const getAssignmentsSliceServerFn = createServerFn({
	method: "GET",
})
	.validator((input: AssignmentsFilters) => input)
	.handler(async ({ data }) => {
		return getAssignments(
			{
				search: data.search ?? "",
				...(data.status?.length ? { status: data.status } : {}),
				...(data.workerId ? { workerId: data.workerId } : {}),
				...(data.projectId ? { projectId: data.projectId } : {}),
				...(data.workCountry ? { workCountry: data.workCountry } : {}),
				page: data.page,
				pageSize: data.pageSize,
			},
			await getFnOptions(),
		);
	});

const getAssignmentServerFn = createServerFn({
	method: "GET",
})
	.validator((input: string) => input)
	.handler(async ({ data }) => {
		return getAssignment(data, await getFnOptions());
	});

const getAssignmentComplianceCatalogueServerFn = createServerFn({
	method: "GET",
})
	.validator((input: string) => input)
	.handler(async ({ data }) => {
		return getAssignmentComplianceCatalogue(data, await getFnOptions());
	});

export function useGetAssignmentsSlice(filter: AssignmentsFilters) {
	return useInfiniteQuery({
		queryKey: assignmentsKeys.list(filter),

		initialPageParam: 1,

		queryFn: ({ pageParam }) =>
			getAssignmentsSliceServerFn({
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

export function useGetAssignment(id: string) {
	return useQuery({
		queryKey: assignmentsKeys.details(id),
		queryFn: () => getAssignmentServerFn({ data: id }),
		enabled: Boolean(id),
	});
}

/*
 * The per person half of the catalogue: what this one posting owes, keyed on the country and the
 * engagement type frozen onto it when it was planned. A second call for the same reason it is one
 * on the project page - the requirements nobody has touched yet exist only here.
 */
export function useGetAssignmentComplianceCatalogue(id: string) {
	return useQuery({
		queryKey: assignmentsKeys.compliance(id),
		queryFn: () => getAssignmentComplianceCatalogueServerFn({ data: id }),
		enabled: Boolean(id),
	});
}
