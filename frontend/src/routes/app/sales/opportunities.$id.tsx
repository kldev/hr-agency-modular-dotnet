import { createFileRoute } from "@tanstack/react-router";
import OpportunityDetailsPage from "#/features/sales/pages/OpportunityDetailsPage";

export const Route = createFileRoute("/app/sales/opportunities/$id")({
	component: RouteComponent,
	staticData: {
		breadcrumb: "Opportunity",
	},
});

function RouteComponent() {
	const { id } = Route.useParams();
	return <OpportunityDetailsPage id={id} />;
}
