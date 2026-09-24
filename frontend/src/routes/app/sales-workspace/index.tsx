import { createFileRoute } from "@tanstack/react-router";
import { useCallback } from "react";
import SalesWorkspacePage from "#/features/sales/workspace/SalesWorkspacePage";
import { parseWorkspaceSearch, type WorkspaceSearch } from "#/features/sales/workspace/search";

export const Route = createFileRoute("/app/sales-workspace/")({
	validateSearch: (search): WorkspaceSearch => parseWorkspaceSearch(search),
	component: RouteComponent,
});

function RouteComponent() {
	const search = Route.useSearch();
	const navigate = Route.useNavigate();

	const onSearchChange = useCallback(
		(next: WorkspaceSearch, replace = false) => navigate({ search: next, replace }),
		[navigate],
	);

	return <SalesWorkspacePage search={search} onSearchChange={onSearchChange} />;
}
