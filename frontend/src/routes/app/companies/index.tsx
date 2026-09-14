import { createFileRoute } from "@tanstack/react-router";
import CompaniesPage from "#/features/companies/pages/CompaniesPage";

export const Route = createFileRoute("/app/companies/")({
	component: RouteComponent,
});

function RouteComponent() {
	return <CompaniesPage />;
}
