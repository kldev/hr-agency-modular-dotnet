import { useQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import axios from "axios";
import { getFnOptions } from "#/server/axios";
import { getAgencyEmployment, listAgencyEmployments } from "@/api/endpoints";
import type { AgencyEmploymentProjection } from "@/api/models";
import { agencyEmploymentKeys } from "@/api/query-keys";

/*
 * Only the raw fetchers are imported. Orval generated the GETs of this module as mutation-shaped
 * hooks, so the generated `useListAgencyEmployments` would fire on `.mutate()` rather than on
 * mount - the same trap already documented in `features/org-structure`.
 */

const listAgencyEmploymentsServerFn = createServerFn({ method: "GET" })
	.validator((input: { year?: number; month?: number }) => input)
	.handler(async ({ data }) =>
		listAgencyEmployments(
			data.year && data.month ? { year: data.year, month: data.month } : undefined,
			await getFnOptions(),
		),
	);

const getAgencyEmploymentServerFn = createServerFn({ method: "GET" })
	.validator((input: string) => input)
	.handler(async ({ data }) => {
		/*
		 * Nobody being on record is an answer, not a failure: the endpoint says 404 for it, and the
		 * whole point of asking is to tell somebody they have no engagement yet. Only that status
		 * becomes null - swallowing the rest would report an outage as "not employed", and the
		 * screen that reads this puts those words in front of the person.
		 */
		try {
			return (await getAgencyEmployment(
				data,
				await getFnOptions(),
			)) as AgencyEmploymentProjection | null;
		} catch (error) {
			if (axios.isAxiosError(error) && error.response?.status === 404) return null;

			throw error;
		}
	});

/**
 * The whole register in one read - it is one row per person in the agency, so there is nothing to
 * page. Passing a month narrows it to the people covered by the duty to record hours in that month.
 */
export function useGetAgencyEmployments(params: { year?: number; month?: number } = {}) {
	return useQuery({
		queryKey: agencyEmploymentKeys.list(params),
		queryFn: () => listAgencyEmploymentsServerFn({ data: params }),
	});
}

export function useGetAgencyEmployment(userId: string | undefined) {
	return useQuery({
		queryKey: agencyEmploymentKeys.details(userId ?? ""),
		queryFn: () => getAgencyEmploymentServerFn({ data: userId ?? "" }),
		enabled: Boolean(userId),
	});
}
