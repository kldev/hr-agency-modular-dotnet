import { createFileRoute } from "@tanstack/react-router";
import type { OpportunityStage } from "#/api/models";

export const Route = createFileRoute("/app/sales/$id")({
	component: RouteComponent,
	validateSearch: (search) => ({
		stage: typeof search.stage === "string" ? (search.stage as OpportunityStage) : undefined,
		search: typeof search.search === "string" ? search.search : undefined,
	}),
});

function RouteComponent() {
	return <div>Hello "/app/sales/$id"!</div>;
}
