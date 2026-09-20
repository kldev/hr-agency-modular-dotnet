import { createFileRoute } from "@tanstack/react-router";
import { TeamDetailsPage } from "#/features/teams/pages/TeamDetailsPage";

export const Route = createFileRoute("/app/teams/$id")({
	component: RouteComponent,
	validateSearch: (search) => ({
		search: typeof search.search === "string" ? search.search : undefined,
	}),
});

function RouteComponent() {
	return <TeamDetailsPage />;
}
