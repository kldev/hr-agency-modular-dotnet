import { createFileRoute } from "@tanstack/react-router";
import {
	ProjectDetailsPage,
	type ProjectTab,
	projectTabs,
} from "#/features/projects/pages/ProjectDetailsPage";

export const Route = createFileRoute("/app/projects/$id")({
	component: RouteComponent,
	staticData: { breadcrumb: "Project" },

	validateSearch: (search) => ({
		/* The open section is part of the address, so a link can point at a project's compliance. */
		tab: projectTabs.includes(search.tab as ProjectTab) ? (search.tab as ProjectTab) : undefined,

		/** Carried over from the list so that going back lands on the same filtered list. */
		search: typeof search.search === "string" ? search.search : undefined,
	}),
});

function RouteComponent() {
	const { tab } = Route.useSearch();
	const navigate = Route.useNavigate();

	return (
		<ProjectDetailsPage
			tab={tab ?? "overview"}
			onTabChange={(next) => navigate({ search: (previous) => ({ ...previous, tab: next }) })}
		/>
	);
}
