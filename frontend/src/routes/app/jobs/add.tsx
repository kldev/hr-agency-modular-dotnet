import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/app/jobs/add")({
	component: RouteComponent,
});

function RouteComponent() {
	return <div>Hello "/jobs/add"!</div>;
}
