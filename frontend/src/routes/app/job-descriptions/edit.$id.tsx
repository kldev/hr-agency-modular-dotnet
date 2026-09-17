import { createFileRoute } from "@tanstack/react-router";
import { EditJobDescriptionWizard } from "#/features/job-descriptions/wizards";

export const Route = createFileRoute("/app/job-descriptions/edit/$id")({
	component: RouteComponent,
});

function RouteComponent() {
	const { id } = Route.useParams();

	return <EditJobDescriptionWizard id={id} />;
}
