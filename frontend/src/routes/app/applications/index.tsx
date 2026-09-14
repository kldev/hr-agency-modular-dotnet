import { createFileRoute } from "@tanstack/react-router";
import AplicationsPage from "#/features/applications/pages/AplicationsPage";

export const Route = createFileRoute("/app/applications/")({
	component: RouteComponent,
	staticData: {
		breadcrumb: "Applications",
	},
});

function RouteComponent() {
	return <AplicationsPage />;
}
