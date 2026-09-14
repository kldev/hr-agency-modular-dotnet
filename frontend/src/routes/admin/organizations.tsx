import { createFileRoute } from "@tanstack/react-router";
import OrganizationsPage from "#/platform-owner/features/organizations/pages/OrganizationsPage";

export const Route = createFileRoute("/admin/organizations")({
	component: RouteComponent,
});

function RouteComponent() {
	return <OrganizationsPage />;
}
