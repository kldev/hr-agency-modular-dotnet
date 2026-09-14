import { createFileRoute } from "@tanstack/react-router";
import { CompanyDetailsPage } from "#/features/companies/pages/CompanyDetailsPage";

export const Route = createFileRoute("/app/companies/$id")({
	component: RouteComponent,
	validateSearch: (search) => ({
		search: typeof search.search === "string" ? search.search : undefined,
	}),
});

function RouteComponent() {
	return <CompanyDetailsPage />;
}
