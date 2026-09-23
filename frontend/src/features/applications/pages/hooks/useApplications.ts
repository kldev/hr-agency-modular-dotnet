import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import type { CandidateSource, JobApplicationStatus } from "#/api/models";
import { getFnOptions } from "#/server/axios";
import { getJobApplication, getJobApplicationsSlice } from "@/api/endpoints";
import { applicationKeys } from "@/api/query-keys";
import type { ApplicationFilters } from "../../searchParams";
import { registeredAsWorker, type WorkerFileFilter } from "../../types";

const PAGE_SIZE = 15;

const getApplicationsSliceServerFn = createServerFn({
	method: "GET",
})
	.validator(
		(input: {
			search?: string;
			source?: CandidateSource;
			status?: JobApplicationStatus;
			worker?: WorkerFileFilter;
			page?: number;
			pageSize?: number;
		}) => input,
	)
	.handler(async ({ data }) => {
		return getJobApplicationsSlice(
			{
				search: data.search ?? "",
				...(data.source ? { source: [data.source] } : {}),
				...(data.status ? { status: [data.status] } : {}),
				registeredAsWorker: registeredAsWorker(data.worker),
				page: data.page,
				pageSize: data.pageSize,
			},
			await getFnOptions(),
		);
	});

const getApplicationDetailsServerFn = createServerFn({
	method: "GET",
})
	.validator((input: { id: string }) => input)
	.handler(async ({ data }) => {
		return getJobApplication(data.id, await getFnOptions());
	});

type SliceOptions = {
	/** The kanban asks for fewer per column than the table does per page. */
	pageSize?: number;
	enabled?: boolean;
};

export function useGetApplicationsSlice(fillter: ApplicationFilters, options: SliceOptions = {}) {
	const pageSize = options.pageSize ?? PAGE_SIZE;

	return useInfiniteQuery({
		queryKey: applicationKeys.list({ ...fillter, pageSize }),
		enabled: options.enabled ?? true,

		initialPageParam: 1,

		queryFn: ({ pageParam }) =>
			getApplicationsSliceServerFn({
				data: {
					search: fillter.search,
					source: fillter.source,
					status: fillter.status,
					worker: fillter.worker,
					page: pageParam,
					pageSize,
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
