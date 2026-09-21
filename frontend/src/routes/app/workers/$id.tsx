import { createFileRoute } from "@tanstack/react-router";
import WorkerDetailsPage, {
	type WorkerTab,
	workerTabs,
} from "#/features/workers/pages/WorkerDetailsPage";

export const Route = createFileRoute("/app/workers/$id")({
	component: RouteComponent,
	staticData: { breadcrumb: "Worker" },

	validateSearch: (search) => ({
		/* The open section is part of the address, so a link can point at somebody's documents. */
		tab: workerTabs.includes(search.tab as WorkerTab) ? (search.tab as WorkerTab) : undefined,

		/* Carried through so that going back from here lands on the list the user came from. */
		search: typeof search.search === "string" ? search.search : undefined,
	}),
});

function RouteComponent() {
	const { tab } = Route.useSearch();
	const navigate = Route.useNavigate();

	return (
		<WorkerDetailsPage
			tab={tab ?? "overview"}
			onTabChange={(next) => navigate({ search: (previous) => ({ ...previous, tab: next }) })}
		/>
	);
}
