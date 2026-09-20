import { createFileRoute } from "@tanstack/react-router";
import type { ProjectStatus } from "#/api/models";
import ProjectsPage from "#/features/projects/pages/ProjectsPage";

export const Route = createFileRoute("/app/projects/")({
	component: RouteComponent,
	staticData: { breadcrumb: "Projects" },

	/*
	 * `companyId` and `country` are here because the API filters on them and a link from a company
	 * or a country report should land on a filtered list. The toolbar itself offers search and
	 * status - the other two arrive in the URL and stay there.
	 */
	validateSearch: (search) => ({
		search: typeof search.search === "string" ? search.search : undefined,
		status: typeof search.status === "string" ? (search.status as ProjectStatus) : undefined,
		companyId: typeof search.companyId === "string" ? search.companyId : undefined,
		country: typeof search.country === "string" ? search.country : undefined,
	}),
});

function RouteComponent() {
	return <ProjectsPage />;
}
