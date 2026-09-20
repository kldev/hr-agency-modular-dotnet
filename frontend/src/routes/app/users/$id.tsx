import { createFileRoute } from "@tanstack/react-router";
import { UserDetailsPage } from "#/features/users/pages/UserDetailsPage";

export const Route = createFileRoute("/app/users/$id")({
	component: RouteComponent,
});

function RouteComponent() {
	return <UserDetailsPage />;
}
