import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/app/jobs/edit/$id")({
	component: RouteComponent,
});

function RouteComponent() {
	return <div>Hello "/jobs/edit/$id"!</div>;
}
