import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/app/job-descriptions/add")({
	component: RouteComponent,
});

function RouteComponent() {
	return <div>Hello "/job-descriptions/add"!</div>;
}
