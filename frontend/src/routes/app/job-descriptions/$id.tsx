import { createFileRoute } from "@tanstack/react-router";
import type { JobDescriptionStatus } from "#/api/models";
import { useGetJobDescription } from "#/features/job-descriptions/pages/hooks/useJobDescription";
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
	var query = useGetJobDescription(id);
	console.log("Fetch before load: ", id);
	if (query.error) throw query.error;
	if (query.isPending) return null;
	return <JobDescriptionDetailsPage jobDescription={query.data} />;
}
