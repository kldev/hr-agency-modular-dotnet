import { createFileRoute } from "@tanstack/react-router";
import WorkerDetailsTabbedPage, {
	type WorkerTab,
	workerTabs,
} from "#/features/workers/pages/WorkerDetailsTabbedPage";

/*
 * The tabbed variant of the worker details page, kept as a separate route so the two layouts can be
 * compared on real records - and so that dropping the loser is dropping a file.
 */
export const Route = createFileRoute("/app/workers/tabs/$id")({
	component: RouteComponent,
	staticData: { breadcrumb: "Worker" },

	validateSearch: (search) => ({
		tab: workerTabs.includes(search.tab as WorkerTab) ? (search.tab as WorkerTab) : undefined,
		search: typeof search.search === "string" ? search.search : undefined,
	}),
});

function RouteComponent() {
	const { tab } = Route.useSearch();
	const navigate = Route.useNavigate();

	return (
		<WorkerDetailsTabbedPage
			tab={tab ?? "overview"}
			onTabChange={(next) => navigate({ search: (previous) => ({ ...previous, tab: next }) })}
		/>
	);
}
