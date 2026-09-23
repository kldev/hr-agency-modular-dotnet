import { useQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getRecruitmentReport } from "#/api/endpoints";
import { reportsKeys } from "#/api/query-keys";
import type { ReportPeriod } from "#/features/reports/period";
import { getFnOptions } from "#/server/axios";

/* Raw fetcher only - orval turned the GET into a mutation-shaped hook. */
const getRecruitmentReportServerFn = createServerFn({ method: "GET" })
	.validator((input: ReportPeriod) => input)
	.handler(async ({ data }) => getRecruitmentReport(data, await getFnOptions()));

export function useRecruitmentReport(period: ReportPeriod) {
	return useQuery({
		queryKey: reportsKeys.recruitment(period.from, period.to),
		queryFn: () => getRecruitmentReportServerFn({ data: period }),
		// Changing the period keeps the old numbers on screen until the new ones arrive.
		placeholderData: (previous) => previous,
	});
}
