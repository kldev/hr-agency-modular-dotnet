import { useQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import type { InterviewStatus } from "#/api/models";
import { getFnOptions } from "#/server/axios";
import { getInterview, getInterviewsForDateRange } from "@/api/endpoints";
import { interviewsKeys } from "@/api/query-keys";

const getInterviewServerFn = createServerFn({
	method: "GET",
})
	.validator((input: { id: string }) => input)
	.handler(async ({ data }) => {
		return getInterview(data.id, await getFnOptions());
	});

type GetRanteFilter = {
	search?: string;
	from: string;
	to: string;
	status?: InterviewStatus;
};

const getInterviewsRangeServerFn = createServerFn({
	method: "GET",
})
	.validator((input: GetRanteFilter) => input)
	.handler(async ({ data }) => {
		return getInterviewsForDateRange(
			{
				search: data.search,
				fromDate: data.from,
				toDate: data.to,
				status: data.status,
			},
			await getFnOptions(),
		);
	});

export function useGetInterviewsRange(filter: GetRanteFilter) {
	return useQuery({
		enabled: Boolean(filter.from) && Boolean(filter.to),
		queryKey: interviewsKeys.range(filter),
		queryFn: () =>
			getInterviewsRangeServerFn({
				data: filter,
			}),
	});
}

export function useGetInterview(id: string) {
	return useQuery({
		queryKey: interviewsKeys.details(id),
		enabled: Boolean(id),
		queryFn: () =>
			getInterviewServerFn({
				data: {
					id,
				},
			}),
	});
}
