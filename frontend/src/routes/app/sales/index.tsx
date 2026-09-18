import { createFileRoute } from "@tanstack/react-router";
import type { OpportunityStage } from "#/api/models";
import SalesPage from "#/features/sales/pages/SalesPage";
import type { SalesView } from "#/features/sales/types";

export const Route = createFileRoute("/app/sales/")({
	component: RouteComponent,
	validateSearch: (search) => ({
		search: typeof search.search === "string" ? search.search : undefined,
		stage: typeof search.stage === "string" ? (search.stage as OpportunityStage) : undefined,
		view: search.view === "kanban" ? ("kanban" as SalesView) : undefined,
	}),
});

function RouteComponent() {
	return <SalesPage />;
}
