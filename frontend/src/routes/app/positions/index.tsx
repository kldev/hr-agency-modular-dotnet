import { createFileRoute } from "@tanstack/react-router";
import type { WorkerContractType } from "#/api/models";
import PositionsPage from "#/features/positions/pages/PositionsPage";

export const Route = createFileRoute("/app/positions/")({
	component: RouteComponent,
	staticData: { breadcrumb: "Positions" },

	/*
	 * `projectId` arrives from a project's own tab, so "all roles on this delivery" is a link
	 * rather than a filter somebody has to rebuild. The toolbar owns the other three.
	 */
	validateSearch: (search) => ({
		search: typeof search.search === "string" ? search.search : undefined,
		projectId: typeof search.projectId === "string" ? search.projectId : undefined,
		contractType:
			typeof search.contractType === "string"
				? (search.contractType as WorkerContractType)
				: undefined,
		includeArchived: search.includeArchived === true ? true : undefined,
	}),
});

function RouteComponent() {
	return <PositionsPage />;
}
