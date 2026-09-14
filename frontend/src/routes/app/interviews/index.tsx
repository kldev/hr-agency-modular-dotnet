import { createFileRoute } from "@tanstack/react-router";
import InterviewsPage from "#/features/interviews/pages/InterviewsPage";

export const Route = createFileRoute("/app/interviews/")({
	component: RouteComponent,
});

function RouteComponent() {
	return <InterviewsPage />;
}
