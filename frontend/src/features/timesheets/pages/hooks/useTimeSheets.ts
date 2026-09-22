import { useQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getFnOptions } from "#/server/axios";
import {
	getMyTimeSheet,
	getTeamTimeSheets,
	getTimeSheet,
	getTimeSheetsForSettlement,
} from "@/api/endpoints";
import type { TimeSheetProjection } from "@/api/models";
import { timeSheetsKeys } from "@/api/query-keys";
import type { MonthInView } from "../../types";

/* Raw fetchers only - orval turned every GET here into a mutation-shaped hook. */

const getMyTimeSheetServerFn = createServerFn({ method: "GET" })
	.validator((input: MonthInView) => input)
	.handler(async ({ data }) => {
		const sheet = await getMyTimeSheet(
			{ year: data.year, month: data.month },
			await getFnOptions(),
		);

		/*
		 * A month nobody has written on answers 200 with an empty body - the sheet comes into being
		 * when the first day is saved - so the generated non-nullable type is a promise the API
		 * does not keep.
		 */
		return (sheet ?? null) as TimeSheetProjection | null;
	});

const getTeamTimeSheetsServerFn = createServerFn({ method: "GET" })
	.validator((input: MonthInView) => input)
	.handler(async ({ data }) =>
		getTeamTimeSheets({ year: data.year, month: data.month }, await getFnOptions()),
	);

const getSettlementServerFn = createServerFn({ method: "GET" })
	.validator((input: MonthInView) => input)
	.handler(async ({ data }) =>
		getTimeSheetsForSettlement({ year: data.year, month: data.month }, await getFnOptions()),
	);

const getTimeSheetServerFn = createServerFn({ method: "GET" })
	.validator((input: { userId: string } & MonthInView) => input)
	.handler(async ({ data }) => {
		const sheet = await getTimeSheet(data.userId, data.year, data.month, await getFnOptions());

		return (sheet ?? null) as TimeSheetProjection | null;
	});

export function useGetMyTimeSheet(month: MonthInView) {
	return useQuery({
		queryKey: timeSheetsKeys.mine(month.year, month.month),
		queryFn: () => getMyTimeSheetServerFn({ data: month }),
	});
}

/**
 * The monitoring list: everybody below the caller in the chart who owes hours for this month,
 * including the people who have not started a sheet at all. Somebody with no subordinates gets an
 * empty list, and that is how the page decides whether the team tabs exist.
 */
export function useGetTeamTimeSheets(month: MonthInView) {
	return useQuery({
		queryKey: timeSheetsKeys.team(month.year, month.month),
		queryFn: () => getTeamTimeSheetsServerFn({ data: month }),
	});
}

/** Payroll only - the endpoint is behind the policy, so this is asked only where the tab exists. */
export function useGetTimeSheetsForSettlement(month: MonthInView, enabled: boolean) {
	return useQuery({
		queryKey: timeSheetsKeys.settlement(month.year, month.month),
		queryFn: () => getSettlementServerFn({ data: month }),
		enabled,
	});
}

/** One person's month, for the panel next to the approval queue. */
export function useGetTimeSheet(userId: string | undefined, month: MonthInView) {
	return useQuery({
		queryKey: timeSheetsKeys.sheet(userId ?? "", month.year, month.month),
		queryFn: () => getTimeSheetServerFn({ data: { userId: userId ?? "", ...month } }),
		enabled: Boolean(userId),
	});
}
