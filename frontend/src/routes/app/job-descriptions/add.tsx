import { createFileRoute } from "@tanstack/react-router";
import { CreateJobDescriptionWizard } from "#/features/job-descriptions/wizards";

export const Route = createFileRoute("/app/job-descriptions/add")({
	component: RouteComponent,
	validateSearch: (search: Record<string, unknown>) => ({
		companyId: typeof search.companyId === "string" ? search.companyId : undefined,
	}),
});

function RouteComponent() {
	return <CreateJobDescriptionWizard />;
}
