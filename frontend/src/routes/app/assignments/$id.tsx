import { createFileRoute } from "@tanstack/react-router";
import AssignmentDetailsPage from "#/features/assignments/pages/AssignmentDetailsPage";

export const Route = createFileRoute("/app/assignments/$id")({
	component: RouteComponent,
	staticData: { breadcrumb: "Assignment" },

	validateSearch: (search) => ({
		search: typeof search.search === "string" ? search.search : undefined,
	}),
});

function RouteComponent() {
	return <AssignmentDetailsPage />;
}
