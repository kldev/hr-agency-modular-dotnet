import { createFileRoute } from "@tanstack/react-router";
import CandidatesPage from "#/features/candidates/pages/CandidatesPage";

export const Route = createFileRoute("/app/candidates/")({
	component: RouteComponent,
});

function RouteComponent() {
	return <CandidatesPage />;
}
