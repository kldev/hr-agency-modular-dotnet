import { createFileRoute } from "@tanstack/react-router";
import { EditJobPostWizard } from "#/features/job-posts/wizards";

export const Route = createFileRoute("/app/jobs/edit/$id")({
	component: RouteComponent,
});

function RouteComponent() {
	const { id } = Route.useParams();

	return <EditJobPostWizard id={id} />;
}
