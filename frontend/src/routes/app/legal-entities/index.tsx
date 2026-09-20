import { createFileRoute } from "@tanstack/react-router";
import { LegalEntitiesPage } from "#/features/legal-entities/pages/LegalEntitiesPage";

export const Route = createFileRoute("/app/legal-entities/")({
	component: RouteComponent,
	validateSearch: (search) => ({
		search: typeof search.search === "string" ? search.search : undefined,
		activeOnly: search.activeOnly === true || search.activeOnly === "true" ? true : undefined,
	}),
});

function RouteComponent() {
	return <LegalEntitiesPage />;
}
