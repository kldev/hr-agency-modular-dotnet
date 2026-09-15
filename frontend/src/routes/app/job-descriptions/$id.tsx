import { createFileRoute } from "@tanstack/react-router";
import type { JobDescriptionStatus } from "#/api/models";
import JobDescriptionDetailsPage from "#/features/job-descriptions/pages/JobDescriptionDetailsPage";

export const Route = createFileRoute("/app/job-descriptions/$id")({
	component: RouteComponent,
	beforeLoad: () => {},
	validateSearch: (search) => ({
		status: typeof search.status === "string" ? (search.status as JobDescriptionStatus) : undefined,
		search: typeof search.search === "string" ? search.search : undefined,
	}),
});

function RouteComponent() {
	const { id } = Route.useParams();

	return <JobDescriptionDetailsPage id={id} />;
}
