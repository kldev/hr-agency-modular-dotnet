import { useQuery } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { getPlatformReport } from "#/api/endpoints";
import { reportsKeys } from "#/api/query-keys";
import type { ReportPeriod } from "#/features/reports/period";
import { getFnOptions } from "#/server/axios";

const getPlatformReportServerFn = createServerFn({ method: "GET" })
	.validator((input: ReportPeriod) => input)
	.handler(async ({ data }) => getPlatformReport(data, await getFnOptions()));

export function usePlatformReport(period: ReportPeriod) {
	return useQuery({
		queryKey: reportsKeys.platform(period.from, period.to),
		queryFn: () => getPlatformReportServerFn({ data: period }),
		placeholderData: (previous) => previous,
	});
}
