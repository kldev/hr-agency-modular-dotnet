import { createFileRoute } from "@tanstack/react-router";
import type { CandidateSource, JobApplicationStatus } from "#/api/models";
import ApplicationDetailsPage from "#/features/applications/pages/ApplicationDetailsPage";

export const Route = createFileRoute("/app/applications/$id")({
	component: RouteComponent,
	validateSearch: (search) => ({
		status: typeof search.status === "string" ? (search.status as JobApplicationStatus) : undefined,
		source: typeof search.source === "string" ? (search.source as CandidateSource) : undefined,

		search: typeof search.search === "string" ? search.search : undefined,
	}),
});

function RouteComponent() {
	const { id } = Route.useParams();
	return <ApplicationDetailsPage id={id} />;
}
