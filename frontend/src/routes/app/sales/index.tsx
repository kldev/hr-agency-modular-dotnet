import { createFileRoute } from "@tanstack/react-router";
import SalesPage from "#/features/sales/pages/SalesPage";

export const Route = createFileRoute("/app/sales/")({
	component: RouteComponent,
});

function RouteComponent() {
	return <SalesPage />;
}
