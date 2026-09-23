import { createFileRoute } from "@tanstack/react-router";

import AplicationsPage from "#/features/applications/pages/AplicationsPage";
import { validateApplicationsSearch } from "#/features/applications/searchParams";

export const Route = createFileRoute("/app/applications/")({
	component: RouteComponent,
	staticData: {
		breadcrumb: "Applications",
	},
	validateSearch: validateApplicationsSearch,
});

function RouteComponent() {
	return <AplicationsPage />;
}
