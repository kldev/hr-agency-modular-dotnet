import { createFileRoute } from "@tanstack/react-router";
import type { CandidateSource } from "#/api/models";
import CandidateDetailsPage from "#/features/candidates/pages/CandidateDetailsPage";

export const Route = createFileRoute("/app/candidates/$id")({
	component: RouteComponent,
	validateSearch: (search) => ({
		source: typeof search.source === "string" ? (search.source as CandidateSource) : undefined,

		search: typeof search.search === "string" ? search.search : undefined,
	}),
});

function RouteComponent() {
	const { id } = Route.useParams();
	return <CandidateDetailsPage id={id} />;
}
