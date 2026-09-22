import type { MonthInView } from "../../types";

/**
 * Where the settlement file for a month is downloaded from. A plain link, not a query: the proxy
 * adds the credential from the cookie, so the browser never holds a bearer and the file is saved
 * the way any download is - no blob kept in memory, no object url to revoke.
 *
 * Downloading changes nothing. The month is settled separately, after the transfers are made.
 */
export function useSettlementExport(month: MonthInView) {
	const params = new URLSearchParams({ year: String(month.year), month: String(month.month) });

	return {
		href: `/api/timesheets/settlement/export?${params}`,
		fileName: `settlement-${month.year}-${String(month.month).padStart(2, "0")}.xlsx`,
	};
}
