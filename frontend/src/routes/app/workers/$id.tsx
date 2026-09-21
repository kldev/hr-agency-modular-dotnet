import { createFileRoute } from "@tanstack/react-router";
import WorkerDetailsPage from "#/features/workers/pages/WorkerDetailsPage";

export const Route = createFileRoute("/app/workers/$id")({
	component: RouteComponent,
	staticData: { breadcrumb: "Worker" },

	/* Carried through so that going back from here lands on the list the user came from. */
	validateSearch: (search) => ({
		search: typeof search.search === "string" ? search.search : undefined,
	}),
});

function RouteComponent() {
	return <WorkerDetailsPage />;
}
