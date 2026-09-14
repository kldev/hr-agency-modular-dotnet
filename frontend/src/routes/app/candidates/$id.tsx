import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/app/candidates/$id")({
	component: RouteComponent,
});

function RouteComponent() {
	return <div>Hello "/candidates/$id"!</div>;
}
