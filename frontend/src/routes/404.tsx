import { createFileRoute } from "@tanstack/react-router";
import NotFoundPage from "#/features/common/NotFoundPage";

export const Route = createFileRoute("/404")({
	component: RouteComponent,
});

function RouteComponent() {
	return <NotFoundPage />;
}
