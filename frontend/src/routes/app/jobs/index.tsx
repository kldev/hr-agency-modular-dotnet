import { createFileRoute } from "@tanstack/react-router";

import JobsPage from "#/features/job-posts/pages/JobsPage";

export const Route = createFileRoute("/app/jobs/")({
	component: RouteComponent,
});

function RouteComponent() {
	return <JobsPage />;
}
