import { createFileRoute } from "@tanstack/react-router";
import JobsDescriptopnPage from "#/features/job-descriptions/pages/JobsDescriptopnPage";

export const Route = createFileRoute("/app/job-descriptions/")({
	component: RouteComponent,
});

function RouteComponent() {
	return <JobsDescriptopnPage />;
}
