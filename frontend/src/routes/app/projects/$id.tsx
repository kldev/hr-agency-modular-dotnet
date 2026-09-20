import { createFileRoute } from "@tanstack/react-router";
import { ProjectDetailsPage } from "#/features/projects/pages/ProjectDetailsPage";

export const Route = createFileRoute("/app/projects/$id")({
	component: RouteComponent,
	staticData: { breadcrumb: "Project" },

	/** Carried over from the list so that going back lands on the same filtered list. */
	validateSearch: (search) => ({
		search: typeof search.search === "string" ? search.search : undefined,
	}),
});

function RouteComponent() {
	return <ProjectDetailsPage />;
}
