import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/app/sales/opportunities/$id")({
	component: RouteComponent,
});

function RouteComponent() {
	return <div>Hello "/sales/opportunities/$id"!</div>;
}
