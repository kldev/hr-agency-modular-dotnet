import { createFileRoute } from "@tanstack/react-router";
import type { OpportunityStage } from "#/api/models";
import { parseViewMode } from "#/components/ui";
import SalesPage from "#/features/sales/pages/SalesPage";

export const Route = createFileRoute("/app/sales/")({
	component: RouteComponent,
	validateSearch: (search) => ({
		search: typeof search.search === "string" ? search.search : undefined,
		stage: typeof search.stage === "string" ? (search.stage as OpportunityStage) : undefined,
		view: parseViewMode(search.view),
	}),
});

function RouteComponent() {
	return <SalesPage />;
}
