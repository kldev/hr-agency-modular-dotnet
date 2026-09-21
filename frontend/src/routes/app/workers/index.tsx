import { createFileRoute } from "@tanstack/react-router";
import WorkersPage from "#/features/workers/pages/WorkersPage";
import { emptyWorkersSearch, validateWorkersSearch } from "#/features/workers/searchParams";

export const Route = createFileRoute("/app/workers/")({
	component: RouteComponent,
	staticData: { breadcrumb: "Workers" },
	validateSearch: validateWorkersSearch,
});

function RouteComponent() {
	const search = Route.useSearch();
	const navigate = Route.useNavigate();

	return (
		<WorkersPage
			scope="all"
			search={search}
			onSearchChange={(next) => navigate({ search: (previous) => ({ ...previous, ...next }) })}
			onClear={() => navigate({ search: () => emptyWorkersSearch })}
		/>
	);
}
