import { createFileRoute } from "@tanstack/react-router";
import TeamsPage from "#/features/teams/pages/TeamsPage";

export const Route = createFileRoute("/app/teams/")({
	component: RouteComponent,
	validateSearch: (search) => ({
		search: typeof search.search === "string" ? search.search : undefined,
	}),
});

function RouteComponent() {
	return <TeamsPage />;
}
