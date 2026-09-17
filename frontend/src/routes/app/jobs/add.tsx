import { createFileRoute } from "@tanstack/react-router";
import { CreateJobPostWizard } from "#/features/job-posts/wizards";

export const Route = createFileRoute("/app/jobs/add")({
	component: RouteComponent,
	validateSearch: (search: Record<string, unknown>) => ({
		jobDescriptionId:
			typeof search.jobDescriptionId === "string" ? search.jobDescriptionId : undefined,
		fromJobPostId: typeof search.fromJobPostId === "string" ? search.fromJobPostId : undefined,
	}),
});

function RouteComponent() {
	return <CreateJobPostWizard />;
}
