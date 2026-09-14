import { createFileRoute } from "@tanstack/react-router";
import type { OrganizationRoleApi } from "#/api/models";

export const Route = createFileRoute("/app/users/$id")({
	component: RouteComponent,
	validateSearch: (search) => ({
		search: typeof search.search === "string" ? search.search : undefined,
		role: typeof search.role === "string" ? (search.role as OrganizationRoleApi) : undefined,
	}),
});

function RouteComponent() {
	return <div>Hello "/app/users/$id"!</div>;
}
