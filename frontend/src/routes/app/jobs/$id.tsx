import { createFileRoute } from "@tanstack/react-router";
import type { JobPostStatus } from "#/api/models";
import JobPostDetailsPage from "#/features/job-posts/pages/JobDetailsPage";

export const Route = createFileRoute("/app/jobs/$id")({
	component: RouteComponent,
	validateSearch: (search) => ({
		search: typeof search.search === "string" ? search.search : undefined,
		status: typeof search.status === "string" ? (search.status as JobPostStatus) : undefined,
	}),
});

function RouteComponent() {
	return <JobPostDetailsPage />;
}
