import { createFileRoute } from "@tanstack/react-router";
import { CompanyDetailsPage } from "#/features/companies/pages/CompanyDetailsPage";

export const Route = createFileRoute("/app/companies/$id")({
	component: RouteComponent,
});

function RouteComponent() {
	return <CompanyDetailsPage />;
}
