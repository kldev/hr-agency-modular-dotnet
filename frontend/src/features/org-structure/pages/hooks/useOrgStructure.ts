import { useQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getFnOptions } from "#/server/axios";
import { getOrgStructure, getSubordinates, getSupervisor } from "@/api/endpoints";
import type { OrgStructureProjection, SupervisorView } from "@/api/models";
import { orgStructureKeys } from "@/api/query-keys";

/*
 * The generated `use*` hooks for these three are not used anywhere, and that is deliberate: orval
 * turned the GETs of this module into mutation-shaped hooks, so `useGetOrgStructure` from the
 * generated client would fire on `.mutate()` rather than on mount. Only the raw fetchers are
 * imported, and only here.
 */

const getOrgStructureServerFn = createServerFn({ method: "GET" }).handler(async () => {
	const structure = await getOrgStructure(await getFnOptions());

	/*
	 * An organization that has never drawn its chart answers 200 with an empty body - "a company on
	 * its first day", as the endpoint puts it - so the generated non-nullable return type is a
	 * promise the API does not keep.
	 */
	return (structure ?? null) as OrgStructureProjection | null;
});

const getSupervisorServerFn = createServerFn({ method: "GET" })
	.validator((input: string) => input)
	.handler(async ({ data }) => {
		const supervisor = await getSupervisor(data, await getFnOptions());

		// Null is an answer here, not a miss: nobody stands above the head of the top unit.
		return (supervisor ?? null) as SupervisorView | null;
	});

const getSubordinatesServerFn = createServerFn({ method: "GET" })
	.validator((input: { userId: string; wholeSubtree: boolean }) => input)
	.handler(async ({ data }) => {
		return getSubordinates(data.userId, { wholeSubtree: data.wholeSubtree }, await getFnOptions());
	});

/** The whole chart in one read. There is nothing to page and every question needs the ancestors. */
export function useGetOrgStructure() {
	return useQuery({
		queryKey: orgStructureKeys.chart(),
		queryFn: () => getOrgStructureServerFn(),
	});
}

/**
 * Who answers for this person. Asked of the backend rather than walked client-side even though the
 * chart is in hand: the rule climbs past a head who is their own unit's head and past units with no
 * head of their own, and a second implementation of that would be a second thing to keep right.
 */
export function useGetSupervisor(userId: string) {
	return useQuery({
		queryKey: orgStructureKeys.supervisor(userId),
		queryFn: () => getSupervisorServerFn({ data: userId }),
		enabled: Boolean(userId),
	});
}

export function useGetSubordinates(userId: string, wholeSubtree: boolean, enabled: boolean) {
	return useQuery({
		queryKey: orgStructureKeys.subordinates(userId, wholeSubtree),
		queryFn: () => getSubordinatesServerFn({ data: { userId, wholeSubtree } }),
		enabled: enabled && Boolean(userId),
	});
}
