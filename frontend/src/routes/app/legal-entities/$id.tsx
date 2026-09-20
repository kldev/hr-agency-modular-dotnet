import { createFileRoute } from "@tanstack/react-router";
import { LegalEntityDetailsPage } from "#/features/legal-entities/pages/LegalEntityDetailsPage";

export const Route = createFileRoute("/app/legal-entities/$id")({
	component: RouteComponent,
});

function RouteComponent() {
	return <LegalEntityDetailsPage />;
}
