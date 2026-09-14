import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/app/job-descriptions/edit/$id")({
	component: RouteComponent,
});

function RouteComponent() {
	return <div>Hello "/job-descriptions/edit/$id"!</div>;
}
