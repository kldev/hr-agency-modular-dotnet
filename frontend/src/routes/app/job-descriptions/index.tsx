import { createFileRoute } from "@tanstack/react-router";
import JobDescriptionPage from "#/features/job-descriptions/pages/JobDescriptionPage";

export const Route = createFileRoute("/app/job-descriptions/")({
	component: RouteComponent,
});

function RouteComponent() {
	return <JobDescriptionPage />;
}
