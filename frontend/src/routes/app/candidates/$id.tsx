import { createFileRoute } from "@tanstack/react-router";
import type { CandidateSource } from "#/api/models";

export const Route = createFileRoute("/app/candidates/$id")({
	component: RouteComponent,
	validateSearch: (search) => ({
		source: typeof search.source === "string" ? (search.source as CandidateSource) : undefined,

		search: typeof search.search === "string" ? search.search : undefined,
	}),
});

//CandidateSource
function RouteComponent() {
	return <div>Hello "/candidates/$id"!</div>;
}
