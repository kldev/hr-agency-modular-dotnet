import { useInfiniteQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getFnOptions } from "#/server/axios";
import { getCandidateTimeline, getJobApplicationTimeline } from "@/api/endpoints";
import { applicationKeys, candidatesKeys } from "@/api/query-keys";

const TIMELINE_PAGE_SIZE = 20;

type TimelinePageInput = { id: string; after?: string };

const getCandidateTimelineServerFn = createServerFn({ method: "GET" })
	.validator((input: TimelinePageInput) => input)
	.handler(async ({ data }) =>
		getCandidateTimeline(
			data.id,
			{ after: data.after, pageSize: TIMELINE_PAGE_SIZE },
			await getFnOptions(),
		),
	);

const getApplicationTimelineServerFn = createServerFn({ method: "GET" })
	.validator((input: TimelinePageInput) => input)
	.handler(async ({ data }) =>
		getJobApplicationTimeline(
			data.id,
			{ after: data.after, pageSize: TIMELINE_PAGE_SIZE },
			await getFnOptions(),
		),
	);

/*
 * The timeline pages by cursor, not by page number: the next page starts after the last entry
 * seen, so an entry written while somebody scrolls does not shift what they get next.
 */

export function useCandidateTimeline(candidateId: string) {
	return useInfiniteQuery({
		queryKey: candidatesKeys.timeline(candidateId),
		enabled: Boolean(candidateId),
		initialPageParam: undefined as string | undefined,
		queryFn: ({ pageParam }) =>
			getCandidateTimelineServerFn({ data: { id: candidateId, after: pageParam } }),
		getNextPageParam: (lastPage) => lastPage.nextCursor ?? undefined,
	});
}

export function useApplicationTimeline(jobApplicationId: string) {
	return useInfiniteQuery({
		queryKey: applicationKeys.timeline(jobApplicationId),
		enabled: Boolean(jobApplicationId),
		initialPageParam: undefined as string | undefined,
		queryFn: ({ pageParam }) =>
			getApplicationTimelineServerFn({ data: { id: jobApplicationId, after: pageParam } }),
		getNextPageParam: (lastPage) => lastPage.nextCursor ?? undefined,
	});
}
