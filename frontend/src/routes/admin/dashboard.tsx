import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/admin/dashboard")({
	component: RouteComponent,
	staticData: {
		breadcrumb: "/",
	},
});

function RouteComponent() {
	return <div>Hello "/admin/dashboard"!</div>;
}
