import { createFileRoute } from "@tanstack/react-router";
import OrgStructurePage from "#/features/org-structure/pages/OrgStructurePage";

export const Route = createFileRoute("/app/org-structure")({
	component: RouteComponent,
	staticData: { breadcrumb: "Org structure" },

	/*
	 * The selected unit is in the address rather than in state, so a link can point at a department.
	 * `includeArchived` sits next to it for the same reason the positions list keeps it there: a
	 * chart with the dissolved units shown is a different view, and it should survive a reload.
	 */
	validateSearch: (search) => ({
		unit: typeof search.unit === "string" ? search.unit : undefined,
		includeArchived: search.includeArchived === true ? true : undefined,
	}),
});

function RouteComponent() {
	return <OrgStructurePage />;
}
