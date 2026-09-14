import { createFileRoute } from "@tanstack/react-router";
import JobPostDetailsPage from "#/features/job-posts/pages/JobDetailsPage";

export const Route = createFileRoute("/app/jobs/$id")({
	component: RouteComponent,
});

function RouteComponent() {
	return <JobPostDetailsPage />;
}
