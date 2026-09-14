import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/admin/organizations/$id")({
	component: RouteComponent,
	validateSearch: (search) => ({
		search: typeof search.search === "string" ? search.search : undefined,
	}),
});

function RouteComponent() {
	return <div>Hello "/admin/organizations/$id"!</div>;
}
