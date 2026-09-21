import { createFileRoute } from "@tanstack/react-router";
import WorkersPage from "#/features/workers/pages/WorkersPage";
import { emptyWorkersSearch, validateWorkersSearch } from "#/features/workers/searchParams";

/*
 * The same page over the other half of the register. A separate route rather than a toolbar toggle
 * because the two desks are separate desks - and because the link has to be bookmarkable.
 */
export const Route = createFileRoute("/app/workers/abroad")({
	component: RouteComponent,
	staticData: { breadcrumb: "Workers abroad" },
	validateSearch: validateWorkersSearch,
});

function RouteComponent() {
	const search = Route.useSearch();
	const navigate = Route.useNavigate();

	return (
		<WorkersPage
			scope="abroad"
			search={search}
			onSearchChange={(next) => navigate({ search: (previous) => ({ ...previous, ...next }) })}
			onClear={() => navigate({ search: () => emptyWorkersSearch })}
		/>
	);
}
