import { createFileRoute } from "@tanstack/react-router";
import { TimeSheetsPage } from "#/features/timesheets/pages/TimeSheetsPage";
import { validateTimeSheetsSearch } from "#/features/timesheets/searchParams";

export const Route = createFileRoute("/app/timesheets")({
	component: RouteComponent,
	staticData: { breadcrumb: "Time sheets" },

	/*
	 * The month, the tab and the person being read all live in the address: the month is the
	 * context of the whole page, so a link has to be able to carry "August, the approval queue,
	 * this person" - which is exactly what somebody pastes into a chat when chasing a month.
	 */
	validateSearch: validateTimeSheetsSearch,
});

function RouteComponent() {
	return <TimeSheetsPage />;
}
